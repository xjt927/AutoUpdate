using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Ezhu.AutoUpdater;
using QuartzSendExcel;

namespace Test
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //自动更新
            TimingCheckUpdate();
            Ezhu.AutoUpdater.Updater.CheckUpdateStatus();

            Application.Run(new Form1());
        }

        /// <summary>
        /// 每半小时检测是否有更新
        /// </summary>
        public static void TimingCheckUpdate()
        {
            string timingCheckUpdate = System.Configuration.ConfigurationManager.AppSettings["TimingCheckUpdate"];
            QuartzManager.AddJob<Updater>("TimingCheckUpdate", timingCheckUpdate);
        }
    }
}
