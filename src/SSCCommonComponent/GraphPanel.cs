using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SSC.CommonComponent.Struct;
using SSC.DataLib;

namespace SSC.CommonComponent
{
    /// <summary>
    /// GraphPanel is responsible for drawing 2D chromatogram curves, stacked 3D line views and heatmaps.
    /// It exposes a simple API for providing data points and basic rendering parameters (Margin, CurvePen).
    /// </summary>
    public class GraphPanel
    {
        /// <summary>
        /// 2D data points (X: retention/time, Y: intensity)
        /// </summary>
        // 2D 数据点集合：X 表示保留时间或扫描时间，Y 表示响应强度
        public List<PointF> DataPoints { get; set; } = new List<PointF>();

        /// <summary>
        /// Pen used to draw the 2D curve.
        /// </summary>
        // 绘制 2D 曲线的画笔，可在运行时调整宽度和颜色
        public Pen CurvePen { get; set; } = new Pen(Color.Blue, 2);

        /// <summary>
        /// Plot margins (left, right, top, bottom) in pixels. Axes rendering uses these values to reserve space.
        /// </summary>
        // 绘图区域边距：用于为坐标轴和标签留出空间（单位：像素）
        public Margin Margin { get; set; } = new Margin(80, 40, 40, 60);

        /// <summary>
        /// If true, axis ranges will be derived from data when asked by the control.
        /// </summary>
        // 是否自动根据数据范围设置坐标轴范围
        public bool AutoScaleAxes { get; set; } = true;

        public GraphPanel() { }

