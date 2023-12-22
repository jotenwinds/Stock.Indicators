using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jo.Backtest.Charter;
internal class Class1
{
}

public interface IHolidayPeriodQuote
{
    public DateTime Date { get; }
    public int PeriodId { get; }
    public decimal Open { get; }
    public decimal Close { get; }
    public decimal Volume { get; }
}

[Serializable]
public class HolidayPeriodQuote : IHolidayPeriodQuote
{
    public DateTime Date { get; set; }
    public int PeriodId { get; set; }
    public decimal Open { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
}
