using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ScreenShot_1
{
    public partial class Cutter : Form
    {
        private Timer Timer = null;
        public Cutter()
        {
            InitializeComponent();
            Timer = new Timer() { Interval = 5 };
            Timer.Tick += new EventHandler(Timer_Tick);
            base.Opacity = 0;
            Timer.Start();
        }
        #region 定义程序变量
        // 定义变量

        // 用来记录鼠标按下的坐标，用来确定绘图起点
        private Point DownPoint;


        // 用来表示截图开始
        private bool CatchStart = false;

        // 用来保存原始图像
        private Bitmap originBmp;
        private Bitmap originUnChange;

        // 用来保存截图的矩形
        private Rectangle CatchRectangle;
        #endregion
        private void Cutter_Load(object sender, EventArgs e)
        {
            this.Visible = false;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.UpdateStyles();
            // 改变鼠标样式
            this.Cursor = Cursors.Cross;
            // 保存全屏图片
            originBmp = new Bitmap(this.BackgroundImage);
            originUnChange = originBmp;
        }

        private void Cutter_MouseDown(object sender, MouseEventArgs e)
        {
            // 鼠标左键按下是开始画图，也就是截图
            if (e.Button == MouseButtons.Left)
            {
                // 如果捕捉没有开始
                if (!CatchStart)
                {
                    CatchStart = true;
                    // 保存此时鼠标按下坐标
                    DownPoint = new Point(e.X, e.Y);
                }
            }
        }

        private void Cutter_MouseMove(object sender, MouseEventArgs e)
        {
            // 确保截图开始
            if (CatchStart)
            {
                // 新建一个图片对象，让它与屏幕图片相同
                Bitmap copyBmp = (Bitmap)originBmp.Clone();

                // 获取鼠标按下的坐标
                Point newPoint = new Point(DownPoint.X, DownPoint.Y);
                // 新建画板和画笔
                Graphics g = Graphics.FromImage(copyBmp);
                // 获取矩形的长宽
                int width = Math.Abs(e.X - DownPoint.X);
                int height = Math.Abs(e.Y - DownPoint.Y);
                if (e.X < DownPoint.X)
                {
                    newPoint.X = e.X;
                }
                if (e.Y < DownPoint.Y)
                {
                    newPoint.Y = e.Y;
                }
                CatchRectangle = new Rectangle(newPoint, new Size(width, height));
                // 将矩形画在画板上
                Brush b = new SolidBrush(Color.FromArgb(100, Color.Green));
                Region fillRegion = new Region(CatchRectangle);
                g.FillRegion(b, fillRegion);

                // 释放目前的画板
                g.Dispose();
                b.Dispose();
                // 从当前窗体创建新的画板
                Graphics g1 = this.CreateGraphics();
                // 将刚才所画的图片画到截图窗体上
                // 为什么不直接在当前窗体画图呢？
                // 如果自己解决将矩形画在窗体上，会造成图片抖动并且有无数个矩形
                // 这样实现也属于二次缓冲技术
                g1.DrawImage(copyBmp, new Point(0, 0));
                g1.Dispose();
                // 释放拷贝图片，防止内存被大量消耗
                copyBmp.Dispose();
            }
        }

        private void Cutter_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // 如果截图已经开始，鼠标左键弹起设置截图完成
                if (CatchStart)
                {
                    CatchStart = false;
                }
                if (CatchRectangle.Width < 100 || CatchRectangle.Height < 50 || CatchRectangle.Width > 900 || CatchRectangle.Height > 600)
                {
                    Form1.info = "截图太小请重新选择";
                    massage Ma = new massage();
                    Ma.Show();
                    this.BackgroundImage = originUnChange;
                    return;
                }
                Bitmap CatchedBmp = new Bitmap(CatchRectangle.Width, CatchRectangle.Height);
                Graphics g = Graphics.FromImage(CatchedBmp);
                g.DrawImage(originBmp, new Rectangle(0, 0, CatchRectangle.Width, CatchRectangle.Height), CatchRectangle, GraphicsUnit.Pixel);
                CatchedBmp.Save("jietu.jpg");
                g.Dispose();
                this.BackgroundImage = originBmp;
                CatchedBmp.Dispose();
                this.DialogResult = DialogResult.OK;
                this.Cursor = Cursors.Default;
                this.Close();
            }
        }

        private void Cutter_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (this.Opacity >= 1)
            {
                Timer.Stop();
            }
            else
            {
                base.Opacity += 0.2;
            }
        }
    }
}
