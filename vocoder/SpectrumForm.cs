using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace vocoder
{
    public partial class SpectrumForm : Form
    {
        public SpectrumForm(double[] yValues)
        {
            InitializeComponent();
            Series series = chart1.Series.Add("Spectrum");
            series.ChartType = SeriesChartType.Line;
            series.Points.AddY(yValues);
        }
    }
}