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
        #region NestedClasses
        public enum SortBy
        {
            ByRow,
            ByColumn
        }

        public class SortHelper
        {
            public object SortObj { get; set; }

            public int OriginalPosition { get; set; }

            public List<object> Values { get; private set; }

            public SortHelper()
            {
                Values = new List<object>();
            }
        } 
        #endregion

        private Rectangle dragBoxFromMouseDown;
        private int rowIndexFromMouseDown;
        private int rowIndexOfItemUnderMouseToDrop;
        private int columnIndexFromMouseDown;
        private int columnIndexOfItemUnderMouseToDrop;

        public bool AllowRowDragDrop { get; set; }

        public bool AllowColumnDragDrop { get; set; }

        public bool AllowThisOneRowDragDrop { get; set; }

        public bool AllowThisOneColumnDragDrop { get; set; }

        public event Action<DragEventArgs, int, int, int, int> OnDrop;

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

        #region private methods
        private void InitalizeDataGridView(DataTable theData)
        {
            int aHeaderColumnIndex = dataGridView.Columns.Add("FirstColumn", "FirstColumn");
            DataGridViewColumn aHeaderColumn = dataGridView.Columns[aHeaderColumnIndex];
            aHeaderColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            aHeaderColumn.Width = 20;

            List<string> aHeaderTexts = new List<string>();
            aHeaderTexts.Add(string.Empty);
            foreach (DataColumn aColumn in theData.Columns)
            {
                DataGridViewColumn aGridColumn = new DataGridViewColumn();
                int aColumnIndex = dataGridView.Columns.Add(aColumn.ColumnName, aColumn.ColumnName);
                aHeaderTexts.Add(aColumn.ColumnName);
                dataGridView.Columns[aColumnIndex].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            DataGridViewRow aHeaderRow = new DataGridViewRow();
            aHeaderRow.CreateCells(dataGridView, aHeaderTexts.ToArray());
            dataGridView.Rows.Add(aHeaderRow);
            for (int aI = 0; aI < theData.Rows.Count; aI++)
            {
                DataRow aRow = theData.Rows[aI];
                List<object> aRowData = new List<object>();
                aRowData.Add(aI + 1);
                aRowData.AddRange(aRow.ItemArray);
                
                DataGridViewRow aGridRow = new DataGridViewRow();
                aGridRow.CreateCells(dataGridView, aRowData.ToArray());
                dataGridView.Rows.Add(aGridRow);
            }
        }

        private void DataGridViewMouseMove(object theSender, MouseEventArgs theE)
        {
            if ((theE.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                if (dragBoxFromMouseDown != Rectangle.Empty && !dragBoxFromMouseDown.Contains(theE.X, theE.Y))
                {
                    AllowThisOneColumnDragDrop = AllowColumnDragDrop;
                    AllowThisOneRowDragDrop = AllowRowDragDrop;
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

            if (!(rowIndexFromMouseDown != rowIndexOfItemUnderMouseToDrop && columnIndexFromMouseDown != columnIndexOfItemUnderMouseToDrop))
            {
                if (OnDrop != null)
                {
                    OnDrop(theE, rowIndexFromMouseDown, rowIndexOfItemUnderMouseToDrop, columnIndexFromMouseDown, columnIndexOfItemUnderMouseToDrop);
                }

                if (theE.Effect == DragDropEffects.Move)
                {
                    if (AllowThisOneRowDragDrop && rowIndexFromMouseDown != rowIndexOfItemUnderMouseToDrop)
                    {
                        DataGridViewRow aRowToMove = theE.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;
                        dataGridView.Rows.RemoveAt(rowIndexFromMouseDown);
                        dataGridView.Rows.Insert(rowIndexOfItemUnderMouseToDrop, aRowToMove);

                        object aSwapRowHeader = dataGridView.Rows[rowIndexFromMouseDown].Cells[0].Value;
                        dataGridView.Rows[rowIndexFromMouseDown].Cells[0].Value =
                            dataGridView.Rows[rowIndexOfItemUnderMouseToDrop].Cells[0].Value;
                        dataGridView.Rows[rowIndexOfItemUnderMouseToDrop].Cells[0].Value = aSwapRowHeader;

                        dataGridView.Rows[rowIndexOfItemUnderMouseToDrop].Cells[columnIndexOfItemUnderMouseToDrop].Selected = true;
                    }
                    else if (AllowThisOneColumnDragDrop)
                    {
                        object aSwapObj;
                        foreach (DataGridViewRow aRow in dataGridView.Rows)
                        {
                            if (aRow.Index == 0)
                            {
                                continue;
                            }

                            if (columnIndexFromMouseDown < columnIndexOfItemUnderMouseToDrop)
                            {
                                for (int aI = columnIndexFromMouseDown; aI < columnIndexOfItemUnderMouseToDrop; aI++)
                                {
                                    aSwapObj = aRow.Cells[aI].Value;
                                    aRow.Cells[aI].Value =
                                        aRow.Cells[aI + 1].Value;
                                    aRow.Cells[aI + 1].Value = aSwapObj;
                                }
                            }
                            else
                            {
                                for (int aI = columnIndexFromMouseDown; aI > columnIndexOfItemUnderMouseToDrop; aI--)
                                {
                                    aSwapObj = aRow.Cells[aI].Value;
                                    aRow.Cells[aI].Value =
                                        aRow.Cells[aI - 1].Value;
                                    aRow.Cells[aI - 1].Value = aSwapObj;
                                }
                            }
                        }

                        dataGridView.Rows[rowIndexOfItemUnderMouseToDrop].Cells[columnIndexOfItemUnderMouseToDrop].Selected = true;
                    }
                }
            }

            AllowThisOneRowDragDrop = AllowRowDragDrop;
            AllowThisOneColumnDragDrop = AllowColumnDragDrop;
        }

        private void SortByRow(int theIndex, Comparison<SortHelper> theCompareFunc)
        {
            List<SortHelper> aList = new List<SortHelper>();
            for (int aI = 1; aI < dataGridView.Columns.Count; aI++)
            {
                var aHelper = new SortHelper();
                aHelper.OriginalPosition = aI;
                aList.Add(aHelper);
            }

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Index == 0)
                {
                    continue;
                }

                for (int aI = 1; aI < row.Cells.Count; aI++)
                {
                    if (row.Index == theIndex)
                    {
                        aList[aI - 1].SortObj = row.Cells[aI].Value;
                    }

                    aList[aI - 1].Values.Add(row.Cells[aI].Value);
                }
            }

            aList.Sort((aSortHelper1, aSortHelper2) => theCompareFunc(aSortHelper1, aSortHelper2));

            for (int aRowIndex = 1; aRowIndex < dataGridView.Rows.Count; aRowIndex++)
            {
                var aRow = dataGridView.Rows[aRowIndex];
                for (int aCellIndex = 1; aCellIndex < aRow.Cells.Count; aCellIndex++)
                {
                    aRow.Cells[aCellIndex].Value = aList[aCellIndex - 1].Values[aRowIndex - 1];
                }
            }
        }

        private void SortByColumn(int theIndex, Comparison<SortHelper> theCompareFunc)
        {
            List<SortHelper> aList = new List<SortHelper>();
            for (int aRowIndex = 1; aRowIndex < dataGridView.Rows.Count; aRowIndex++)
            {
                var aRow = dataGridView.Rows[aRowIndex];
                var aHelper = new SortHelper();
                aList.Add(aHelper);
                aHelper.OriginalPosition = aRowIndex;
                for (int acellIndex = 1; acellIndex < aRow.Cells.Count; acellIndex++)
                {
                    aHelper.Values.Add(aRow.Cells[acellIndex].Value);
                    if (acellIndex == theIndex)
                    {
                        aHelper.SortObj = aRow.Cells[acellIndex].Value;
                    }
                }
            }

            aList.Sort((aSortHelper1, aSortHelper2) => theCompareFunc(aSortHelper1, aSortHelper2));

            for (int aRowIndex = 1; aRowIndex < dataGridView.Rows.Count; aRowIndex++)
            {
                var aRow = dataGridView.Rows[aRowIndex];
                for (int aCellIndex = 1; aCellIndex < aRow.Cells.Count; aCellIndex++)
                {
                    aRow.Cells[aCellIndex].Value = aList[aRowIndex - 1].Values[aCellIndex - 1];
                }
            }
        } 
        #endregion

        #region public methods
        public void Sort(int theIndex, SortBy theSortBy, Comparison<SortHelper> theCompareFunc)
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
        #endregion
    }
}