        /// <summary>
        /// Draws a simple 2D curve into the provided client rectangle. The method respects the configured Margin
        /// and assumes DataPoints contains at least two points.
        /// </summary>
        public void Draw(Graphics g, Rectangle clientRect)
        {
            g.Clear(Color.White);

            if (DataPoints == null || DataPoints.Count < 2)
                return;

            // Compute inner drawing area (respect margins)
            float width = clientRect.Width - Margin.Left - Margin.Right;
            float height = clientRect.Height - Margin.Top - Margin.Bottom;

            // Find maximum values for simple normalization
            float maxX = float.MinValue, maxY = float.MinValue;
            foreach (var pt in DataPoints)
            {
                if (pt.X > maxX) maxX = pt.X;
                if (pt.Y > maxY) maxY = pt.Y;
            }

            // 如果数据是非常小的数量级，maxX 或 maxY 可能为极小值，但后续代码已有保护。
            // Draw the polyline connecting consecutive data points
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
        /// This is used by controls to determine axis ranges when AutoScaleAxes is enabled.
        /// </summary>
        public (float minX, float maxX, float minY, float maxY) GetDataBounds()
        {
            if (DataPoints == null || DataPoints.Count == 0)
                return (0f, 0f, 0f, 0f);

            // 计算数据边界（min/max），用于坐标轴自动缩放
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            foreach (var p in DataPoints)
            {
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }

            // Avoid zero-range which would cause division by zero in rendering code
            if (Math.Abs(maxX - minX) < 1e-12f) maxX = minX + 1e-6f;
            if (Math.Abs(maxY - minY) < 1e-12f) maxY = minY + 1e-6f;

            return (minX, maxX, minY, maxY);
        }

        /// <summary>
        /// DrawStacked3D renders a stacked line view from 3D points grouped into Z bins.
        /// It maps X/Y into the inner plotting rectangle (respecting Margin) and applies a per-layer vertical offset
        /// to create a stacked appearance. The method clips drawing to the inner area and will translate the
        /// entire set upward if parts would overlap the axis area.
        /// </summary>
        public void DrawStacked3D(Graphics g, Rectangle rc, List<Point3F> data, float depthOffset, int zBins)
        {
            // 3D 堆叠渲染：将三维点按 Z 分箱，然后对每个箱按 X 排序并绘制折线
            if (data == null || data.Count == 0) return;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Determine numeric ranges in the data
            var minX = data.Min(p => p.X);
            var maxX = data.Max(p => p.X);
            var minY = data.Min(p => p.Y);
            var maxY = data.Max(p => p.Y);
            var minZ = data.Min(p => p.Z);
            var maxZ = data.Max(p => p.Z);

            if (minX == maxX) maxX = minX + 1e-6f;
            if (minY == maxY) maxY = minY + 1e-6f;
            if (minZ == maxZ) maxZ = minZ + 1e-6f;

            // Bin points by Z to create layers
            int bins = Math.Max(1, zBins);
            var layers = new List<List<Point3F>>(bins);
            for (int i = 0; i < bins; i++) layers.Add(new List<Point3F>());

            foreach (var p in data)
            {
                int idx = (int)((p.Z - minZ) / (maxZ - minZ) * (bins - 1));
                idx = Math.Clamp(idx, 0, bins - 1);
                layers[idx].Add(p);
            }

            // Compute inner plotting rectangle
            var margin = this.Margin;
            float innerLeft = rc.Left + margin.Left;
            float innerTop = rc.Top + margin.Top;
            float innerWidth = rc.Width - margin.Left - margin.Right;
            float innerHeight = rc.Height - margin.Top - margin.Bottom;

            // Clip to inner area so axes/labels are preserved
            // 将绘制限制在内部绘图区域，避免覆盖坐标轴与标签
            var savedState = g.Save();
            g.SetClip(new RectangleF(innerLeft, innerTop, innerWidth, innerHeight));

            // Map points to screen coordinates per layer
            var mappedLayers = new List<PointF[]>();
            var penColors = new List<Color>();
            for (int layer = 0; layer < bins; layer++)
            {
                var pts = layers[layer];
                if (pts.Count == 0)
                {
                    mappedLayers.Add(Array.Empty<PointF>());
                    penColors.Add(Color.Empty);
                    continue;
                }

                pts.Sort((a, b) => a.X.CompareTo(b.X));
                var mapped = new PointF[pts.Count];
                for (int i = 0; i < pts.Count; i++)
                {
                    var p = pts[i];
                    float x = innerLeft + (p.X - minX) / (maxX - minX) * innerWidth;
                    float yScaled = (p.Y - minY) / (maxY - minY);
                    float y = innerTop + innerHeight - yScaled * innerHeight;
                    float offset = (layer - (bins - 1) / 2.0f) * depthOffset;
                    y += offset;
                    mapped[i] = new PointF(x, y);
                }
                mappedLayers.Add(mapped);
                var penColor = Color.FromArgb(255 - (int)(layer * (200.0 / Math.Max(1, bins - 1))), Color.Blue.R, Color.Blue.G);
                penColors.Add(penColor);
            }

            // Determine vertical bounds and translate if necessary so nothing is clipped by axes
            // 计算映射后所有点的垂直范围，如果超出内部区域则整体上移，避免被 X 轴遮挡
            float globalMinY = float.MaxValue, globalMaxY = float.MinValue;
            foreach (var arr in mappedLayers)
            {
                foreach (var pt in arr)
                {
                    if (pt.Y < globalMinY) globalMinY = pt.Y;
                    if (pt.Y > globalMaxY) globalMaxY = pt.Y;
                }
            }

            if (globalMinY == float.MaxValue)
            {
                g.Restore(savedState);
                return;
            }

            float innerBottom = innerTop + innerHeight;
            float translateY = 0f;
            if (globalMaxY > innerBottom)
            {
                translateY = innerBottom - globalMaxY - 1f;
            }
            if (globalMinY + translateY < innerTop)
            {
                translateY = innerTop - globalMinY + 1f;
            }

            // Draw layers (translated) using DrawLines for performance
            // 绘制每一层：单点用小圆表示，多点用 DrawLines 提高性能
            for (int layer = 0; layer < mappedLayers.Count; layer++)
            {
                var mapped = mappedLayers[layer];
                if (mapped.Length == 0) continue;
                var penColor = penColors[layer];
                using var pen = new Pen(penColor, 1.2f);

                if (mapped.Length == 1)
                {
                    var p = mapped[0];
                    p.Y += translateY;
                    g.DrawEllipse(pen, p.X - 1.5f, p.Y - 1.5f, 3f, 3f);
                }
                else
                {
                    var arr = new PointF[mapped.Length];
                    for (int i = 0; i < mapped.Length; i++)
                    {
                        arr[i] = new PointF(mapped[i].X, mapped[i].Y + translateY);
                    }
                    g.DrawLines(pen, arr);
                }
            }

            g.Restore(savedState);
        }

        /// <summary>
        /// DrawHeatmap aggregates 3D points into a 2D grid (X vs Z) and renders colored cells whose intensity
        /// corresponds to the maximum Y value observed in each bin. Drawing is clipped to the inner plotting area.
        /// </summary>
        public void DrawHeatmap(Graphics g, Rectangle rc, List<Point3F> data, int zBins)
        {
            // 热图渲染：将 X 和 Z 分箱，取每格的最大 Y 作为强度，然后用颜色表示
            if (data == null || data.Count == 0) return;

            var margin = this.Margin;
            float innerLeft = rc.Left + margin.Left;
            float innerTop = rc.Top + margin.Top;
            float innerWidth = rc.Width - margin.Left - margin.Right;
            float innerHeight = rc.Height - margin.Top - margin.Bottom;

            var saved = g.Save();
            g.SetClip(new RectangleF(innerLeft, innerTop, innerWidth, innerHeight));

            int xBins = Math.Clamp((int)innerWidth / 4, 10, 200);
            int zbin = Math.Max(1, zBins);

            var minX = data.Min(p => p.X);
            var maxX = data.Max(p => p.X);
            var minZ = data.Min(p => p.Z);
            var maxZ = data.Max(p => p.Z);
            var minY = data.Min(p => p.Y);
            var maxY = data.Max(p => p.Y);

            var grid = new double[xBins, zbin];

            float epsX = Math.Abs(maxX - minX) < 1e-12f ? 1e-6f : 0f;
            float epsZ = Math.Abs(maxZ - minZ) < 1e-12f ? 1e-6f : 0f;

            // Aggregate data into grid (take maximum Y per cell)
            // 将数据聚合到网格中（每格保留最大 Y）
            foreach (var p in data)
            {
                int ix = (int)((p.X - minX) / (maxX - minX + epsX) * (xBins - 1));
                int iz = (int)((p.Z - minZ) / (maxZ - minZ + epsZ) * (zbin - 1));
                ix = Math.Clamp(ix, 0, xBins - 1);
                iz = Math.Clamp(iz, 0, zbin - 1);
                grid[ix, iz] = Math.Max(grid[ix, iz], p.Y);
            }

            // Find global maximum for normalization
            // 找到全局最大值用于颜色归一化
            double maxVal = 0;
            for (int i = 0; i < xBins; i++)
                for (int j = 0; j < zbin; j++)
                    if (grid[i, j] > maxVal) maxVal = grid[i, j];

            if (maxVal <= 0) maxVal = 1.0;

            float cellW = innerWidth / xBins;
            float cellH = innerHeight / zbin;

            // Render each cell using a color mapping function
            // 渲染网格单元，注意 Y 方向与坐标轴一致（原点在内部绘图区域底部）
            for (int ix = 0; ix < xBins; ix++)
            {
                for (int iz = 0; iz < zbin; iz++)
                {
                    float v = (float)(grid[ix, iz] / maxVal);
                    var color = HeatmapColor(v);
                    using var brush = new SolidBrush(color);
                    float x = innerLeft + ix * cellW;
                    float y = innerTop + innerHeight - (iz + 1) * cellH;
                    var rect = new RectangleF(x, y, cellW + 1, cellH + 1);
                    g.FillRectangle(brush, rect);
                }
            }

            g.Restore(saved);
        }

        /// <summary>
        /// Simple color mapping for heatmap values in range [0,1].
        /// </summary>
        private Color HeatmapColor(float value)
        {
            // 简单线性颜色映射：value 越大，红色越强；绿色逐渐减弱；蓝色为常量偏移
            value = Math.Clamp(value, 0f, 1f);
            int r = (int)(value * 255);
            int g = (int)((1 - value) * 255);
            int b = 64;
            return Color.FromArgb(200, r, g, b);
        }
    }
}
