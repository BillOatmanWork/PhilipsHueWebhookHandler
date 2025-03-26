using System.Net;
using System.Text.Json;

namespace PhilipsHueWebhookHandler
{
    internal class PhilipsHueWebhookHandler
    {
        static async Task Main(string[] args)
        {
            File.Delete("PhilipsHueWebhookHandler.log");

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
    }
}
