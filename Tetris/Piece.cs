using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Tetris
{
    public class Piece : ICloneable, IDrawableImage
    {
        public static readonly int BLOCK_WIDTH = 10;

        //列行
        private bool[,] m_data;
        private TransFormation m_trans;
        private readonly Color m_color;
        private readonly PieceType m_type;
        
        public int Width{
            get { return m_data.GetLength(1); }
            //get
            //{
            //    var max = 0;
            //    for (var j = 0; j < m_data.GetLength(0); ++j)
            //    {
            //        var length = 0;
            //        for (var i = 0; i < m_data.GetLength(1); ++i)
            //        {
            //             //
            //            if (m_data[j, i])
            //                length++;
            //        }
            //        if (length > max)
            //            max = length;
            //    }
            //    return max;
            //}
        }

        public int Height
        {
            get { return m_data.GetLength(0); }
            //get
            //{
            //    var max = 0;
            //    for (var i = 0; i < m_data.GetLength(1); ++i)
            //    {
            //        var length = 0;
            //        for (var j = 0; j < m_data.GetLength(0); ++j)
            //        {
            //             //
            //            if (m_data[j, i])
            //                length++;
            //        }
            //        if (length > max)
            //            max = length;
            //    }
            //    return max;
            //}
        }

        public int X
        {
            get { return m_trans.LeftTop.X; }
        }

        public int Y
        {
            get { return m_trans.LeftTop.Y; }
        }

        public bool this[int y, int x]{
            get { return m_data[y, x]; }
        }

        public int Rotation
        {
            get
            {
                return m_trans.Rotation;
            }
        }

        public Point Center
        {
            get
            {
                return m_trans.Center;
            }
        }

        public PieceType Type
        {
            get { return m_type; }
        }

        public bool IsHide
        {
            get
            {
                //
                return Height == 0 || Width == 0 ? true : false;
            }
        }

        public Piece(bool[,] _data, PieceType _type, TransFormation _trans, Color _color)
        {
            m_data = _data;
            m_type = _type;
            m_trans = _trans;
            m_color = _color;
        }

        public void SetPosition(int x, int y)
        {
            m_trans.LeftTop = new Point(x, y);
           // m_trans.LeftTop.X = x;
           // m_trans.LeftTop.Y = y;
        }

        public string ToString()
        {
            //
            var s = new System.Text.StringBuilder();
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    //
                    s.Append(this[i, j] ? "■" : "　");
                }
                s.AppendLine();
            }
            return s.ToString();
        }

        public Bitmap ToImage()
        {
            //
            if (IsHide)
            {
                return new Bitmap(1, 1);
            }


            var im = new Bitmap(this.Width * BLOCK_WIDTH, this.Height * BLOCK_WIDTH);
            var g = Graphics.FromImage(im);
            for (var i = 0; i < this.Height; i++)
            {
                for (var j = 0; j < this.Width; j++)
                {
                    //
                    if (!this[i, j])
                        continue;
                    g.FillRectangle(new SolidBrush(this.m_color), new Rectangle(BLOCK_WIDTH * j,  BLOCK_WIDTH * i, BLOCK_WIDTH, BLOCK_WIDTH));

                }
            }
            g.Dispose();
            return im;
        }

        //TODO : 
        //animateするならblock単位に動かす必要あり
        public void Move(Direction direction, Stage parent, bool forceF = false)
        {
            if (!forceF && !CanMove(direction, parent))
                return;

            Point movedP = m_trans.LeftTop;
            switch (direction)
            {
                case Direction.Bottom:
                    Update();
                    ++movedP.Y;
                    break;

                case Direction.Left:
                    --movedP.X;
                    break;

                case Direction.Right:
                    ++movedP.X;
                    break;

                case Direction.Up:
                    --movedP.Y;
                    break;
            }


            m_trans.LeftTop = movedP;

        }

        public bool CanMove(Direction direction, Stage parent)
        {
            //
            Piece copyedActivePiece = (Piece)this.Clone();
            copyedActivePiece.Move(direction, parent, forceF:true);

            //壁と衝突？
            if (copyedActivePiece.X < 0)
                return false;
            //１２３４５６７８９１０１
            //□□□□□□□□□■□
            //□□□□□□□□■■■
            if (copyedActivePiece.X + copyedActivePiece.Width > parent.Width)
                return false;

            if (copyedActivePiece.Y < 0)
                return false;
            if (copyedActivePiece.Y + copyedActivePiece.Height > parent.Height)
                return false;

            //他のピースと衝突？

            bool[,] lockedLayer = new bool[parent.Height, parent.Width];
            bool[,] activeLayer = new bool[parent.Height, parent.Width];
            Piece p;

            for (int y = 0; y < parent.Width; ++y)
            {
                for (int x = 0; x < parent.Width; ++x)
                {
                    lockedLayer[y, x] = false;
                    activeLayer[y, x] = false;
                }
            }
            for (int i = 0; i < parent.Pieces.Count ; ++i)
            {
                p = parent.Pieces[i];

                if (p.X == this.X && p.Y == this.Y)
                    continue;

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

        public void Rotate(Direction direction)
        {
            //
            //this.m_data.

            Point rotatedPoint;
            Point offsetPoint;

            switch (direction)
            {

                case Direction.Right:

                    //重心の移動
                     rotatedPoint = Util.RotatePointRight(this.m_trans.Center, this.Height-1);
                     offsetPoint = Util.GetDistancePoint2Point(this.m_trans.Center, rotatedPoint);
                    this.m_trans.Center = rotatedPoint;
                    this.m_trans.LeftTop = new Point(this.m_trans.LeftTop.X + offsetPoint.X, this.m_trans.LeftTop.Y + offsetPoint.Y);

                    m_data = m_data.Rotate90();
                    m_trans.Rotation += 90;
                    if (m_trans.Rotation >= 360)
                        m_trans.Rotation -= 360;

                    break;

                case Direction.Left:
                    //重心の移動
                     rotatedPoint = Util.RotatePointLeft(this.m_trans.Center, this.Height - 1);
                     offsetPoint = Util.GetDistancePoint2Point(this.m_trans.Center, rotatedPoint);
                    this.m_trans.Center = rotatedPoint;
                    this.m_trans.LeftTop = new Point(this.m_trans.LeftTop.X + offsetPoint.X, this.m_trans.LeftTop.Y + offsetPoint.Y);

                    m_data = m_data.Rotate90().Rotate90().Rotate90();
                    m_trans.Rotation += 270;
                    if (m_trans.Rotation >= 360)
                        m_trans.Rotation -= 360;
                    break;
            }
        }

        public void EraseLines(int lineIndex)
        {
            //
            if (!(this.Y <= lineIndex && lineIndex <= this.Y + this.Height - 1))
                return;

            //for (var i = 0; i < Height; i++)
            //{
            for (var j = 0; j < Width; j++)
            {
                //
                m_data[lineIndex - Y, j] = false;
            }
            //    }

            //}
        }

        //x?
        public void Update()
        {
            //
            if (!HasBlankedLines())
                return;

            var lineIndex = GetBlankedLinesIndex();
            bool[,] newBitArray= new bool[Height - lineIndex.Count, Width];
            var offset = 0;

            for (var i = 0; i < Height; i++)
            {

                if (lineIndex.IndexOf(i) >= 0)
                {
                    offset++;
                    continue;
                }

                for (var j = 0; j < Width; j++)
                {
                    //
                    newBitArray[i - offset, j] = this[i, j];
                }
            }

            m_data = newBitArray;

        }

        public override int GetHashCode()
        {
            //return base.GetHashCode();
            return this.X * 10 + this.Y;
        }

        public bool HasBlankedLines()
        {
            //
            var indexex = GetBlankedLinesIndex();
            if (indexex.Count > 0)
                return true;

            return false;

        }

        
        private IList<int> GetBlankedLinesIndex()
        {
            //
            var list = new List<int>();
            for (var i = 0; i < Height; i++)
            {
                var blankedF = false;
                for (var j = 0; j < Width; j++)
                {
                    //
                    if (j == 0)
                        blankedF = true;

                    blankedF = blankedF && !this[i, j];
                }
                if (blankedF)
                    list.Add(i);
            }

            return list;
        }

        public object Clone()
        {
            Piece p;
            p = new Piece(this.m_data, this.m_type, new TransFormation(this.m_trans), this.m_color);
            return p;


            switch (Type)
            {
                case PieceType.P1:
                    p = Piece.P1;
                    break;

                default:
                    p = Piece.P1;
                    break;

            }
            p.SetPosition(X, Y);

            switch (m_trans.Rotation)
            {
                case 90:
                    p.Rotate(Direction.Right);
                    break;

                case 180:
                    p.Rotate(Direction.Right);
                    p.Rotate(Direction.Right);
                    break;

                case 270:
                    p.Rotate(Direction.Left);
                    break;

            }

            return p;
        }

        //□■□
        //■■■
        //1,-1 -> r -> -1+1,-1 -> 0,-1
        // +1 dx

        //1, 1 => R => -1+1, 1 => 0,1
        public  static   Piece P1
        {
            get{
                var t = new TransFormation( new Point(-3,-3), new Point(1,1) , 0);
            bool[,] data = new bool[,] { { false, true, false }, { true, true, true } };
            return new Piece(data, PieceType.P1, t, Color.YellowGreen);
            }
        }
        public static Piece P2
        {
            get
            {
                //

                var t = new TransFormation(new Point(-3, -3), new Point(0, 2), 0);

                bool[,] data = new bool[,] { { true }, { true }, { true }, { true } };
                return new Piece(data, PieceType.P2, t, Color.Purple);

            }
        }
        

    }

     class PieceFactory
    {
        //
        private static List<Piece> m_Stocked = new List<Piece>();
        private static int m_index = 0;
        private const int MAX_STOCKED_PIECES = 7;

        public static Piece Random()
        {
            //
            //if (m_Stocked == null)
            //    m_Stocked = new List<Piece>();

            //在庫がない
            if (!HasStocked())
                InitStocked();

            //順番にかえす
            return   m_Stocked[m_index++];

        }
        private static void InitStocked()
        {
            m_index = 0;

            //shuffle
            Piece[] pieces = new Piece[] { Piece.P1, Piece.P2, Piece.P1, Piece.P2, Piece.P1, Piece.P2, Piece.P1 };
            new Random().Shuffle(pieces);

            m_Stocked.Clear();
            m_Stocked.AddRange(pieces);

        }
        public static Piece Next()
        {
            //
            if (!HasStocked())
                InitStocked();

            return m_Stocked[m_index];
        }
        private static bool HasStocked()
        {
            //
            if (m_Stocked.Count == 0 || (m_index == MAX_STOCKED_PIECES))
                return false;

            return true;

        }
    }

    public enum PieceType
    {
        P1,
        P2
    }

    public interface IDrawableImage
    {
        Bitmap ToImage();
    }

    public class TransFormation
    {
        //
        private Point m_LeftTop;
        private Point m_Center;
        private int m_Rotation;


        public Point LeftTop
        {
            get
            {
                return m_LeftTop;
            }
            set
            {
                m_LeftTop = value;
            }
        }

        public Point Center
        {
            get
            {
                return m_Center;
            }
            set
            {
                m_Center = value;
            }
        }

        public int Rotation
        {
            get
            {
                return m_Rotation;
            }
            set
            {
                m_Rotation = value;
            }
        }


        public TransFormation(Point leftTop, Point center, int rotation)
        {
            //
            m_LeftTop = leftTop;
            m_Center = center;
            m_Rotation = rotation;

        }

        public TransFormation(TransFormation o)
        {
            //
            m_LeftTop = o.LeftTop;
            m_Center = o.Center;
            m_Rotation = o.Rotation;

        }


    }
}
