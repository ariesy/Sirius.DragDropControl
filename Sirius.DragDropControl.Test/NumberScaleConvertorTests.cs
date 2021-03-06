﻿// -----------------------------------------------------------------------
// <copyright file="NumberScaleConvertorTests.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using NUnit.Framework;
using Sirius.DragDropControl;

namespace Sirius.DragDropControl.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class NumberScaleConvertorTests
    {
        [Test]
        public void TestToNumber()
        {
            List<string> aAlphabet = new List<string>();
            aAlphabet.Add(string.Empty);
            for (int aI = 0; aI < 26; aI++)
            {
                aAlphabet.Add(char.ConvertFromUtf32(65 + aI));
            }

            int aNumber = 26;
            var aResult = aNumber.ToNumber(26);
            Assert.AreEqual("AA", string.Join(string.Empty, aResult.Select(aBit => aAlphabet[aBit]).ToArray()));

            int aNumber2 = 25;
            var aResult2 = aNumber2.ToNumber(26);
            Assert.AreEqual("Z", string.Join(string.Empty, aResult2.Select(aBit => aAlphabet[aBit]).ToArray()));

            int aNumber3 = 0;
            var aResult3 = aNumber3.ToNumber(26);
            Assert.AreEqual("A", string.Join(string.Empty, aResult3.Select(aBit => aAlphabet[aBit]).ToArray()));

            int aNumber4 = 27;
            var aResult4 = aNumber4.ToNumber(26);
            Assert.AreEqual("AB", string.Join(string.Empty, aResult4.Select(aBit => aAlphabet[aBit]).ToArray()));

            int aNumber5 = 52;
            var aResult5 = aNumber5.ToNumber(26);
            Assert.AreEqual("AZ", string.Join(string.Empty, aResult5.Select(aBit => aAlphabet[aBit]).ToArray()));

            //int aNumber2 = 26 * 26 - 1;
            //var aResult2 = aNumber2.ToNumber(26);
            //Assert.AreEqual(0, aResult[0]);
            //Assert.AreEqual(0, aResult[1]);
            //Assert.AreEqual(0, aResult[2]);
        }
    }
}
