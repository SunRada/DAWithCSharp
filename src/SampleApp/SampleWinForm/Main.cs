using WindowsForms;

namespace SampleWinForm
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PropertySetting propertySettingScrean = new PropertySetting();
            propertySettingScrean.ShowDialog();
        }
    }
}
