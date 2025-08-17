using System;
using System.Drawing;
using SSCCommonComponent.Struct;

namespace SSCCommonComponent
{
    public class Axis
    {
        public Margin Margin { get; set; } = new Margin(80, 40, 40, 60);
        public void DrawAxis(Graphics g, Rectangle clientArea)
        {
            int width = clientArea.Width - Margin.Left - Margin.Right;
            int height = clientArea.Height - Margin.Top - Margin.Bottom;

            Point origin = new Point(Margin.Left, clientArea.Height - Margin.Bottom);

            Pen axisPen = new Pen(Color.Black, 2);
            Pen majorTickPen = Pens.Gray;
            Pen minorTickPen = new Pen(Color.LightGray, 1);

            Font labelFont = new Font("Arial", 10);
            Brush labelBrush = Brushes.Black;

            // X 轴
            g.DrawLine(axisPen, origin.X, origin.Y, origin.X + width, origin.Y);
            g.DrawString("Time (min)", labelFont, labelBrush, origin.X + width / 2 - 30, origin.Y + 30);

            // X 轴主刻度（每 100）
            int xMax = 200;
            int xMajorStep = 20;
            int xMinorStep = 4;

            for (int xVal = 0; xVal <= xMax; xVal += xMinorStep)
            {
                int x = origin.X + (int)(xVal * width / (float)xMax);

                if (xVal % xMajorStep == 0)
                {
                    // 主刻度
                    g.DrawLine(majorTickPen, x, origin.Y + 1, x, origin.Y + 10);
                    string label = (xVal * 0.1).ToString("0.0");
                    g.DrawString(label, labelFont, labelBrush, x - 10, origin.Y + 10);
                }
                else
                {
                    // 辅助刻度
                    g.DrawLine(minorTickPen, x, origin.Y + 1, x, origin.Y + 6);
                }
            }

            // Y 轴
            g.DrawLine(axisPen, origin.X, origin.Y, origin.X, origin.Y - height);
            g.TranslateTransform(origin.X - 50, origin.Y - height / 2 - 10);       // 设置旋转中心
            g.RotateTransform(90);           // 逆时针旋转90度
            g.DrawString("Intensity", labelFont, labelBrush, 0, 0);
            g.ResetTransform();               // 恢复原始坐标系

            // Y 轴主刻度（每 100）
            int yMax = 800;
            int yMajorStep = 100;
            int yMinorStep = 20;

            for (int yVal = 0; yVal <= yMax; yVal += yMinorStep)
            {
                int y = origin.Y - (int)(yVal * height / (float)yMax);

                if (yVal % yMajorStep == 0)
                {
                    // 主刻度
                    g.DrawLine(majorTickPen, origin.X - 10, y, origin.X - 1, y);
                    g.DrawString(yVal.ToString(), labelFont, labelBrush, origin.X - 50, y - 8);
                }
                else
                {
                    // 辅助刻度
                    g.DrawLine(minorTickPen, origin.X - 6, y, origin.X - 1, y);
                }
            }
        }
    }
}
