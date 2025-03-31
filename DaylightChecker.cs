using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PhilipsHueWebhookHandler
{
    public static class DaylightChecker
    {
        private const string SunriseSunsetApiUrlTemplate = "https://api.sunrise-sunset.org/json?lat={0}&lng={1}&formatted=0";

        public static async Task<bool> IsDaylightAsync(double latitude, double longitude)
        {
            try
            {
                var apiUrl = string.Format(CultureInfo.InvariantCulture, SunriseSunsetApiUrlTemplate, latitude, longitude);
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(apiUrl).ConfigureAwait(false);
                    var jsonResponse = JsonSerializer.Deserialize<SunriseSunsetResponse>(response);

                    if (jsonResponse?.Results == null)
                    {
                        throw new InvalidOperationException("Invalid response from Sunrise-Sunset API.");
                    }

                    var sunrise = DateTime.Parse(jsonResponse.Results.Sunrise, CultureInfo.InvariantCulture);
                    var sunset = DateTime.Parse(jsonResponse.Results.Sunset, CultureInfo.InvariantCulture);
                    var currentTime = DateTime.UtcNow;

                    // Return true if the current UTC time is between sunrise and sunset
                    return currentTime >= sunrise && currentTime <= sunset;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
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
