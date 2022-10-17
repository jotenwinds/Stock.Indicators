using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Skender.Stock.Indicators;
using Stooq.Data.Library;

namespace Jo.Backtest;

internal sealed class CheckWeakGapsDaysOfTheWeek
{
    private static ILogger _logger = LogManager.GetCurrentClassLogger();

    private const int NumberOfDays = 7;


    internal sealed class Gap
    {
        public DayOfWeek Dow;
        public decimal Open;
        public decimal Close;
        public decimal PreviousClose;
        public decimal PreviousOpen;

        public bool IsGapUp => Open > Math.Max(PreviousOpen, PreviousClose);
        public bool IsGapDown => Open < Math.Min(PreviousOpen, PreviousClose);
        public bool IsBarGreen => Close > Open;
    }

    internal class GapDataPerDay
    {
        public List<Gap> Gaps = new List<Gap>();
    }


    public static void Run(string ticker)
    {
        /* This check the days of week to see if the day has a gap up or down from the previous day:
         * 
         * Week Gap => checking the candle's body
         *  gap up  : Today's open > Max(Yesterday's Close or Open)
         *  gap down: Today's open < Min(Yesterday's Close or Open)
         *  
         * Strong Gap => checking the candle's body + wicks
         *  gap up  : Today's open > Max(Yesterday's High or low)
         *  gap down: Today's open < Min(Yesterday's High or Low)
         *  
         * Days of the week:
         *  Mon Tue Wed Thu Fri
         */

        // fetch historical quotes from data provider
        var interval = Period._daily;
        IStooqQuote stookQuote1day = Program.GetHistoryFromFeed(ticker, interval);

        int indexOfFirstFriday = stookQuote1day.QuotesList.FindIndex(i => i.Date.DayOfWeek == DayOfWeek.Friday);

        _logger.Info("Check Week Gaps Of The Week OPEN vs previous Max[CLOSE, Open] (so only the body candle)");
        _logger.Info("-------------------------------------------------------------");
        _logger.Info($"There are # {stookQuote1day.QuotesList.Count} periods in the set [Interval: {interval}]");
        IQuote firstQuoteDay = stookQuote1day.QuotesList.First();
        _logger.Info($"First day in set: {firstQuoteDay.Date.ToString("yyyy-MM-dd")} ({firstQuoteDay.Date.DayOfWeek})");
        IQuote lastQuoteDay = stookQuote1day.QuotesList.Last();
        _logger.Info($"Last  day in set: {lastQuoteDay.Date.ToString("yyyy-MM-dd")} ({lastQuoteDay.Date.DayOfWeek})");

        // roll through history
        decimal previousClose = 0;
        decimal previousOpen = 0;
        bool isFirst = true;

        int[] noGapFromPrevious = new int[NumberOfDays];
        GapDataPerDay[] gapUpFromPrevious = new GapDataPerDay[noGapFromPrevious.Length];
        GapDataPerDay[] gapDownFromPrevious = new GapDataPerDay[noGapFromPrevious.Length];

        for (int i = 0; i < gapUpFromPrevious.Length; ++i)
            gapUpFromPrevious[i] = new GapDataPerDay();
        for (int i = 0; i < gapDownFromPrevious.Length; ++i)
            gapDownFromPrevious[i] = new GapDataPerDay();

        for (int i = 1; i < stookQuote1day.QuotesList.Count; i++)
        {
            IQuote q = stookQuote1day.QuotesList[i];
            if (isFirst)
            {
                previousClose = q.Close;
                previousOpen = q.Open;
                isFirst = false;
                continue;
            }

            var gap = new Gap
            {
                Dow = q.Date.DayOfWeek,
                Open = q.Open,
                Close = q.Close,
                PreviousClose = previousClose,
                PreviousOpen = previousOpen
            };
            if (gap.IsGapUp)
            {
                gapUpFromPrevious[(int)gap.Dow].Gaps.Add(gap);
            }
            else if (gap.IsGapDown)
            {
                gapDownFromPrevious[(int)gap.Dow].Gaps.Add(gap);
            }
            else
            {
                noGapFromPrevious[(int)gap.Dow] = noGapFromPrevious[(int)gap.Dow] + 1;
            }

            // Update the Close for the next check
            previousClose = q.Close;
            previousOpen = q.Open;
        }

        _logger.Info("Index|DayOfWeek|[Up: #|%][Down: #|%][NoGap: #|%][Up: Green/Red][Down: Green/Red]");
        _logger.Info("---------------------------------------------------------------------");
        for (int i = 0; i < gapUpFromPrevious.Length; i++)
        {
            int gapUp = gapUpFromPrevious[i].Gaps.Count;
            int greenUpBars = gapUpFromPrevious[i].Gaps.Count(p => p.IsBarGreen);
            int redUpBars = gapUp - greenUpBars;
            int gapDown = gapDownFromPrevious[i].Gaps.Count;
            int greenDownBars = gapDownFromPrevious[i].Gaps.Count(p => p.IsBarGreen);
            int redDownBars = gapDown - greenDownBars;
            int noGap = noGapFromPrevious[i];
            int total = gapUp + gapDown + noGap;
            decimal prctUp = (total == 0) ? 0m : 100m * (((decimal)gapUp) / total);
            decimal prctDown = (total == 0) ? 0m : 100m * (((decimal)gapDown) / total);
            decimal prctNoGap = (total == 0) ? 0m : 100m * (((decimal)noGap) / total);
            _logger.Info($"{i}|{(DayOfWeek)i}|[{gapUp}|{prctUp:#.##}%][{gapDown}|{prctDown:#.##}%][{noGap}|{prctNoGap:#.##}%][{greenUpBars}/{redUpBars}][{greenDownBars}/{redDownBars}]");
        }

    }
}