using System;
using System.Collections.Generic;
using System.Drawing;

namespace SSC.CommonComponent
{
    public class HorizontalRightAxis : HorizontalAxis
    {
        // Collection of right-side vertical axes to draw
        public List<VerticalAxis> RightAxes { get; } = new List<VerticalAxis>();

        // Spacing in pixels between successive right vertical axes
        public int RightAxisSpacing { get; set; } = 48;

        // Public entry: draw Horizontal axes
        public override void DrawAxis(Graphics g, Rectangle clientArea)
        {
            DrawHorizontal(g, clientArea);
        }

        // Draw an additional Horizontal axis at the right of the plotting area
        protected override void DrawHorizontal(Graphics g, Rectangle clientArea)
        {
            // Draw the normal horizontal axis (bottom X axis)
            base.DrawHorizontal(g, clientArea);

            if (RightAxes == null || RightAxes.Count == 0)
                return;

            var margin = this.Margin;
            int width = clientArea.Width - margin.Left - margin.Right;
            int height = clientArea.Height - margin.Top - margin.Bottom;
            float originY = clientArea.Height - margin.Bottom;

            using var axisPen = new Pen(Color.Black, 2);
            using var minorTickPen = new Pen(Color.LightGray, 1);
            using Font labelFont = new Font("Arial", 10);
            Brush labelBrush = Brushes.Black;

            for (int idx = 0; idx < RightAxes.Count; idx++)
            {
                var vaxis = RightAxes[idx] ?? new VerticalAxis();

                // x position for this right-side axis: start from client right edge minus margin.Right, then offset outward
                float x = clientArea.Right - margin.Right + idx * RightAxisSpacing;

                // Draw axis line (vertical)
                g.DrawLine(axisPen, x, originY, x, originY - height);

                // Determine y range for this axis (allow per-axis override)
                float yMin = vaxis.YMin ?? this.YMin ?? 0f;
                float yMax = vaxis.YMax ?? this.YMax ?? 800f;
                float yRange = Math.Max(1e-6f, yMax - yMin);

                int desiredYTicks = 8;
                float yStep = NiceNumber(yRange / desiredYTicks);
                float yStart = (float)Math.Ceiling(yMin / yStep) * yStep;

                // Draw ticks and labels; ticks point outward to the right
                for (float yv = yStart; yv <= yMax; yv += yStep)
                {
                    float yf = originY - (yv - yMin) / yRange * height;
                    g.DrawLine(minorTickPen, x + 1, yf, x + 6, yf);
                    string label = yv.ToString("0.##");
                    // draw label to the right of the tick
                    g.DrawString(label, labelFont, labelBrush, x + 8, yf - 8);
                }

                // Draw Y label rotated (right-side should rotate -90 degrees)
                g.TranslateTransform(x + 50, originY - height / 2 - 10);
                g.RotateTransform(-90);
                g.DrawString(vaxis.YLabel, labelFont, labelBrush, 0, 0);
                g.ResetTransform();
            }
        }
    }
}
