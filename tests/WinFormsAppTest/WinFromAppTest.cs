using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SSCControls;

namespace WinFormsAppTest
{
    public partial class WinFromAppTest : Form
    {
        private SSCChromatogramViewControl chromatogramView;

        public WinFromAppTest()
        {
            InitializeComponent();
            this.Text = "Chromatogram Viewer";
            this.Width = 800;
            this.Height = 600;

            chromatogramView = new SSCChromatogramViewControl
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(chromatogramView);

            // Ä£ÄâÊý¾Ý
            var data = GenerateSimulatedChromatogram();
            chromatogramView.SetData(data);
        }

        private List<PointF> GenerateSimulatedChromatogram()
        {
            var list = new List<PointF>();
            for (int i = 0; i < 1000; i++)
            {
                // Gaussian function
                // f(x) = a \cdot e^{-\frac{(x - b)^2}{2c^2}}
                //   - a = amplitude (height of the peak)
                //   - b = center (location of the peak)
                //   - c = standard deviation (controls the width of the curve)
                //   - e = Euler¡¯s number, approximately 2.71828
                float x = i * 0.02f;
                float y = 
                    1.5f * (float)Math.Exp(-Math.Pow(x - 5, 2) / (2 * 0.5 * 0.5)) +
                    2.0f * (float)Math.Exp(-Math.Pow(x - 10, 2) / (2 * 0.8 * 0.8)) +
                    1.0f * (float)Math.Exp(-Math.Pow(x - 15, 2) / (2 * 0.6 * 0.6)) +
                    0.0f
                    ;
                list.Add(new PointF(x, y));
            }
            return list;
        }

    }
}
