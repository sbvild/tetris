using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace Tetris
{
    class Painter
    {
        private PictureBox m_pi;

        public Painter(PictureBox pi)
        {
            //
            m_pi = pi;
        }

        public void Start()
        {
            //
            m_pi.Paint += Paint;
            this.isAlive = true;
            this.wthread = this.MainThread;
            this.ar = this.wthread.BeginInvoke(null, null);

        }

        public void UpdateCanvas(UpdateEventArgs eventargs)
        {
            this.backBuffer = eventargs.Image;

        }
        private void Paint(object sender, PaintEventArgs e)
        {
            lock (this)
            {
                if (this.backBuffer != null)
                {
                    e.Graphics.DrawImageUnscaled(this.backBuffer, 0, 0);
                }
            }
        }



        private bool isAlive;
        private Bitmap backBuffer;
        delegate void WorkerThread();
        private WorkerThread wthread;
        IAsyncResult ar;
        private const int WEIGHT = 10;

        private void MainThread()
        {
            while (this.isAlive)
            {
                lock (this)
                {
                    if (this.backBuffer == null)
                    {
                        this.backBuffer = new Bitmap(this.m_pi.Width, this.m_pi.Height);
                    }
                    //Graphics g = Graphics.FromImage(this.backBuffer);
                    //this.UpdatePanel(g, this.pictureBox1.Width, this.pictureBox1.Height);
                    //g.Dispose();
                }
                this.m_pi.Invalidate();
                Thread.Sleep(WEIGHT);
            }
        }

    }
}
