using System;
using System.Collections.Generic;
using System.Drawing;
using SSCCommonComponent.Struct;

namespace SSCCommonComponent
{
    public class GraphPanel
    {
        public List<PointF> DataPoints { get; set; } = new List<PointF>();
        public Pen CurvePen { get; set; } = new Pen(Color.Blue, 2);
        public Margin Margin { get; set; } = new Margin(80, 40, 40, 60);

        /// <summary>
        /// If true, axis ranges will be derived from data when asked.
        /// </summary>
        public bool AutoScaleAxes { get; set; } = true;

        public GraphPanel() { }

        public void Draw(Graphics g, Rectangle clientRect)
        {
            g.Clear(Color.White);

            if (DataPoints == null || DataPoints.Count < 2)
                return;

            float width = clientRect.Width - Margin.Left - Margin.Right;
            float height = clientRect.Height - Margin.Top - Margin.Bottom;

            float maxX = float.MinValue, maxY = float.MinValue;
            foreach (var pt in DataPoints)
            {
                if (pt.X > maxX) maxX = pt.X;
                if (pt.Y > maxY) maxY = pt.Y;
            }

            // 曲线
            for (int i = 1; i < DataPoints.Count; i++)
            {
                var p1 = DataPoints[i - 1];
                var p2 = DataPoints[i];

                float x1 = Margin.Left + (p1.X / maxX) * width;
                float y1 = Margin.Top + height - (p1.Y / maxY) * height;
                float x2 = Margin.Left + (p2.X / maxX) * width;
                float y2 = Margin.Top + height - (p2.Y / maxY) * height;

                g.DrawLine(CurvePen, x1, y1, x2, y2);
            }
        }

        /// <summary>
        /// Return data bounds: minX, maxX, minY, maxY. If no data, returns zeros.
        /// </summary>
        public (float minX, float maxX, float minY, float maxY) GetDataBounds()
        {
            if (DataPoints == null || DataPoints.Count == 0)
                return (0f, 0f, 0f, 0f);

            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            foreach (var p in DataPoints)
            {
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }

            if (Math.Abs(maxX - minX) < 1e-12f) maxX = minX + 1e-6f;
            if (Math.Abs(maxY - minY) < 1e-12f) maxY = minY + 1e-6f;

            return (minX, maxX, minY, maxY);
        }
    }
}
