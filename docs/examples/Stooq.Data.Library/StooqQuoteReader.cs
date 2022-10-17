using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using NLog;
using Skender.Stock.Indicators;

namespace Stooq.Data.Library;

// DATA PERIOD OF TIME
public enum Market : Int32
{
    Undefined = 0,
    USA = 1,
    World = 999
}

public interface IStooqQuoteReader
{
    public string InitialDataFolder { get; }

    public IStooqQuote GetHistoryFromFeed(Period period, Market market, string ticker, int numberOfPeriodsBack, DateOnly StartDate);

    public IStooqQuote GetHistoryFromFeed(Period period, Market market, DataFile dataFile);
}

public sealed class StooqQuoteReader : IStooqQuoteReader
{
    private const string RAW_DATA_SEPARATOR = @",";

    private static ILogger _logger = LogManager.GetCurrentClassLogger();

    public StooqQuoteReader(string dataFolder)
    {
        if (string.IsNullOrEmpty(dataFolder)) throw new ArgumentNullException(nameof(dataFolder), "Cannot be null or empty.");
        InitialDataFolder = dataFolder;
    }

    public string InitialDataFolder { get; private set; }

    public IStooqQuote GetHistoryFromFeed(Period period, Market market, string ticker, int numberOfPeriodsBack, DateOnly StartDate)
    {
        string marketFolder = market.ToMarketPathName();
        string periodFolder = period.ToPeriodPathName();

        string tickerFile = "unknown";
        switch (ticker.ToUpper().Trim())
        {
            case "DIA":
                tickerFile = "nyse etfs/1/dia.us.txt";
                break;
            case "PFE":
                tickerFile = "nyse stocks/2/pfe.us.txt";
                break;
            case "QQQ":
                tickerFile = "nasdaq etfs/qqq.us.txt";
                break;
            case "SPY":
                tickerFile = "nyse etfs/1/spy.us.txt";
                break;
            default:
                throw new ArgumentException($"Ticker '{ticker}' not yet supported", nameof(ticker));
        }

        string currentFilePath = InitialDataFolder;
        string quoteFileName = Path.Combine(currentFilePath, periodFolder, marketFolder, tickerFile);
        FileInfo sourceFi = new FileInfo(quoteFileName);

        StooqQuote result = ProcessFile(ticker, period, sourceFi);
        return result;
    }
    public IStooqQuote GetHistoryFromFeed(Period period, Market market, DataFile dataFile)
    {
        string currentFilePath = InitialDataFolder;
        string periodFolder = period.ToPeriodPathName();
        string marketFolder = market.ToMarketPathName();
        string quoteFileName = Path.Combine(currentFilePath, periodFolder, marketFolder, dataFile.RelativePath, dataFile.FileName);
        FileInfo sourceFi = new FileInfo(quoteFileName);

        StooqQuote result = ProcessFile(dataFile.Ticker, period, sourceFi);
        return result;
    }


    private StooqQuote ProcessFile(string ticker, Period period, FileInfo sourceFi)
    {
        var result = new StooqQuote
        {
            Ticker = ticker,
            DataPeriod = period
        };

        try
        {
            _logger.Info($"Retrieving quote records from [filename: '{sourceFi.FullName}'].");
            int lineNumber;

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = (args) =>
                {
                    if (args.HeaderNames == null || args.Index == -1)
                        return;
                    var headers = string.Join(RAW_DATA_SEPARATOR, args.HeaderNames);
                    _logger.Warn($"Unable to find field(s) '{headers}' at parser line '{args.Context.Parser.RawRow}'.");
                },
                LeaveOpen = false
            };
            using (var reader = new StreamReader(sourceFi.FullName))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                // Number of the line in the file - Starting at '1' (not zero - easier to do a lookup in Notepad++ by line number).
                lineNumber = 1;
                // Get the header

                csv.Read();
                csv.ReadHeader();
                var firstLine = csv.HeaderRecord;
                var headers = string.Join(RAW_DATA_SEPARATOR, firstLine);
                _logger.Trace($" - line:{lineNumber:000000000}] File header from file: '{headers}'");

                List<string> columns = new List<string>();
                while (csv.Read())
                {
                    lineNumber++;
                    string rawData = csv.Parser.RawRecord;
                    _logger.Trace($" - line:{lineNumber:000000000}] rawData: '{rawData}'");
                    var record = new Quote();
                    try
                    {
                        ProcessRowOfData(csv, record);
                        result.QuotesList.Add(record);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($" - line:{lineNumber:000000000}] *** Error: '{ex.Message}'.");
                        throw;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"|An error occured during the parsing of the file '{sourceFi.Name}'.");
            throw;
        }
        return result;
    }

    private void ProcessRowOfData(CsvReader csv, Quote record)
    {
        //Guard.ArgumentNotNull(columns, @"columns");
        //Guard.ArgumentNotNull(rating, @"rating");
        try
        {
            string date = csv.GetField<string>("<DATE>")?.Trim();
            record.Date = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);

            record.Open = csv.GetField<decimal>("<OPEN>");
            record.High = csv.GetField<decimal>("<HIGH>");
            record.Low = csv.GetField<decimal>("<LOW>");
            record.Close = csv.GetField<decimal>("<CLOSE>");
            record.Volume = csv.GetField<decimal>("<VOL>");
        }
        catch (Exception ex)
        {
            string errorMsg = string.Format("An error occurred whilst parsing the recored data [error: '{0}'].", ex.Message);
            _logger.Error(ex, errorMsg);
        }
    }
}