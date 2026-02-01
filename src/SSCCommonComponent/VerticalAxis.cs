using System;
using System.Drawing;

namespace SSC.CommonComponent
{
    public class VerticalAxis : Axis
    {
        // Public entry: draw Vertical axes
        public override void DrawAxis(Graphics g, Rectangle clientArea)
        {
            DrawVertical(g, clientArea);
        }

        // Derived classes can override to customize Vertical axis drawing
        protected override void DrawVertical(Graphics g, Rectangle clientArea)
        {
            var margin = this.Margin;
            int width = clientArea.Width - margin.Left - margin.Right;
            int height = clientArea.Height - margin.Top - margin.Bottom;
            Point origin = new Point(margin.Left, clientArea.Height - margin.Bottom);

            using var axisPen = new Pen(Color.Black, 2);
            using var minorTickPen = new Pen(Color.LightGray, 1);
            using Font labelFont = new Font("Arial", 10);
            Brush labelBrush = Brushes.Black;

            // Determine axis ranges
            float yMin = this.YMin ?? 0f;
            float yMax = this.YMax ?? 800f;
            float yRange = Math.Max(1e-6f, yMax - yMin);

            // Draw Y axis line and rotated label
            g.DrawLine(axisPen, origin.X, origin.Y, origin.X, origin.Y - height);
            g.TranslateTransform(origin.X - 50, origin.Y - height / 2 - 10);
            g.RotateTransform(90);
            g.DrawString(this.YLabel, labelFont, labelBrush, 0, 0);
            g.ResetTransform();

            int desiredYTicks = 8;
            float yStep = this.NiceNumber(yRange / desiredYTicks);
            float yStart = (float)Math.Ceiling(yMin / yStep) * yStep;

            for (float yv = yStart; yv <= yMax; yv += yStep)
            {
                float yf = origin.Y - (yv - yMin) / yRange * height;
                g.DrawLine(minorTickPen, origin.X - 6, yf, origin.X - 1, yf);
                string label = yv.ToString("0.##");
                g.DrawString(label, labelFont, labelBrush, origin.X - 50, yf - 8);
            }
        }
    }
}
