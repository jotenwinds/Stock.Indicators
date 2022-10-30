using System;
using Skender.Stock.Indicators;

namespace Jo.CustomIndicatorsLibrary.PPO;
public static partial class Indicator
{
    [Serializable]
    public sealed class PpoResult : ResultBase //, IReusableResult
    {
        public PpoResult(DateTime date)
        {
            Date = date;
        }

        // Percentage Price Oscillator (like MACD, but a % instead and its histogram)
        public double? Ppo { get; set; }
        public double? Signal { get; set; }
        public double? Histogram { get; set; }

        // extra interim data
        public double? FastEma { get; set; }
        public double? SlowEma { get; set; }

        //double? IReusableResult.Value => Ppo;
    }
}
