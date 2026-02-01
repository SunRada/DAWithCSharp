using System.ComponentModel;
using SSC.CommonComponent;
using SSC.DataLib;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;

namespace SSC.Controls
{
    public enum ChromatogramDisplayMode
    {
        TwoD,
        ThreeD,
        Heatmap
    }

    public class SSCChromatogramViewControl : Control
    {
        private readonly GraphPanel _graphRenderer = new GraphPanel();
        private readonly Axis _axisRenderer = new Axis();
        private readonly HorizontalAxis _horizontalAxisRenderer = new HorizontalAxis();
        private readonly VerticalAxis _verticalAxisRenderer = new VerticalAxis();

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

        // 3D representation
        [Category("Data")]
        [Description("三维色谱数据点 (X: 保留时间, Y: 响应值, Z: 第三维（例如 m/z 或 scan）)")]
        public List<Point3F> DataPoints3D { get; set; } = new List<Point3F>();

        [Category("Display")]
        [Description("显示模式: 2D, 3D 堆叠视图 或 热图")]
        public ChromatogramDisplayMode DisplayMode { get; set; } = ChromatogramDisplayMode.TwoD;

        [Category("Display")]
        [Description("三维堆叠时每层的像素偏移量")]
        public float DepthOffset { get; set; } = 8f;

        [Category("Display")]
        [Description("将 Z 值分箱为多少层进行堆叠显示")]
        public int ZBins { get; set; } = 12;

        // Expose axis labels so test form can change them
        [Category("Display")]
        public string XAxisLabel
        {
            get => _axisRenderer.XLabel;
            set => _axisRenderer.XLabel = value;
        }

        public void SetData(List<PointF> data)
        {
            DataPoints = data;
        }

        public void SetData3D(List<Point3F> data)
        {
            DataPoints3D = data ?? new List<Point3F>();
            this.Invalidate();
        }

        public void Clear3DData()
        {
            DataPoints3D.Clear();
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Sync axis ranges with data when auto-scaling is desired
            if (_graphRenderer.AutoScaleAxes)
            {
                if (DisplayMode == ChromatogramDisplayMode.TwoD)
                {
                    var (minX, maxX, minY, maxY) = _graphRenderer.GetDataBounds();
                    _axisRenderer.XMin = minX;
                    _axisRenderer.XMax = maxX;
                    _axisRenderer.YMin = minY;
                    _axisRenderer.YMax = maxY;
                }
                else if (DisplayMode == ChromatogramDisplayMode.ThreeD || DisplayMode == ChromatogramDisplayMode.Heatmap)
                {
                    if (DataPoints3D != null && DataPoints3D.Count > 0)
                    {
                        float minX = DataPoints3D.Min(p => p.X);
                        float maxX = DataPoints3D.Max(p => p.X);
                        float minY = DataPoints3D.Min(p => p.Y);
                        float maxY = DataPoints3D.Max(p => p.Y);

                        if (Math.Abs(maxX - minX) < 1e-12f) maxX = minX + 1e-6f;
                        if (Math.Abs(maxY - minY) < 1e-12f) maxY = minY + 1e-6f;

                        _axisRenderer.XMin = minX;
                        _axisRenderer.XMax = maxX;
                        _axisRenderer.YMin = minY;
                        _axisRenderer.YMax = maxY;
                    }
                }
            }

            if (DisplayMode == ChromatogramDisplayMode.TwoD)
            {
                _graphRenderer.Draw(
                    e.Graphics, 
                    this.ClientRectangle
                );
                //_axisRenderer.DrawAxis(e.Graphics, this.ClientRectangle);
                _horizontalAxisRenderer.DrawAxis(e.Graphics, this.ClientRectangle);
                _verticalAxisRenderer.DrawAxis(e.Graphics, this.ClientRectangle);
                return;
            }

            if (DisplayMode == ChromatogramDisplayMode.ThreeD)
            {
                _graphRenderer.DrawStacked3D(
                    e.Graphics,
                    this.ClientRectangle,
                    DataPoints3D ?? new List<Point3F>(),
                    DepthOffset,
                    ZBins
                );
                //_axisRenderer.DrawAxis(e.Graphics, this.ClientRectangle);
                _horizontalAxisRenderer.DrawAxis(e.Graphics, this.ClientRectangle);
                _verticalAxisRenderer.DrawAxis(e.Graphics, this.ClientRectangle);
                return;
            }

            if (DisplayMode == ChromatogramDisplayMode.Heatmap)
            {
                _graphRenderer.DrawHeatmap(
                    e.Graphics,
                    this.ClientRectangle,
                    DataPoints3D ?? new List<Point3F>(),
                    ZBins
                );
                //_axis_renderer.DrawAxis(e.Graphics, this.ClientRectangle);
                _horizontalAxisRenderer.DrawAxis(e.Graphics, this.ClientRectangle);
                _verticalAxisRenderer.DrawAxis(e.Graphics, this.ClientRectangle);
                return;
            }
        }
    }
}
