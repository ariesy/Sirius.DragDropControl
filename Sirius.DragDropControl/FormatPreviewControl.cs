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
        }

        private void InitializeInnerControl(Parameters theParameters)
        {
            DataTable aDataTable = new DataTable();
            int aColumnCount = 1;
            for (int aI = 0; aI < theParameters.RowLabels.Count; aI++)
            {
                var aLabel = theParameters.RowLabels[aI];
                aColumnCount *= theParameters.Data[aLabel].Count;
            }

            var aColumnHeaders = CreateColumnHeaders(theParameters.Data[theParameters.RowLabels[0]].Count);
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
                for (int aIndex = 0; aIndex < a26ScaleNumber.Count; aIndex++)
                {
                    aHeader.Add(aAlphabet[aI]);
                }

                aResult.Add(string.Join(string.Empty, aHeader.ToArray()));
            }

            return aResult;
        }
    }
}
