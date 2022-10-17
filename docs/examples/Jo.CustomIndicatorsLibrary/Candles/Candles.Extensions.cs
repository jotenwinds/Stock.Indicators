using System.Collections.Generic;
using System.Linq;

namespace Jo.CustomIndicatorsLibrary.Candles;

public static class CandlesExtensions
{
    public static JoCandleProperties ToCandle<TQuote>(
    this TQuote quote)
    where TQuote : Skender.Stock.Indicators.IQuote => new()
    {
        Date = quote.Date,
        Open = quote.Open,
        High = quote.High,
        Low = quote.Low,
        Close = quote.Close,
        Volume = quote.Volume
    };

    // convert/sort quotes into candle results
    public static List<JoCandleResult> ToCandleResults<TQuote>(
        this IEnumerable<TQuote> quotes)
        where TQuote : Skender.Stock.Indicators.IQuote
    {
        List<JoCandleResult> candlesList = quotes
            .Select(x => new JoCandleResult(x.Date)
            {
                Match = Skender.Stock.Indicators.Match.None,
                Candle = x.ToCandle()
            })
            .OrderBy(x => x.Date)
            .ToList();

        // validate
        return candlesList;
    }
}


