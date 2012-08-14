using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sirius.DragDropControl
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            var aParameter = new FormatPreviewControl.Parameters();
            aParameter.ColumnLabels.Add("Entity");
            aParameter.RowLabels.Add("Date");
            aParameter.RowLabels.Add("Data");
            aParameter.Data["Entity"] = new List<object> { "Direct Group", "Fortor Motor", "Home Depot" };
            aParameter.Data["Date"] = new List<object> { "2011-01-01", "2012-02-01" };
            aParameter.Data["Data"] = new List<object> { "Deposit", "debt" };
            var aFormatPreviewControl = new FormatPreviewControl();
            panel1.Controls.Add(aFormatPreviewControl);

            aFormatPreviewControl.Refresh();

            aParameter.Data["Data"].Add("one more");

            aFormatPreviewControl.LoadData(aParameter);

            aFormatPreviewControl.Sort(FormatPreviewControl.SortOn.RowLabels, "Date", FormatPreviewControl.SortOrder.Desc);

            aFormatPreviewControl.Sort(FormatPreviewControl.SortOn.ColumnLabels, "Entity", FormatPreviewControl.SortOrder.Asc);

            aFormatPreviewControl.ReOrdered += FormatPreviewControlReOrdered;

            var aResult = aFormatPreviewControl.GetOutPut();
        }

        private void FormatPreviewControlReOrdered(object theSender, FormatPreviewControl.ReOrderEventArgs theE)
        {
            var aColumnLabels = string.Join(",", theE.ColumnLabels.ToArray());
            var aRowLabels = string.Join(",", theE.RowLabels.ToArray());

            MessageBox.Show("Column labels:" + aColumnLabels + "\r\n" + "RowLabels:" + aRowLabels);
        }
    }
}
