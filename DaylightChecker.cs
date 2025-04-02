using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PhilipsHueWebhookHandler
{
    public static class DaylightChecker
    {
        private const string SunriseSunsetApiUrlTemplate = "https://api.sunrise-sunset.org/json?lat={0}&lng={1}&formatted=0";
        private static DateTime? lastFetchTime;
        private static DateTime sunrise;
        private static DateTime sunset;

        public static async Task<bool> IsDaylightAsync(double latitude, double longitude)
        {
            try
            {
                if (!lastFetchTime.HasValue || (DateTime.UtcNow - lastFetchTime.Value).TotalHours >= 24)
                {
                    // Fetch new data if it's the first time or more than 24 hours since the last fetch
                    var apiUrl = string.Format(CultureInfo.InvariantCulture, SunriseSunsetApiUrlTemplate, latitude, longitude);
                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.GetStringAsync(apiUrl).ConfigureAwait(false);
                        var jsonResponse = JsonSerializer.Deserialize<SunriseSunsetResponse>(response);

                        if (jsonResponse?.Results is null)
                        {
                            throw new InvalidOperationException("Invalid response from Sunrise-Sunset API.");
                        }

                        sunrise = DateTime.Parse(jsonResponse.Results.Sunrise, CultureInfo.InvariantCulture);
                        sunset = DateTime.Parse(jsonResponse.Results.Sunset, CultureInfo.InvariantCulture);
                        lastFetchTime = DateTime.UtcNow; // Update the fetch timestamp
                    }
                }

                var currentTime = DateTime.UtcNow;
                return currentTime >= sunrise && currentTime <= sunset;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Exception in Sunrise-Sunset API. Exception: {ex.Message}");
            }
        }

        private class SunriseSunsetResponse
        {
            [JsonPropertyName("results")]
            public required SunriseSunsetResults Results { get; set; }
        }

        private class SunriseSunsetResults
        {
            [JsonPropertyName("sunrise")]
            public required string Sunrise { get; set; }

            [JsonPropertyName("sunset")]
            public required string Sunset { get; set; }
        }
    }
}
