using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skender.Stock.Indicators;

namespace Jo.Backtest.Charter;

internal class HolidaysPeriodsData
{
    // Read & Parse the Holidays periods data file.
    internal static List<IHolidayPeriod> GetHolidaysPeriods()
        => File.ReadAllLines("_common/data/Holidays-Periods-From-2000-To-2024.csv")
            .Skip(1)    // Skip the header line
            .Select(Importer.PeriodsFromCsv)
            .OrderBy(x => x.YearId)
            .ToList();

    internal static Dictionary<int, IHolidayYear> GetHolidayYears(
        IList<IHolidayPeriod> holidaysPeriods)
    {
        var years = holidaysPeriods.GroupBy(x => x.Year);
        var result = new Dictionary<int, IHolidayYear>();
        foreach (IGrouping<int, IHolidayPeriod> holidayPeriods in years)
        {
            var year = new HolidayYear
            {
                Year = holidayPeriods.Key,
                Period01 = holidayPeriods.First(x => x.Id == 1),
                Period02 = holidayPeriods.First(x => x.Id == 2),
                Period03 = holidayPeriods.First(x => x.Id == 3),
                Period04 = holidayPeriods.First(x => x.Id == 4),
                Period05 = holidayPeriods.First(x => x.Id == 5)
            };
            result.Add(year.Year, year);
        }
        return result;
    }
}

public interface IHolidayYear
{
    int Year { get; }

    public IHolidayPeriod Period01 { get; }
    public IHolidayPeriod Period02 { get; }
    public IHolidayPeriod Period03 { get; }
    public IHolidayPeriod Period04 { get; }
    public IHolidayPeriod Period05 { get; }
    public decimal YearVolume { get; }
}
public class HolidayYear : IHolidayYear
{
    public int Year { get; internal set; }
    public IHolidayPeriod Period01 { get; internal set; }
    public IHolidayPeriod Period02 { get; internal set; }
    public IHolidayPeriod Period03 { get; internal set; }
    public IHolidayPeriod Period04 { get; internal set; }
    public IHolidayPeriod Period05 { get; internal set; }
    public decimal YearVolume { get; internal set; }
}

public interface IHolidayPeriod
{
    string YearId { get; }
    int Year { get; }
    int Id { get; }
    DateOnly From { get; }
    DateOnly To { get; }
    public decimal PeriodOpen { get; }
    public decimal PeriodClose { get; }

    int HowManyDays { get; }
    int HowManyWeeks { get; }

    bool WithinPeriod(DateTime dt);

    List<IQuote> Quotes { get; }
}

public sealed class HolidayPeriod : IHolidayPeriod
{
    public const decimal InvalidPrice = -1m;

    public string YearId { get; internal set; }
    public int Year { get; internal set; }
    public int Id { get; internal set; }
    public DateOnly From { get; internal set; }
    public DateOnly To { get; internal set; }

    public decimal PeriodOpen => Quotes.Any() ? Quotes.First().Open : InvalidPrice;
    public decimal PeriodClose => Quotes.Any() ? Quotes.Last().Open : InvalidPrice;

    public int HowManyDays { get; internal set; }
    public int HowManyWeeks { get; internal set; }

    public List<IQuote> Quotes { get; init; } = new();

    public bool WithinPeriod(DateTime dt)
    {
        DateOnly dtOnly = DateOnly.FromDateTime(dt);
        return (dtOnly >= From && dtOnly <= To);
    }
}

// Holidays periods IMPORTER
internal static class Importer
{
    private static readonly CultureInfo EnglishCulture = new("en-US", false);

    // importer / parser
    internal static IHolidayPeriod PeriodsFromCsv(string csvLine)
    {
        if (string.IsNullOrEmpty(csvLine))
        {
            return new HolidayPeriod();
        }

        string[] values = csvLine.Split(',');
        HolidayPeriod period = new();

        // YearId,Year,Id,From,To,HowManyDays,HowManyWeeks
        // <YearId>,<Year>,<Id>,<From>,<To>,<HowManyDays>,<HowManyWeeks>
        //  P      , Y    , I  , F    , T  , D           , W
        //  0        1      2    3      4    5             6
        HandleLine(period, "P", values[0]);
        HandleLine(period, "Y", values[1]);
        HandleLine(period, "I", values[2]);
        HandleLine(period, "F", values[3]);
        HandleLine(period, "T", values[4]);
        HandleLine(period, "D", values[5]);
        HandleLine(period, "W", values[6]);

        return period;
    }

    private static void HandleLine(HolidayPeriod period, string position, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        switch (position)
        {
            case "P":
                period.YearId = value;
                break;
            case "Y":
                period.Year = Convert.ToInt32(value, EnglishCulture);
                break;
            case "I":
                period.Id = Convert.ToInt32(value, EnglishCulture);
                break;
            case "F":
                period.From = DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                break;
            case "T":
                period.To = DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                break;
            case "D":
                period.HowManyDays = Convert.ToInt32(value, EnglishCulture);
                break;
            case "W":
                period.HowManyWeeks = Convert.ToInt32(value, EnglishCulture);
                break;
        }
    }
}