using System;
using System.Linq;
using NLog;
using Skender.Stock.Indicators;
using Stooq.Data.Library;

namespace Jo.Backtest;

public partial class Program
{
    internal sealed class CheckOpenCloseDaysOfTheWeek
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        public static void Run(string ticker)
        {
            /* This check the days of week to see if the day has open lower than the previous day:
             *  Mon Tue Wed Thu Fri
             *  
             * 
             * As a result, there will always be one open LONG or SHORT
             * position that is opened and closed at signal crossover
             * points in the overbought and oversold regions of the indicator.
             */

            // fetch historical quotes from data provider

            IStooqQuote stookQuote1day = GetHistoryFromFeed(ticker, Period._daily);

            int indexOfFirstFriday = stookQuote1day.QuotesList.FindIndex(i => i.Date.DayOfWeek == DayOfWeek.Friday);

            _logger.Info("Check Days Of The Week OPEN vs previous CLOSE");
            _logger.Info("-------------------------------------------------------");
            _logger.Info($"There are # {stookQuote1day.QuotesList.Count} periods in the set");
            IQuote firstQuoteDay = stookQuote1day.QuotesList.First();
            _logger.Info($"First day in set: {firstQuoteDay.Date.ToString("yyyy-MM-dd")} ({firstQuoteDay.Date.DayOfWeek})");
            IQuote lastQuoteDay = stookQuote1day.QuotesList.Last();
            _logger.Info($"Last  day in set: {lastQuoteDay.Date.ToString("yyyy-MM-dd")} ({lastQuoteDay.Date.DayOfWeek})");


            // roll through history
            decimal previousClose = 0;
            bool isFirst = true;

            int[] higherFromPrevious = new int[7];
            int[] lowerFromPrevious = new int[higherFromPrevious.Length];

            for (int i = 1; i < stookQuote1day.QuotesList.Count; i++)
            {
                IQuote q = stookQuote1day.QuotesList[i];
                if (isFirst)
                {
                    previousClose = q.Close;
                    isFirst = false;
                    continue;
                }

                var day = q.Date.DayOfWeek;
                if (q.Open > previousClose)
                    higherFromPrevious[(int)day] = higherFromPrevious[(int)day] + 1;
                else
                    lowerFromPrevious[(int)day] = lowerFromPrevious[(int)day] + 1;

                // Update the Close for the next check
                previousClose = q.Close;
            }

            _logger.Info("Index|DayOfWeek|higherFromPrevious|Higher %|lowerFromPrevious|lower %");
            _logger.Info("---------------------------------------------------------------------");
            for (int i = 0; i < higherFromPrevious.Length; i++)
            {
                int total = higherFromPrevious[i] + lowerFromPrevious[i];
                decimal prctHigher = (total == 0) ? 0m : 100m * (((decimal)higherFromPrevious[i]) / total);
                decimal prctLower = (total == 0) ? 0m : 100m * (((decimal)lowerFromPrevious[i]) / total);
                _logger.Info($"{i}|{(DayOfWeek)i}|{higherFromPrevious[i]}|{prctHigher:#.##}%|{lowerFromPrevious[i]}|{prctLower:#.##}%");
            }
        }
    }
}
