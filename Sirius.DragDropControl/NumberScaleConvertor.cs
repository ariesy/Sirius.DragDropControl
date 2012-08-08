// -----------------------------------------------------------------------
// <copyright file="NumberConvertor.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Sirius.DragDropControl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
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
