using System.Net;
using System.Text.Json;

namespace PhilipsHueWebhookHandler
{
    public class PhilipsHueWebhookHandler
    {
        static async Task Main(string[] args)
        {
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
                if (arg == "-?")
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

                    return;
                }

                switch (arg.Substring(0, arg.IndexOf('=')).ToLower())
                {
                    case "-h":
                    case "-help":
                        DisplayHelp();
                        return;

                    default:
                        Utility.ConsoleWithLog("Unknown parameter: " + arg);
                        return;
                }
            }

            BridgeController.InitializeAsync("", "");

            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/webhook/");
            listener.Start();

            Utility.ConsoleWithLog("Webhook endpoint is running on http://localhost:8080/webhook/");

            while (true)
            {
                // Wait for a request
                var context = listener.GetContext();

                if (context.Request.HttpMethod == "POST")
                {
                    using var reader = new StreamReader(context.Request.InputStream);
                    var payload = reader.ReadToEnd();

                    try
                    {
                        var webhookData = JsonSerializer.Deserialize<Root>(payload);
                        if (webhookData is null)
                            Utility.ConsoleWithLog("Deserialized to null payload.");
                        else
                            PayloadDump.DumpPayload(webhookData);
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
            Utility.ConsoleWithLog("\tUser = PD user name.");
            Utility.ConsoleWithLog("\tPass = PD password.");
            Utility.ConsoleWithLog("\tLocale = The case sensitive LanguageCode-CountryCode to use for the guide data. Default is the locale of the computer. Currently supported are en-US, en-GB, and de-DE. Others by request.");
            Utility.ConsoleWithLog("\tnflLength = Length of time to use, in hours, for NFL games.  Used to calculate stop time.  Default 4.");
            Utility.ConsoleWithLog("\tncaafLength = Length of time to use, in hours, for NCAA football games.  Used to calculate stop time.  Default 4.");
            Utility.ConsoleWithLog("\tncaabLength = Length of time to use, in hours, for NCAA basketball games.  Used to calculate stop time.  Default 3.");
            Utility.ConsoleWithLog("\tncaawLength = Length of time to use, in hours, for NCAA Womens basketball games.  Used to calculate stop time.  Default 3.");
            Utility.ConsoleWithLog("\tnhlLength = Length of time to use, in hours, for NHL/ECHL games.  Used to calculate stop time.  Default 3.");
            Utility.ConsoleWithLog("\tnbaLength = Length of time to use, in hours, for NBA games.  Used to calculate stop time.  Default 3.");
            Utility.ConsoleWithLog("\twnbaLength = Length of time to use, in hours, for WNBA games.  Used to calculate stop time.  Default 3.");
            Utility.ConsoleWithLog("\tmlbLength = Length of time to use, in hours, for MLB games.  Used to calculate stop time.  Default 4.");
            Utility.ConsoleWithLog("\tppvLength = Length of time to use, in hours, for PPV events.  Used to calculate stop time.  Default 5.");
            Utility.ConsoleWithLog("\tmlsLength = Length of time to use, in hours, for MLS games.  Used to calculate stop time.  Default 3.");
            Utility.ConsoleWithLog("\tnwslLength = Length of time to use, in hours, for NWSL games.  Used to calculate stop time.  Default 3.");
            Utility.ConsoleWithLog("\tuse12Hour = Use 12 hour format when putting times in desciptions.  Default 24 hour format.");
            Utility.ConsoleWithLog("\tisDST = Assume daylight savings time is in effect.  Default use operating system flag.");
            Utility.ConsoleWithLog("\tisNotDST = Assume daylight savings time is not in effect.  Default use operating system flag.");
            Utility.ConsoleWithLog("\tShortDesc = Do not put the localized date and time in EPG descriptions.");
            Utility.ConsoleWithLog("\tDumpM3U = Optional - The full path and filename of the file you wish to save the m3u data.");
            Utility.ConsoleWithLog("\tDumpXmltv = Optional - The full path and filename of the file you wish to save the xmltv data.");
            Utility.ConsoleWithLog("\tUseTabs = Optional - Use tabs instead of spaces when generating NHL stats in the EPG description.");
            Utility.ConsoleWithLog("\tUseMetric = Optional - Use metric usits for weather instead of the default imperial units.");
            Utility.ConsoleWithLog("\tRetryTennis = Optional - Retry tennis plus channels that report no data. Default no retries.  Note this will cause a significant slow down.");
            Utility.ConsoleWithLog("\tDisableEmoji = Optional - Disable the use of emoji in the guide data.");
            Utility.ConsoleWithLog("\tNoLog = Optional - Do not log activity.");
            Utility.ConsoleWithLog("\tOddsApiKey = Optional - Free X-RapidAPI-Key from https://rapidapi.com/theoddsapi/api/live-sports-odds/ if you want the current money line in the description.");
            Utility.ConsoleWithLog("\tUseCustomLogos = Optional - Use generated cusTOM logo icons for game chnnels.  Read PDF for how to configure.");
            Utility.ConsoleWithLog("\tGuide = Optional - Create PDF with all of the guide data.");
            Utility.ConsoleWithLog("\tMoneyLine = Optional - Create PDF with all of the moneyline betting information.");
            Utility.ConsoleWithLog("\tNote:  No spaces before or after the =, so for example -user=IamTheUser.");
            Utility.ConsoleWithLog("");
            Utility.ConsoleWithLog("");

            Utility.ConsoleWithLog("Hit enter to continue");
            Console.ReadLine();
        }
    }
}
