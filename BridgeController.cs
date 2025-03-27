using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Bridge;

// https://github.com/michielpost/Q42.HueApi/blob/master/src/HueApi.ConsoleSample/Program.cs

namespace PhilipsHueWebhookHandler
{
    public static class BridgeController
    {
        private static string? _bridgeIp;
        private static string? _key;
        private static ILocalHueClient? _client;
        private static readonly string appName = "EmbyHueHandler";

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

        public static async Task<int> DiscoverBridges(bool fromRegisterWithBridge = false)
        {
            var locator = new HttpBridgeLocator();
            IEnumerable<LocatedBridge> bridges = (await locator.LocateBridgesAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false)).ToList();

            if (bridges.Any())
            {
                if(fromRegisterWithBridge == true && bridges.Count() == 1)
                    _bridgeIp = bridges.First().IpAddress;

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

        public static async Task<bool> RegisterWithBridge(string bridgeIp)
        {
            ILocalHueClient client = new LocalHueClient(bridgeIp);

            Console.WriteLine($"Press the button on your Hue Bridge at {bridgeIp}, then press Enter to register ...");
            Console.ReadLine();

            try
            {
                string? appKey = await client.RegisterAsync(appName, "EmbyServer").ConfigureAwait(false);
                if (appKey == null)
                {
                    throw new InvalidOperationException($"Failed to register with the bridge at {bridgeIp}.");
                }

                Console.WriteLine($"App registered successfully! Your app key: {appKey}");
                Console.WriteLine("Your app key is stored in Keys.txt");
                Console.WriteLine("");

                using (StreamWriter file = File.AppendText("Keys.txt"))
                {
                    file.Write($"Bridge IP: {bridgeIp}  Key: {appKey}{Environment.NewLine}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception registering with the bridge at {bridgeIp}: {ex.Message}");
                return false;
            }
        }

        public static string? GetBridgeIp()
        {
            return _bridgeIp;
        }
    }
}
