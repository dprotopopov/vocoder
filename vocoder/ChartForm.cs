using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace vocoder
{
    public partial class ChartForm : Form
    {
        public ChartForm(IEnumerable<double> values)
        {
            InitializeComponent();
            chart1.Series.Clear();
            Series series = chart1.Series.Add("Spectrum");
            series.ChartType = SeriesChartType.Line;
            series.Points.DataBindY(values);
        }
    }
}