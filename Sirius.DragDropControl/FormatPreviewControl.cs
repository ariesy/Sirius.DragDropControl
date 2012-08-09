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

        public FormatPreviewControl(Parameters theParameters)
        {
            InitializeComponent();
            InitializeInnerControl(theParameters);
            Controls.Add(innerControl);
        }

        private void InitializeInnerControl(Parameters theParameters)
        {
            DataTable aDataTable = new DataTable();
            int aColumnCount = theParameters.RowLabels.Aggregate(1, (current, aLabel) => current * theParameters.Data[aLabel].Count);
            aColumnCount += theParameters.ColumnLabels.Count;

            var aColumnHeaders = CreateColumnHeaders(aColumnCount);
            foreach (var aHeader in aColumnHeaders)
            {
                aDataTable.Columns.Add(aHeader);
            }

            List<List<object>> aRowLabelsData = CreateHeadersData(theParameters, theParameters.RowLabels);

            for (int aI = 0; aI < theParameters.RowLabels.Count; aI ++)
            {
                List<object> aRow = new List<object>();
                for (int aCount = 0; aCount < theParameters.ColumnLabels.Count; aCount++)
                {
                    aRow.Add(string.Empty);
                }

                foreach (var aSubList in aRowLabelsData)
                {
                    aRow.Add(aSubList[aI]);
                }

                aDataTable.Rows.Add(aRow.ToArray());
            }

            List<List<object>> aColumnLabelsData = CreateHeadersData(theParameters, theParameters.ColumnLabels);
            foreach (var aSubList in aColumnLabelsData)
            {
                aDataTable.Rows.Add(aSubList.ToArray());
            }
            
            innerControl = new DragDropUserControl(aDataTable);
        }

        private List<List<object>> CreateHeadersData(Parameters theParameters, IEnumerable<string> theLabelList)
        {
            var aResult = new List<List<object>>();
            var aLabelList = theLabelList.Reverse();
            
            return aLabelList.Aggregate(aResult, (current, aLabel) => CreateOne(theParameters.Data[aLabel], current));
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
