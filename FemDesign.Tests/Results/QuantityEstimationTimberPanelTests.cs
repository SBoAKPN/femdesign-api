﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using FemDesign.Results;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FemDesign.Results
{
    [TestClass()]
    public class QuantityEstimationTimberPanelTests
    {
        [TestMethod]
        public void Parse()
        {
        }

        [TestMethod]
        public void Identification()
        {
            var headers = new string[]
            {
                "Quantity estimation, Timber panel",
            };

            foreach (var header in headers)
            {
                var match = QuantityEstimationTimberPanel.IdentificationExpression.Match(header);
                Assert.IsTrue(match.Success, $"Should identify type of \"{header}\" as {typeof(QuantityEstimationTimberPanel).Name}");
                Assert.IsTrue(match.Groups["type"].Success);
                Assert.IsTrue(match.Groups["result"].Success);
            }
        }

        [TestMethod]
        public void Headers()
        {
            var headers = new string[]
            {
                "Quantity estimation, Timber panel",
                "Storey	Struct.	Identifier	Quality	Thickness	Panel type	Length	Width	Area	Weight	Pcs	Total weight",
                "	type			[mm]	[-]	[mm]	[mm]	[mm2]	[t]	[-]	[t]"
            };

            foreach (var header in headers)
            {
                var match = QuantityEstimationTimberPanel.HeaderExpression.Match(header);
                Assert.IsTrue(match.Success, $"Should identify \"{header}\" as header");
            }
        }
    }
}
