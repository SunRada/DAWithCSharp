using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SSCCommonComponent;

namespace SSCControls
{
    public class SSCChromatogramViewControl : Control
    {
        private GraphPanel _graphRenderer = new GraphPanel();
        private Axis _axisRenderer = new Axis();

        public SSCChromatogramViewControl()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
        }

        [Category("Data")]
        [Description("色谱数据点 (X: 保留时间, Y: 响应值)")]
        public List<PointF> DataPoints
        {
            get => _graphRenderer.DataPoints;
            set
            {
                _graphRenderer.DataPoints = value ?? new List<PointF>();
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
            _graphRenderer.Draw(e.Graphics, this.ClientRectangle);

            // 可选：调用外部坐标轴绘制
            _axisRenderer.DrawAxes(e.Graphics, this.ClientRectangle);
        }

    }

}
