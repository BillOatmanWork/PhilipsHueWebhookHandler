using System.Text.Json;

namespace PhilipsHueWebhookHandler
{
    public static class Configuration
    {
        public static ConfigRoot? Config { get; set; }

        public static bool LoadConfig(string configFilePath)
        {
            try
            {
                Config = JsonSerializer.Deserialize<ConfigRoot>(File.ReadAllText(configFilePath));
            }
            catch (Exception ex)
            {
                Utility.ConsoleWithLog($"Error loading configuration file: {ex.Message}");
                return false;
            }

            return true;
        }

        public static Device? GetUserConfig(string userName, string deviceName)
        {
            if (Config is not null)
            {
                foreach (EmbyUser user in Config.Users!)
                {
                    if (user.Name == userName)
                    {
                        foreach (Device device in user.Devices!)
                        {
                            if (device.Name == deviceName)
                            {
                                return device;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }

    #region Classes
    public class ConfigRoot
    {
        public string? Description { get; set; }
        public string? BridgeIP { get; set; }
        public string? Key { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? LogLevel { get; set; }
        public List<EmbyUser>? Users { get; set; }
    }

    public class Device
    {
        public string? Name { get; set; }
        public bool? DaytimeOverride { get; set; }
        public string? PlayScene { get; set; }
        public string? PauseScene { get; set; }
        public string? UnPauseScene { get; set; }
        public string? StopScene { get; set; }
    }

    public class EmbyUser
    {
        public string? Name { get; set; }
        public List<Device>? Devices { get; set; }
    }

    #endregion Classes
}
