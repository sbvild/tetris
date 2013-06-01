using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using System.Threading;
namespace Tetris
{
    public partial class TetrisForm : Form
    {
        private Tetris m_tetris;    //テトリス + 描画データ生成
        private Painter m_painter;  //描画データを受け取って描画
        KeyStateBackgroundWatcher o;    //←↑→Sキーの監視
        KeyStateBackgroundWatcher o2;   //SPACEキーの監視

        public TetrisForm()
        {
            InitializeComponent();

        }

        //描画データに変更があったとき
        private void OnUpdateCanvas(UpdateEventArgs eventargs)
        {

            m_painter.UpdateCanvas(eventargs);

        }

        //テトリスのステータスが変更したとき
        private void OnUpdateStatus(TetrisEventArgs eventargs)
        {
            //

        }

        //キーが押されたとき
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    m_tetris.MovePiece(Direction.Bottom);
                    break;

                case Keys.Left:
                    m_tetris.MovePiece(Direction.Left);
                    break;

                case Keys.Right:
                    m_tetris.MovePiece(Direction.Right);
                    break;

                case Keys.Space:
                    m_tetris.RotatePiece(Direction.Right);
                    break;

                case Keys.S:
                    m_tetris.Start();
                    break;


            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.Name = "FormThread";

            this.pictureBox1.Width = Piece.BLOCK_WIDTH * 11;
            this.pictureBox1.Height = Piece.BLOCK_WIDTH * 20;
            this.ActiveControl = this.textBox2;

            m_painter = new Painter(this.pictureBox1);
            m_painter.Start();
            m_tetris = new Tetris();
            m_tetris.Stage.AddUpdateListener(OnUpdateCanvas);
            m_tetris.Updated += new TetrisUpdateDelegate(OnUpdateStatus);

            o = new KeyStateBackgroundWatcher(50, OnKeyDown, Keys.Down, Keys.Left, Keys.Right, Keys.S);
            o2 = new KeyStateBackgroundWatcher(150, OnKeyDown, Keys.Space);
            o.Watch();
            o2.Watch();
        }
    }
}
