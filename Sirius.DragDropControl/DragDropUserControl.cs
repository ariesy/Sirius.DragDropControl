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
    public partial class DragDropUserControl : UserControl
    {
        private Rectangle dragBoxFromMouseDown;
        private int rowIndexFromMouseDown;
        private int rowIndexOfItemUnderMouseToDrop;
        private int columnIndexFromMouseDown;
        private int columnIndexOfItemUnderMouseToDrop;

        public bool AllowRowDragDrop { get; set; }

        public bool AllowColumnDragDrop { get; set; }

        public DragDropUserControl()
        {
            InitializeComponent();
            AllowColumnDragDrop = true;
            AllowRowDragDrop = true;
            dataGridView.Rows.Add("11", "12", "13");
            dataGridView.Rows.Add("21", "22", "23");
        }

        private void DataGridViewMouseMove(object theSender, MouseEventArgs theE)
        {
            if ((theE.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                if (dragBoxFromMouseDown != Rectangle.Empty && !dragBoxFromMouseDown.Contains(theE.X, theE.Y))
                {
                    dataGridView.DoDragDrop(dataGridView.Rows[rowIndexFromMouseDown], DragDropEffects.Move);
                }
            }
        }

        private void DataGridViewmouseDown(object theSender, MouseEventArgs theE)
        {
            var aHitTestInfo = dataGridView.HitTest(theE.X, theE.Y);
            rowIndexFromMouseDown = aHitTestInfo.RowIndex;
            columnIndexFromMouseDown = aHitTestInfo.ColumnIndex;

            if (rowIndexFromMouseDown != -1)
            {
                Size dragSize = SystemInformation.DragSize;
                dragBoxFromMouseDown = new Rectangle(new Point(theE.X - (dragSize.Width / 2), theE.Y - (dragSize.Height / 2)), dragSize);
            }
            else
            {
                dragBoxFromMouseDown = Rectangle.Empty;
            }
        }

        private void DataGridViewDragOver(object theSender, DragEventArgs theE)
        {
            theE.Effect = DragDropEffects.Move;
        }

        private void DataGridViewDragDrop(object theSender, DragEventArgs theE)
        {
            Point aClientPoint = dataGridView.PointToClient(new Point(theE.X, theE.Y));
            var aHitTest = dataGridView.HitTest(aClientPoint.X, aClientPoint.Y);
            rowIndexOfItemUnderMouseToDrop = aHitTest.RowIndex;
            columnIndexOfItemUnderMouseToDrop = aHitTest.ColumnIndex;
            if (theE.Effect == DragDropEffects.Move)
            {
                if (AllowRowDragDrop && rowIndexFromMouseDown != rowIndexOfItemUnderMouseToDrop)
                {
                    DataGridViewRow aRowToMove = theE.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;
                    dataGridView.Rows.RemoveAt(rowIndexFromMouseDown);
                    dataGridView.Rows.Insert(rowIndexOfItemUnderMouseToDrop, aRowToMove);
                }
                else if (AllowColumnDragDrop)
                {
                    object aSwapObj;
                    foreach (DataGridViewRow aRow in dataGridView.Rows)
                    {
                        aSwapObj = aRow.Cells[columnIndexFromMouseDown].Value;
                        aRow.Cells[columnIndexFromMouseDown].Value = aRow.Cells[columnIndexOfItemUnderMouseToDrop].Value;
                        aRow.Cells[columnIndexOfItemUnderMouseToDrop].Value = aSwapObj;
                    }
                }
            }
        }
    }
}
