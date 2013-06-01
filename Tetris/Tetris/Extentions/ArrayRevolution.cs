using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetris
{
    //もらってきた
    //http://gushwell.ifdef.jp/etude/ArrayRevolution.html
      static class RotateArray {
        // これが基準
          public static bool[,] Rotate90(this bool[,] array)
          {
              bool[,] newarray = new bool[array.GetLength(1), array.GetLength(0)];
              int i = 0;

              for (int x = 0; x < array.GetLength(1); ++x)
              {
                  for (int y = array.GetLength(0) - 1; y >= 0; --y)
                  {
                      newarray[i / array.GetLength(0), i % array.GetLength(0)] = array[y, x];
                      i++;
                  }

              }
              return newarray;
          }
    }


}
