---
title: McGinley Dynamic
description: Created by John R. McGinley, the McGinley Dynamic is a more responsive variant of exponential moving average.
permalink: /indicators/Dynamic/
image: /assets/charts/Dynamic.png
type: moving-average
layout: indicator
---

# {{ page.title }}

Created by John R. McGinley, the [McGinley Dynamic](https://www.investopedia.com/terms/m/mcginley-dynamic.asp) is a more responsive variant of exponential moving average.
[[Discuss] :speech_balloon:]({{site.github.repository_url}}/discussions/866 "Community discussion about this indicator")

![image]({{site.baseurl}}{{page.image}})

```csharp
// usage (with Close price)
IEnumerable<DynamicResult> results =
  quotes.GetDynamic(lookbackPeriods);
```

## Parameters

| name | type | notes
| -- |-- |--
| `lookbackPeriods` | int | Number of periods (`N`) in the moving average.  Must be greater than 0.

### Historical quotes requirements

You must have at least `2` periods of `quotes`, to cover the initialization periods.  Since this uses a smoothing technique, we recommend you use at least `4×N` data points prior to the intended usage date for better precision.

`quotes` is a collection of generic `TQuote` historical price quotes.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide]({{site.baseurl}}/guide/#historical-quotes) for more information.

## Response

```csharp
IEnumerable<DynamicResult>
```

- This method returns a time series of all available indicator values for the `quotes` provided.
- It always returns the same number of elements as there are in the historical quotes.
- It does not return a single incremental indicator value.
- The first period will have a `null` value since there's not enough data to calculate.

:hourglass: **Convergence Warning**: The first `4×N` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.

### DynamicResult

| name | type | notes
| -- |-- |--
| `Date` | DateTime | Date
| `Dynamic` | double | McGinley Dynamic

### Utilities

- [.Condense()]({{site.baseurl}}/utilities#condense)
- [.Find(lookupDate)]({{site.baseurl}}/utilities#find-indicator-result-by-date)
- [.RemoveWarmupPeriods(qty)]({{site.baseurl}}/utilities#remove-warmup-periods)

See [Utilities and Helpers]({{site.baseurl}}/utilities#utilities-for-indicator-results) for more information.

## Chaining

This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = quotes
    .Use(CandlePart.HL2)
    .GetDynamic(..);
```

Results can be further processed on `Dynamic` with additional chain-enabled indicators.

```csharp
// example
var results = quotes
    .GetDynamic(..)
    .GetRsi(..);
```
