using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using Stooq.Data.Library;
using static Jo.Backtest.CheckIndecisionWindow;

namespace Jo.Backtest.Scanners;
internal class ScanIndecisionWindow
{
    private static ILogger _logger = LogManager.GetCurrentClassLogger();

    public static void Run(Period period, Market market)
    {
        var indexBuilder2 = new StooqDataIndexBuilder2();
        var r = indexBuilder2.BuildIndexFile(dataRoot: Program.InitialFolder);

        var r2 = r.GetResultByTickers(period, market);

        int totalTickers = r2.DataByTickers.Count;
        _logger.Info($"Scanning # {totalTickers} securities for an Indecision Candle:");
        _logger.Info($"------------------------------------------------------------------------");

        var results = new List<CheckIndecisionResult>();
        int i = 0;
        foreach (var dataKV in r2.DataByTickers)
        {
            _logger.Info($"{++i:#####}/{totalTickers:#####}|Scanning ticker '{dataKV.Key}' ...");
            IStooqQuoteReader stooqQuoteReader = new StooqQuoteReader(Program.InitialFolder);
            IStooqQuote stooqQuotes1day = null;
            try
            {
                stooqQuotes1day = stooqQuoteReader.GetHistoryFromFeed(period, market, dataKV.Value);
            }
            catch (System.Exception ex)
            {
                _logger.Warn(ex, $"An error occured whilst reading data for ticker '{dataKV.Key}'.");
            }

            if (stooqQuotes1day == null)
                continue;

            CheckIndecisionResult checkResult = CheckIndecisionWindow.Run(stooqQuotes1day);
            if (checkResult.Matches.Any())
            {
                _logger.Info($" - Ticker '{checkResult.Ticker}' has # {checkResult.Matches.Count} Indecision Candle(s).");
                results.Add(checkResult);
            }
        }

        _logger.Info($"There are # {results.Count} tickers with Indecision Candle(s).");

        results = results.OrderByDescending(x => x.Matches.Count).ToList();

        //string targetCheckResultTXTFilename = @"C:\MyDev\f1776\20220909\scan-CheckIndecisionWindow.txt";
        //if (!string.IsNullOrEmpty(targetCheckResultTXTFilename))
        //{
        //    var json = JsonConvert.SerializeObject(results);
        //    File.WriteAllText(targetCheckResultFilename, json, System.Text.Encoding.UTF8);
        //}

        string targetCheckResultJSONFilename = @"C:\MyDev\f1776\20220909\scan-CheckIndecisionWindow.json";
        if (!string.IsNullOrEmpty(targetCheckResultJSONFilename))
        {
            var json = JsonConvert.SerializeObject(results);
            File.WriteAllText(targetCheckResultJSONFilename, json, System.Text.Encoding.UTF8);
        }

    }
}
