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
            DataTable aTable = new DataTable();
            aTable.Columns.Add("A");
            aTable.Columns.Add("B");
            aTable.Columns.Add("C");
            aTable.Rows.Add("11", "12", "13");
            aTable.Rows.Add("21", "22", "23");
            DataRow aRow = aTable.NewRow();
            DragDropUserControl aDragDropUserControl = new DragDropUserControl(aTable);
            panel1.Controls.Add(aDragDropUserControl);
        }
    }
}
