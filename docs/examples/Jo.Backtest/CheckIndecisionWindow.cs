using System.Collections.Generic;
using System.Linq;
using Jo.CustomIndicatorsLibrary.Candles;
using Jo.CustomIndicatorsLibrary.IndecisionWindow;
using NLog;
using Skender.Stock.Indicators;
using Stooq.Data.Library;

namespace Jo.Backtest;

public partial class Program
{
    internal sealed class CheckIndecisionWindow
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        internal sealed class CheckIndecisionResult
        {
            public string Ticker;
            public List<JoCandleResult> Matches => new List<JoCandleResult>();
        }


        public static CheckIndecisionResult Run(string ticker)
        {
            // fetch historical quotes from data provider
            IStooqQuote stookQuotes1day = GetHistoryFromFeed(ticker, Period._daily);
            return Run(stookQuotes1day);
        }

        public static CheckIndecisionResult Run(IStooqQuote stookQuotes)
        {
            /* This check an indecision candle following 3 strongs candle:
             *  1       2       3                               4                   5
             *  Bullish Bullish Bullish/Bearish (strong body)   Indecision Candle   Usually a Gap Down
             * 
             *  Bearish Bearish Bullish/Bearish (strong body)   Indecision Candle   Usually a Gap Up
             *  
             */
            var results = new CheckIndecisionResult { Ticker = stookQuotes.Ticker };

            _logger.Info($"Check for an Indecision Candle - ticker '{stookQuotes.Ticker}");
            _logger.Info($"-------------------------------------------------");
            _logger.Info($"There are # {stookQuotes.QuotesList.Count} periods in the set");
            IQuote firstQuoteDay = stookQuotes.QuotesList.First();
            _logger.Info($"First day in set: {firstQuoteDay.Date.ToString("yyyy-MM-dd")} ({firstQuoteDay.Date.DayOfWeek})");
            IQuote lastQuoteDay = stookQuotes.QuotesList.Last();
            _logger.Info($"Last  day in set: {lastQuoteDay.Date.ToString("yyyy-MM-dd")} ({lastQuoteDay.Date.DayOfWeek})");


            // calculate Indecision Window Candles
            List<JoCandleResult> resultsList =
                stookQuotes.QuotesList
                .GetIndecisionWindow()
                .ToList();

            // roll through history
            _logger.Info("Date|Close|BodyPct %");
            _logger.Info("--------------------");
            for (int i = 1; i < resultsList.Count; i++)
            {
                JoCandleResult r = resultsList[i];
                if (r.Match != Match.None)
                {
                    results.Matches.Add(r);
                    _logger.Info($"{r.Date}|{r.Candle.Close}|{r.Candle.BodyPct:#.##}%");
                }
            }
            return results;
        }
    }

}
