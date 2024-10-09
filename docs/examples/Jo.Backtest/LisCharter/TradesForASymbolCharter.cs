using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Jo.Backtest.Infrastructure.Apis.Lis.OptionsTape;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using Newtonsoft.Json;
using NLog;
using Skender.Stock.Indicators;
using Stooq.Data.Library;
using Jo.Backtest.Domain;

namespace Jo.Backtest.LisCharter;

internal class TradesForASymbolCharter
{
    internal const string TradesForASymbolCharterFolder = @"TradesForASymbolCharter";

    private static ILogger _logger = LogManager.GetCurrentClassLogger();

    // IOptionsTapeApiClient

    public static void Run(Period period, Market market, string symbol = "DAL", OptionType optionType = OptionType.Both)
    {
        var indexBuilder2 = new StooqDataIndexBuilder2();
        var r = indexBuilder2.BuildIndexFile(dataRoot: Program.InitialFolder);

        var r2 = r.GetResultByTickers(period, market);
        int totalTickers = r2.DataByTickers.Count;
        _logger.Info($"Found # {totalTickers} securities in market '{market}'.");

        var ticker = market == Market.USA ? string.Concat(symbol, ".US") : symbol;

        if (r2.DataByTickers.TryGetValue(ticker, out DataFile df))
        {
            _logger.Info($"Found security series for '{ticker}':");
            _logger.Info($"  - '{df.Ticker}' ({df.TickerMarket})");
            _logger.Info($"  - '{df.FileName}'");
            _logger.Info($"  - '{df.Market}'");
            _logger.Info($"  - '{df.Period}'");
            _logger.Info($"--------------------");
            IStooqQuoteReader stooqQuoteReader = new StooqQuoteReader(Program.InitialFolder);
            IStooqQuote? stooqQuotes = null;
            try
            {
                stooqQuotes = stooqQuoteReader.GetHistoryFromFeed(period, market, df);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, $"An error occured whilst reading data for ticker '{ticker}'.");
            }

            if (stooqQuotes == null)
            {
                _logger.Error($"Unable to find quotes for the symbol '{symbol}'.");
                return;
            }


            _logger.Info($"Building chart for ticker '{ticker}");
            _logger.Info($"-------------------------------------------------");
            _logger.Info($"There are # {stooqQuotes.QuotesList.Count} '{period}' periods in the set");
            IQuote firstQuoteDay = stooqQuotes.QuotesList.First();
            _logger.Info($"First day in set: {firstQuoteDay.Date:yyyy-MM-dd} ({firstQuoteDay.Date.DayOfWeek})");
            IQuote lastQuoteDay = stooqQuotes.QuotesList.Last();
            _logger.Info($"Last  day in set: {lastQuoteDay.Date:yyyy-MM-dd} ({lastQuoteDay.Date.DayOfWeek})");

            // Retrieve the trades
            string? lisApiUrl = "https://localhost:7224"; //builder.Configuration["Lis.Apis.Systems.Url"];

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(lisApiUrl, UriKind.Absolute);
            IOptionsTapeApiClient apiClient = new OptionsTapeApiClient(httpClient);
            var response = apiClient.SearchTradesForASymbolAsync(symbol).Result;

            if (!response?.IsSuccess ?? true)
            {
                _logger.Error($"Error during search trades for the symbol '{symbol}': '{JsonConvert.SerializeObject(response)}'.");
                return;
            }

            if (response.Data == null || !response.Data.Trades.Any())
            {
                _logger.Warn($"There is no trades on the tape for the symbol '{symbol}': '{JsonConvert.SerializeObject(response)}'.");
                return;
            }

            IList<TradeForASymbolDto> trades = response.Data.Trades;
            if (optionType == OptionType.Call)
                trades = trades.Where(x => x.Type == "C").ToList();
            else if (optionType == OptionType.Put)
                trades = trades.Where(x => x.Type == "P").ToList();

            _logger.Info($"There are # {trades.Count} trades in the set for ticker '{symbol}'.");

            var chartFolder = new DirectoryInfo(Program.BacktestFolder);
            if (!chartFolder.Exists)
                chartFolder.Create();
            var tickerChartFolder = new DirectoryInfo(Path.Combine(chartFolder.FullName, ticker, TradesForASymbolCharterFolder));
            if (!tickerChartFolder.Exists)
                tickerChartFolder.Create();

            // Find the first trade date and the last.
            var today = DateTime.Now.Date;
            var tradesOrderedByDay = trades.OrderBy(x => x.Day).ToList();
            var oldestTrade = tradesOrderedByDay.First();
            var daysSinceOldest = (today - oldestTrade.Day).TotalDays;
            var mostRecentTrade = tradesOrderedByDay.Last();
            var daysSinceMostRecent = (today - mostRecentTrade.Day).TotalDays;

            _logger.Info($"The oldest trade from the tape is '{oldestTrade.Day}' ({daysSinceOldest}).");
            _logger.Info($"The most recent trade trade from the tape is '{mostRecentTrade.Day}' ({daysSinceMostRecent}).");

            // Find the first index in the quotes collections and last index for the trades range.
            int index = 0; // Start at the beginning.
            int lastIndex = stooqQuotes.QuotesList.Count - 1;
            IQuote currentQuote = stooqQuotes.QuotesList[index];
            var currentDtOnly = currentQuote.Date;
            bool moreData = index < lastIndex;
            while (currentDtOnly < oldestTrade.Day.Date && moreData)
            {
                currentQuote = stooqQuotes.QuotesList[++index];
                currentDtOnly = currentQuote.Date;
                moreData = index < lastIndex;
            }

            if (!moreData)
            {
                _logger.Warn($"There is no trades on the tape for the symbol '{symbol}'.");
                return;
            }

            int indexDateOldestTrade = index;
            int indexDateMostRecentTrade = index;
            if (mostRecentTrade.Day.Date > oldestTrade.Day.Date)
            {

                while (currentDtOnly < mostRecentTrade.Day.Date && moreData)
                {
                    currentQuote = stooqQuotes.QuotesList[++index];
                    currentDtOnly = currentQuote.Date;
                    moreData = index < lastIndex;
                }

                if (!moreData)
                {
                    _logger.Warn($"Not enougth quotes to cover all the trades for the symbol '{symbol}'.");
                }

                indexDateMostRecentTrade = index;
            }

            List<IQuote> quotes = new();
            for (int i = indexDateOldestTrade; i <= indexDateMostRecentTrade; i++)
            {
                currentQuote = stooqQuotes.QuotesList[i];
                quotes.Add(currentQuote);
            }

            if (quotes.Count > 0)
            {
                List<ISeries> series = new()
                {
                    new CandlesticksSeries<FinancialPointI>
                    {
                        Values = quotes
                            .Select(x => new FinancialPointI((double)x.High, (double)x.Open, (double)x.Close, (double)x.Low))
                            .ToArray()
                    }
                };

                foreach (var trade in tradesOrderedByDay)
                {
                    ISeries s = BuildSeriesForTrade(trade, quotes);
                    series.Add(s);
                }


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
                    @$"{ticker}-{oldestTrade.Day.Year:0000}-{mostRecentTrade.Day.Year:0000}-{optionType}.png";
                //@$"{ticker}-{holidaysPeriod.Year:0000}-{holidaysPeriod.Id:00}-{holidaysPeriod.From:MMdd}-{holidaysPeriod.To:MMdd}.png";
                var fullFilename = Path.Combine(tickerChartFolder.FullName, fileName);
                // See Aspect Calculator: https://andrew.hedges.name/experiments/aspect_ratio/ .
                var candleCartesianChart = new SKCartesianChart
                {
                    Series = series.ToArray(),
                    XAxes = xAxes,
                    Width = 1600, //1100,
                    Height = 900, //700,
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
        else
        {
            _logger.Error($"Could not find '{symbol}' in market '{market}'.");
        }
    }

    private static ISeries BuildSeriesForTrade(TradeForASymbolDto trade, List<IQuote> quotes)
    {
        List<int?> values = new(quotes.Count);
        foreach (IQuote quote in quotes)
        {
            if (quote.Date >= trade.Day && quote.Date <= trade.ExpirationDate.Date)
                values.Add((int)trade.StrikePrice);
            else
                values.Add(null);
        }
        ISeries result = new LineSeries<int?>
        {
            Values = values,
            LineSmoothness = 1
        };
        return result;
    }
}
