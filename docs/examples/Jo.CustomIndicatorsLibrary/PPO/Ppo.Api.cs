using System.Collections.Generic;
using Skender.Stock.Indicators;

namespace Jo.CustomIndicatorsLibrary.PPO;
public static partial class Indicator
{
    // SERIES, from TQuote
    public static IEnumerable<PpoResult> GetPpo<TQuote>(
        this IEnumerable<TQuote> quotes,
        int fastLength = 13,
        int slowLength = 21,
        int signalSmoothingLength = 8)
        where TQuote : IQuote => quotes
            .ToBasicTuple(CandlePart.Close)
            .CalcPpo(fastLength, slowLength, signalSmoothingLength);
}
