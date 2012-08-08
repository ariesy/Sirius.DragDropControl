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
        public enum SortCriteria
        {
            ByRow,
            ByColumn
        }

        public enum SortOrder
        {
            Asc,
            Desc
        }

        private Rectangle dragBoxFromMouseDown;
        private int rowIndexFromMouseDown;
        private int rowIndexOfItemUnderMouseToDrop;
        private int columnIndexFromMouseDown;
        private int columnIndexOfItemUnderMouseToDrop;
        private DataTable data;

        public bool AllowRowDragDrop { get; set; }

        public bool AllowColumnDragDrop { get; set; }

        public DataGridView InnerDataGridView
        {
            get { return dataGridView; }
        }

        public DragDropUserControl(DataTable theData)
        {
            InitializeComponent();
            data = theData;
            AllowColumnDragDrop = true;
            AllowRowDragDrop = true;
            InitalizeDataGridView();
        }

        private void InitalizeDataGridView()
        {
            foreach (DataColumn aColumn in data.Columns)
            {
                dataGridView.Columns.Add(aColumn.ColumnName, aColumn.ColumnName);
            }

            for (int aI = 0; aI < data.Rows.Count; aI++)
            {
                DataRow aRow = data.Rows[aI];
                DataGridViewRow aGridRow = new DataGridViewRow();
                aGridRow.HeaderCell.Value = (aI + 1).ToString();
                aGridRow.CreateCells(dataGridView, aRow.ItemArray);
                dataGridView.Rows.Add(aGridRow);
            }
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

                    object aSwapRowHeader = dataGridView.Rows[rowIndexFromMouseDown].HeaderCell.Value;
                    dataGridView.Rows[rowIndexFromMouseDown].HeaderCell.Value =
                        dataGridView.Rows[rowIndexOfItemUnderMouseToDrop].HeaderCell.Value;
                    dataGridView.Rows[rowIndexOfItemUnderMouseToDrop].HeaderCell.Value = aSwapRowHeader;
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

        //public void Sort(int theIndex, SortCriteria theCriterial, SortOrder theOrder)
        //{}
    }
}
