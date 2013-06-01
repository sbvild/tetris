using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace Tetris
{
    class Util
    {
        public static string[] Convert2StringArray(string[,] str)
        {
            string[] stringArray = new string[str.GetLength(0)];
            StringBuilder line = new StringBuilder();
            for (int y = 0; y < str.GetLength(0); ++y)
            {
                line.Clear();
                for (int x = 0; x < str.GetLength(1); ++x)
                {
                    line.Append(str[y, x]);
                }
                stringArray[y] = line.ToString();
            }

            return stringArray;
        }


        public static bool HitTest(bool[,] a, bool[,] b)
        {
            var re = false;

            for (var i = 0; i < a.GetLength(0); ++i)
            {
                for (var j = 0; j < a.GetLength(1); ++j)
                {
                    if (a[i, j] && b[i, j])
                        return true;
                }
            }

            return false;
        }


        public static Point RotatePointRight(Point pt, int offsetHeight)
        {

            //
            return new Point(-1*pt.Y + offsetHeight,  pt.X);

        }

        //TODO:
        //なおす
        public static Point RotatePointLeft(Point pt, int offsetHeight)
        {

            //
            return new Point(pt.Y, pt.X);

        }

        public static Point GetDistancePoint2Point(Point ptFrom, Point ptTo)
        {
            //
            return new Point((ptFrom.X - ptTo.X), (ptFrom.Y - ptTo.Y));
        }

        /// <summary>
        /// ビットマップ(Bitmap)を回転する
        /// </summary>
        /// <param name="bmp">ビットマップ</param>
        /// <param name="angle">回転角度</param>
        /// <param name="x">中心点Ｘ</param>
        /// <param name="y">中心点Ｙ</param>
        /// <returns></returns>
        public static Bitmap RotateBitmap(Bitmap bmp, float angle, int x, int y)
        {
            Bitmap bmp2 = new Bitmap((int)bmp.Width, (int)bmp.Height);
            Graphics g = Graphics.FromImage(bmp2);
            //g.Clear(Color.Black);

            g.TranslateTransform(-x, -y);
            g.RotateTransform(angle, System.Drawing.Drawing2D.MatrixOrder.Append);
            g.TranslateTransform(x, y, System.Drawing.Drawing2D.MatrixOrder.Append);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;

            g.DrawImageUnscaled(bmp, 0, 0);
            g.Dispose();

            return bmp2;
        }
    }
}
