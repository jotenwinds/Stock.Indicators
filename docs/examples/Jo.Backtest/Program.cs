using System;
using Jo.Backtest.Backtests;
using Jo.Backtest.Scanners;
using NLog;
using Stooq.Data.Library;

namespace Jo.Backtest;

public partial class Program
{
    private static ILogger _logger = LogManager.Setup().LoadConfigurationFromFile().GetCurrentClassLogger();

    internal const string InitialFolder = @"C:\MyDev\f1776\20220909\data";
    internal const Market MarketUSA = Market.USA;


    public static void Main()
    {
        // Build Index File v1 of the Algo
        if (false)
        {
            string targetDataIndexFilename = @"C:\MyDev\f1776\20220909\data-index.txt";
            var indexBuilder = new StooqDataIndexBuilder();
            indexBuilder.BuildIndexFile(InitialFolder, targetDataIndexFilename);
        }

        // Build Index File v2 of the Algo
        if (false)
        {
            string targetDataIndexFilename2 = @"C:\MyDev\f1776\20220909\data-index2.txt";
            var indexBuilder2 = new StooqDataIndexBuilder2();
            var r = indexBuilder2.BuildIndexFile(InitialFolder, targetDataIndexFilename2);

            var r2 = r.GetResultByTickers(Period._daily, MarketUSA);
        }

        // First Scanner
        {
            ScanIndecisionWindow.Run(Period._daily, MarketUSA);
        }

        // See ConsoleApp first.  This is more advanced.
        if (true)
        {
            //string ticker = "DIA";
            //string ticker = "QQQ";
            //string ticker = "SPY";
            string ticker = "PFE";

            // Backtest strategies examples
            if (false)
            {
                RsiBackTest.Run(ticker);
            }

            // Checks examples
            if (false)
            {
                CheckOpenCloseDaysOfTheWeek.Run(ticker);
                CheckOpenCloseDaysOfTheWeekPerMonth.Run(ticker);
                CheckWeakGapsDaysOfTheWeek.Run(ticker);
            }
            if (true)
            {
                CheckIndecisionWindow.Run(ticker);
            }
        }
    }


    internal static IStooqQuote GetHistoryFromFeed(string ticker, Period period)
    {
        /************************************************************

         We're mocking a data provider here by simply importing a
         JSON file, a similar format of many public APIs.

         This approach will vary widely depending on where you are
         getting your quote history.

         See https://github.com/DaveSkender/Stock.Indicators/discussions/579
         for free or inexpensive market data providers and examples.

         The return type of IEnumerable<Quote> can also be List<Quote>
         or ICollection<Quote> or other IEnumerable compatible types.

         ************************************************************/

        //string json = File.ReadAllText("quotes.data.json");
        //List<Quote> quotes = JsonConvert.DeserializeObject<IReadOnlyCollection<Quote>>(json)
        //    .ToSortedList();
        //return quotes;

        IStooqQuoteReader stooqQuoteReader = new StooqQuoteReader(InitialFolder);
        IStooqQuote result = stooqQuoteReader.GetHistoryFromFeed(period, Market.USA, ticker, 5000, DateOnly.FromDateTime(DateTime.Now));

        return result;

    }
}
