using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FootballFanHub.Exceptions;

namespace FootballFanHub.Services;

public static class HttpClientExtensions
{
    public static async Task<T> GetFromFootballDataAsync<T>(this HttpClient http, string path)
    {
        try
        {
            var response = await http.GetAsync(path);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                throw new ApiException("football-data.org rate limit reached. Try again in a minute.");
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                throw new ApiException("football-data.org rejected the API key, or this data isn't on the free plan.");
            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new ApiException("That team or competition wasn't found.");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>()
                   ?? throw new ApiException("Empty response from football-data.org.");
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException("Couldn't reach football-data.org.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ApiException("football-data.org took too long to respond.", ex);
        }
        catch (JsonException ex)
        {
            throw new ApiException("Unexpected data format from football-data.org.", ex);
        }
    }
}