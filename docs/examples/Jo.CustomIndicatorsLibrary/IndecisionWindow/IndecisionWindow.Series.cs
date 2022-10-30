using System;
using System.Collections.Generic;
using Jo.CustomIndicatorsLibrary.Candles;
using Skender.Stock.Indicators;

namespace Jo.CustomIndicatorsLibrary.IndecisionWindow;


public static partial class CustomIndicators
{
    const double StrongBodySize = 0.65f;
    const double IndecisionBodySize = 0.15f;

    // Custom ATR WMA calculation
    public static List<JoCandleResult> GetIndecisionWindow<TQuote>(
        this IEnumerable<TQuote> quotes)
        where TQuote : IQuote
    {
        //int lookbackPeriods = 3;
        // initialize
        List<JoCandleResult> results = quotes.ToCandleResults();
        // maxPriceChangePercent /= 100;
        int length = results.Count;

        // Use a List as a 'queue' so we can have a 'sliding window' of 3 elements.
        var queue = new List<JoCandleProperties>();
        // roll through candles
        for (int i = 0; i < length; i++)
        {
            JoCandleResult r = results[i];

            switch (queue.Count)
            {
                case 0:
                    if (IsFirstCandleBullish(r))
                        queue.Add(r.Candle);
                    continue;
                case 1:
                    if (r.Candle.IsBullish &&
                        r.Candle.BodyPct >= StrongBodySize
                        && r.Candle.High > queue[0].High)
                    {
                        queue.Add(r.Candle);
                    }
                    else
                    {
                        queue.Clear();
                        if (IsFirstCandleBullish(r))
                            queue.Add(r.Candle);
                    }
                    continue;
                case 2:
                    // 3rd Candle can be either Bullish or Bearish, as long as it has a big body size and Higher High
                    if (r.Candle.BodyPct >= StrongBodySize && r.Candle.High > queue[1].High)
                    {
                        queue.Add(r.Candle);
                    }
                    else
                    {
                        queue.Clear();
                        if (IsFirstCandleBullish(r))
                            queue.Add(r.Candle);
                    }
                    continue;
                case 3:
                    // The last and 4th body is 'indecision', small body (less than 15 or 10%) and candle within previous candle
                    if (r.Candle.BodyPct <= IndecisionBodySize &&
                        r.Candle.High <= queue[2].High && r.Candle.Low >= queue[2].Low)
                    {
                        r.Match = Match.BearSignal;
                        queue.Clear();
                    }
                    else
                    {
                        queue.Clear();
                        if (IsFirstCandleBullish(r))
                            queue.Add(r.Candle);
                    }
                    continue;
                default:
                    throw new InvalidOperationException($"Invalid scenario for q.Count '{queue.Count}'.");
            }

        }
        return results;

    }

    private static bool IsFirstCandleBullish(JoCandleResult r) => r.Candle.IsBullish & r.Candle.BodyPct >= StrongBodySize;
}

