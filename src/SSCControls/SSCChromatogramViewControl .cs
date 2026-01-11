using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SSCCommonComponent;

namespace SSCControls
{
    public enum ChromatogramDisplayMode
    {
        TwoD,
        ThreeD,
        Heatmap
    }

    public struct Point3F
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Point3F(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }
    }

    public class SSCChromatogramViewControl : Control
    {
        private readonly GraphPanel _graphRenderer = new GraphPanel();
        private readonly Axis _axisRenderer = new Axis();

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
                _graphRenderer.Draw(e.Graphics, this.ClientRectangle);
                _axisRenderer.DrawAxis(e.Graphics, this.ClientRectangle);
                return;
            }

            if (DisplayMode == ChromatogramDisplayMode.ThreeD)
            {
                DrawStacked3D(e.Graphics, this.ClientRectangle);
                _axisRenderer.DrawAxis(e.Graphics, this.ClientRectangle);
                return;
            }

            if (DisplayMode == ChromatogramDisplayMode.Heatmap)
            {
                DrawHeatmap(e.Graphics, this.ClientRectangle);
                _axisRenderer.DrawAxis(e.Graphics, this.ClientRectangle);
                return;
            }
        }

        private void DrawStacked3D(Graphics g, Rectangle rc)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (DataPoints3D == null || DataPoints3D.Count == 0) return;

            // Determine ranges
            var minX = DataPoints3D.Min(p => p.X);
            var maxX = DataPoints3D.Max(p => p.X);
            var minY = DataPoints3D.Min(p => p.Y);
            var maxY = DataPoints3D.Max(p => p.Y);
            var minZ = DataPoints3D.Min(p => p.Z);
            var maxZ = DataPoints3D.Max(p => p.Z);

            if (minX == maxX) maxX = minX + 1e-6f;
            if (minY == maxY) maxY = minY + 1e-6f;
            if (minZ == maxZ) maxZ = minZ + 1e-6f;

            // Create bins for Z
            int bins = Math.Max(1, ZBins);
            var layers = new List<List<Point3F>>(bins);
            for (int i = 0; i < bins; i++) layers.Add(new List<Point3F>());

            foreach (var p in DataPoints3D)
            {
                int idx = (int)((p.Z - minZ) / (maxZ - minZ) * (bins - 1));
                idx = Math.Clamp(idx, 0, bins - 1);
                layers[idx].Add(p);
            }

            // prepare inner drawing rect once
            var margin = _graphRenderer.Margin;
            float innerLeft = rc.Left + margin.Left;
            float innerTop = rc.Top + margin.Top;
            float innerWidth = rc.Width - margin.Left - margin.Right;
            float innerHeight = rc.Height - margin.Top - margin.Bottom;

            // Clip drawing to inner area so plotted lines do not overlap axis region
            var savedState = g.Save();
            g.SetClip(new RectangleF(innerLeft, innerTop, innerWidth, innerHeight));

            // Draw from back to front
            for (int layer = 0; layer < bins; layer++)
            {
                var pts = layers[layer];
                if (pts.Count == 0) continue;

                // Sort by X
                pts.Sort((a, b) => a.X.CompareTo(b.X));

                var penColor = Color.FromArgb(255 - (int)(layer * (200.0 / Math.Max(1, bins - 1))), Color.Blue.R, Color.Blue.G);
                using var pen = new Pen(penColor, 1.2f);

                // Map points once into screen coordinates and draw using DrawLines for performance
                var mapped = new List<PointF>(pts.Count);
                for (int i = 0; i < pts.Count; i++)
                {
                    var p = pts[i];
                    float x = innerLeft + (p.X - minX) / (maxX - minX) * innerWidth;
                    float yScaled = (p.Y - minY) / (maxY - minY);
                    float y = innerTop + innerHeight - yScaled * innerHeight;
                    float offset = (layer - (bins - 1) / 2.0f) * DepthOffset;
                    y += offset;
                    mapped.Add(new PointF(x, y));
                }

                if (mapped.Count == 1)
                {
                    var p = mapped[0];
                    g.DrawEllipse(pen, p.X - 1.5f, p.Y - 1.5f, 3f, 3f);
                }
                else if (mapped.Count > 1)
                {
                    g.DrawLines(pen, mapped.ToArray());
                }
            }

            // restore graphics state (remove clip)
            g.Restore(savedState);
        }

        private void DrawHeatmap(Graphics g, Rectangle rc)
        {
            if (DataPoints3D == null || DataPoints3D.Count == 0) return;

            // Respect margins so axis ticks/labels are not overlapped
            var margin = _graphRenderer.Margin;
            float innerLeft = rc.Left + margin.Left;
            float innerTop = rc.Top + margin.Top;
            float innerWidth = rc.Width - margin.Left - margin.Right;
            float innerHeight = rc.Height - margin.Top - margin.Bottom;

            // Clip heatmap drawing to inner area so it doesn't draw over axes
            var saved = g.Save();
            g.SetClip(new RectangleF(innerLeft, innerTop, innerWidth, innerHeight));

            // bins for X and Z (use inner drawing area)
            int xBins = Math.Clamp((int)innerWidth / 4, 10, 200);
            int zBins = Math.Max(1, ZBins);

            var minX = DataPoints3D.Min(p => p.X);
            var maxX = DataPoints3D.Max(p => p.X);
            var minZ = DataPoints3D.Min(p => p.Z);
            var maxZ = DataPoints3D.Max(p => p.Z);
            var minY = DataPoints3D.Min(p => p.Y);
            var maxY = DataPoints3D.Max(p => p.Y);

            var grid = new double[xBins, zBins];

            float epsX = Math.Abs(maxX - minX) < 1e-12f ? 1e-6f : 0f;
            float epsZ = Math.Abs(maxZ - minZ) < 1e-12f ? 1e-6f : 0f;

            foreach (var p in DataPoints3D)
            {
                int ix = (int)((p.X - minX) / (maxX - minX + epsX) * (xBins - 1));
                int iz = (int)((p.Z - minZ) / (maxZ - minZ + epsZ) * (zBins - 1));
                ix = Math.Clamp(ix, 0, xBins - 1);
                iz = Math.Clamp(iz, 0, zBins - 1);
                grid[ix, iz] = Math.Max(grid[ix, iz], p.Y);
            }

            double maxVal = 0;
            for (int i = 0; i < xBins; i++)
                for (int j = 0; j < zBins; j++)
                    if (grid[i, j] > maxVal) maxVal = grid[i, j];

            if (maxVal <= 0) maxVal = 1.0;

            float cellW = innerWidth / xBins;
            float cellH = innerHeight / zBins;

            for (int ix = 0; ix < xBins; ix++)
            {
                for (int iz = 0; iz < zBins; iz++)
                {
                    float v = (float)(grid[ix, iz] / maxVal);
                    var color = HeatmapColor(v);
                    using var brush = new SolidBrush(color);
                    // Draw with origin at bottom-left of inner area so Z=0 appears near bottom (consistent with axis)
                    float x = innerLeft + ix * cellW;
                    float y = innerTop + innerHeight - (iz + 1) * cellH;
                    var rect = new RectangleF(x, y, cellW + 1, cellH + 1);
                    g.FillRectangle(brush, rect);
                }
            }

            // restore graphics state
            g.Restore(saved);
        }

        private Color HeatmapColor(float value)
        {
            value = Math.Clamp(value, 0f, 1f);
            int r = (int)(value * 255);
            int g = (int)((1 - value) * 255);
            int b = 64;
            return Color.FromArgb(200, r, g, b);
        }
    }
}
