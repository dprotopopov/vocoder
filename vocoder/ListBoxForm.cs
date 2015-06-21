using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace vocoder
{
    public partial class ListBoxForm : Form
    {
        public ListBoxForm(IEnumerable<object> items, IEnumerable<double> values)
        {
            InitializeComponent();
            listBox1.Items.AddRange(
                items.Zip(values, (item, value) => item.ToString() + " : " + value.ToString()).Cast<object>().ToArray());
        }
    }
}