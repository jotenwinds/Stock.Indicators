using System;
using System.Collections.Generic;
using System.Globalization;
using Jo.Tests.Indicators.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skender.Stock.Indicators;


[assembly: CLSCompliant(true)]
//[assembly: InternalsVisibleTo("Tests.Other")]
//[assembly: InternalsVisibleTo("Tests.Performance")]

namespace Jo.Tests.Indicators;
// GLOBALS & INITIALIZATION OF TEST DATA

[TestClass]
public abstract class TestBase
{
    internal static readonly CultureInfo EnglishCulture = new("en-US", false);

    internal static readonly IEnumerable<IQuote> DailyINTCquotes = TestData.GetDailyINTC();
}
