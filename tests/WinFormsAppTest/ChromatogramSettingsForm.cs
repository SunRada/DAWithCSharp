using System;
using System.Drawing;
using System.Windows.Forms;
using SSC.Controls;

namespace WinFormsAppTest
{
    public class ChromatogramSettingsForm : Form
    {
        private readonly SSCChromatogramViewControl _target;

        private ComboBox comboDisplayMode;
        private NumericUpDown numericDepthOffset;
        private NumericUpDown numericZBins;
        private TextBox txtXAxisLabel;
        private Button btnApply;
        private Button btnCancel;

        public ChromatogramSettingsForm(SSCChromatogramViewControl target)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            InitializeComponents();
            LoadFromTarget();
        }

        private void InitializeComponents()
        {
            this.Text = "Chromatogram Settings";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(360, 220);
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblMode = new Label { Text = "Display Mode:", Left = 12, Top = 16, Width = 100 };
            comboDisplayMode = new ComboBox { Left = 120, Top = 12, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            comboDisplayMode.DataSource = Enum.GetValues(typeof(ChromatogramDisplayMode));

            var lblDepth = new Label { Text = "Depth Offset:", Left = 12, Top = 52, Width = 100 };
            numericDepthOffset = new NumericUpDown { Left = 120, Top = 48, Width = 100, DecimalPlaces = 1, Minimum = 0, Maximum = 200, Increment = 0.5M };

            var lblZ = new Label { Text = "Z Bins:", Left = 12, Top = 88, Width = 100 };
            numericZBins = new NumericUpDown { Left = 120, Top = 84, Width = 100, Minimum = 1, Maximum = 100, Value = 12 };

            var lblXLabel = new Label { Text = "X Axis Label:", Left = 12, Top = 124, Width = 100 };
            txtXAxisLabel = new TextBox { Left = 120, Top = 120, Width = 220 };

            // Increase button height for better touch/visibility
            btnApply = new Button { Text = "Apply", Left = 120, Top = 156, Width = 80, Height = 32 }; btnApply.Click += BtnApply_Click;
            btnCancel = new Button { Text = "Cancel", Left = 220, Top = 156, Width = 80, Height = 32 }; btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblMode, comboDisplayMode, lblDepth, numericDepthOffset, lblZ, numericZBins, lblXLabel, txtXAxisLabel, btnApply, btnCancel });
        }

        private void LoadFromTarget()
        {
            if (_target == null) return;
            comboDisplayMode.SelectedItem = _target.DisplayMode;
            numericDepthOffset.Value = (decimal)Math.Max(0, _target.DepthOffset);
            numericZBins.Value = Math.Max(1, Math.Min((int)numericZBins.Maximum, _target.ZBins));
            txtXAxisLabel.Text = _target.XAxisLabel;
        }

        private void BtnApply_Click(object? sender, EventArgs e)
        {
            // Apply changes back to target control
            _target.DisplayMode = (ChromatogramDisplayMode)comboDisplayMode.SelectedItem;
            _target.DepthOffset = (float)numericDepthOffset.Value;
            _target.ZBins = (int)numericZBins.Value;
            _target.XAxisLabel = txtXAxisLabel.Text ?? string.Empty;

            _target.Invalidate();

            // keep the dialog open so user can make more changes, or close? we'll keep open
            // Optionally close: this.Close();
        }
    }
}
