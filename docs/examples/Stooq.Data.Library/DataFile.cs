namespace Stooq.Data.Library;

public class DataFile
{
    public string Period { get; set; }
    public string Market { get; set; }
    public string Ticker { get; set; }
    public string TickerMarket => $"{Ticker?.ToUpper().Trim()}.{Market?.ToUpper().Trim()}";

    public string RelativePath { get; set; }
    public string FileName { get; set; }
}
