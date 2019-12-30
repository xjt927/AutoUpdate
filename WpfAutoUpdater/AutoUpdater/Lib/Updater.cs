using System;
using System.IO;
using System.Net;
using System.Windows;


using System.Xml.Linq;
using Quartz;
using 内部运营管理系统;

namespace Ezhu.AutoUpdater
{
    /// <summary>
    /// 更新主体方法
    /// 定时更新
    /// </summary>
    public class Updater : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            CheckUpdateStatus();
        }

        public static string ServiceVersion = "";
        private static Updater _instance;
        public static Updater Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Updater();
                }
                return _instance;
            }
        }

        public static bool CheckUpdateStatus()
        {
            bool isUpdate = false;
            System.Threading.ThreadPool.QueueUserWorkItem((s) =>
            {
                //string url = Constants.WpfAutoUpdateUrl + Updater.Instance.CallExeName + "/update.xml";
                string url = Constants.WpfAutoUpdateUrl + "/update.xml";
                var client = new System.Net.WebClient();
                client.DownloadDataCompleted += (x, y) =>
                {
                    MemoryStream stream = null;
                    try
                    {
                        try
                        {

                            stream = new MemoryStream(y.Result);

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        try
                        {
                            XDocument xDoc = XDocument.Load(stream);
                            UpdateInfo updateInfo = new UpdateInfo();
                            XElement root = xDoc.Element("UpdateInfo");
                            updateInfo.AppName = root.Element("AppName").Value;
                            updateInfo.AppVersion = root.Element("AppVersion") == null ||
                                                    string.IsNullOrEmpty(root.Element("AppVersion").Value)
                                ? null
                                : new Version(root.Element("AppVersion").Value);
                            updateInfo.RequiredMinVersion = root.Element("RequiredMinVersion") == null ||
                                                            string.IsNullOrEmpty(root.Element("RequiredMinVersion").Value)
                                ? null
                                : new Version(root.Element("RequiredMinVersion").Value);
                            updateInfo.Desc = root.Element("Desc").Value;
                            updateInfo.MD5 = Guid.NewGuid();

                            stream.Close();
                            isUpdate = Updater.Instance.StartUpdate(updateInfo);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                };
                client.DownloadDataAsync(new Uri(url));
            });
            GC.Collect();
            return isUpdate;
        }

        public bool StartUpdate(UpdateInfo updateInfo)
        {
            try
            {

                if (updateInfo.RequiredMinVersion != null && Updater.Instance.CurrentVersion < updateInfo.RequiredMinVersion)
                {
                    //当前版本比需要的版本小，不更新
                    return false;
                }

                if (Updater.Instance.CurrentVersion >= updateInfo.AppVersion)
                {
                    //当前版本是最新的，不更新
                    //System.Windows.MessageBox.Show("当前是最新版本！");
                    return false;
                }

                ////自己修改的更新方法
                //var ini = new IniClass(System.AppDomain.CurrentDomain.BaseDirectory + @"\Update.ini");
                //string clientVersion = ini.IniReadValue("update", "version");//客户端版本
                //ServiceVersion = updateInfo.AppVersion.ToString();//服务端版本
                //if (clientVersion == ServiceVersion)
                //{
                //    return;
                //}

                //更新程序复制到缓存文件夹
                string appDir = System.IO.Path.Combine(System.Reflection.Assembly.GetEntryAssembly().Location.Substring(0, System.Reflection.Assembly.GetEntryAssembly().Location.LastIndexOf(System.IO.Path.DirectorySeparatorChar)));
                string updateFileDir = System.IO.Path.Combine(System.IO.Path.Combine(appDir.Substring(0, appDir.LastIndexOf(System.IO.Path.DirectorySeparatorChar))), "Update");
                if (!Directory.Exists(updateFileDir))
                {
                    Directory.CreateDirectory(updateFileDir);
                }
                updateFileDir = System.IO.Path.Combine(updateFileDir, updateInfo.MD5.ToString());
                if (!Directory.Exists(updateFileDir))
                {
                    Directory.CreateDirectory(updateFileDir);
                }

                string exePath = System.IO.Path.Combine(updateFileDir, "AutoUpdater.exe");
                File.Copy(System.IO.Path.Combine(appDir, "AutoUpdater.exe"), exePath, true);

                var info = new System.Diagnostics.ProcessStartInfo(exePath);
                //info.UseShellExecute = true;
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.WorkingDirectory = exePath.Substring(0, exePath.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
                updateInfo.Desc = updateInfo.Desc;
                info.Arguments = "update " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(CallExeName)) +
                                 " " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(updateFileDir)) + " " +
                                 Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(appDir)) + " " +
                                 Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(updateInfo.AppName)) + " " +
                                 Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(updateInfo.AppVersion.ToString())) + " " +
                                 (string.IsNullOrEmpty(updateInfo.Desc) ? "" : Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(updateInfo.Desc))) + " " +
                                 (string.IsNullOrEmpty(Constants.WpfAutoUpdateUrl) ? "" : Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Constants.WpfAutoUpdateUrl)));
                System.Diagnostics.Process.Start(info);

                //var compiler = new System.Diagnostics.Process();
                //compiler.StartInfo.FileName = "csc.exe";
                //compiler.StartInfo.Arguments = "/r:System.dll /out:sample.exe stdstr.cs";
                //compiler.StartInfo.UseShellExecute = false;
                //compiler.StartInfo.RedirectStandardOutput = true;
                //compiler.Start();    
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool UpdateFinished = false;

        private string _callExeName;
        public string CallExeName
        {
            get
            {
                if (string.IsNullOrEmpty(_callExeName))
                {
                    _callExeName = System.Reflection.Assembly.GetEntryAssembly().Location.Substring(System.Reflection.Assembly.GetEntryAssembly().Location.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1).Replace(".exe", "");
                    //_callExeName = "wpf内部运营系统";
                }
                return _callExeName;
            }
        }

        /// <summary>
        /// 获得当前应用软件的版本
        /// </summary>
        public Version CurrentVersion
        {
            get { return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location).ProductVersion); }
        }

        /// <summary>
        /// 获得当前应用程序的根目录
        /// </summary>
        public string CurrentApplicationDirectory
        {
            get { return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); }
        }

    }
}
