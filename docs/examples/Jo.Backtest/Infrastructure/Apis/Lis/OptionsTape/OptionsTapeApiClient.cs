using System.Net.Http;

namespace Jo.Backtest.Infrastructure.Apis.Lis.OptionsTape;

/// <summary>
/// Interface Auctions Api Client
/// </summary>
public partial interface IOptionsTapeApiClient
{
    /// <summary>
    /// HttpClient
    /// </summary>
    public HttpClient HttpClient { get; }

}

internal partial class OptionsTapeApiClient : IOptionsTapeApiClient
{
    public HttpClient HttpClient => _httpClient;
}
