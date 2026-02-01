using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSC.CommonComponent
{
    public class HorizontalAxis : Axis
    {
        // Public entry: draw Horizontal axes
        public override void DrawAxis(Graphics g, Rectangle clientArea)
        {
            DrawHorizontal(g, clientArea);
        }

        // Derived classes can override to customize horizontal axis drawing
        protected override void DrawHorizontal(Graphics g, Rectangle clientArea)
        {
            var margin = Margin;
            int width = clientArea.Width - margin.Left - margin.Right;
            int height = clientArea.Height - margin.Top - margin.Bottom;
            Point origin = new Point(margin.Left, clientArea.Height - margin.Bottom);

            using var axisPen = new Pen(Color.Black, 2);
            using var minorTickPen = new Pen(Color.LightGray, 1);
            using Font labelFont = new Font("Arial", 10);
            Brush labelBrush = Brushes.Black;

            // Determine axis ranges
            float xMin = XMin ?? 0f;
            float xMax = XMax ?? 200f;
            float xRange = Math.Max(1e-6f, xMax - xMin);

            // Draw X axis line and label
            g.DrawLine(axisPen, origin.X, origin.Y, origin.X + width, origin.Y);
            g.DrawString(XLabel, labelFont, labelBrush, origin.X + width / 2 - 30, origin.Y + 30);

            int desiredXTicks = 10;
            float xStep = NiceNumber(xRange / desiredXTicks);
            float xStart = (float)Math.Ceiling(xMin / xStep) * xStep;

            for (float xv = xStart; xv <= xMax; xv += xStep)
            {
                float xf = origin.X + (xv - xMin) / xRange * width;
                g.DrawLine(minorTickPen, xf, origin.Y + 1, xf, origin.Y + 6);
                string label = xv.ToString("0.##");
                g.DrawString(label, labelFont, labelBrush, xf - 10, origin.Y + 10);
            }
        }
    }
}
