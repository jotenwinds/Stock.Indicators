using System;
using System.Collections.Generic;
using System.Linq;
using Skender.Stock.Indicators;

namespace Jo.CustomIndicatorsLibrary.PPO;
public static partial class Indicator
{
    internal static List<PpoResult> CalcPpo(
    this List<(DateTime, double)> tpList,
    int fastLength,
    int slowLength,
    int signalSmoothingLength)
    {
        // check parameter arguments
        ValidatePpo(fastLength, slowLength, signalSmoothingLength);

        // initialize
        List<EmaResult> emaFast = tpList.GetEma(fastLength).ToList();
        List<EmaResult> emaSlow = tpList.GetEma(slowLength).ToList();

        int length = tpList.Count;
        List<(DateTime, double)> emaDiff = new();
        List<PpoResult> results = new(length);

        // roll through quotes
        for (int i = 0; i < length; i++)
        {
            (DateTime date, double _) = tpList[i];
            EmaResult df = emaFast[i];
            EmaResult ds = emaSlow[i];

            PpoResult r = new(date)
            {
                FastEma = df.Ema,
                SlowEma = ds.Ema
            };
            results.Add(r);

            if (i >= slowLength - 1)
            {
                double ppo = (100D * (df.Ema - ds.Ema) / ds.Ema).Null2NaN();
                r.Ppo = ppo.NaN2Null();

                // temp data for interim EMA of PPO
                (DateTime, double) diff = (date, ppo);

                emaDiff.Add(diff);
            }
        }

        // add signal and histogram to result
        List<EmaResult> emaSignal = emaDiff.GetEma(signalSmoothingLength).ToList();

        for (int d = slowLength - 1; d < length; d++)
        {
            PpoResult r = results[d];
            EmaResult ds = emaSignal[d + 1 - slowLength];

            r.Signal = ds.Ema.NaN2Null();
            r.Histogram = (r.Ppo - r.Signal).NaN2Null();
        }

        return results;
    }

    // parameter validation
    private static void ValidatePpo(
        int fastPeriods,
        int slowPeriods,
        int signalPeriods)
    {
        // check parameter arguments
        if (fastPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fastPeriods), fastPeriods,
                "Fast periods must be greater than 0 for PPO.");
        }

        if (signalPeriods < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(signalPeriods), signalPeriods,
                "Signal periods must be greater than or equal to 0 for PPO.");
        }

        if (slowPeriods <= fastPeriods)
        {
            throw new ArgumentOutOfRangeException(nameof(slowPeriods), slowPeriods,
                "Slow periods must be greater than the fast period for PPO.");
        }
    }
}
