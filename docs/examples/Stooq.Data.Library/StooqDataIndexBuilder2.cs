using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NLog;

namespace Stooq.Data.Library;

public class StooqDataIndexBuilder2
{
    private static ILogger _logger = LogManager.GetCurrentClassLogger();

    public sealed class IndexResultByPeriods
    {
        public string RootDataFolder;
        public Dictionary<string, IndexResultsByMarket> DataByPeriods; // {k: period; v: {ResultByMarket} }

        public IndexResultsByTicker GetResultByTickers(Period period, Market market)
        {
            string periodKeyName = period.ToPeriodPathName();
            if (!DataByPeriods.ContainsKey(periodKeyName))
                throw new InvalidOperationException($"Period with key name '{periodKeyName}' does not exist (period: '{period}').");
            IndexResultsByMarket resultByMarket = DataByPeriods[periodKeyName];

            string marketKeyName = market.ToMarketPathName();
            if (!resultByMarket.DataByMarkets.ContainsKey(marketKeyName))
                throw new InvalidOperationException($"Market with key name '{marketKeyName}' does not exist (market: '{market}').");
            var resultsByTicker = resultByMarket.DataByMarkets[marketKeyName];

            return resultsByTicker;
        }
    }

    public sealed class IndexResultsByMarket
    {
        public Dictionary<string, IndexResultsByTicker> DataByMarkets; // {k: market; v: {ResultByTicker} }
    }

    public sealed class IndexResultsByTicker
    {
        public Dictionary<string, DataFile> DataByTickers; // {k: ticker; v: DataFile }
    }





    // Stoop Data Root structure:
    //  -data
    //      -5 min
    //          -us
    //              -nasdaq etfs
    //              -nasdaq stocks
    //                  -1
    //                  -2
    //                  -3
    //              -nyse etfs
    //                  -1
    //                  -2
    //              -nyse stocks
    //                  -1
    //                  -2
    //              -nysemkt etfs
    //              -nysemkt stocks
    //      -daily
    //          -us
    //              <same>
    //      -hourly
    //          -us
    //              <same>
    public IndexResultByPeriods BuildIndexFile(string dataRoot, string targetDataIndexFilename = null)
    {
        DirectoryInfo rootDi = new DirectoryInfo(dataRoot);
        var results = new IndexResultByPeriods
        {
            RootDataFolder = dataRoot,
            DataByPeriods = new Dictionary<string, IndexResultsByMarket>()
        };

        var periodFolders = rootDi.GetDirectories(); //(null, SearchOption.AllDirectories);
        int totalDataFiles = 0;
        foreach(var periodFolder in periodFolders)
        {
            var periodDataFiles = BuildIndexFilePerPeriod(periodFolder);
            foreach(var dataByMarket in periodDataFiles.DataByMarkets)
            totalDataFiles += dataByMarket.Value.DataByTickers.Count;
            results.DataByPeriods.Add(periodFolder.Name, periodDataFiles);
        }

        _logger.Info($"There are # {totalDataFiles} files (Stocks/ETFs).");

        if (!string.IsNullOrEmpty(targetDataIndexFilename))
        {
            var json = JsonConvert.SerializeObject(results);
            File.WriteAllText(targetDataIndexFilename, json, System.Text.Encoding.UTF8);
        }
        return results;
    }

    private IndexResultsByMarket BuildIndexFilePerPeriod(DirectoryInfo periodFolder)
    {
        var results = new IndexResultsByMarket { DataByMarkets = new Dictionary<string, IndexResultsByTicker>() };

        List<DataFile> dataFiles = new List<DataFile>();
        var marketFolders = periodFolder.GetDirectories();
        foreach (var marketFolder in marketFolders)
        {
            string currentMarketName = marketFolder.Name;
            var currentMarketTickers = BuildIndexFilePerMarket(periodFolder.Name, currentMarketName, marketFolder);
            results.DataByMarkets.Add(currentMarketName, currentMarketTickers);
        }
        return results;
    }

    private IndexResultsByTicker BuildIndexFilePerMarket(string currentPeriod, string currentMarketName, DirectoryInfo marketFolder)
    {
        var results = new IndexResultsByTicker { DataByTickers = new Dictionary<string, DataFile>() };
        var securityFolders = marketFolder.GetDirectories();
        foreach (var securityFolder in securityFolders)
        {
            var securityDataFiles = BuildIndexFilePerSecurity(marketFolder.FullName, currentPeriod, currentMarketName, securityFolder);
            foreach (var dataFile in securityDataFiles)
            {
                results.DataByTickers.Add(dataFile.TickerMarket, dataFile);
            }
        }
        return results;
    }

    private List<DataFile> BuildIndexFilePerSecurity(string marketRoot, string currentPeriod, string currentMarket, DirectoryInfo securityFolder)
    {
        List<DataFile> dataFiles = new List<DataFile>();
        var subFolders = securityFolder.GetDirectories();
        if (subFolders.Any())
        {
            foreach (var subFolder in subFolders)
            {
                var securityDataFiles = BuildIndexFilePerSecurity(marketRoot, currentPeriod, currentMarket, subFolder);
                dataFiles.AddRange(securityDataFiles);
            }
        }
        else
        {
            var securityFiles = securityFolder.GetFiles();
            foreach(var securityFile in securityFiles)
            {
                string name = securityFile.Name.ToUpper().Trim();
                if (name.StartsWith('_'))
                {
                    if (name != "_prn.us.txt")
                        continue;
                    name = name.Substring(1);
                }
                DataFile data = new DataFile();
                var splits = name.Split('.');
                data.Period = currentPeriod;
                data.Market = currentMarket;
                data.Ticker = splits[0];
                data.RelativePath = Path.GetRelativePath(marketRoot, securityFile.DirectoryName);
                data.FileName = securityFile.Name;
                dataFiles.Add(data);
            }

        }
        return dataFiles;
    }
}
