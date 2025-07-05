using System;
using System.Drawing;

namespace SSCCommonComponent
{
    public class Axis
    {
        public void DrawAxes(Graphics g, Rectangle clientArea)
        {
            int marginLeft = 80;
            int marginBottom = 60;
            int marginTop = 40;
            int marginRight = 40;

            int width = clientArea.Width - marginLeft - marginRight;
            int height = clientArea.Height - marginTop - marginBottom;

            Point origin = new Point(marginLeft, clientArea.Height - marginBottom);

            Pen axisPen = new Pen(Color.Black, 2);
            Pen majorTickPen = Pens.Gray;
            Pen minorTickPen = new Pen(Color.LightGray, 1);

            Font labelFont = new Font("Arial", 10);
            Brush labelBrush = Brushes.Black;

            // X 轴
            g.DrawLine(axisPen, origin.X, origin.Y, origin.X + width, origin.Y);
            g.DrawString("Time (min)", labelFont, labelBrush, origin.X + width / 2 - 30, origin.Y + 30);

            // Y 轴
            g.DrawLine(axisPen, origin.X, origin.Y, origin.X, origin.Y - height);
            g.DrawString("Intensity", labelFont, labelBrush, origin.X - 60, origin.Y - height / 2 - 10);

            // X 轴主刻度
            int xTicks = 10;
            for (int i = 0; i <= xTicks; i++)
            {
                int x = origin.X + i * width / xTicks;
                g.DrawLine(majorTickPen, x, origin.Y - 5, x, origin.Y + 5);
                string label = (i * 0.5).ToString("0.0");
                g.DrawString(label, labelFont, labelBrush, x - 10, origin.Y + 10);
            }

            // Y 轴主刻度（每 100）
            int yMax = 800;
            int yMajorStep = 100;
            int yMinorStep = 5;

            for (int yVal = 0; yVal <= yMax; yVal += yMinorStep)
            {
                int y = origin.Y - (int)(yVal * height / (float)yMax);

                if (yVal % yMajorStep == 0)
                {
                    // 主刻度
                    g.DrawLine(majorTickPen, origin.X - 5, y, origin.X + 5, y);
                    g.DrawString(yVal.ToString(), labelFont, labelBrush, origin.X - 40, y - 8);
                }
                else
                {
                    // 辅助刻度
                    g.DrawLine(minorTickPen, origin.X - 3, y, origin.X + 3, y);
                }
            }
        }
    }
}
