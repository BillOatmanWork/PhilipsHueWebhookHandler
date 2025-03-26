namespace PhilipsHueWebhookHandler
{
    public static class Utility
    {
        public static void ConsoleWithLog(string text)
        {
            Console.WriteLine(text);

            using (StreamWriter file = File.AppendText("PhilipsHueWebhookHandler.log"))
            {
                file.Write(text + Environment.NewLine);
            }
        }
    }
}
