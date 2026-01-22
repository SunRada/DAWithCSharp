using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SSCCommonComponent.Struct;
using SSCDataLib;

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

        public void DrawStacked3D(Graphics g, Rectangle rc, List<Point3F> data, float depthOffset, int zBins)
        {
            if (data == null || data.Count == 0) return;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Determine ranges
            var minX = data.Min(p => p.X);
            var maxX = data.Max(p => p.X);
            var minY = data.Min(p => p.Y);
            var maxY = data.Max(p => p.Y);
            var minZ = data.Min(p => p.Z);
            var maxZ = data.Max(p => p.Z);

            if (minX == maxX) maxX = minX + 1e-6f;
            if (minY == maxY) maxY = minY + 1e-6f;
            if (minZ == maxZ) maxZ = minZ + 1e-6f;

            int bins = Math.Max(1, zBins);
            var layers = new List<List<Point3F>>(bins);
            for (int i = 0; i < bins; i++) layers.Add(new List<Point3F>());

            foreach (var p in data)
            {
                int idx = (int)((p.Z - minZ) / (maxZ - minZ) * (bins - 1));
                idx = Math.Clamp(idx, 0, bins - 1);
                layers[idx].Add(p);
            }

            var margin = this.Margin;
            float innerLeft = rc.Left + margin.Left;
            float innerTop = rc.Top + margin.Top;
            float innerWidth = rc.Width - margin.Left - margin.Right;
            float innerHeight = rc.Height - margin.Top - margin.Bottom;

            var savedState = g.Save();
            g.SetClip(new RectangleF(innerLeft, innerTop, innerWidth, innerHeight));

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

        public void DrawHeatmap(Graphics g, Rectangle rc, List<Point3F> data, int zBins)
        {
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

            foreach (var p in data)
            {
                int ix = (int)((p.X - minX) / (maxX - minX + epsX) * (xBins - 1));
                int iz = (int)((p.Z - minZ) / (maxZ - minZ + epsZ) * (zbin - 1));
                ix = Math.Clamp(ix, 0, xBins - 1);
                iz = Math.Clamp(iz, 0, zbin - 1);
                grid[ix, iz] = Math.Max(grid[ix, iz], p.Y);
            }

            double maxVal = 0;
            for (int i = 0; i < xBins; i++)
                for (int j = 0; j < zbin; j++)
                    if (grid[i, j] > maxVal) maxVal = grid[i, j];

            if (maxVal <= 0) maxVal = 1.0;

            float cellW = innerWidth / xBins;
            float cellH = innerHeight / zbin;

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
