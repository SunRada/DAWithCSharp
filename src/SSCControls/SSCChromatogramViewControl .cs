using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SSCControls
{
    public class SSCChromatogramViewControl : Control
    {
        private List<PointF> _dataPoints = new List<PointF>();
        private Pen _curvePen = new Pen(Color.Blue, 2);
        private Pen _axisPen = new Pen(Color.Black, 1);

        public SSCChromatogramViewControl()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
        }

        [Category("Data")]
        [Description("色谱数据点 (X: 保留时间, Y: 响应值)")]
        public List<PointF> DataPoints
        {
            get => _dataPoints;
            set
            {
                _dataPoints = value ?? new List<PointF>();
                this.Invalidate();
            }
        }

        public void SetData(List<PointF> data)
        {
            DataPoints = data;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(Color.White);

            if (_dataPoints.Count < 2)
                return;

            float margin = 40;
            float width = this.ClientSize.Width - 2 * margin;
            float height = this.ClientSize.Height - 2 * margin;

            float maxX = float.MinValue, maxY = float.MinValue;
            foreach (var pt in _dataPoints)
            {
                if (pt.X > maxX) maxX = pt.X;
                if (pt.Y > maxY) maxY = pt.Y;
            }

            g.DrawLine(_axisPen, margin, margin + height, margin + width, margin + height); // X轴
            g.DrawLine(_axisPen, margin, margin, margin, margin + height); // Y轴

            for (int i = 1; i < _dataPoints.Count; i++)
            {
                var p1 = _dataPoints[i - 1];
                var p2 = _dataPoints[i];

                float x1 = margin + (p1.X / maxX) * width;
                float y1 = margin + height - (p1.Y / maxY) * height;
                float x2 = margin + (p2.X / maxX) * width;
                float y2 = margin + height - (p2.Y / maxY) * height;

                g.DrawLine(_curvePen, x1, y1, x2, y2);
            }
        }
    }

}
