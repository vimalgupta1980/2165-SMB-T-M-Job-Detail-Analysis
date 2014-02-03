using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using SysconCommon.Common.Environment;

namespace JobDetailAnalysisDesktop
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            this.chkDebug.Checked = Env.GetConfigVar("debug", false, true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Env.SetConfigVar("debug", this.chkDebug.Checked);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
