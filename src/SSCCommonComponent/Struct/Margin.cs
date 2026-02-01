using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSC.CommonComponent.Struct
{
    /// <summary>
    /// 表示图形绘制区域的边距（单位：像素）
    /// </summary>
    public struct Margin
    {
        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }

        public Margin(int left, int right, int top, int bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public override string ToString()
        {
            return $"Left: {Left}, Right: {Right}, Top: {Top}, Bottom: {Bottom}";
        }

        /// <summary>
        /// 提供一个默认边距（左80，右40，上40，下60）
        /// </summary>
        public static Margin Default => new Margin(80, 40, 40, 60);
    }

}
