using System;
using System.Globalization;
using Skender.Stock.Indicators;

namespace Jo.Tests.Indicators.Common;

// TEST QUOTE IMPORTER
internal static class Importer
{
    private static readonly CultureInfo EnglishCulture = new("en-US", false);

    // importer / parser
    internal static IQuote QuoteFromCsv(string csvLine)
    {
        if (string.IsNullOrEmpty(csvLine))
        {
            return new Quote();
        }

        string[] values = csvLine.Split(',');
        Quote quote = new();

        // <TICKER>,<PER>,<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOL>,<OPENINT>
        //         ,     , D    ,      , O    , H    , L   , C     , V   ,
        //  0       1      2      3      4      5      6     7       8     9
        HandleOHLCV(quote, "D", values[2]);
        HandleOHLCV(quote, "O", values[4]);
        HandleOHLCV(quote, "H", values[5]);
        HandleOHLCV(quote, "L", values[6]);
        HandleOHLCV(quote, "C", values[7]);
        HandleOHLCV(quote, "V", values[8]);

        return quote;
    }

    internal static decimal ToDecimal(this string value)
        => decimal.TryParse(value, out decimal d) ? d : d;

    internal static decimal? ToDecimalNull(this string value)
        => decimal.TryParse(value, out decimal d) ? d : null;

    internal static double? ToDoubleNull(this string value)
    => double.TryParse(value, out double d) ? d : null;

    private static void HandleOHLCV(Quote quote, string position, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        switch (position)
        {
            case "D":
                quote.Date = DateTime.ParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture);
                break;
            case "O":
                quote.Open = Convert.ToDecimal(value, EnglishCulture);
                break;
            case "H":
                quote.High = Convert.ToDecimal(value, EnglishCulture);
                break;
            case "L":
                quote.Low = Convert.ToDecimal(value, EnglishCulture);
                break;
            case "C":
                quote.Close = Convert.ToDecimal(value, EnglishCulture);
                break;
            case "V":
                quote.Volume = Convert.ToDecimal(value, EnglishCulture);
                break;
            default:
                break;
        }
    }
}