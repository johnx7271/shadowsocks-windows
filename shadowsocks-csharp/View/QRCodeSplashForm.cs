using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Diagnostics;


namespace Shadowsocks.View
{
	public class QRCodeSplashForm : PerPixelAlphaForm
    {
        public Rectangle TargetRect;

        public QRCodeSplashForm()
        {
            this.Load += QRCodeSplashForm_Load;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1, 1);
            this.ControlBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "QRCodeSplashForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
        }

        private Timer timer;
        private int flashStep;
        private static double FPS = 1.0 / 15 * 1000; // System.Windows.Forms.Timer resolution is 15ms
        private static double ANIMATION_TIME = 0.5;
        private static int ANIMATION_STEPS = (int)(ANIMATION_TIME * FPS);
        Stopwatch sw;
        int x;
        int y;
        int w;
        int h;
        Bitmap bitmap;
        Graphics g;
        Pen pen;
        SolidBrush brush;

        private void QRCodeSplashForm_Load(object sender, EventArgs e)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            flashStep = 0;
            x = 0;
            y = 0;
            w = Width;
            h = Height;
            sw = Stopwatch.StartNew();
            timer = new Timer();
			//if (this.DesignMode)
			//	timer.Enabled = false;
            timer.Interval = (int)(ANIMATION_TIME * 1000 / ANIMATION_STEPS);
            timer.Tick += timer_Tick;
            timer.Start();
            bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            g = Graphics.FromImage(bitmap);
            pen = new Pen(Color.Red, 3);
            brush = new SolidBrush(Color.FromArgb(30, Color.Red));
        }

        void timer_Tick(object sender, EventArgs e)
        {
            double percent = (double)sw.ElapsedMilliseconds / 1000.0 / (double)ANIMATION_TIME;
            if (percent < 1)
            {
                // ease out
                percent = 1 - Math.Pow((1 - percent), 4);
                x = (int)(TargetRect.X * percent);
                y = (int)(TargetRect.Y * percent);
                w = (int)(TargetRect.Width * percent + this.Size.Width * (1 - percent));
                h = (int)(TargetRect.Height * percent + this.Size.Height * (1 - percent));
                //codeRectView.Location = new Point(x, y);
                //codeRectView.Size = new Size(w, h);
                pen.Color = Color.FromArgb((int)(255 * percent), Color.Red);
                brush.Color = Color.FromArgb((int)(30 * percent), Color.Red);
                g.Clear(Color.Transparent);
                g.FillRectangle(brush, x, y, w, h);
                g.DrawRectangle(pen, x, y, w, h);
                SetBitmap(bitmap);
            }
            else
            {
                if (flashStep == 0)
                {
                    timer.Interval = 100;
                    g.Clear(Color.Transparent);
                    SetBitmap(bitmap);
                }
                else if (flashStep == 1)
                {
                    timer.Interval = 50;
                    g.FillRectangle(brush, x, y, w, h);
                    g.DrawRectangle(pen, x, y, w, h);
                    SetBitmap(bitmap);
                }
                else if (flashStep == 1)
                {
                    g.Clear(Color.Transparent);
                    SetBitmap(bitmap);
                }
                else if (flashStep == 2)
                {
                    g.FillRectangle(brush, x, y, w, h);
                    g.DrawRectangle(pen, x, y, w, h);
                    SetBitmap(bitmap);
                }
                else if (flashStep == 3)
                {
                    g.Clear(Color.Transparent);
                    SetBitmap(bitmap);
                }
                else if (flashStep == 4)
                {
                    g.FillRectangle(brush, x, y, w, h);
                    g.DrawRectangle(pen, x, y, w, h);
                    SetBitmap(bitmap);
                }
                else
                {
                    sw.Stop();
                    timer.Stop();
                    pen.Dispose();
                    brush.Dispose();
                    bitmap.Dispose();
                    this.Close();
                }
                flashStep++;
            }
        }
    }
}
