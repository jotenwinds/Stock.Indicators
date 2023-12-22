using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Jo.CustomIndicatorsLibrary.DivePattern;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Skender.Stock.Indicators;

namespace Jo.Tests.Indicators;

[TestClass]
public class Adx : TestBase
{
    public sealed class AdxResultMap : ClassMap<AdxResult>
    {
        public AdxResultMap()
        {
            Map(m => m.Date);
            Map(m => m.Pdi);
            Map(m => m.Mdi);
            Map(m => m.Adx);
            Map(m => m.Adxr);
        }
    }

    public sealed class DivePatternResultMap : ClassMap<DivePatternResult>
    {
        public DivePatternResultMap()
        {
            Map(m => m.Date);
            Map(m => m.Match);

            Map(m => m.Pdi);
            Map(m => m.Mdi);
            Map(m => m.Adx);
            Map(m => m.Adxr);
            Map(m => m.HasPdiCrossMdiAbove);
            Map(m => m.HasMdiCrossPdiAbove);
            Map(m => m.IsPdiAboveMdi);
            Map(m => m.IsMdiAbovePdi);

            Map(m => m.Ppo);
            Map(m => m.Signal);
            Map(m => m.Histogram);
            Map(m => m.HasPpoCrossSignalAbove);
            Map(m => m.HasSignalCrossPpoAbove);
            Map(m => m.IsPpoBearish);
            Map(m => m.IsPpoBullish);
        }
    }

    [TestMethod]
    public void Adx_Standard_13()
    {
        int lookbackPeriods = 13;
        List<AdxResult> results = DailyINTCquotes.GetAdx(lookbackPeriods).ToList();


        DirectoryInfo rootDi = new DirectoryInfo(Directory.GetCurrentDirectory());

        // Write to JSON file
        {
            string targetFileNameJson = Path.Combine(rootDi.FullName, $"intc-adx-{lookbackPeriods}.json");
            System.Console.WriteLine($"Writing data to '{targetFileNameJson}'");

            if (!string.IsNullOrEmpty(targetFileNameJson))
            {
                var json = JsonConvert.SerializeObject(results);
                File.WriteAllText(targetFileNameJson, json, System.Text.Encoding.UTF8);
            }
        }

        // Write to CSV file
        {
            string targetFileNameCsv = Path.Combine(rootDi.FullName, $"intc-adx-{lookbackPeriods}.csv");
            using (var writer = new StreamWriter(targetFileNameCsv))
            {
                using (var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture))
                {
                    csvWriter.Context.RegisterClassMap<AdxResultMap>(); // Provides the order of the fields
                    csvWriter.WriteHeader<AdxResult>(); // Writes the headers
                    csvWriter.NextRecord(); // adds new line after header
                    csvWriter.WriteRecords(results);    // writes the records
                }
            }
        }


        //// assertions

        //// proper quantities
        //// should always be the same number of results as there is quotes
        //Assert.AreEqual(502, results.Count);
        //Assert.AreEqual(475, results.Count(x => x.Adx != null));
        //Assert.AreEqual(462, results.Count(x => x.Adxr != null));

        //// sample values
        //AdxResult r19 = results[19];
        //Assert.AreEqual(21.0361, NullMath.Round(r19.Pdi, 4));
        //Assert.AreEqual(25.0124, NullMath.Round(r19.Mdi, 4));
        //Assert.AreEqual(null, r19.Adx);

        //AdxResult r29 = results[29];
        //Assert.AreEqual(37.9719, NullMath.Round(r29.Pdi, 4));
        //Assert.AreEqual(14.1658, NullMath.Round(r29.Mdi, 4));
        //Assert.AreEqual(19.7949, NullMath.Round(r29.Adx, 4));

        //AdxResult r39 = results[39];
        //Assert.IsNull(r29.Adxr);

        //AdxResult r40 = results[40];
        //Assert.AreEqual(29.1062, NullMath.Round(r40.Adxr, 4));

        //AdxResult r248 = results[248];
        //Assert.AreEqual(32.3167, NullMath.Round(r248.Pdi, 4));
        //Assert.AreEqual(18.2471, NullMath.Round(r248.Mdi, 4));
        //Assert.AreEqual(30.5903, NullMath.Round(r248.Adx, 4));
        //Assert.AreEqual(29.1252, NullMath.Round(r248.Adxr, 4));

