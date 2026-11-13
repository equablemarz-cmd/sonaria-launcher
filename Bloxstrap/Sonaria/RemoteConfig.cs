namespace Bloxstrap.Sonaria
{
    public class RemoteNotice
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("link")]
        public string? Link { get; set; }
    }

    public class RemoteSwitches
    {
        /// <summary>
        /// Stops TickTracker from being downloaded or started
        /// </summary>
        [JsonPropertyName("tickTracker")]
        public bool TickTracker { get; set; }

        /// <summary>
        /// Holds the frame rate limit at the highest value Roblox itself supports
        /// </summary>
        [JsonPropertyName("fpsAbove240")]
        public bool FpsAbove240 { get; set; }

        /// <summary>
        /// Stops the engine settings file (FPS boost included) from being given to Roblox
        /// </summary>
        [JsonPropertyName("engineSettings")]
        public bool EngineSettings { get; set; }

        /// <summary>
        /// Stops the font and cursor mods from being given to Roblox
        /// </summary>
        [JsonPropertyName("mods")]
        public bool Mods { get; set; }

        /// <summary>
        /// Takes the community server button off the Discord status (for example if the invite link stops working)
        /// </summary>
        [JsonPropertyName("discordServerButton")]
        public bool DiscordServerButton { get; set; }
    }

    public class RemoteConfigData
    {
        [JsonPropertyName("notice")]
        public RemoteNotice? Notice { get; set; }

        [JsonPropertyName("disable")]
        public RemoteSwitches? Disable { get; set; }
    }

    /// <summary>
    /// A small file kept in the launcher's own GitHub repository that lets Sonaria Software show a notice
    /// on the launch menu, or switch one of the launcher's features off for everyone without a new version
    /// (for example if a game update breaks TickTracker).
    ///
    /// It can only ever turn features off or show a line of text. It is a plain download: nothing about
    /// the user or their PC is sent. If it can't be fetched, the last copy that was fetched is used, and
    /// with no copy at all nothing is switched off.
    /// </summary>
    public static class RemoteConfig
    {
        private const int MaxFileLength = 16 * 1024;

        private const int MaxNoticeLength = 240;

        private static readonly TimeSpan FetchTimeout = TimeSpan.FromSeconds(4);

        private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(5);

        private static readonly string[] AllowedLinkPrefixes =
        {
            $"https://github.com/{App.ProjectRepository}",
            "https://discord.gg/"
        };

        private static readonly object _lock = new();

        private static Task? _refreshTask;

        private static DateTime _refreshStarted = DateTime.MinValue;

        private static RemoteSwitches _switches = new();

        private static string Url => $"https://raw.githubusercontent.com/{App.ProjectRepository}/config/launcher.json";

        private static string CacheLocation => Path.Combine(Paths.Base, "RemoteConfig.json");

        public static RemoteSwitches Disabled => _switches;

        /// <summary>
        /// The notice to show on the launch menu, already cleaned up. Empty when there is none.
        /// </summary>
        public static string NoticeText { get; private set; } = "";

        /// <summary>
        /// Where the notice links to. Empty when it has no link, or the link isn't one the launcher allows.
        /// </summary>
        public static string NoticeLink { get; private set; } = "";

        public static bool TickTrackerAllowed => !_switches.TickTracker;

        /// <summary>
        /// A plain line naming whatever has been switched off, so it is never done silently. Empty when nothing is.
        /// </summary>
        public static string DisabledSummary
        {
            get
            {
                var names = new List<string>();

                if (_switches.TickTracker)
                    names.Add("TickTracker");

                if (_switches.FpsAbove240)
                    names.Add($"frame rates above {RobloxSettings.HighestSupported}");

                if (_switches.EngineSettings)
                    names.Add("engine settings and FPS boost");

                if (_switches.Mods)
                    names.Add("font and cursor mods");

                if (_switches.DiscordServerButton)
                    names.Add("the Discord server button");

                if (names.Count == 0)
                    return "";

                return "Switched off by Sonaria Software for now: " + String.Join(", ", names) + ".";
            }
        }

        /// <summary>
        /// Uses the copy saved by the last successful fetch. Never touches the network.
        /// </summary>
        public static void LoadCached()
        {
            const string LOG_IDENT = "RemoteConfig::LoadCached";

            try
            {
                if (String.IsNullOrEmpty(Paths.Base) || !File.Exists(CacheLocation))
                    return;

                var info = new FileInfo(CacheLocation);

                if (info.Length > MaxFileLength)
                    return;

                Apply(File.ReadAllText(CacheLocation));
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not read the saved copy, carrying on without it");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        /// <summary>
        /// Fetches the current file. Never throws, and gives up after a few seconds so it can't hold up a launch.
        /// Calls made within a few minutes of each other share one fetch.
        /// </summary>
        public static Task Refresh()
        {
            lock (_lock)
            {
                if (_refreshTask is null || DateTime.UtcNow - _refreshStarted > RefreshInterval)
                {
                    _refreshStarted = DateTime.UtcNow;
                    _refreshTask = Task.Run(Fetch);
                }

                return _refreshTask;
            }
        }

        private static async Task Fetch()
        {
            const string LOG_IDENT = "RemoteConfig::Fetch";

            try
            {
                using var cts = new CancellationTokenSource(FetchTimeout);
                using var response = await App.HttpClient.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead, cts.Token);

                response.EnsureSuccessStatusCode();

                if (response.Content.Headers.ContentLength > MaxFileLength)
                    throw new InvalidDataException("The file is larger than the launcher accepts");

                string json = await response.Content.ReadAsStringAsync(cts.Token);

                if (json.Length > MaxFileLength)
                    throw new InvalidDataException("The file is larger than the launcher accepts");

                // throws if it isn't valid, in which case the saved copy is left as it is
                Apply(json);

                if (!String.IsNullOrEmpty(Paths.Base) && Directory.Exists(Paths.Base))
                {
                    string tempLocation = CacheLocation + ".tmp";

                    File.WriteAllText(tempLocation, json);
                    File.Move(tempLocation, CacheLocation, true);
                }

                App.Logger.WriteLine(LOG_IDENT, "Fetched");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not fetch, keeping what was already loaded");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private static void Apply(string json)
        {
            var data = JsonSerializer.Deserialize<RemoteConfigData>(json);

            if (data is null)
                throw new InvalidDataException("The file is empty");

            string text = CleanText(data.Notice?.Text);
            string link = data.Notice?.Link?.Trim() ?? "";

            if (text.Length == 0 || !AllowedLinkPrefixes.Any(x => link.StartsWith(x, StringComparison.Ordinal)))
                link = "";

            lock (_lock)
            {
                _switches = data.Disable ?? new();
                NoticeText = text;
                NoticeLink = link;
            }
        }

        private static string CleanText(string? text)
        {
            if (String.IsNullOrWhiteSpace(text))
                return "";

            // one line of plain text: control characters (line breaks included) become spaces
            var builder = new StringBuilder(text.Length);

            foreach (char c in text)
                builder.Append(Char.IsControl(c) ? ' ' : c);

            string cleaned = Regex.Replace(builder.ToString(), " {2,}", " ").Trim();

            if (cleaned.Length > MaxNoticeLength)
                cleaned = cleaned.Substring(0, MaxNoticeLength - 1).TrimEnd() + "…";

            return cleaned;
        }
    }
}
