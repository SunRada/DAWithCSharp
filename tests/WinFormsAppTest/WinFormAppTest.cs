using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SSC.Controls;
using SSC.CommonComponent;
using SSC.DataLib;

namespace WinFormsAppTest
{
    public partial class WinFormAppTest : Form
    {
        private SSCChromatogramViewControl chromatogramView;
        private ComboBox displayModeCombo;

        public WinFormAppTest()
        {
            InitializeComponent();
            this.Text = "Chromatogram Viewer";
            this.Width = 1280;
            this.Height = 1024;

            // 下拉列表用于选择显示模式（2D/3D/Heatmap）
            displayModeCombo = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Height = 28
            };
            displayModeCombo.DataSource = Enum.GetValues(typeof(ChromatogramDisplayMode));
            displayModeCombo.SelectedItem = ChromatogramDisplayMode.TwoD;
            displayModeCombo.SelectedIndexChanged += DisplayModeCombo_SelectedIndexChanged;

            chromatogramView = new SSCChromatogramViewControl
            {
                Dock = DockStyle.Fill
            };

            // 添加控件：先添加顶部下拉，再添加填充视图
            this.Controls.Add(displayModeCombo);
            this.Controls.Add(chromatogramView);

            // 右键菜单
            var ctx = new ContextMenuStrip();
            var settingsItem = new ToolStripMenuItem("Settings...");
            settingsItem.Click += SettingsItem_Click;
            ctx.Items.Add(settingsItem);
            chromatogramView.ContextMenuStrip = ctx;

            // 异步加载并设置数据，避免阻塞 UI 线程
            _ = LoadDataAsync();
        }

        private void SettingsItem_Click(object? sender, EventArgs e)
        {
            // Use ShowDialog to keep the dialog alive until user closes it.
            using var dlg = new ChromatogramSettingsForm(chromatogramView);
            dlg.ShowDialog(this);
        }

        private void DisplayModeCombo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (displayModeCombo.SelectedItem is ChromatogramDisplayMode mode)
            {
                chromatogramView.DisplayMode = mode;
                chromatogramView.Invalidate();
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // 先加载 2D 数据用于快速展示
                var data = await Task.Run(() => GenerateSimulatedChromatogram());
                var safe2D = Sanitize2D(data);
                if (safe2D.Count > 0)
                {
                    if (this.IsHandleCreated)
                        this.BeginInvoke(() => chromatogramView.SetData(safe2D));
                }

                // 生成 3D 数据并设置，但不自动切换显示模式，让用户通过下拉选择
                var data3D = await Task.Run(() => GenerateSimulatedChromatogram3D());
                var safe3D = Sanitize3D(data3D);
                if (safe3D.Count > 0)
                {
                    if (this.IsHandleCreated)
                    {
                        this.BeginInvoke(() => chromatogramView.SetData3D(safe3D));
                    }
                }
            }
            catch (Exception ex)
            {
                // 发生异常时在 UI 上提示
                if (this.IsHandleCreated)
                {
                    this.BeginInvoke(() => MessageBox.Show(this, $"加载数据发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error));
                }
            }
        }

        private List<PointF> Sanitize2D(List<PointF> raw)
        {
            if (raw == null) return new List<PointF>();
            // 去除含非法值的点，并按 X 排序，去重
            var cleaned = raw.Where(p => !float.IsNaN(p.X) && !float.IsNaN(p.Y) && !float.IsInfinity(p.X) && !float.IsInfinity(p.Y)).ToList();
            cleaned.Sort((a, b) => a.X.CompareTo(b.X));
            // 可选：移除重复的 X
            var result = new List<PointF>(cleaned.Count);
            float? lastX = null;
            foreach (var p in cleaned)
            {
                if (lastX.HasValue && Math.Abs(p.X - lastX.Value) < 1e-6f) continue;
                result.Add(p);
                lastX = p.X;
            }
            return result;
        }

        private List<Point3F> Sanitize3D(List<Point3F> raw)
        {
            if (raw == null) return new List<Point3F>();
            var cleaned = raw.Where(p => !float.IsNaN(p.X) && !float.IsNaN(p.Y) && !float.IsNaN(p.Z)
                                          && !float.IsInfinity(p.X) && !float.IsInfinity(p.Y) && !float.IsInfinity(p.Z)).ToList();
            return cleaned;
        }

        private List<PointF> GenerateSimulatedChromatogram()
        {
            var list = new List<PointF>();
            for (int i = 0; i < 1000; i++)
            {
                float x = i * 0.02f;
                float y =
                    1.5f * (float)Math.Exp(-Math.Pow(x - 5, 2) / (2 * 0.5 * 0.5)) +
                    2.0f * (float)Math.Exp(-Math.Pow(x - 10, 2) / (2 * 0.8 * 0.8)) +
                    1.0f * (float)Math.Exp(-Math.Pow(x - 15, 2) / (2 * 0.6 * 0.6));
                list.Add(new PointF(x, y));
            }
            return list;
        }

        private List<Point3F> GenerateSimulatedChromatogram3D()
        {
            // 生成多个 scan 层，每层在 Z 方向上略有偏移
            var list = new List<Point3F>();
            int scans = 20;
            for (int s = 0; s < scans; s++)
            {
                float z = s * 0.2f; // 第三维值，例如 scan 或 m/z
                for (int i = 0; i < 500; i++)
                {
                    float x = i * 0.04f;
                    // 在不同层中稍微改变峰的宽度或高度，制造可视差异
                    float scale = 1.0f + (s - scans / 2) * 0.03f;
                    float y =
                        1.5f * scale * (float)Math.Exp(-Math.Pow(x - 5, 2) / (2 * 0.5 * 0.5)) +
                        2.0f * scale * (float)Math.Exp(-Math.Pow(x - 10, 2) / (2 * 0.8 * 0.8)) +
                        1.0f * scale * (float)Math.Exp(-Math.Pow(x - 15, 2) / (2 * 0.6 * 0.6));
                    list.Add(new Point3F(x, y, z));
                }
            }
            return list;
        }
    }
}
