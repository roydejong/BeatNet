using System.Net;
using System.Text;
using BeatNet.GameServer.BSSB.Models;
using BeatNet.GameServer.Util;
using Serilog;

namespace BeatNet.GameServer.BSSB;

public class BssbClient
{
    public const string BaseUrl = "https://bssb.app";

    private readonly HttpClient _httpClient;
    private ILogger? _logger;

    public BssbClient()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", ServerVersion.UserAgent);
        _httpClient.DefaultRequestHeaders.Add("X-BSSB", "oh yeah");
    }

    public void SetLogger(ILogger logger)
    {
        _logger = logger.ForContext<BssbClient>();
    }
    
    public async Task<AnnounceResponse?> Announce(BssbServer announceData)
    {
        try
        {
            var rawJson = announceData.ToJson();
            _logger?.Information("Sending announce payload: {RawJson}", rawJson);
            var requestContent = new StringContent(rawJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"/api/v1/announce", requestContent);
                
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return AnnounceResponse.FromJson<AnnounceResponse>(responseBody);
        }
        catch (Exception ex)
        {
            LogApiException(ex);
            return null;
        }
    }
    
    public async Task<UnAnnounceResponse?> UnAnnounce(UnAnnounceParams unannounceData)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/v2/unannounce",
                unannounceData.ToRequestContent());
                
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return UnAnnounceResponse.FromJson(responseBody);
        }
        catch (Exception ex)
        {
            LogApiException(ex);
            return null;
        }
    }
    
    private void LogApiException(Exception ex)
    {
        // Try to reduce verbosity of simple network errors in the log
        if (ex is TaskCanceledException)
        {
            _logger?.Warning($"HTTP request was cancelled");
            return;
        }
            
        if (ex is HttpRequestException)
        {
            if (ex.InnerException is WebException)
            {
                ex = ex.InnerException;
            }
            else
            {
                _logger?.Error("HTTP request failed: {ExMessage}", ex.Message);
                return;
            }
        }

        if (ex is WebException)
        {
            _logger?.Error("Web request failed: {ExMessage}", ex.Message);
            return;
        }
            
        // Fallback for unexpected errors
        _logger?.Error(ex, "HTTP request failed: {ExMessage}", ex.Message);
    }
}