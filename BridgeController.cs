using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models;
using Q42.HueApi.Models.Bridge;
using Q42.HueApi.Models.Groups;

// https://github.com/michielpost/Q42.HueApi/blob/master/src/HueApi.ConsoleSample/Program.cs

namespace PhilipsHueWebhookHandler
{
    public static class BridgeController
    {
        private static string? _bridgeIp;
        private static string? _key;
        private static ILocalHueClient? _client;
        private const string _appName = "EmbyHueHandler";
        private static List<Scene> _scenes = new List<Scene>();

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
                if (fromRegisterWithBridge == true && bridges.Count() == 1)
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
                string? appKey = await client.RegisterAsync(_appName, "EmbyServer").ConfigureAwait(false);
                if (appKey is null)
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

        public static async Task GetScenes(string bridgeIp, string appKey)
        {
            var client = new LocalHueClient(bridgeIp);
            client.Initialize(appKey);

            try
            {
                var scenes = await client.GetScenesAsync().ConfigureAwait(false);
                _scenes = scenes.ToList();

                Utility.ConsoleWithLog($"Scenes available on the bridge at {bridgeIp}:");
                foreach (var scene in _scenes)
                {
                    Utility.ConsoleWithLog($"{scene.Name}");
                }
            }
            catch (Exception ex)
            {
                Utility.ConsoleWithLog($"Error: {ex.Message}");
            }
        }

        public static async Task<bool> SetScene(string sceneName, string logLevel, bool considerDaylight)
        {
            if (considerDaylight is true && Configuration.Config is not null && Configuration.Config.Latitude != 0 && Configuration.Config.Longitude != 0)
            {
                try
                {
                    bool isDaylight = await DaylightChecker.IsDaylightAsync(Configuration.Config.Latitude, Configuration.Config.Longitude).ConfigureAwait(false);
                    if (isDaylight)
                    {
                        if (logLevel.ToLower() == "detail")
                            Utility.ConsoleWithLog("SetScene: Scene not set due to it being daylight.");
                    }
                }
                catch (Exception ex)
                {
                    Utility.ConsoleWithLog($"{ex.Message}");
                    return false;
                }

                return false;
            }

            if (_client is not null)
            {
                if(_scenes.Count == 0)
                {
                    var scenes = await _client.GetScenesAsync().ConfigureAwait(false);
                    _scenes = scenes.ToList();
                }

                var scene = _scenes.FirstOrDefault(s => s.Name.Equals(sceneName, StringComparison.OrdinalIgnoreCase));
                if (scene is null)
                {
                    Utility.ConsoleWithLog($"Scene {sceneName} not found on the bridge.");
                    return false;
                }

                if (logLevel .ToLower()== "detail")
                    Utility.ConsoleWithLog($"SetScene: Scene: {sceneName}");

                if (scene is not null)
                {
                    HueResults result = await _client.RecallSceneAsync(scene.Id).ConfigureAwait(false);
                    if (!result.HasErrors())
                    {
                        return true;
                    }
                    else
                    {
                        if (logLevel .ToLower()== "detail")
                        {
                            Utility.ConsoleWithLog($"Error(s) setting scene {sceneName}: ");
                            foreach (var error in result.Errors)
                            {
                                Utility.ConsoleWithLog($"Error: {error.Error!.Description}");
                            }
                        }

                        return false;
                    }
                }
            }

            return false;
        }
    }
}
