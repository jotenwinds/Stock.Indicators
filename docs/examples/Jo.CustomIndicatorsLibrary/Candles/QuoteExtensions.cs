using System.Collections.Generic;
using System.Linq;
using Skender.Stock.Indicators;

namespace Jo.CustomIndicatorsLibrary.Candles;

public static class QuoteExtensions
{
    internal static List<QuoteD> ToQuoteD<TQuote>(
    this IEnumerable<TQuote> quotes)
    where TQuote : IQuote => quotes
        .Select(x => new QuoteD
        {
            Date = x.Date,
            Open = (double)x.Open,
            High = (double)x.High,
            Low = (double)x.Low,
            Close = (double)x.Close,
            Volume = (double)x.Volume
        })
        .OrderBy(x => x.Date)
        .ToList();
}
