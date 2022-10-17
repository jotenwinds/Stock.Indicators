using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NLog;

namespace Stooq.Data.Library;
public class StooqDataIndexBuilder
{
    private static ILogger _logger = LogManager.GetCurrentClassLogger();

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
    public void BuildIndexFile(string dataRoot, string targetDataIndexFilename)
    {
        DirectoryInfo rootDi = new DirectoryInfo(dataRoot);
        List<DataFile> dataFiles = new List<DataFile>();

        var periodFolders = rootDi.GetDirectories(); //(null, SearchOption.AllDirectories);
        foreach(var periodFolder in periodFolders)
        {
            var periodDataFiles = BuildIndexFilePerPeriod(periodFolder);
            dataFiles.AddRange(periodDataFiles);
        }

        _logger.Info($"There are # {dataFiles.Count} files (Stocks/ETFs).");

        var json = JsonConvert.SerializeObject(dataFiles);
        File.WriteAllText(targetDataIndexFilename, json, System.Text.Encoding.UTF8);
    }

    private List<DataFile> BuildIndexFilePerPeriod(DirectoryInfo periodFolder)
    {
        List<DataFile> dataFiles = new List<DataFile>();
        var marketFolders = periodFolder.GetDirectories();
        foreach (var marketFolder in marketFolders)
        {
            var marketDataFiles = BuildIndexFilePerMarket(periodFolder.Name, marketFolder);
            dataFiles.AddRange(marketDataFiles);
        }
        return dataFiles;
    }

    private List<DataFile> BuildIndexFilePerMarket(string currentPeriod, DirectoryInfo marketFolder)
    {
        List<DataFile> dataFiles = new List<DataFile>();
        var securityFolders = marketFolder.GetDirectories();
        foreach (var securityFolder in securityFolders)
        {
            var securityDataFiles = BuildIndexFilePerSecurity(marketFolder.FullName, currentPeriod, marketFolder.Name, securityFolder);
            dataFiles.AddRange(securityDataFiles);
        }
        return dataFiles;
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
