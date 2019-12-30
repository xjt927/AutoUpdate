using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AutoUpdateDemo.AutoUpdateService;
using Update;

namespace AutoUpdate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                IniClass ini = new IniClass(Application.StartupPath + @"\Update.ini");
                AutoUpdateService service = new AutoUpdateService();
                string clientVersion = ini.IniReadValue("update", "version");//客户端版本
                string serviceVersion = service.GetVersion();//服务端版本
                if (clientVersion != serviceVersion)
                {
                    DialogResult dialogResult = MessageBox.Show("有新版本，是否更新？", "升级", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.OK)
                    {
                        Application.Exit();
                        Process.Start("AutoUpdateApplecation.exe");
                    }
                }
                else
                {
                    MessageBox.Show("已更新至最高版本！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
