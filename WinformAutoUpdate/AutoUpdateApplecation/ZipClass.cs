using System;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;

namespace AutoUpdateApplecation
{
    public class ZipClass
    {
        //设置进度条的委托
        public delegate void SetProgressDelegate(int maximum, string msg);
        #region 解压
        /// <summary>   
        /// 功能：解压zip格式的文件。   
        /// </summary>   
        /// <param name="zipFilePath">压缩文件路径，全路径格式</param>   
        /// <param name="unZipDir">解压文件存放路径,全路径格式，为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹</param>   
        /// <param name="err">出错信息</param>   
        /// <returns>解压是否成功</returns>   
        public static bool UnZip(string zipFilePath, string unZipDir, int maximum, SetProgressDelegate setProgressDelegate)
        {
            if (zipFilePath == string.Empty)
            {
                throw new System.IO.FileNotFoundException("压缩文件不不能为空！");
            }
            if (!File.Exists(zipFilePath))
            {
                throw new System.IO.FileNotFoundException("压缩文件: " + zipFilePath + " 不存在!");
            }
            //解压文件夹为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹   
            if (unZipDir == string.Empty)
                unZipDir = zipFilePath.Replace(Path.GetFileName(zipFilePath), "");
            if (!unZipDir.EndsWith("//"))
                unZipDir += "//";
            if (!Directory.Exists(unZipDir))
                Directory.CreateDirectory(unZipDir);

            try
            {
                using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFilePath)))
                {
                    ZipEntry theEntry;
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        string directoryName = Path.GetDirectoryName(theEntry.Name);
                        string fileName = Path.GetFileName(theEntry.Name);
                        if (directoryName.Length > 0)
                        {
                            Directory.CreateDirectory(unZipDir + directoryName);
                        }
                        if (!directoryName.EndsWith("//"))
                            directoryName += "//";
                        if (fileName != String.Empty)
                        {
                            using (FileStream streamWriter = File.Create(unZipDir + theEntry.Name))
                            {

                                int size = 2048;
                                byte[] data = new byte[2048];
                                while (true)
                                {
                                    size = s.Read(data, 0, data.Length);
                                    if (size > 0)
                                    {
                                        streamWriter.Write(data, 0, size);
                                    }
                                    else
                                    {
                                        setProgressDelegate(maximum, theEntry.Name);
                                        break;
                                    }
                                }
                            }
                        }
                    }//while   
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return true;
        }//解压结束  
        #endregion

        public static int GetMaximum(string[] zips)
        {
            int maximum = 0;
            ZipInputStream s = null;
            ZipEntry theEntry = null;
            foreach (string zip in zips)
            {
                s = new ZipInputStream(File.OpenRead(Application.StartupPath + @"\" + zip));
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    if (Path.GetFileName(theEntry.Name) != "")
                    {
                        maximum++;
                    }
                }
            }
            if (s != null)
            {
                s.Close();
            }
            return maximum;
        }
    }
}

