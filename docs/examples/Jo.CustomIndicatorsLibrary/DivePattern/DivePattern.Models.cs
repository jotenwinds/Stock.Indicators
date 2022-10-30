using System;
using System.Runtime.InteropServices;
using Skender.Stock.Indicators;

namespace Jo.CustomIndicatorsLibrary.DivePattern;
[Serializable]
public sealed class DivePatternResult : ResultBase
{
    public DivePatternResult(DateTime date)
    {
        Date = date;
    }

    // 
    public Match Match { get; set; }

    // ADX
    public double? Pdi { get; set; }    // DI+
    public double? Mdi { get; set; }    // DI-
    public double? Adx { get; set; }    // ADX
    public double? Adxr { get; set; }
    public bool HasPdiCrossMdiAbove { get; set; }
    public bool HasMdiCrossPdiAbove { get; set; }

    // Percentage Price Oscillator (like MACD, but a % instead and its histogram)
    public double? Ppo { get; set; }    // PPO (% of Price Signal)
    public double? Signal { get; set; } // Signal
    public double? Histogram { get; set; }  // % Histogram
    public bool HasPpoCrossSignalAbove { get; set; }
    public bool HasSignalCrossPpoAbove { get; set; }


    // ADC - Directional info
    public bool IsPdiAboveMdi => Pdi > Mdi;
    public bool IsMdiAbovePdi => Mdi > Pdi;

    // PPO - trend info
    public bool IsPpoBearish => Signal > Ppo;
    public bool IsPpoBullish => Ppo > Signal;

}
