using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace AutoUpdateApplecation
{
    public partial class FrmUpdate : Form
    {
        //关闭进度条的委托
        public delegate void CloseProgressDelegate();
        //声明关闭进度条事件
        public event CloseProgressDelegate CloseProgress;
        UpdateService.AutoUpdateService service = null;//webservice服务
        WebClient wc = null;
        string url;//获取下载地址 
        string[] zips;//获取升级压缩包
        string zip = "";//当前正在下载的压缩包
        int zipsIndex = 0;//当前正在下载的zips下标
        long preBytes = 0;//上一次下载流量
        long currBytes = 0;//当前下载流量
        public FrmUpdate()
        {
            InitializeComponent();
            service = new UpdateService.AutoUpdateService();//webservice服务
            url = service.GetUrl();//获取下载地址 
            zips = service.GetZips();//获取升级压缩包
        }

        private void FrmUpdate_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcesses();
            foreach (System.Diagnostics.Process p in ps)
            {
                //MessageBox.Show(p.ProcessName);
                if (p.ProcessName.ToLower() == "ytjyz" || p.ProcessName.ToLower() == "ytjyz.vshost")
                {
                    p.Kill();
                    break;
                }
            }

            try
            {

                //用子线程工作
                Thread t = new Thread(new ThreadStart(DownLoad));
                t.IsBackground = true;//设为后台线程
                t.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 下载更新
        /// </summary>
        private void DownLoad()
        {
            try
            {
                CloseProgress += new CloseProgressDelegate(FrmUpdate_CloseProgress);
                if (zips.Length > 0)
                {
                    wc = new WebClient();
                    wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
                    wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
                    wc.DownloadFileAsync(new Uri(url + zips[zipsIndex]), zips[zipsIndex]);
                }
                else
                {
                    FrmUpdate_CloseProgress();//调用关闭进度条事件
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 下载完成后触发
        /// </summary>
        void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                zipsIndex++;
                if (zipsIndex < zips.Length)
                {
                    //继续下载下一个压缩包
                    wc = new WebClient();
                    wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
                    wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
                    wc.DownloadFileAsync(new Uri(url + zips[zipsIndex]), zips[zipsIndex]);
                }
                else
                {
                    foreach (string filePath in zips)
                    {
                        try
                        {
                            if (filePath.Contains(".zip"))
                            {
                                //解压
                                int maximum = ZipClass.GetMaximum(zips);
                                ZipClass.UnZip(Application.StartupPath + @"\" + filePath, "", maximum, FrmUpdate_SetProgress);
                            }
                            else if (filePath.Contains(".rar"))
                            {
                                RarClass.UnRar(Application.StartupPath, Application.StartupPath + @"\" + filePath);
                            }
                            File.Delete(Application.StartupPath + @"\" + filePath);
                        }
                        catch (Exception ex)
                        {
                            
                            throw ex;
                        }
                    }
                    FrmUpdate_CloseProgress();//调用关闭进度条事件
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 下载时触发
        /// </summary>
        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged), new object[] { sender, e });
                }
                catch (Exception ex)
                { 
                }
            }
            else
            {
                label1.Text = "正在下载自解压包：" + zips[zipsIndex] + "(" + (zipsIndex + 1).ToString() + "/" + zips.Length + ")";
                progressBar1.Maximum = 100;
                progressBar1.Value = e.ProgressPercentage;
                currBytes = e.BytesReceived;//当前下载流量
            }
        }
        /// <summary>
        /// 解压时进度条事件
        /// </summary>
        /// <param name="maximum">进度条最大值</param>
        private void FrmUpdate_SetProgress(int maximum, string msg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ZipClass.SetProgressDelegate(FrmUpdate_SetProgress), new object[] { maximum, msg });
            }
            else
            {
                timer1.Enabled = false;
                this.Text = "升级";
                if (zipsIndex == zips.Length)
                {
                    //刚压缩完
                    progressBar1.Value = 0;
                    zipsIndex++;
                }
                label1.Text = "正在解压：" + msg + "(" + (progressBar1.Value + 1).ToString() + "/" + maximum + ")";
                progressBar1.Maximum = maximum;
                progressBar1.Value++;

            }
        }

        /// <summary>
        /// 实现关闭进度条事件
        /// </summary>
        private void FrmUpdate_CloseProgress()
        {
            string startProcessName = @"ytjyz.exe";
            if (this.InvokeRequired)
            {
                this.Invoke(new CloseProgressDelegate(FrmUpdate_CloseProgress), null);
            }
            else
            {
                if (wc != null)
                {
                    wc.Dispose();
                }
                if (zips.Length > 0)
                {
                    MessageBox.Show("升级成功！");
                }
                else
                {
                    MessageBox.Show("未找到升级包！");
                }
                IniClass ini = new IniClass(Application.StartupPath + @"\Update.ini");
                string serviceVersion = service.GetVersion();//服务端版本
                ini.IniWriteValue("update", "version", serviceVersion);//更新成功后将版本写入配置文件
                Application.Exit();//退出升级程序
                try
                {
                    Process.Start(startProcessName);//打开主程序Main.exe 
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("升级成功!找不到启动程序 {0}.", startProcessName));
                }
            }
        }
        //1秒计算一次速度
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Text = ((currBytes - preBytes) / 1024).ToString() + "kb/s";//速度
            preBytes = currBytes;//上一次下载流量
        }
    }
}
