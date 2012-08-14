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
    public partial class FormatPreviewControl : UserControl
    {
        #region NestedClasses

        public class ReOrderEventArgs : EventArgs
        {
            public IList<string> RowLabels { get; private set; }

            public IList<string> ColumnLabels { get; private set; }

            public ReOrderEventArgs()
            {
                RowLabels = new List<string>();
                ColumnLabels = new List<string>();
            }

            public ReOrderEventArgs(IList<string> theRowLabels, IList<string> theColumnLabels)
            {
                RowLabels = theRowLabels;
                ColumnLabels = theColumnLabels;
            }
        }

        public enum SortOn
        {
            RowLabels,
            ColumnLabels
        }

        public enum SortOrder
        {
            Asc,
            Desc
        }

        public class Parameters
        {
            public List<string> RowLabels { get; private set; }

            public List<string> ColumnLabels { get; private set; }

            public Dictionary<string, List<object>> Data { get; private set; }

            public Parameters()
            {
                RowLabels = new List<string>();
                ColumnLabels = new List<string>();
                Data = new Dictionary<string, List<object>>();
            }
        } 
        #endregion

        private const int TOTAL_ROW = 17;

        private int activeRowCount;
        
        private DragDropUserControl innerControl;

        private Parameters parameters;

        private Dictionary<int, string> rowLabels;

        private Dictionary<int, string> columnLabels;

        public event Action<object, ReOrderEventArgs> ReOrdered;

        public FormatPreviewControl()
        {
            InitializeComponent();
            rowLabels = new Dictionary<int, string>();
            columnLabels = new Dictionary<int, string>();
        }

        public void LoadData(Parameters theParameters)
        {
            parameters = theParameters;
            Controls.Remove(innerControl);
            InitializeInnerControl();
            for (int aRowIdx = 1; aRowIdx <= parameters.RowLabels.Count; aRowIdx++)
            {
                rowLabels[aRowIdx] = parameters.RowLabels[aRowIdx - 1];
            }

            for (int aColumnIdx = 1; aColumnIdx <= parameters.ColumnLabels.Count; aColumnIdx++)
            {
                columnLabels[aColumnIdx] = parameters.ColumnLabels[aColumnIdx - 1];
            }
             
            Controls.Add(innerControl);
        }

        public void Sort(SortOn theSortOn, string theLabel, SortOrder theOrder)
        {
            DragDropUserControl.SortBy aSortBy;
            int aSortIndex;
            if (theSortOn == SortOn.ColumnLabels)
            {
                aSortBy = DragDropUserControl.SortBy.ByColumn;
                aSortIndex = parameters.ColumnLabels.IndexOf(theLabel) + 1;
            }
            else
            {
                aSortBy = DragDropUserControl.SortBy.ByRow;
                aSortIndex = parameters.RowLabels.IndexOf(theLabel) + 1;
            }

            innerControl.Sort(
                aSortIndex,
                aSortBy,
                (aHelper1, aHelper2) =>
                    {
                        var aItem1 = aHelper1.SortObj;
                        var aItem2 = aHelper2.SortObj;

                        if (string.IsNullOrWhiteSpace(aItem1.ToString()) && string.IsNullOrWhiteSpace(aItem2.ToString()))
                        {
                            return aHelper1.OriginalPosition - aHelper2.OriginalPosition;
                        }

                        if (string.IsNullOrWhiteSpace(aItem1.ToString()))
                        {
                            if (IsAllValueBlank(aHelper1.Values))
                            {
                                return 1;
                            }

                            return -1;
                        }

                        if (string.IsNullOrWhiteSpace(aItem2.ToString()))
                        {
                            if (IsAllValueBlank(aHelper2.Values))
                            {
                                return -1;
                            }

                            return 1;
                        }

                        if (aItem1.Equals(aItem2))
                        {
                            return aHelper1.OriginalPosition - aHelper2.OriginalPosition;
                        }

                        if (theOrder == SortOrder.Asc)
                        {
                            return aItem1.ToString().CompareTo(aItem2.ToString());
                        }
                        else
                        {
                            return aItem2.ToString().CompareTo(aItem1.ToString());
                        }
                    });
        }

        public string[,] GetOutPut()
        {
            //var aWedth = innerControl.InnerDataGridView.Columns.Count - parameters.ColumnLabels.Count - 1;
            //var aHeight = activeRowCount - 1 - parameters.RowLabels.Count;
            var aWedth = innerControl.InnerDataGridView.Columns.Count - 1;
            var aHeight = activeRowCount - 1;
            var aResult = new string[aHeight, aWedth];
            for (int aI = 0; aI < aHeight; aI++)
            {
                for (int aJ = 0; aJ < aWedth; aJ++)
                {
                    //int aRowIdx = aI + parameters.RowLabels.Count + 1;
                    //int aColumnIdx = aJ + parameters.ColumnLabels.Count + 1;
                    int aRowIdx = aI + 1;
                    int aColumnIdx = aJ  + 1;

                    List<string> aValueContainer = new List<string>();
                    for (int aColumnLabelIdx = 1; aColumnLabelIdx < parameters.ColumnLabels.Count + 1; aColumnLabelIdx++)
                    {
                        aValueContainer.Add(innerControl.InnerDataGridView.Rows[aRowIdx].Cells[aColumnLabelIdx].Value.ToString());
                    }

                    for (int aRowLabelIdx = 1; aRowLabelIdx < parameters.RowLabels.Count + 1; aRowLabelIdx++)
                    {
                        aValueContainer.Add(innerControl.InnerDataGridView.Rows[aRowLabelIdx].Cells[aColumnIdx].Value.ToString());
                    }

                    aResult[aI, aJ] = string.Join(",", aValueContainer.ToArray());
                }
            }

            return aResult;
        }

        #region private methods
        private bool IsAllValueBlank(List<object> theList)
        {
            bool aResult = true;
            foreach (var aItem in theList)
            {
                if (aItem != null && !string.IsNullOrWhiteSpace(aItem.ToString()))
                {
                    aResult = false;
                }
            }

            return aResult;
        }

        private void InitializeInnerControl()
        {
            DataTable aDataTable = new DataTable();
            int aColumnCount = parameters.RowLabels.Aggregate(1, (current, aLabel) => current * parameters.Data[aLabel].Count);
            aColumnCount += parameters.ColumnLabels.Count;

            var aColumnHeaders = CreateColumnHeaders(aColumnCount);
            foreach (var aHeader in aColumnHeaders)
            {
                if (!aDataTable.Columns.Contains(aHeader))
                {
                aDataTable.Columns.Add(aHeader);
            }
            }

            List<List<object>> aRowLabelsData = CreateHeadersData(parameters.RowLabels);

            for (int aI = 0; aI < parameters.RowLabels.Count; aI++)
            {
                List<object> aRow = new List<object>();
                for (int aCount = 0; aCount < parameters.ColumnLabels.Count; aCount++)
                {
                    aRow.Add(string.Empty);
                }

                foreach (var aSubList in aRowLabelsData)
                {
                    aRow.Add(aSubList[aI]);
                }

                aDataTable.Rows.Add(aRow.ToArray());
            }

            List<List<object>> aColumnLabelsData = CreateHeadersData(parameters.ColumnLabels);
            foreach (var aSubList in aColumnLabelsData)
            {
                aDataTable.Rows.Add(aSubList.ToArray());
            }

            activeRowCount = aDataTable.Rows.Count;
            for (int aI = activeRowCount + 1; aI < TOTAL_ROW; aI++)
            {
                aDataTable.Rows.Add(new List<object>().ToArray());
            }

            innerControl = new DragDropUserControl(aDataTable);
            innerControl.OnDrop += InnerControlOnDrop;
            ChangeInerControlStyle();
        }

        private void ChangeInerControlStyle()
        {
            var aFont = innerControl.InnerDataGridView.DefaultCellStyle.Font;
            innerControl.InnerDataGridView.DefaultCellStyle.Font = new Font(aFont, FontStyle.Bold);
            innerControl.InnerDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            innerControl.InnerDataGridView.GridColor = ColorTranslator.FromHtml("#D0D7E5");

            innerControl.InnerDataGridView.Size = new Size { Height = 340, Width = 662 };
            innerControl.Size = new Size { Height = 400, Width = 668 };
            innerControl.InnerDataGridView.Left = innerControl.InnerDataGridView.Left + 3;

            DataGridViewCell aFirstCell = innerControl.InnerDataGridView.Rows[0].Cells[0];
            var aStyle = new DataGridViewCellStyle(aFirstCell.InheritedStyle);
            aStyle.BackColor = ColorTranslator.FromHtml("#A9C4E9");
            aStyle.SelectionBackColor = aStyle.BackColor;
            aFirstCell.Style = aStyle;

            innerControl.InnerDataGridView.Columns[0].Width = 25;
            for (int aI = 1; aI < innerControl.InnerDataGridView.Columns.Count; aI++)
            {
                var aCell = innerControl.InnerDataGridView.Rows[0].Cells[aI];
                var aCellStype = new DataGridViewCellStyle(aCell.InheritedStyle);
                aCellStype.BackColor = ColorTranslator.FromHtml("#DFE6EF");
                aCellStype.SelectionBackColor = aCellStype.BackColor;
                aCellStype.SelectionForeColor = aCellStype.ForeColor;
                aCellStype.Font = new Font(aCellStype.Font, FontStyle.Regular);
                aCell.Style = aCellStype;

                innerControl.InnerDataGridView.Columns[aI].Width = 100;
            }

            innerControl.InnerDataGridView.Rows[0].Height = 20;
            for (int aI = 1; aI < innerControl.InnerDataGridView.Rows.Count; aI++)
            {
                var aCell = innerControl.InnerDataGridView.Rows[aI].Cells[0];
                var aCellStype = new DataGridViewCellStyle(aCell.InheritedStyle);
                aCellStype.Font = new Font(aCellStype.Font, FontStyle.Regular);
                aCellStype.BackColor = ColorTranslator.FromHtml("#E4ECF7");
                aCellStype.SelectionBackColor = aCellStype.BackColor;
                aCellStype.SelectionForeColor = aCellStype.ForeColor;
                aCell.Style = aCellStype;

                innerControl.InnerDataGridView.Rows[aI].Height = 20;
            }
        }

        private void InnerControlOnDrop(object theSender, DragDropUserControl.DragDropEventArgs theE)
        {
            var aSender = theSender as DragDropUserControl;
            if (aSender == null)
            {
                return;
            }

            var aRowIndexFromMouseDown = theE.RowIndexFromMouseDown;
            var aRowIndexOfItemUnderMouseToDrop = theE.RowIndexOfItemUnderMouseToDrop;
            var aColumnIndexFromMouseDown = theE.ColumnIndexFromMouseDown;
            var aColumnIndexOfItemUnderMouseToDrop = theE.ColumnIndexOfItemUnderMouseToDrop;
            if (aRowIndexFromMouseDown == 0 || aRowIndexOfItemUnderMouseToDrop == 0 || aColumnIndexFromMouseDown == 0 || aColumnIndexOfItemUnderMouseToDrop == 0)
            {
                aSender.AllowThisOneColumnDragDrop = false;
                aSender.AllowThisOneRowDragDrop = false;
            }

            if (aRowIndexFromMouseDown <= parameters.RowLabels.Count && aRowIndexOfItemUnderMouseToDrop > parameters.RowLabels.Count)
            {
                aSender.AllowThisOneRowDragDrop = false;
            }

            if (aRowIndexFromMouseDown > parameters.RowLabels.Count && aRowIndexOfItemUnderMouseToDrop <= parameters.RowLabels.Count)
            {
                aSender.AllowThisOneRowDragDrop = false;
            }

            if (aColumnIndexFromMouseDown <= parameters.ColumnLabels.Count && aColumnIndexOfItemUnderMouseToDrop > parameters.ColumnLabels.Count)
            {
                aSender.AllowThisOneColumnDragDrop = false;
            }

            if (aColumnIndexFromMouseDown > parameters.ColumnLabels.Count && aColumnIndexOfItemUnderMouseToDrop <= parameters.ColumnLabels.Count)
            {
                aSender.AllowThisOneColumnDragDrop = false;
            }

            if (aRowIndexFromMouseDown > activeRowCount || aRowIndexOfItemUnderMouseToDrop > activeRowCount)
            {
                aSender.AllowThisOneRowDragDrop = false;
            }

            if (aRowIndexFromMouseDown == aRowIndexOfItemUnderMouseToDrop)
            {
                aSender.AllowThisOneRowDragDrop = false;
            }

            if (aColumnIndexFromMouseDown == aColumnIndexOfItemUnderMouseToDrop)
            {
                aSender.AllowThisOneColumnDragDrop = false;
            }

            if (ReOrdered != null)
            {
                TriggerReOrder(aRowIndexFromMouseDown, aRowIndexOfItemUnderMouseToDrop, aColumnIndexFromMouseDown, aColumnIndexOfItemUnderMouseToDrop, aSender);
            }
        }

        private void TriggerReOrder(int theRowIndexFromMouseDown, int theRowIndexOfItemUnderMouseToDrop, int theColumnIndexFromMouseDown, int theColumnIndexOfItemUnderMouseToDrop, DragDropUserControl theSender)
        {
            if (theSender.AllowThisOneRowDragDrop && theRowIndexFromMouseDown <= parameters.RowLabels.Count &&
                theRowIndexOfItemUnderMouseToDrop <= parameters.RowLabels.Count)
            {
                if (theRowIndexFromMouseDown > theRowIndexOfItemUnderMouseToDrop)
                {
                    var aSwapValue = rowLabels[theRowIndexFromMouseDown];
                    int aI = theRowIndexFromMouseDown;
                    for (; aI > theRowIndexOfItemUnderMouseToDrop; aI--)
                    {
                        rowLabels[aI] = rowLabels[aI - 1];
                    }

                    rowLabels[aI] = aSwapValue;
                }
                else
                {
                    var aSwapValue = rowLabels[theRowIndexFromMouseDown];
                    int aI = theRowIndexFromMouseDown;
                    for (; aI < theRowIndexOfItemUnderMouseToDrop; aI++)
                    {
                        rowLabels[aI] = rowLabels[aI + 1];
                    }

                    rowLabels[aI] = aSwapValue;
                }

                ReOrderEventArgs aE = new ReOrderEventArgs(rowLabels.Values.ToList(), columnLabels.Values.ToList());
                ReOrdered(this, aE);
            }
            else if (theSender.AllowThisOneColumnDragDrop && theColumnIndexFromMouseDown <= parameters.ColumnLabels.Count &&
                     theColumnIndexOfItemUnderMouseToDrop <= parameters.ColumnLabels.Count)
            {
                if (theColumnIndexFromMouseDown > theColumnIndexOfItemUnderMouseToDrop)
                {
                    var aSwapValue = rowLabels[theColumnIndexFromMouseDown];
                    int aI = theColumnIndexFromMouseDown;
                    for (; aI > theRowIndexOfItemUnderMouseToDrop; aI--)
                    {
                        rowLabels[aI] = rowLabels[aI - 1];
                    }

                    rowLabels[aI] = aSwapValue;
                }
                else
                {
                    var aSwapValue = rowLabels[theColumnIndexFromMouseDown];
                    int aI = theColumnIndexFromMouseDown;
                    for (; aI < theColumnIndexOfItemUnderMouseToDrop; aI++)
                    {
                        rowLabels[aI] = rowLabels[aI + 1];
                    }

                    rowLabels[aI] = aSwapValue;
                }

                ReOrderEventArgs aE = new ReOrderEventArgs(rowLabels.Values.ToList(), columnLabels.Values.ToList());
                ReOrdered(this, aE);
            }
        }

        private List<List<object>> CreateHeadersData(IEnumerable<string> theLabelList)
        {
            var aResult = new List<List<object>>();
            var aLabelList = theLabelList.Reverse();

            return aLabelList.Aggregate(aResult, (current, aLabel) => CreateOne(parameters.Data[aLabel], current));
        }

        private List<List<object>> CreateOne(List<object> theData, List<List<object>> theLastList)
        {
            if (theLastList == null)
            {
                theLastList = new List<List<object>>();
            }

            var aNewList = new List<List<object>>();
            foreach (var aItem in theData)
            {
                if (theLastList.Count == 0)
                {
                    var aNewSubList = new List<object>();
                    aNewSubList.Add(aItem);
                    aNewList.Add(aNewSubList);
                }
                else
                {
                    foreach (var aSubList in theLastList)
                    {
                        var aNewSubList = new List<object>();
                        aNewSubList.Add(aItem);
                        aNewSubList.AddRange(aSubList);
                        aNewList.Add(aNewSubList);
                    }
                }
            }

            return aNewList;
        }

        private List<string> CreateColumnHeaders(int theCount)
        {
            List<string> aAlphabet = new List<string>();
            List<string> aResult = new List<string>();

            //aAlphabet.Add(string.Empty);
            for (int aI = 0; aI < 26; aI++)
            {
                aAlphabet.Add(char.ConvertFromUtf32(65 + aI));
            }

            for (int aI = 0; aI < theCount; aI++)
            {
                var a26ScaleNumber = aI.ToNumber(26);
                List<string> aHeader = new List<string>();
                aResult.Add(string.Join(string.Empty, a26ScaleNumber.Select(aBit => aAlphabet[aBit]).ToArray()));
            }

            return aResult;
        } 
        #endregion
    }

    public static class NumberScaleConvertor
    {
        public static List<int> ToNumber(this int theDecimal, int theScale)
        {
            if (theDecimal < 0)
            {
                throw new NotSupportedException();
            }

            var aDecimal = theDecimal;
            var aResult = new Stack<int>();
            while (aDecimal >= theScale)
            {
                aResult.Push(aDecimal % theScale);
                aDecimal = aDecimal / theScale;
            }

            aResult.Push(aDecimal);

            return aResult.ToList();
        }
    }
}
