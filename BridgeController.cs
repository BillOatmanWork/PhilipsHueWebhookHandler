using HueApi;
using HueApi.BridgeLocator;
using HueApi.Models;
using HueApi.Models.Requests;

// https://github.com/michielpost/Q42.HueApi/blob/master/src/HueApi.ConsoleSample/Program.cs

namespace PhilipsHueWebhookHandler
{
    public static class BridgeController
    {
        private static string? _bridgeIp;
        private static string? _key;
        private static LocalHueApi? _client;
        private const string _appName = "EmbyHueHandler";
        private static List<Scene> _scenes = new List<Scene>();

        public static void InitializeAsync(string ip, string key)
        {
            _bridgeIp = ip;
            _key = key;

            // Initialize the client
            if (!string.IsNullOrEmpty(_bridgeIp) && !string.IsNullOrEmpty(_key))
                _client = new LocalHueApi(_bridgeIp, _key);
        }

        public static async Task<int> DiscoverBridges(bool fromRegisterWithBridge = false)
        {
            var locator = new HttpBridgeLocator();
            IEnumerable<LocatedBridge> bridges = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

            if (bridges.Any())
            {
                if (fromRegisterWithBridge == true && bridges.Count() == 1)
                    _bridgeIp = bridges.First().BridgeId;

                foreach (var bridge in bridges)
                {
                    Utility.ConsoleWithLog($"Bridge found: IP = {bridge.BridgeId}");
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
            Console.WriteLine($"Press the button on your Hue Bridge at {bridgeIp}, then press Enter to register ...");
            Console.ReadLine();

            try
            {
                var result = await LocalHueApi.RegisterAsync(bridgeIp, _appName, "EmbyServer").ConfigureAwait(false);
                if (result is null || string.IsNullOrEmpty(result.Username))
                {
                    throw new InvalidOperationException($"Failed to register with the bridge at {bridgeIp}.");
                }

                string appKey = result.Username;
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
            var client = new LocalHueApi(bridgeIp, appKey);

            try
            {
                var scenesResponse = await client.Scene.GetAllAsync().ConfigureAwait(false);
                _scenes = scenesResponse.Data.ToList();

                Utility.ConsoleWithLog($"Scenes available on the bridge at {bridgeIp}:");
                foreach (var scene in _scenes)
                {
                    Utility.ConsoleWithLog($"{scene.Metadata?.Name}");
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
                    var scenesResponse = await _client.Scene.GetAllAsync().ConfigureAwait(false);
                    _scenes = scenesResponse.Data.ToList();
                }

                var scene = _scenes.FirstOrDefault(s => s.Metadata?.Name?.Equals(sceneName, StringComparison.OrdinalIgnoreCase) == true);
                if (scene is null)
                {
                    Utility.ConsoleWithLog($"Scene {sceneName} not found on the bridge.");
                    return false;
                }

                if (logLevel.ToLower() == "detail")
                    Utility.ConsoleWithLog($"SetScene: Scene: {sceneName}");

                try
                {
                    var updateScene = new UpdateScene()
                    {
                        Recall = new Recall { Action = SceneRecallAction.active }
                    };

                    var result = await _client.Scene.UpdateAsync(scene.Id, updateScene).ConfigureAwait(false);

                    if (result.HasErrors)
                    {
                        if (logLevel.ToLower() == "detail")
                        {
                            Utility.ConsoleWithLog($"Error(s) setting scene {sceneName}: ");
                            foreach (var error in result.Errors)
                            {
                                Utility.ConsoleWithLog($"Error: {error.Description}");
                            }
                        }
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    if (logLevel.ToLower() == "detail")
                    {
                        Utility.ConsoleWithLog($"Exception setting scene {sceneName}: {ex.Message}");
                    }
                    return false;
                }
            }

            return false;
        }
    }
}
