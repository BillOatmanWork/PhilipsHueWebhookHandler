namespace PhilipsHueWebhookHandler
{
    public class ImageTags
    {
        public string Primary { get; set; } = string.Empty;
    }

    public class Item
    {
        public string Name { get; set; } = string.Empty;
        public string ServerId { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public string Container { get; set; } = string.Empty;
        public string SortName { get; set; } = string.Empty;
        public List<object> ExternalUrls { get; set; } = new List<object>();
        public string Path { get; set; } = string.Empty;
        public List<object> Taglines { get; set; } = new List<object>();
        public List<object> Genres { get; set; } = new List<object>();
        public long RunTimeTicks { get; set; }
        public long Size { get; set; }
        public string FileName { get; set; } = string.Empty;
        public int Bitrate { get; set; }
        public List<object> RemoteTrailers { get; set; } = new List<object>();
        public ProviderIds ProviderIds { get; set; } = new ProviderIds();
        public bool IsFolder { get; set; }
        public string Type { get; set; } = string.Empty;
        public List<object> Studios { get; set; } = new List<object>();
        public List<object> GenreItems { get; set; } = new List<object>();
        public List<object> TagItems { get; set; } = new List<object>();
        public double PrimaryImageAspectRatio { get; set; }
        public ImageTags ImageTags { get; set; } = new ImageTags();
        public List<object> BackdropImageTags { get; set; } = new List<object>();
        public string MediaType { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class PlaybackInfo
    {
        public int PositionTicks { get; set; }
        public int PlaylistIndex { get; set; }
        public int PlaylistLength { get; set; }
        public string PlaySessionId { get; set; } = string.Empty;
    }

    public class ProviderIds
    {
    }

    public class Root
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Event { get; set; } = string.Empty;
        public User User { get; set; } = new User();
        public Item Item { get; set; } = new Item();
        public Server Server { get; set; } = new Server();
        public Session Session { get; set; } = new Session();
        public PlaybackInfo PlaybackInfo { get; set; } = new PlaybackInfo();
    }

    public class Server
    {
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }

    public class Session
    {
        public string RemoteEndPoint { get; set; } = string.Empty;
        public string Client { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string ApplicationVersion { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }

    public class User
    {
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }

    public static class PayloadDump
    {
        public static void DumpPayload(Root payload)
        {
            Utility.ConsoleWithLog("");
            Utility.ConsoleWithLog("Payload Dump: ");
            Utility.ConsoleWithLog("");

            Utility.ConsoleWithLog("Title: " + payload.Title);
            Utility.ConsoleWithLog("Description: " + payload.Description);
            Utility.ConsoleWithLog("Date: " + payload.Date);
            Utility.ConsoleWithLog("Event: " + payload.Event);

            if (payload.User != null)
            {
                Utility.ConsoleWithLog("User Name: " + payload.User.Name);
                Utility.ConsoleWithLog("User Id: " + payload.User.Id);
            }

            if (payload.Item != null)
            {
                Utility.ConsoleWithLog("Item Name: " + payload.Item.Name);
                Utility.ConsoleWithLog("Item ServerId: " + payload.Item.ServerId);
                Utility.ConsoleWithLog("Item Id: " + payload.Item.Id);
                Utility.ConsoleWithLog("Item DateCreated: " + payload.Item.DateCreated);
                Utility.ConsoleWithLog("Item Container: " + payload.Item.Container);
                Utility.ConsoleWithLog("Item SortName: " + payload.Item.SortName);
                Utility.ConsoleWithLog("Item Path: " + payload.Item.Path);
                Utility.ConsoleWithLog("Item RunTimeTicks: " + payload.Item.RunTimeTicks);
                Utility.ConsoleWithLog("Item Size: " + payload.Item.Size);
                Utility.ConsoleWithLog("Item FileName: " + payload.Item.FileName);
                Utility.ConsoleWithLog("Item Bitrate: " + payload.Item.Bitrate);
                Utility.ConsoleWithLog("Item IsFolder: " + payload.Item.IsFolder);
                Utility.ConsoleWithLog("Item Type: " + payload.Item.Type);
                Utility.ConsoleWithLog("Item PrimaryImageAspectRatio: " + payload.Item.PrimaryImageAspectRatio);
                Utility.ConsoleWithLog("Item MediaType: " + payload.Item.MediaType);
                Utility.ConsoleWithLog("Item Width: " + payload.Item.Width);
                Utility.ConsoleWithLog("Item Height: " + payload.Item.Height);

                if (payload.Item.ImageTags != null)
                {
                    Utility.ConsoleWithLog("Item ImageTags Primary: " + payload.Item.ImageTags.Primary);
                }
            }

            if (payload.Server != null)
            {
                Utility.ConsoleWithLog("Server Name: " + payload.Server.Name);
                Utility.ConsoleWithLog("Server Id: " + payload.Server.Id);
                Utility.ConsoleWithLog("Server Version: " + payload.Server.Version);
            }

            if (payload.Session != null)
            {
                Utility.ConsoleWithLog("Session RemoteEndPoint: " + payload.Session.RemoteEndPoint);
                Utility.ConsoleWithLog("Session Client: " + payload.Session.Client);
                Utility.ConsoleWithLog("Session DeviceName: " + payload.Session.DeviceName);
                Utility.ConsoleWithLog("Session DeviceId: " + payload.Session.DeviceId);
                Utility.ConsoleWithLog("Session ApplicationVersion: " + payload.Session.ApplicationVersion);
                Utility.ConsoleWithLog("Session Id: " + payload.Session.Id);
            }

            if (payload.PlaybackInfo != null)
            {
                Utility.ConsoleWithLog("PlaybackInfo PositionTicks: " + payload.PlaybackInfo.PositionTicks);
                Utility.ConsoleWithLog("PlaybackInfo PlaylistIndex: " + payload.PlaybackInfo.PlaylistIndex);
                Utility.ConsoleWithLog("PlaybackInfo PlaylistLength: " + payload.PlaybackInfo.PlaylistLength);
                Utility.ConsoleWithLog("PlaybackInfo PlaySessionId: " + payload.PlaybackInfo.PlaySessionId);
            }

            Utility.ConsoleWithLog("");
        }
    }
}
