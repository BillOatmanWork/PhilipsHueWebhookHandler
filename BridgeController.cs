using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Bridge;
using Q42.HueApi.Models.Groups;
using System.Collections;
using System.Reflection.Metadata.Ecma335;

// https://github.com/michielpost/Q42.HueApi/blob/master/src/HueApi.ConsoleSample/Program.cs

namespace PhilipsHueWebhookHandler
{
    public static class BridgeController
    {
        private static string? _bridgeIp;
        private static string? _key;
        private static ILocalHueClient? _client = null;

        public static void InitializeAsync(string ip, string key)
        {
            _bridgeIp = ip;
            _key = key;

            // Initialize the client
            if (!string.IsNullOrEmpty(_bridgeIp))
                _client = new LocalHueClient(_bridgeIp);

            if (!string.IsNullOrEmpty(_key) && _client is not null)
                _client.Initialize(_key);
        }

        public static async Task<int> DiscoverBridges()
        {
            // Discover Hue Bridges on the network
            var locator = new HttpBridgeLocator();
            IEnumerable<LocatedBridge> bridges = (await locator.LocateBridgesAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false)).ToList();

            if (bridges.Any())
            {
                foreach (var bridge in bridges)
                {
                    Utility.ConsoleWithLog($"Bridge found: IP = {bridge.IpAddress}");
                }
            }
            else
            {
                Utility.ConsoleWithLog("No bridges found on the network.");
            }

            return bridges.Count();
        }
    }
}
