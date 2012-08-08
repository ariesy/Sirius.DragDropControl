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

            object[] columnLabelsData = CreateColumnLabelsData(theParameters);
            foreach (var aRowLabel in theParameters.RowLabels)
            {
                for (int aI = 0; aI < theParameters.ColumnLabels.Count; aI++)
                {
                    List<object> aRow = new List<object>();
                    aRow.Add(string.Empty);
                    foreach (var aRowData in theParameters.Data[aRowLabel])
                    {
                        int aCount = theParameters.Data[aRowLabel].Count;
                        for (int i = 0; i < aColumnCount / aCount; i++)
                        {
                            aRow.Add(aRowData);
                        }
                    }

                    aDataTable.Rows.Add(aRow.ToArray());
                }
            }

            foreach (var aColumnLabel in theParameters.ColumnLabels)
            {
                List<object> aRow = new List<object>();
            }
            
            innerControl = new DragDropUserControl(aDataTable);
        }

        private object[] CreateColumnLabelsData(Parameters theParameters)
        {
            throw new NotImplementedException();
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
}
