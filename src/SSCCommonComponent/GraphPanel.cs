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
    }
}
