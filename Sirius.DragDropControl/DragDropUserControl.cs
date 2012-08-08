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
        public enum SortBy
        {
            ByRow,
            ByColumn
        }

        private class SortHelper
        {
            public object SortObj { get; set; }

            public List<object> Values { get; private set; }

            public SortHelper()
            {
                Values = new List<object>();
            }
        }

        private Rectangle dragBoxFromMouseDown;
        private int rowIndexFromMouseDown;
        private int rowIndexOfItemUnderMouseToDrop;
        private int columnIndexFromMouseDown;
        private int columnIndexOfItemUnderMouseToDrop;

        public bool AllowRowDragDrop { get; set; }

        public bool AllowColumnDragDrop { get; set; }

        public DataGridView InnerDataGridView
        {
            get { return dataGridView; }
        }

        public DragDropUserControl(DataTable theData)
        {
            InitializeComponent();
            AllowColumnDragDrop = true;
            AllowRowDragDrop = true;
            InitalizeDataGridView(theData);
        }

        private void InitalizeDataGridView(DataTable theData)
        {
            foreach (DataColumn aColumn in theData.Columns)
            {
                dataGridView.Columns.Add(aColumn.ColumnName, aColumn.ColumnName);
            }

            for (int aI = 0; aI < theData.Rows.Count; aI++)
            {
                DataRow aRow = theData.Rows[aI];
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

        private void SortByRow(int theIndex, Comparison<object> theCompareFunc)
        {
            List<SortHelper> aList = new List<SortHelper>();
            for (int aI = 0; aI < dataGridView.Columns.Count; aI++)
            {
                aList.Add(new SortHelper());
            }

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                for (int aI = 0; aI < row.Cells.Count; aI++)
                {
                    if (row.Index == theIndex)
                    {
                        aList[aI].SortObj = row.Cells[aI].Value;
                    }

                    aList[aI].Values.Add(row.Cells[aI].Value);
                }
            }

            aList.Sort((aSortHelper1, aSortHelper2) => theCompareFunc(aSortHelper1.SortObj, aSortHelper2.SortObj));

            for (int aRowIndex = 0; aRowIndex < dataGridView.Rows.Count; aRowIndex++)
            {
                var aRow = dataGridView.Rows[aRowIndex];
                for (int aCellIndex = 0; aCellIndex < aRow.Cells.Count; aCellIndex++)
                {
                    aRow.Cells[aCellIndex].Value = aList[aCellIndex].Values[aRowIndex];
                }
            }
        }

        private void SortByColumn(int theIndex, Comparison<object> theCompareFunc)
        {
            List<SortHelper> aList = new List<SortHelper>();
            for (int aRowIndex = 0; aRowIndex < dataGridView.Rows.Count; aRowIndex++)
            {
                var aRow = dataGridView.Rows[aRowIndex];
                var aHelper = new SortHelper();
                aList.Add(aHelper);
                for (int acellIndex = 0; acellIndex < aRow.Cells.Count; acellIndex++)
                {
                    aHelper.Values.Add(aRow.Cells[acellIndex].Value);
                    if (acellIndex == theIndex)
                    {
                        aHelper.SortObj = aRow.Cells[acellIndex].Value;
                    }
                }
            }

            aList.Sort((aSortHelper1, aSortHelper2) => theCompareFunc(aSortHelper1.SortObj, aSortHelper2.SortObj));

            for (int aRowIndex = 0; aRowIndex < dataGridView.Rows.Count; aRowIndex++)
            {
                var aRow = dataGridView.Rows[aRowIndex];
                for (int aCellIndex = 0; aCellIndex < aRow.Cells.Count; aCellIndex++)
                {
                    aRow.Cells[aCellIndex].Value = aList[aRowIndex].Values[aCellIndex];
                }
            }
        }

        public void Sort(int theIndex, SortBy theSortBy, Comparison<object> theCompareFunc)
        {
            if (theSortBy == SortBy.ByRow)
            {
                SortByRow(theIndex, theCompareFunc);
            }
            else
            {
                SortByColumn(theIndex, theCompareFunc);
            }
        }
    }
}
