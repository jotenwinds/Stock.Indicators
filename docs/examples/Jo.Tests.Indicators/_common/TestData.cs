using System.Collections.Generic;
using System.IO;
using System.Linq;
using Skender.Stock.Indicators;

namespace Jo.Tests.Indicators.Common;
internal class TestData
{
    // Daily data of Intel Corporation (INTC).
    internal static IEnumerable<IQuote> GetDailyINTC(int days = 800)
        => File.ReadAllLines("_common/data/intc.us.txt")
            .Skip(1)
            .Select(v => Importer.QuoteFromCsv(v))
            .OrderByDescending(x => x.Date)
            .Take(days)
            .ToList();
}
