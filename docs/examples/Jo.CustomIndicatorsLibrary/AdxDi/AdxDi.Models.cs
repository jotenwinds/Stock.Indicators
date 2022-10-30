using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jo.CustomIndicatorsLibrary.Candles;
using Skender.Stock.Indicators;

namespace Jo.CustomIndicatorsLibrary.AdxDi;

public static partial class QuoteUtility
{
    // DOUBLE QUOTES

    // convert to quotes in double precision
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

[Serializable]
public sealed class AdxDiResult : ResultBase //, IReusableResult
{
    public AdxDiResult(DateTime date)
    {
        Date = date;
    }

    public double? Pdi { get; set; }
    public double? Mdi { get; set; }
    public double? Adx { get; set; }
    public double? Adxr { get; set; }

    //double? IReusableResult.Value => Adx;
}
