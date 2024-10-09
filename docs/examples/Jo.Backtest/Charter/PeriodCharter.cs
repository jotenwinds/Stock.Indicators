using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using NLog;
using Skender.Stock.Indicators;
using Stooq.Data.Library;

namespace Jo.Backtest.Charter;
internal class PeriodCharter
{

    internal const string PeriodCharterFolder = @"PeriodCharter";

    private static ILogger _logger = LogManager.GetCurrentClassLogger();

    internal static readonly List<IHolidayPeriod> HolidaysPeriods =
        HolidaysPeriodsData.GetHolidaysPeriods();

    public static void Run(Period period, Market market, string ticker = "PFE.US")
    {
        var indexBuilder2 = new StooqDataIndexBuilder2();
        var r = indexBuilder2.BuildIndexFile(dataRoot: Program.InitialFolder);

        var r2 = r.GetResultByTickers(period, market);
        int totalTickers = r2.DataByTickers.Count;
        _logger.Info($"Found # {totalTickers} securities in market '{market}'.");

        if (r2.DataByTickers.TryGetValue(ticker, out DataFile df))
        {
            _logger.Info($"Found security series for '{ticker}':");
            _logger.Info($"  - '{df.Ticker}' ({df.TickerMarket})");
            _logger.Info($"  - '{df.FileName}'");
            _logger.Info($"  - '{df.Market}'");
            _logger.Info($"  - '{df.Period}'");
            _logger.Info($"--------------------");
            IStooqQuoteReader stooqQuoteReader = new StooqQuoteReader(Program.InitialFolder);
            IStooqQuote stooqQuotes = null;
            try
            {
                stooqQuotes = stooqQuoteReader.GetHistoryFromFeed(period, market, df);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, $"An error occured whilst reading data for ticker '{ticker}'.");
            }

            if (stooqQuotes != null)
            {
                _logger.Info($"Building chart for ticker '{ticker}");
                _logger.Info($"-------------------------------------------------");
                _logger.Info($"There are # {stooqQuotes.QuotesList.Count} '{period}' periods in the set");
                IQuote firstQuoteDay = stooqQuotes.QuotesList.First();
                _logger.Info($"First day in set: {firstQuoteDay.Date:yyyy-MM-dd} ({firstQuoteDay.Date.DayOfWeek})");
                IQuote lastQuoteDay = stooqQuotes.QuotesList.Last();
                _logger.Info($"Last  day in set: {lastQuoteDay.Date:yyyy-MM-dd} ({lastQuoteDay.Date.DayOfWeek})");

                var chartFolder = new DirectoryInfo(Program.BacktestFolder);
                if (!chartFolder.Exists)
                    chartFolder.Create();
                var tickerChartFolder = new DirectoryInfo(Path.Combine(chartFolder.FullName, ticker, PeriodCharterFolder));
                if (!tickerChartFolder.Exists)
                    tickerChartFolder.Create();

                int index = 0; // Start at the beginning.
                int lastIndex = stooqQuotes.QuotesList.Count - 1;
                IQuote currentQuote = stooqQuotes.QuotesList[index];
                var holidaysPeriods = PeriodCharter.HolidaysPeriods;
                foreach (var holidaysPeriod in holidaysPeriods)
                {
                    if (index > lastIndex)
                    {
                        _logger.Info($" *** No more quotes (Last quote in set: {currentQuote.Date:yyyy-MM-dd} ({currentQuote.Date.DayOfWeek})");
                        break;
                    }

                    currentQuote = stooqQuotes.QuotesList[index];
                    var currentDtOnly = DateOnly.FromDateTime(currentQuote.Date);
                    while (currentDtOnly < holidaysPeriod.From && index < lastIndex)
                    {
                        currentQuote = stooqQuotes.QuotesList[++index];
                        currentDtOnly = DateOnly.FromDateTime(currentQuote.Date);
                    }

                    List<IQuote> quotes = new();
                    while (holidaysPeriod.WithinPeriod(currentQuote.Date) && index < lastIndex)
                    {
                        quotes.Add(currentQuote);
                        currentQuote = stooqQuotes.QuotesList[++index];
                    }

                    _logger.Info($"There are # {quotes.Count} in '{holidaysPeriod.YearId}' [index: '{index}']");

                    if (quotes.Count > 0)
                    {
                        ISeries[] series = new ISeries[]
                        {
                            new CandlesticksSeries<FinancialPointI>
                            {
                                Values = quotes
                                    .Select(x => new FinancialPointI((double)x.High, (double)x.Open, (double)x.Close, (double)x.Low))
                                    .ToArray()
                            }
                        };

                        Axis[] xAxes = new[]
                        {
                            new Axis
                            {
                                LabelsRotation = 15,
                                Labels = quotes
                                    .Select(x => x.Date.ToString("yyyy MMM dd"))
                                    .ToArray()
                            }
                        };

                        var fileName =
                            @$"{ticker}-{holidaysPeriod.Year:0000}-{holidaysPeriod.Id:00}.png";
                        //@$"{ticker}-{holidaysPeriod.Year:0000}-{holidaysPeriod.Id:00}-{holidaysPeriod.From:MMdd}-{holidaysPeriod.To:MMdd}.png";
                        var fullFilename = Path.Combine(tickerChartFolder.FullName, fileName);
                        var candleCartesianChart = new SKCartesianChart
                        {
                            Series = series,
                            XAxes = xAxes,
                            Width = 1100,
                            Height = 700,
                            // out of livecharts properties...
                            //Location = new System.Drawing.Point(0, 0),
                            //Size = new System.Drawing.Size(50, 50),
                            //Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
                        };

                        // you can save the image to png (by default)
                        // or use the second argument to specify another format.
                        candleCartesianChart.SaveImage(fullFilename);
                        _logger.Info($" => chart generated '{fullFilename}'.");
                    }
                }

                var holidaysPeriodsWithQuotes = holidaysPeriods
                    .Where(x => x.Quotes.Any()).ToList();
                Dictionary<int, IHolidayYear> holidayYears =
                    HolidaysPeriodsData.GetHolidayYears(holidaysPeriodsWithQuotes);
            }
        }
        else
        {
            _logger.Error($"Could not find '{ticker}' in market '{market}'.");
        }
    }
}


