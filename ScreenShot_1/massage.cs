using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShot_1
{
    public partial class massage : Form
    {
        public massage()
        {
            InitializeComponent();
            label1.Text = Form1.info;
        }

        private void massage_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            this.Opacity = 0.8;
            this.Location = new Point(SystemInformation.VirtualScreen.Width/2-121,SystemInformation.VirtualScreen.Height-70);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Dispose();
            this.Close();
        }
    }
}
