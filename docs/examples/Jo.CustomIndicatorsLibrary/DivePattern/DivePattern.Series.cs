using System;
using System.Collections.Generic;
using System.Linq;
using Jo.CustomIndicatorsLibrary.PPO;
using Skender.Stock.Indicators;

namespace Jo.CustomIndicatorsLibrary.DivePattern;

public static partial class DivePattern
{

    public static List<DivePatternResult> GetDivePattern<TQuote>(
        this IEnumerable<TQuote> q,
        int adxLength = 14,
        int adxThreshold = 21,
        int ppoFastLength = 13,
        int ppoSlowLength = 21,
        int ppoSignalSmoothingLength = 8)
    where TQuote : IQuote
    {
        // check parameter arguments
        ValidateDivePattern(adxLength, adxThreshold, ppoFastLength, ppoSlowLength, ppoSignalSmoothingLength);

        // Make sure quotes are ordered by Date.
        var quotes = q.OrderBy(x => x.Date).ToList();
        var adxResults = quotes.GetAdx(adxLength).ToList();
        var ppoResults = quotes.GetPpo(ppoFastLength, ppoSlowLength, ppoSignalSmoothingLength).ToList();

        // Initialize
        int length = adxResults.Count;
        List<DivePatternResult> results = new(length);

        // Roll through quotes
        for (int i = 0; i < length; i++)
        {
            // Result for the current date
            var r = new DivePatternResult(adxResults[i].Date)
            {
                Match = Match.None,
                Pdi = adxResults[i].Pdi,
                Mdi = adxResults[i].Mdi,
                Adx = adxResults[i].Adx,
                Adxr = adxResults[i].Adxr,
                Ppo = ppoResults[i].Ppo,
                Signal = ppoResults[i].Signal,
                Histogram = ppoResults[i].Histogram
            };
            results.Add(r);

            // The series of quotes is only "valid" once we reach the slowlength
            if (i < ppoSlowLength - 1)
                continue;

            var prvDate = results[i - 1]; // previous date

            // Has any di cross over the other di?
            r.HasPdiCrossMdiAbove = prvDate.IsMdiAbovePdi && r.IsPdiAboveMdi;
            r.HasMdiCrossPdiAbove = prvDate.IsPdiAboveMdi && r.IsMdiAbovePdi;

            // Has PPO and Signal crossover?
            r.HasPpoCrossSignalAbove = prvDate.IsPpoBullish && r.IsPpoBearish;
            r.HasSignalCrossPpoAbove = prvDate.IsPpoBearish && r.IsPpoBullish;

            if (r.HasMdiCrossPdiAbove && r.IsPpoBearish)
            {
                r.Match = Match.BearConfirmed;
            } else if (r.HasMdiCrossPdiAbove || r.IsPpoBearish)
            {
                r.Match = Match.BearSignal;
            }
        }
        return results;
    }

    // parameter validation
    private static void ValidateDivePattern(
        int adxLength,
        int adxThreshold,
        int ppoFastLength,
        int ppoSlowLength,
        int ppoSignalSmoothingLength)
    {
        // check parameter arguments
        if (adxLength < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(adxLength), adxLength,
                "ADX Length must be 2 or greater for DivePattern.");
        }

        if (adxThreshold < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(adxThreshold), adxThreshold,
                "Adx Threshold must be 2 or greater for DivePattern.");
        }

        if (ppoFastLength < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(ppoFastLength), ppoFastLength,
                "PPO Fast Length must be 2 or greater for DivePattern.");
        }

        if (ppoSignalSmoothingLength < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(ppoSignalSmoothingLength), ppoSignalSmoothingLength,
                "PPO Signal Smoothing Length must be greater than or equal to 2 for DivePattern.");
        }

        // At the minimum slowLength must be >= 3.
        if (ppoSlowLength <= ppoFastLength)
        {
            throw new ArgumentOutOfRangeException(nameof(ppoSlowLength), ppoSlowLength,
                $"PPO Slow Length must be greater than the Fast Length ('{ppoFastLength}') for DivePattern.");
        }

    }
}
