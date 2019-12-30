using System;
using System.IO;
using SharpCompress.Common;
using SharpCompress.Reader;

namespace AutoUpdateApplecation
{
    public class RarClass
    {
        /// <summary>
        /// 利用 WinRAR 进行解压缩
        /// </summary>
        /// <param name="unRarPath">文件解压路径（绝对）</param>
        /// <param name="rarPath">将要解压缩的 .rar 文件的存放目录（绝对路径）</param>
        public static void UnRar(string unRarPath, string rarPath)
        {
            try
            {
                using (Stream stream = File.OpenRead(rarPath))
                {
                    var reader = ReaderFactory.Open(stream);
                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            reader.WriteEntryToDirectory(unRarPath, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(@"解压缩失败!");
            }
        }
    }

}
