using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skender.Stock.Indicators;

namespace Jo.Tests.Indicators;

[TestClass]
public class Adx : TestBase
{
    [TestMethod]
    public void Standard()
    {
        int lookbackPeriods = 13;
        List<AdxResult> results = DailyINTCquotes.GetAdx(lookbackPeriods).ToList();

        // assertions

        // proper quantities
        // should always be the same number of results as there is quotes
        Assert.AreEqual(502, results.Count);
        Assert.AreEqual(475, results.Count(x => x.Adx != null));
        Assert.AreEqual(462, results.Count(x => x.Adxr != null));

        // sample values
        AdxResult r19 = results[19];
        Assert.AreEqual(21.0361, NullMath.Round(r19.Pdi, 4));
        Assert.AreEqual(25.0124, NullMath.Round(r19.Mdi, 4));
        Assert.AreEqual(null, r19.Adx);

        AdxResult r29 = results[29];
        Assert.AreEqual(37.9719, NullMath.Round(r29.Pdi, 4));
        Assert.AreEqual(14.1658, NullMath.Round(r29.Mdi, 4));
        Assert.AreEqual(19.7949, NullMath.Round(r29.Adx, 4));

        AdxResult r39 = results[39];
        Assert.IsNull(r29.Adxr);

        AdxResult r40 = results[40];
        Assert.AreEqual(29.1062, NullMath.Round(r40.Adxr, 4));

        AdxResult r248 = results[248];
        Assert.AreEqual(32.3167, NullMath.Round(r248.Pdi, 4));
        Assert.AreEqual(18.2471, NullMath.Round(r248.Mdi, 4));
        Assert.AreEqual(30.5903, NullMath.Round(r248.Adx, 4));
        Assert.AreEqual(29.1252, NullMath.Round(r248.Adxr, 4));

        AdxResult r501 = results[501];
        Assert.AreEqual(17.7565, NullMath.Round(r501.Pdi, 4));
        Assert.AreEqual(31.1510, NullMath.Round(r501.Mdi, 4));
        Assert.AreEqual(34.2987, NullMath.Round(r501.Adx, 4));
    }
}