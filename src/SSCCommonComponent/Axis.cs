using System;
using System.Drawing;
using SSCCommonComponent.Struct;

namespace SSCCommonComponent
{
    public class Axis
    {
        public Margin Margin { get; set; } = new Margin(80, 40, 40, 60);

        // Optional overrides for axis ranges; when null, Axis can try to use renderer-provided bounds
        public float? XMin { get; set; }
        public float? XMax { get; set; }
        public float? YMin { get; set; }
        public float? YMax { get; set; }

        // Labels (can be customized by caller)
        public string XLabel { get; set; } = "Time (min)";
        public string YLabel { get; set; } = "Intensity";

        public void DrawAxis(Graphics g, Rectangle clientArea)
        {
            int width = clientArea.Width - Margin.Left - Margin.Right;
            int height = clientArea.Height - Margin.Top - Margin.Bottom;

            Point origin = new Point(Margin.Left, clientArea.Height - Margin.Bottom);

            using var axisPen = new Pen(Color.Black, 2);
            Pen majorTickPen = Pens.Gray;
            using var minorTickPen = new Pen(Color.LightGray, 1);

            using Font labelFont = new Font("Arial", 10);
            Brush labelBrush = Brushes.Black;

            // Determine axis ranges
            float xMin = XMin ?? 0f;
            float xMax = XMax ?? 200f;
            float yMin = YMin ?? 0f;
            float yMax = YMax ?? 800f;

            float xRange = Math.Max(1e-6f, xMax - xMin);
            float yRange = Math.Max(1e-6f, yMax - yMin);

            // X 轴
            g.DrawLine(axisPen, origin.X, origin.Y, origin.X + width, origin.Y);
            g.DrawString(XLabel, labelFont, labelBrush, origin.X + width / 2 - 30, origin.Y + 30);

            // Determine nice tick spacing based on range
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

            // Y 轴
            g.DrawLine(axisPen, origin.X, origin.Y, origin.X, origin.Y - height);
            g.TranslateTransform(origin.X - 50, origin.Y - height / 2 - 10);       // 设置旋转中心
            g.RotateTransform(90);           // 逆时针旋转90度
            g.DrawString(YLabel, labelFont, labelBrush, 0, 0);
            g.ResetTransform();               // 恢复原始坐标系

            int desiredYTicks = 8;
            float yStep = NiceNumber(yRange / desiredYTicks);
            float yStart = (float)Math.Ceiling(yMin / yStep) * yStep;

            for (float yv = yStart; yv <= yMax; yv += yStep)
            {
                float yf = origin.Y - (yv - yMin) / yRange * height;
                g.DrawLine(minorTickPen, origin.X - 6, yf, origin.X - 1, yf);
                string label = yv.ToString("0.##");
                g.DrawString(label, labelFont, labelBrush, origin.X - 50, yf - 8);
            }
        }

        private float NiceNumber(float value)
        {
            // Return a "nice" number for tick spacing (1, 2, 5 * 10^n)
            float exp = (float)Math.Floor(Math.Log10(value));
            float f = value / (float)Math.Pow(10, exp);
            float nice;
            if (f < 1.5f) nice = 1f;
            else if (f < 3f) nice = 2f;
            else if (f < 7f) nice = 5f;
            else nice = 10f;
            return nice * (float)Math.Pow(10, exp);
        }
    }
}
