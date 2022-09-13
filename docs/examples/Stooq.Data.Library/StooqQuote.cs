using System;
using System.Collections.Generic;
using Skender.Stock.Indicators;

namespace Stooq.Data.Library;

// DATA PERIOD OF TIME
public enum Period : Int32
{
    Undefined = 0,
    _5min = 5,
    _15min = 15,
    _30min = 30,
    _hourly = 101,
    _1hr = _hourly,
    _4hr = _hourly + (4-1),
    _daily = 1001,
    _1Day = _daily,
    _1week = _daily + (7-1),
    _monthly = 2001,
    _1Month = _monthly,
    _10Months = _monthly + (10-1),
    _yearly = 3001,
    _1year = _yearly,
}

// STOOQ QUOTE MODELS
public interface IStooqQuote
{
    public string Ticker { get; }
    public Period DataPeriod { get; }

    List<IQuote> QuotesList { get; }
}

[Serializable]
public sealed class StooqQuote : IStooqQuote
{
    public string Ticker { get; set; }

    public List<IQuote> QuotesList { get; set; } = new List<IQuote>();

    public Period DataPeriod { get; set; }
}