using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Skender.Stock.Indicators;
using Stooq.Data.Library;

namespace Jo.Backtest;

public partial class Program
{
    public enum Month
    {
        NotSet = 0,
        January = 1,
        First = January,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12,
        Last = December
    }

    internal sealed class CheckOpenCloseDaysOfTheWeekPerMonth
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        private const int NumberOfDays = 7;
        private const int NumberOfMonths = 12;

        internal class TopMonthDay
        {
            public int Month;
            public int Day;
            public int Higher;
            public int Lower;
            public decimal HigherPct;
            public decimal LowerPct;
        }

        internal class MonthData
        {

            public int[] higherFromPrevious = new int[NumberOfDays];
            public int[] lowerFromPrevious = new int[NumberOfDays];
        }

        public static void Run(string ticker)
        {
            /* This check the days of week for each month to see if the day has open lower than the previous day:
             *       Mon Tue Wed Thu Fri
             *  Jan
             *  Feb
             *  ...
             *  Nov
             *  Dec
             * 
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


            MonthData[] monthData = new MonthData[(int)Month.Last + 1];
            for (int i = 0; i < monthData.Length; ++i)
                monthData[i] = new MonthData();

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
                var month = monthData[q.Date.Month];
                if (q.Open > previousClose)
                    month.higherFromPrevious[(int)day] = month.higherFromPrevious[(int)day] + 1;
                else
                    month.lowerFromPrevious[(int)day] = month.lowerFromPrevious[(int)day] + 1;

                // Update the Close for the next check
                previousClose = q.Close;
            }

            // Capture list of best days with a higher %
            decimal topLevelpct = 60m;
            List<TopMonthDay> topHighers = new List<TopMonthDay>();
            List<TopMonthDay> topLowers = new List<TopMonthDay>();

            for (int m = (int)Month.First; m < monthData.Length; m++)
            {
                var currentMonth = monthData[m];
                _logger.Info($"Month: {(Month)m}");
                _logger.Info("Index|DayOfWeek|higherFromPrevious|Higher %|lowerFromPrevious|lower %");
                _logger.Info("---------------------------------------------------------------------");
                for (int i = 0; i < currentMonth.higherFromPrevious.Length; i++)
                {
                    int total = currentMonth.higherFromPrevious[i] + currentMonth.lowerFromPrevious[i];
                    decimal prctHigher = (total == 0) ? 0m : 100m * (((decimal)currentMonth.higherFromPrevious[i]) / total);
                    decimal prctLower = (total == 0) ? 0m : 100m * (((decimal)currentMonth.lowerFromPrevious[i]) / total);
                    _logger.Info($"{i}|{(DayOfWeek)i}|{currentMonth.higherFromPrevious[i]}|{prctHigher:#.##}%|{currentMonth.lowerFromPrevious[i]}|{prctLower:#.##}%");

                    if (prctHigher > topLevelpct)
                    {
                        var temp = new TopMonthDay {
                            Month = m

                            };
                        topHighers.Add(temp);
                    }
                    else if (prctLower > topLevelpct)
                    {

                    }
                }
                _logger.Info("");
            }
        }
    }
}
