using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Ezhu.AutoUpdater.Commons;

namespace Ezhu.AutoUpdater
{
    public class Constants
    {
        //public static readonly string _wpfAutoUpdateUrl = "http://localhost:8081/";
        private static string _wpfAutoUpdateUrl = "";
        public static string WpfAutoUpdateUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_wpfAutoUpdateUrl))
                {
                    //_wpfAutoUpdateUrl = System.Configuration.ConfigurationManager.AppSettings["ED"];
                    //_wpfAutoUpdateUrl = EncodeHelper.AES_Decrypt(_wpfAutoUpdateUrl);

                    _wpfAutoUpdateUrl = System.Configuration.ConfigurationManager.AppSettings["WpfAutoUpdateUrl"];

                    return _wpfAutoUpdateUrl;
                }
                else
                {
                    return _wpfAutoUpdateUrl;
                }
            }
            set { _wpfAutoUpdateUrl = value; }
        }
    }
}