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
        
        private DragDropUserControl innerControl;

        private Parameters parameters;

        public FormatPreviewControl(Parameters theParameters)
        {
            parameters = theParameters;
            InitializeComponent();
            InitializeInnerControl();
            Controls.Add(innerControl);
        }

        public void Refresh(Parameters theParameters)
        {
            parameters = theParameters;
            Controls.Remove(innerControl);
            InitializeInnerControl();
            Controls.Add(innerControl);
        }

        public void Sort(SortOn theSortOn, string theLabel, SortOrder theOrder)
        {
            DragDropUserControl.SortBy aSortBy;
            int aSortIndex;
            if (theSortOn == SortOn.ColumnLabels)
            {
                aSortBy = DragDropUserControl.SortBy.ByColumn;
                aSortIndex = parameters.ColumnLabels.IndexOf(theLabel);
            }
            else
            {
                aSortBy = DragDropUserControl.SortBy.ByRow;
                aSortIndex = parameters.RowLabels.IndexOf(theLabel);
            }

            innerControl.Sort(
                aSortIndex,
                aSortBy,
                (aItem1, aItem2) =>
                    {
                        if (aItem1 == null && aItem2 == null)
                        {
                            return 0;
                        }

                        if (aItem1 == null)
                        {
                            return -1;
                        }

                        if (aItem2 == null)
                        {
                            return 1;
                        }

                        if (string.IsNullOrWhiteSpace(aItem1.ToString()) && string.IsNullOrWhiteSpace(aItem2.ToString()))
                        {
                            return 0;
                        }

                        if (string.IsNullOrWhiteSpace(aItem1.ToString()))
                        {
                            return -1;
                        }

                        if (string.IsNullOrWhiteSpace(aItem2.ToString()))
                        {
                            return 1;
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

        #region private methods
        private void InitializeInnerControl()
        {
            DataTable aDataTable = new DataTable();
            int aColumnCount = parameters.RowLabels.Aggregate(1, (current, aLabel) => current * parameters.Data[aLabel].Count);
            aColumnCount += parameters.ColumnLabels.Count;

            var aColumnHeaders = CreateColumnHeaders(aColumnCount);
            foreach (var aHeader in aColumnHeaders)
            {
                aDataTable.Columns.Add(aHeader);
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

            innerControl = new DragDropUserControl(aDataTable);
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
            var aResult = new Stack<int>();
            while (theDecimal > theScale)
            {
                aResult.Push(theDecimal % theScale);
                theDecimal = theDecimal / theScale;
            }

            aResult.Push(theDecimal);

            return aResult.ToList();
        }
    }
}
