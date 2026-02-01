using System;
using System.Drawing;

namespace SSC.CommonComponent
{
    public class VerticalTopAxis : VerticalAxis
    {
        // Public entry: draw Vertical axes
        public override void DrawAxis(Graphics g, Rectangle clientArea)
        {
            DrawVertical(g, clientArea);
        }

        // Draw an additional Vertical axis at the top of the plotting area
        protected override void DrawVertical(Graphics g, Rectangle clientArea)
        {
            // call base to draw the bottom axis as usual
            base.DrawVertical(g, clientArea);

            var margin = this.Margin;
            int width = clientArea.Width - margin.Left - margin.Right;
            int height = clientArea.Height - margin.Top - margin.Bottom;

            // top origin (x origin for horizontal top axis)
            float originX = margin.Left;
            float originY = margin.Top; // top inside plotting area

            using var axisPen = new Pen(Color.Black, 2);
            using var minorTickPen = new Pen(Color.LightGray, 1);
            using Font labelFont = new Font("Arial", 10);
            Brush labelBrush = Brushes.Black;

            // Determine axis ranges
            float xMin = XMin ?? 0f;
            float xMax = XMax ?? 200f;
            float xRange = Math.Max(1e-6f, xMax - xMin);

            // Draw top X axis line and label (label placed above the axis)
            g.DrawLine(axisPen, originX, originY, originX + width, originY);
            g.DrawString(XLabel, labelFont, labelBrush, originX + width / 2 - 30, originY - 30);

            int desiredXTicks = 10;
            float xStep = NiceNumber(xRange / desiredXTicks);
            float xStart = (float)Math.Ceiling(xMin / xStep) * xStep;

            for (float xv = xStart; xv <= xMax; xv += xStep)
            {
                float xf = originX + (xv - xMin) / xRange * width;
                // draw tick pointing upward from the axis line
                g.DrawLine(minorTickPen, xf, originY - 1, xf, originY - 6);
                string label = xv.ToString("0.##");
                // draw label above the tick
                g.DrawString(label, labelFont, labelBrush, xf - 10, originY - 26);
            }
        }
    }
}
