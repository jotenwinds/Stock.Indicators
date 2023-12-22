using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Jo.Backtest.Charter;

internal class HolidaysPeriodsData
{
    // Read & Parse the Holidays periods data file.
    internal static IEnumerable<IHolidayPeriod> GetHolidaysPeriods()
        => File.ReadAllLines("_common/data/Holidays-Periods-From-2000-To-2024.csv")
            .Skip(1)    // Skip the header line
            .Select(Importer.PeriodsFromCsv)
            .OrderBy(x => x.YearId)
            .ToList();

    internal static Dictionary<int, IHolidayYear> GetHolidayYears()
    {
        var years = GetHolidaysPeriods().GroupBy(x => x.Year);
        var result = new Dictionary<int, IHolidayYear>();
        foreach (IGrouping<int, IHolidayPeriod> holidayPeriods in years)
        {
            var year = new HolidayYear();
            year.Year = holidayPeriods.Key;
            year.Period01 = holidayPeriods.First(x => x.Id == 1);
            year.Period02 = holidayPeriods.First(x => x.Id == 2);
            year.Period03 = holidayPeriods.First(x => x.Id == 3);
            year.Period04 = holidayPeriods.First(x => x.Id == 4);
            year.Period05 = holidayPeriods.First(x => x.Id == 5);
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
    int HowManyDays { get; }
    int HowManyWeeks { get; }

    bool WithinPeriod(DateTime dt);
}

public sealed class HolidayPeriod : IHolidayPeriod
{
    public string YearId { get; internal set; }
    public int Year { get; internal set; }
    public int Id { get; internal set; }
    public DateOnly From { get; internal set; }
    public DateOnly To { get; internal set; }
    public int HowManyDays { get; internal set; }
    public int HowManyWeeks { get; internal set; }

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