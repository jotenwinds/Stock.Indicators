using System;

namespace Stooq.Data.Library;
public static class StooqQuoteExtensions
{
    // convert the period enum to its 'path' name
    public static string ToPeriodPathName(this Period period)
    {
        string periodPathName;
        switch (period)
        {
            case Period._5min:
                periodPathName = @"5 min";
                break;
            case Period._hourly:
                periodPathName = @"hourly";
                break;
            case Period._daily:
                periodPathName = @"daily";
                break;
            default:
                throw new InvalidOperationException($"Invalid period '{period}'.");
        }
        return periodPathName;
    }

    // convert the market enum to its 'path' name
    public static string ToMarketPathName(this Market market)
    {
        string marketPathName;
        switch (market)
        {
            case Market.USA:
                marketPathName = @"us";
                break;
            default:
                throw new InvalidOperationException($"Invalid market '{market}'.");
        }
        return marketPathName;
    }
}
