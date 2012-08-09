﻿using System;
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
            aParameter.Data["Date"] = new List<object> { 2011, 2012 };
            aParameter.Data["Data"] = new List<object> { "Deposit", "debt" };
            var aFormatPreviewControl = new FormatPreviewControl(aParameter);
            panel1.Controls.Add(aFormatPreviewControl);

            aParameter.Data["Data"].Add("one more");

            aFormatPreviewControl.Refresh(aParameter);

            aFormatPreviewControl.Sort(FormatPreviewControl.SortOn.RowLabels, "Date", FormatPreviewControl.SortOrder.Desc);
        }
    }
}
