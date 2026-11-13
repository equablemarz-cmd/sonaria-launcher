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

            /// <summary>
            /// When the server was left. Empty while still in it.
            /// </summary>
            public DateTime? Left { get; set; }
        }

        // only ever hand Roblox a plain join link in exactly this shape
        private static readonly Regex DeeplinkPattern = new(
            @"^roblox://experiences/start\?placeId=[0-9]+&(gameInstanceId|accessCode)=[A-Za-z0-9_\-]+$",
            RegexOptions.CultureInvariant
        );

        // a Roblox server ID (JobId), which is always a GUID
        private static readonly Regex ServerIdPattern = new(
            @"[?&]gameInstanceId=([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})$",
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

        /// <summary>
        /// Notes that the remembered server has been left, so it is no longer offered as the current one.
        /// It can still be rejoined.
        /// </summary>
        public static void MarkLeft()
        {
            const string LOG_IDENT = "LastServer::MarkLeft";

            try
            {
                var entry = Load();

                if (entry is null || entry.Left is not null)
                    return;

                entry.Left = DateTime.Now;

                File.WriteAllText(FileLocation, JsonSerializer.Serialize(entry));
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not note that the server was left");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        /// <summary>
        /// A link other players can click to join the Creatures of Sonaria server you are in right now.
        /// Empty when you aren't in a public CoS server at the moment.
        /// </summary>
        public static string? GetCurrentServerLink()
        {
            var entry = Load();

            if (entry is null || entry.Left is not null)
                return null;

            var match = ServerIdPattern.Match(entry.Deeplink);

            if (!match.Success || !IsRobloxRunningSince(entry.Joined))
                return null;

            return $"{App.ServerLinkPage}?s={match.Groups[1].Value.ToLowerInvariant()}";
        }

        /// <summary>
        /// Whether a Roblox player that was already open at the given time is still running,
        /// which rules out a server remembered from an earlier session (for example after Roblox crashed)
        /// </summary>
        private static bool IsRobloxRunningSince(DateTime joined)
        {
            foreach (var process in Process.GetProcessesByName(App.RobloxPlayerAppName))
            {
                using (process)
                {
                    try
                    {
                        if (process.StartTime <= joined)
                            return true;
                    }
                    catch (Exception)
                    {
                        // can't read its start time, so give it the benefit of the doubt
                        return true;
                    }
                }
            }

            return false;
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
