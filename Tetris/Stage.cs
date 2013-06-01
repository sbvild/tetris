using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Tetris
{

    public delegate void UpdateDelegate(UpdateEventArgs args);

    public class Stage : IStage, IDrawableImage
    {
        //フィールド
        private List<Piece> m_pieces;
        private bool[,] m_pieces_bitarray;
        private readonly int m_width;
        private readonly int m_height;

        public event UpdateDelegate UpdateEvent;

        //プロパティ
        public int Width
        {
            get { return m_width; }
        }
        public int Height
        {
            get { return m_height; }
        }
        public Piece ActivePiece
        {
            get
            {
                return m_pieces[m_pieces.Count - 1]; 
            }
        }
        public IList<Piece> Pieces
        {
            get
            {
                return m_pieces;
            }
        }

        //コンストラクタ
        public Stage(int width, int height)
        {
            m_width = width;
            m_height = height;
            m_pieces = new List<Piece>();
            m_pieces_bitarray = new bool[m_height, m_width];
        }

        //メソッド
        public void AddPiece()
        {
            var p = PieceFactory.Random();
            var mannaka = Math.Ceiling(m_width / 2.0) - Math.Ceiling(p.Width / 2.0);
            p.SetPosition((int)mannaka, 0);
            m_pieces.Add(p);
            Piece p2 = m_pieces[0];
            //p2.SetPosition(2, 2);
            //m_pieces[0].TEST = 20;
            Update();
        }

        public void Update(bool animateF = false)
        {


                //test
                bool[,] canvas = new bool[m_height, m_width];
                Piece p;

                for (int y = 0; y < m_height; ++y)
                {
                    for (int x = 0; x < m_width; ++x)
                    {
                        canvas[y, x] = false;
                    }
                }
                for (int i = 0; i < m_pieces.Count; ++i)
                {
                    p = m_pieces[i];

                    for (int y = p.Y; y < p.Y + p.Height; ++y)
                    {
                        for (int x = p.X; x < p.X + p.Width; ++x)
                        {
                            canvas[y, x] = canvas[y, x] || p[y - p.Y, x - p.X];
                        }
                    }
                }

                m_pieces_bitarray = canvas;

            if(animateF)
                //this.Animate();
                UpdateEvent(new UpdateEventArgs(this.ToImage()));
            else
                UpdateEvent(new UpdateEventArgs(this.ToImage()));

        }

        public Bitmap ToImage()
        {
            //
            var im = new Bitmap(this.Width * Piece.BLOCK_WIDTH, this.Height * Piece.BLOCK_WIDTH);
            var g = Graphics.FromImage(im);

            g.Clear(Color.White);

            for (var i = 0; i < this.Height; i++)
            {
                for (var j = 0; j < this.Width; j++)
                {
                    g.DrawRectangle(new Pen(Color.Silver), new Rectangle(j * Piece.BLOCK_WIDTH, i * Piece.BLOCK_WIDTH, Piece.BLOCK_WIDTH, Piece.BLOCK_WIDTH));
                }
            }

            foreach (var p in m_pieces)
            {
                //
                g.DrawImage(p.ToImage(), p.X * Piece.BLOCK_WIDTH, p.Y * Piece.BLOCK_WIDTH);

            }
            g.Dispose();

            return im;
        }


        public void Animate()
        {
            //
            var stage = new Bitmap(this.Width * Piece.BLOCK_WIDTH, this.Height * Piece.BLOCK_WIDTH);
            var g = Graphics.FromImage(stage);

            g.Clear(Color.White);

            for (var i = 0; i < this.Height; i++)
            {
                for (var j = 0; j < this.Width; j++)
                {
                    g.DrawRectangle(new Pen(Color.Silver), new Rectangle(j * Piece.BLOCK_WIDTH, i * Piece.BLOCK_WIDTH, Piece.BLOCK_WIDTH, Piece.BLOCK_WIDTH));
                }
            }

            foreach (var p1 in m_pieces)
            {
                //

                if (p1.GetHashCode() == ActivePiece.GetHashCode())
                {
                    continue;
                }

                g.DrawImage(p1.ToImage(), p1.X * Piece.BLOCK_WIDTH, p1.Y * Piece.BLOCK_WIDTH);

            }

            var ostage = new Bitmap(stage);

            var p = ActivePiece;

            var angle2 = p.Rotation;
            if (angle2 == 0)
                angle2 = 360;

            //p.Rotate(Direction.Left);
            p.Rotate(Direction.Right);
            p.Rotate(Direction.Right);
            p.Rotate(Direction.Right);

            var angle1 = p.Rotation;

            var img = p.ToImage();
            var dx = p.X * Piece.BLOCK_WIDTH + p.Center.X * Piece.BLOCK_WIDTH + Piece.BLOCK_WIDTH / 2;
            var dy = p.Y * Piece.BLOCK_WIDTH + p.Center.Y * Piece.BLOCK_WIDTH + Piece.BLOCK_WIDTH / 2;

            for (var i = 0; i <= angle2 - angle1; i++)
            {
                stage = new Bitmap(ostage);
                var g2 = Graphics.FromImage(stage);

                g2.ResetTransform();
                g2.TranslateTransform(-dx, -dy);
                g2.RotateTransform(i, System.Drawing.Drawing2D.MatrixOrder.Append);
                g2.TranslateTransform(dx, dy, System.Drawing.Drawing2D.MatrixOrder.Append);

                g2.DrawImage(img, p.X * Piece.BLOCK_WIDTH, p.Y * Piece.BLOCK_WIDTH);
                UpdateEvent(new UpdateEventArgs(stage));
                //System.Threading.Thread.Sleep(1);
            }
            p.Rotate(Direction.Right);
            Update();


            g.Dispose();


        }

        public void AddUpdateListener(UpdateDelegate updateMethod)
        {
            //
            UpdateEvent += updateMethod;
        }


        //TODO:
        //アクティブのテトリスのクラスと
        //それ以外を別々にする
        //ActivePiece.Move
        //NotActivePieces.Move
        public void MovePiece(Direction direction)
        {
            //bottom
            //if(CanMovePiece(direction))
                ActivePiece.Move(direction, this);
            Update();
        }

        public void RotatePiece(Direction direction)
        {
            //
            if (CanRotatePiece(direction))
                ActivePiece.Rotate(direction);
            Update(true);
        }

        public void HardDropAll()
        {
            //
            while (this.HasBlankedLines())
            {

                foreach (var p in m_pieces)
                {
                    //
                    p.Move(Direction.Bottom, this);
                }
                Update();
            }
            //m_status = StageStatus.Progressing;
            Update();

        }
        

        public bool CanRotatePiece(Direction direction)
        {
            //□■□ 0 {0,1,0}
            //■■■ 1 {1,1,1}

            //↓
            //　■□
            //　■■
            //　■□
            //return true;

            Piece copyedActivePiece = (Piece)ActivePiece.Clone();
            copyedActivePiece.Rotate(direction);

            //壁と衝突？
            if (copyedActivePiece.X < 0)
                return false;
            //１２３４５６７８９１０１
            //□□□□□□□□□■□
            //□□□□□□□□■■■
            if (copyedActivePiece.X + copyedActivePiece.Width > this.m_width)
                return false;

            if (copyedActivePiece.Y < 0)
                return false;
            if (copyedActivePiece.Y + copyedActivePiece.Height > this.m_height)
                return false;

            //他のピースと衝突？

            bool[,] lockedLayer = new bool[m_height, m_width];
            bool[,] activeLayer = new bool[m_height, m_width];
            Piece p;

            for (int y = 0; y < m_height; ++y)
            {
                for (int x = 0; x < m_width; ++x)
                {
                    lockedLayer[y, x] = false;
                    activeLayer[y, x] = false;
                }
            }
            for (int i = 0; i < m_pieces.Count - 1; ++i)
            {
                p = m_pieces[i];

                for (int y = p.Y; y < p.Y + p.Height; ++y)
                {
                    for (int x = p.X; x < p.X + p.Width; ++x)
                    {
                        lockedLayer[y, x] = p[y - p.Y, x - p.X];
                    }
                }

            }

            p = copyedActivePiece;
            for (int y = p.Y; y < p.Y + p.Height; ++y)
            {
                for (int x = p.X; x < p.X + p.Width; ++x)
                {
                    activeLayer[y, x] = p[y - p.Y, x - p.X];
                }
            }

            return !Util.HitTest(lockedLayer, activeLayer);
        }

        public bool HasMovablePiece()
        {
            //

                if (m_pieces.Count < 1)
                    return false;

                //if (m_status == StageStatus.Paused)
                //    return false;

                //if (CanMovePiece(Direction.Bottom))
                if (ActivePiece.CanMove(Direction.Bottom, this))
                    return true;

                return false;
            
        }

        public bool HasLinkedLines()
        {
            //
            //for (var i = 0; i < m_height; i++)
            //{
            //    var linkedF = false;
            //    for (var j = 0; j < m_width; j++)
            //    {
            //        //
            //        linkedF = linkedF && m_canvas[i, j];
            //    }
            //}
            if (GetLinkedLinesIndex().Count > 0)
                return true;

            return false;
        }

        public bool HasBlankedLines()
        {
            //
            var trueBlankedLines = new List<int>();

            var blankedLines = GetBlankedLinesIndex();
            var nblankedLines = GetNotBlankedLinesIndex();

            foreach (var bline in blankedLines)
            {
                //
                foreach (var nbline in nblankedLines)
                {
                    //
                    if (bline > nbline)
                    {
                        trueBlankedLines.Add(bline); goto label_next;
                    }
                }
            label_next:
                ;//dummy
 
            }

            if (trueBlankedLines.Count > 0)
                return true;

            return false;
        }

        public void EraseLines()
        {
            //
            var lines = GetLinkedLinesIndex();

            foreach (var i in lines)
            {

                foreach (var p in m_pieces)
                {
                    //
                    p.EraseLines(i);
                }
            }

                //m_status = StageStatus.Paused;
                Update();
        }

        private IList<int> GetLinkedLinesIndex()
        {
            //
           var list = new List<int>();
            for (var i = 0; i < m_height; i++)
            {
                var linkedF = false;
                for (var j = 0; j < m_width; j++)
                {
                    //
                    if (j == 0)
                        linkedF = true;

                    linkedF = linkedF && m_pieces_bitarray[i, j];
                }
                if (linkedF)
                    list.Add(i);
            }

            return list;
        }

        private IList<int> GetBlankedLinesIndex()
        {
            //
            var list = new List<int>();
            for (var i = 0; i < m_height; i++)
            {
                var blankedF = false;
                for (var j = 0; j < m_width; j++)
                {
                    //
                    if (j == 0)
                        blankedF = true;

                    blankedF = blankedF && !m_pieces_bitarray[i, j];
                }
                if (blankedF)
                    list.Add(i);
            }

            return list;
        }

        private IList<int> GetNotBlankedLinesIndex()
        {
            //
            var list = new List<int>();
            for (var i = 0; i < m_height; i++)
            {
                for (var j = 0; j < m_width; j++)
                {
                    //
                    if (m_pieces_bitarray[i, j])
                    {
                        list.Add(i);
                        j = m_width;
                    }
                }
            
            }

            return list;
        }


    }

    public enum Direction
    {
        Bottom,
        Left,
        Right,
        Up
    }
    
    public interface IStage
    {
        //
        int Width { get; }
        int Height { get; }

        IList<Piece> Pieces { get; }

        void AddUpdateListener(UpdateDelegate updateMethod);

    }

    public class UpdateEventArgs : EventArgs
    {
        private Bitmap m_image;

        public Bitmap Image
        {
            get
            {
                return m_image;
            }
        }

        public UpdateEventArgs(Bitmap image)
        {
            m_image = image;
        }

    }

}
