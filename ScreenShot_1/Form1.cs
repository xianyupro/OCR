using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using Baidu.Aip;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//Install-Package ImageProcessor -Version 2.6.2.25
//包管理
namespace ScreenShot_1
{
    public partial class Form1 : Form
    {
        public static string info = "";
        public enum KeyModifiers
        {
            None = 0,
            Alt = 1,
            Ctrl = 2,
            Shift = 4,
            WindowsKey = 8
        }
        public static string OCRchoose = "sougou";
        private string OCRresult = "";
        private bool flag_close = false;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            uint ctrlHotKey = (uint)(KeyModifiers.WindowsKey | KeyModifiers.Ctrl);
            uint ctrlHotKey1 = (uint)(KeyModifiers.Alt | KeyModifiers.Ctrl);
            //uint ctrlHotKey = (uint)(KeyModifiers.WindowsKey);
            // 注册热键为Alt+Ctrl+C, "100"为唯一标识热键
            HotKey.RegisterHotKey(Handle, 100, ctrlHotKey, Keys.A);
            HotKey.RegisterHotKey(Handle, 200, ctrlHotKey1, Keys.A);
        }

        /// <summary>
        /// 热键操作函数
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m) 
        {
            //如果m.Msg的值为0x0312那么表示用户按下了热键
            const int WM_HOTKEY = 0x0312;
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    ProcessHotkey(m);
                    break;
            }
            // 将系统消息传递自父类的WndProc
            base.WndProc(ref m);
        }
        private void ProcessHotkey(Message m) //按下设定的键时调用该函数
        { 
            switch (m.WParam.ToInt32())
            {
                case 100:
                    OCR();
                    break;
                case 200:
                    jietu();
                    break;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Visible&&!flag_close)
            {
                e.Cancel = true;
                this.Hide();
                this.notifyIcon1.ShowBalloonTip(10, "Tip", "最小化到托盘", ToolTipIcon.Info);
            }
            else
            {
                HotKey.UnregisterHotKey(Handle, 100);
                HotKey.UnregisterHotKey(Handle, 200);
                e.Cancel = false;
            }
        }

        private void 退出ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            HotKey.UnregisterHotKey(Handle, 100);
            HotKey.UnregisterHotKey(Handle, 200);
            flag_close = true;
            Application.Exit();
        }
        private void screenshot()//打开截图窗口并进行截图
        {
            // 新建一个和屏幕大小相同的图片
            Bitmap CatchBmp = new Bitmap(Screen.AllScreens[0].Bounds.Width, Screen.AllScreens[0].Bounds.Height);
            Graphics g = Graphics.FromImage(CatchBmp);
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(Screen.AllScreens[0].Bounds.Width, Screen.AllScreens[0].Bounds.Height));
            Cutter cutter1 = new Cutter();
            // 指示窗体的背景图片为屏幕图片
            cutter1.BackgroundImage = CatchBmp;
            cutter1.Show();
        }
        public void BaiduAPI()
        {
            var API_KEY = "p8Tgf4cVCWi0QOGjnqfu22G9";
            var SECRET_KEY = "UvzNMtiR728kmjai8UjMLEctfZ2eVPNm";
            var client = new Baidu.Aip.Ocr.Ocr(API_KEY, SECRET_KEY);
            client.Timeout = 6000;  // 修改超时时间
            var image = File.ReadAllBytes("jietu.jpg");
            try
            {
                var result = client.GeneralBasic(image);
                // 调用通用文字识别, 图片参数为本地图片，可能会抛出网络等异常，请使用try/catch捕获
                for (int i = 0; i < result["words_result"].Count(); i++)
                {
                    OCRresult = OCRresult + result["words_result"][i]["words"].ToString() + "\r\n";
                }

            }
            catch (OverflowException)
            {
                info = "网络出错请重试";
                massage Ma = new massage();
                Ma.Show();
                return;
            }
        }
        public async void SougouAPI()//POST一个多部分编码
        {
            HttpClient client = new HttpClient();
            client.MaxResponseContentBufferSize = 256000;
            client.DefaultRequestHeaders.Add("user-agent", "User-Agent    Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; Touch; MALNJS; rv:11.0) like Gecko");//设置请求头
            string url = "http://ocr.shouji.sogou.com/v2/ocr/json";
            HttpResponseMessage response;
            MultipartFormDataContent mulContent = new MultipartFormDataContent("----WebKitFormBoundaryrXRBKlhEeCbfHIY");//创建用于可传递文件的容器
            string path = "jietu.jpg";
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);  // 读文件流
            HttpContent fileContent = new StreamContent(fs);//为文件流提供的HTTP容器
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");//设置媒体类型
            mulContent.Add(fileContent, "pic", "pic.jpg");//这里第二个参数是表单名，第三个是文件名。如果接收的时候用表单名来获取文件，那第二个参数就是必要的了 
            try
            {
                response = await client.PostAsync(new Uri(url), mulContent);
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                JObject sougouJson = (JObject)JsonConvert.DeserializeObject(result);
                if (sougouJson.Count == 1)
                {
                    info = "当前选择没有文本";
                    massage Ms = new massage();
                    Ms.Show();
                    return;
                }
                for (int i = 0; i < sougouJson["result"].Count(); i++)
                {
                    OCRresult = OCRresult + sougouJson["result"][i]["content"].ToString(); //+ "\n";
                }

                Clipboard.SetText(OCRresult);
                info = "文字已复制到剪切板";
                massage Ma = new massage();
                Ma.Show();
            }
            catch (OverflowException)
            {
                info = "网络出错请重试";
                massage Ma = new massage();
                Ma.Show();
                return;
            }
        }
        private void OCR()
        {
            OCRresult = "";
            Bitmap CatchBmp = new Bitmap(Screen.AllScreens[0].Bounds.Width, Screen.AllScreens[0].Bounds.Height);
            Graphics g = Graphics.FromImage(CatchBmp);
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(Screen.AllScreens[0].Bounds.Width, Screen.AllScreens[0].Bounds.Height));
            Cutter cutter1 = new Cutter();
            cutter1.BackgroundImage = CatchBmp;
            cutter1.Show();
            if (cutter1.ShowDialog() == DialogResult.OK)
            {

                if (OCRchoose == "sougou")
                {
                    SougouAPI();
                    return;
                }
                else if (OCRchoose == "baidu")
                {
                    BaiduAPI();
                }
                else
                {
                    info = "没有选择合适的API接口";
                    massage Ma = new massage();
                    Ma.Show();
                }
                if (OCRresult == "")
                {
                    info = "当前选择没有文本";
                    massage Ma = new massage();
                    Ma.Show();
                }
                else
                {
                    Clipboard.SetText(OCRresult);
                    info = "文字已复制到剪切板";
                    massage Ma = new massage();
                    Ma.Show();
                }
            }
        }
        private void jietu()
        {
            Bitmap CatchBmp = new Bitmap(Screen.AllScreens[0].Bounds.Width, Screen.AllScreens[0].Bounds.Height);
            Graphics g = Graphics.FromImage(CatchBmp);
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(Screen.AllScreens[0].Bounds.Width, Screen.AllScreens[0].Bounds.Height));
            Cutter cutter1 = new Cutter();
            cutter1.BackgroundImage = CatchBmp;
            cutter1.Show();
            if (cutter1.ShowDialog() == DialogResult.OK)
            {
                info = "截图已复制到剪切板";
                massage Ma = new massage();
                Ma.Show();   
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.Visible)
            {
                this.Hide();
            }
            else
            {
                this.Show();
            }
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void 快捷键设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Setting s = new Setting();
            s.Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            this.Hide();
        }
    }
}