        //AdxResult r501 = results[501];
        //Assert.AreEqual(17.7565, NullMath.Round(r501.Pdi, 4));
        //Assert.AreEqual(31.1510, NullMath.Round(r501.Mdi, 4));
        //Assert.AreEqual(34.2987, NullMath.Round(r501.Adx, 4));
    }


    [TestMethod]
    public void DivePattern_Standard_13_21_8_21()
    {
        int adxLength = 14;
        int adxThreshold = 21;
        int fastLength = 13;
        int slowLength = 21;
        int signalSmoothingLength = 8;
        List<DivePatternResult> results = DailyINTCquotes.GetDivePattern(adxLength, adxThreshold, fastLength, slowLength, signalSmoothingLength).ToList();


        DirectoryInfo rootDi = new DirectoryInfo(Directory.GetCurrentDirectory());

        // Write to JSON file
        {
            string targetFileNameJson = Path.Combine(rootDi.FullName, $"intc-divePattern-{adxLength}_{fastLength}_{slowLength}_{signalSmoothingLength}.json");
            System.Console.WriteLine($"Writing JSON data to '{targetFileNameJson}'");
            if (!string.IsNullOrEmpty(targetFileNameJson))
            {
                var json = JsonConvert.SerializeObject(results);
                File.WriteAllText(targetFileNameJson, json, System.Text.Encoding.UTF8);
            }
        }

        // Write to CSV file
        {
            string targetFileNameCsv = Path.Combine(rootDi.FullName, $"intc-divePattern-{adxLength}_{fastLength}_{slowLength}_{signalSmoothingLength}.csv");
            System.Console.WriteLine($"Writing CVS data to '{targetFileNameCsv}'");
            using (var writer = new StreamWriter(targetFileNameCsv))
            {
                using (var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture))
                {
                    csvWriter.Context.RegisterClassMap<DivePatternResultMap>(); // Provides the order of the fields
                    csvWriter.WriteHeader<DivePatternResult>(); // Writes the headers
                    csvWriter.NextRecord(); // adds new line after header
                    csvWriter.WriteRecords(results);    // writes the records
                }
            }
        }


        //// assertions

        //// proper quantities
        //// should always be the same number of results as there is quotes
        //Assert.AreEqual(502, results.Count);
        //Assert.AreEqual(475, results.Count(x => x.Adx != null));
        //Assert.AreEqual(462, results.Count(x => x.Adxr != null));

        //// sample values
        //AdxResult r19 = results[19];
        //Assert.AreEqual(21.0361, NullMath.Round(r19.Pdi, 4));
        //Assert.AreEqual(25.0124, NullMath.Round(r19.Mdi, 4));
        //Assert.AreEqual(null, r19.Adx);

        //AdxResult r29 = results[29];
        //Assert.AreEqual(37.9719, NullMath.Round(r29.Pdi, 4));
        //Assert.AreEqual(14.1658, NullMath.Round(r29.Mdi, 4));
        //Assert.AreEqual(19.7949, NullMath.Round(r29.Adx, 4));

        //AdxResult r39 = results[39];
        //Assert.IsNull(r29.Adxr);

        //AdxResult r40 = results[40];
        //Assert.AreEqual(29.1062, NullMath.Round(r40.Adxr, 4));

        //AdxResult r248 = results[248];
        //Assert.AreEqual(32.3167, NullMath.Round(r248.Pdi, 4));
        //Assert.AreEqual(18.2471, NullMath.Round(r248.Mdi, 4));
        //Assert.AreEqual(30.5903, NullMath.Round(r248.Adx, 4));
        //Assert.AreEqual(29.1252, NullMath.Round(r248.Adxr, 4));

        //AdxResult r501 = results[501];
        //Assert.AreEqual(17.7565, NullMath.Round(r501.Pdi, 4));
        //Assert.AreEqual(31.1510, NullMath.Round(r501.Mdi, 4));
        //Assert.AreEqual(34.2987, NullMath.Round(r501.Adx, 4));
    }
}