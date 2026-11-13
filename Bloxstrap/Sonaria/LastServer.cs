namespace Bloxstrap.Sonaria
{
    /// <summary>
    /// Remembers the last Creatures of Sonaria server that was joined, so the launcher can offer to rejoin it.
    /// Kept in its own small file in the launcher's folder. Nothing here leaves the PC.
    /// </summary>
    public static class LastServer
    {
        public class Entry
        {
            public long PlaceId { get; set; }

            public string Deeplink { get; set; } = "";

            public DateTime Joined { get; set; }
        }

        // only ever hand Roblox a plain join link in exactly this shape
        private static readonly Regex DeeplinkPattern = new(
            @"^roblox://experiences/start\?placeId=[0-9]+&(gameInstanceId|accessCode)=[A-Za-z0-9_\-]+$",
            RegexOptions.CultureInvariant
        );

        private static string FileLocation => Path.Combine(Paths.Base, "LastServer.json");

        public static void Save(ActivityData data)
        {
            const string LOG_IDENT = "LastServer::Save";

            try
            {
                if (!Paths.Initialized)
                    return;

                string deeplink = data.GetInviteDeeplink(false);

                if (!DeeplinkPattern.IsMatch(deeplink))
                {
                    App.Logger.WriteLine(LOG_IDENT, "Server has no usable join link, not remembering it");
                    return;
                }

                var entry = new Entry
                {
                    PlaceId = data.PlaceId,
                    Deeplink = deeplink,
                    Joined = DateTime.Now
                };

                File.WriteAllText(FileLocation, JsonSerializer.Serialize(entry));
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not remember the server");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public static Entry? Load()
        {
            const string LOG_IDENT = "LastServer::Load";

            try
            {
                if (!Paths.Initialized || !File.Exists(FileLocation))
                    return null;

                var entry = JsonSerializer.Deserialize<Entry>(File.ReadAllText(FileLocation));

                if (entry is null || !DeeplinkPattern.IsMatch(entry.Deeplink))
                    return null;

                return entry;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not read the remembered server");
                App.Logger.WriteException(LOG_IDENT, ex);
                return null;
            }
        }
    }
}
