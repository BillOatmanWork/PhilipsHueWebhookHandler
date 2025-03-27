using System.Net;
using System.Text.Json;

namespace PhilipsHueWebhookHandler
{
    public class PhilipsHueWebhookHandler
    {
        static async Task Main(string[] args)
        {
            bool run = false;
            string configFilePath;

            File.Delete("PhilipsHueWebhookHandler.log");

            Utility.ConsoleWithLog($"PhilipsHueWebhookHandler version {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
            Utility.ConsoleWithLog("");

            if (args.Length == 0)
            {
                DisplayHelp();
                return;
            }

            Utility.ConsoleWithLog("Passed Parameters: ");
            foreach (string arg in args)
            {
                if (!arg.ToLower().Contains("-key"))
                {
                    Utility.ConsoleWithLog(arg);
                }
            }

            Utility.ConsoleWithLog("");

            foreach (string arg in args)
            {
                if (arg == "-?" || arg.ToLower() == "-h" || arg.ToLower() == "-help")
                {
                    DisplayHelp();
                    return;
                }

                if (arg == "-discover")
                {
                    await BridgeController.DiscoverBridges().ConfigureAwait(false);
                    Console.WriteLine("");
                    Console.WriteLine("Hit enter to continue");
                    Console.ReadLine();
                    return;
                }

                if (arg == "-listscenes")
                {
                    await BridgeController.GetScenes("192.168.50.204", "8fLBDiVmqfRkZxqEZgOSJ4xPHAzjf8FqsnT1loay").ConfigureAwait(false);
                    Console.WriteLine("");
                    Console.WriteLine("Hit enter to continue");
                    Console.ReadLine();
                    return;
                }

                if (arg == "-autoregister")
                {
                    int numBridges = await BridgeController.DiscoverBridges(true).ConfigureAwait(false);
                    Console.WriteLine("");

                    if (numBridges == 0)
                    {
                        Utility.ConsoleWithLog("No bridges found on the network. Nothing to register with.");
                        Console.WriteLine("Hit enter to continue");
                        Console.ReadLine();
                        return;
                    }

                    if (numBridges > 1)
                    {
                        Utility.ConsoleWithLog($"{numBridges} bridges found on the network. Select one and call with -Register=IPAddress.");
                        Console.WriteLine("Hit enter to continue");
                        Console.ReadLine();
                        return;
                    }

                    string? bridgeIp = BridgeController.GetBridgeIp();
                    if (bridgeIp is null)
                    {
                        Utility.ConsoleWithLog("Bridge IP is null. Cannot register with the bridge.");
                        return;
                    }

                    bool success = await BridgeController.RegisterWithBridge(bridgeIp).ConfigureAwait(false);
                    if (success)
                    {
                        Utility.ConsoleWithLog($"Successfully registered with the bridge at {bridgeIp}.");
                    }
                    else
                    {
                        Utility.ConsoleWithLog($"Failed to register with the bridge at {bridgeIp}.");
                    }

                    Console.WriteLine("");
                    Console.WriteLine("Hit enter to continue");
                    Console.ReadLine();

                    return;
                }

                switch (arg.Substring(0, arg.IndexOf('=')).ToLower())
                {
                    case "-run":
                        configFilePath = arg.Substring(arg.IndexOf('=') + 1);
                        if (Configuration.LoadConfig(configFilePath) == false)
                        {
                            Utility.ConsoleWithLog("Error loading configuration file.");
                            return;
                        }

                        run = true;
                        break;

                    default:
                        Utility.ConsoleWithLog("Unknown parameter: " + arg);
                        return;
                }
            }

            if(run == false)
            {
                return;
            }

            if (Configuration.Config?.BridgeIP == null || Configuration.Config?.Key == null)
            {
                Utility.ConsoleWithLog("Configuration is missing BridgeIP or Key.");
                return;
            }

            BridgeController.InitializeAsync(Configuration.Config.BridgeIP, Configuration.Config.Key);

            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/webhook/");
            listener.Start();

            Utility.ConsoleWithLog("Webhook endpoint is running on http://localhost:8080/webhook/");

            while (true)
            {
                var context = listener.GetContext();

                if (context.Request.HttpMethod == "POST")
                {
                    using var reader = new StreamReader(context.Request.InputStream);
                    var payload = reader.ReadToEnd();

                    try
                    {
                        var webhookData = JsonSerializer.Deserialize<Root>(payload);
                        if (webhookData is null)
                        {
                            Utility.ConsoleWithLog("Deserialized to null payload.");
                            break;
                        }

                        PayloadDump.DumpPayload(webhookData);

                        string user = webhookData.User.Name;
                        string device = webhookData.Session.DeviceName;
                        string playbackEvent = webhookData.Event;
                        bool success = false;

                        Device? userConfig = Configuration.GetUserConfig(user, device);

                        if(userConfig is not null)
                        {
                            if(Configuration.Config.LogLevel == "Detail")
                            {
                                Utility.ConsoleWithLog($"User: {user} Device: {device} Event: {playbackEvent}");
                            }                        

                            switch (playbackEvent)
                            {
                                case "playback.start":
                                    if (userConfig.PlayScene != null)
                                    {
                                        success = await BridgeController.SetScene(userConfig.PlayScene).ConfigureAwait(false);
                                    }
                                    break;

                                case "playback.stop":
                                    if (userConfig.StopScene != null)
                                    {
                                        success = await BridgeController.SetScene(userConfig.StopScene).ConfigureAwait(false);
                                    }
                                    break;

                                case "playback.pause":
                                    if (userConfig.PauseScene != null)
                                    {
                                        success = await BridgeController.SetScene(userConfig.PauseScene).ConfigureAwait(false);
                                    }
                                    break;

                                default:
                                    Utility.ConsoleWithLog($"Unknown playback event: {playbackEvent}");
                                    break;
                            }
                        }
                        else
                        {
                            Utility.ConsoleWithLog($"User {user} or device {device} not found in configuration.");
                        }

                        if (Configuration.Config.LogLevel == "Detail")
                        {
                            if(success)
                                Utility.ConsoleWithLog("Successful");
                            else
                                Utility.ConsoleWithLog("Failed");
                        }
                    }
                    catch (Exception ex)
                    {
                        Utility.ConsoleWithLog("Error parsing JSON payload:");
                        Utility.ConsoleWithLog(ex.Message);
                    }

                    // Send a success response to the webhook sender
                    context.Response.StatusCode = 200;
                    using var writer = new StreamWriter(context.Response.OutputStream);
                    writer.Write("Webhook processed successfully.");
                }

                context.Response.Close();
            }
        }

        private static void DisplayHelp()
        {
            Utility.ConsoleWithLog("Parameters: (Case Insensitive)");
            Utility.ConsoleWithLog("\t-Discover  List all Hue Bridges on your network.");
            Utility.ConsoleWithLog("\t-AutoRegister  Register this applicaiton with the bridge. Note only works is there is only 1 bringe on your network.");
            Utility.ConsoleWithLog("\t-Register=<Bridge IP Address> Register this application on the specified bridge.");
            Utility.ConsoleWithLog("\t-ListScenes List all scenes on the bridge.");
            Utility.ConsoleWithLog("\t-Run=<Config JSON file path> Run using the specified config file.");
            Utility.ConsoleWithLog("\tNote:  No spaces before or after the =, so for example -user=IamTheUser.");
            Utility.ConsoleWithLog("");
            Utility.ConsoleWithLog("");

            Utility.ConsoleWithLog("Hit enter to continue");
            Console.ReadLine();
        }
    }
}
