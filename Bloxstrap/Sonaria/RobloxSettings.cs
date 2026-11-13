namespace Bloxstrap.Sonaria
{
    /// <summary>
    /// Sets Roblox's own "Maximum Frame Rate" setting before Roblox starts.
    /// This is the same value Roblox writes to its settings file when you change it in the in-game menu.
    /// Roblox's menu stops at 240 and Roblox does not officially support going higher; the option above
    /// that is there because it was asked for, and is labelled as such.
    /// </summary>
    public static class RobloxSettings
    {
        /// <summary>
        /// 0 means "leave whatever is set in Roblox alone"
        /// </summary>
        public static readonly IReadOnlyList<int> FramerateLimits = new[] { 0, 60, 120, 144, 240, 320 };

        /// <summary>
        /// The highest frame rate Roblox's own settings menu offers
        /// </summary>
        public const int HighestSupported = 240;

        private static readonly Regex FramerateCapPattern = new(
            "(<int name=\"FramerateCap\">)(-?[0-9]+)(</int>)",
            RegexOptions.CultureInvariant
        );

        private static string FileLocation => Path.Combine(Paths.LocalAppData, "Roblox", "GlobalBasicSettings_13.xml");

        public static string GetLabel(int limit)
        {
            if (limit <= 0)
                return "Leave as set in Roblox";

            return limit > HighestSupported ? $"{limit} FPS (above Roblox's limit)" : $"{limit} FPS";
        }

        public static void ApplyFramerateLimit()
        {
            const string LOG_IDENT = "RobloxSettings::ApplyFramerateLimit";

            int limit = App.Settings.Prop.FramerateLimit;

            if (limit <= 0)
                return;

            if (!FramerateLimits.Contains(limit))
            {
                App.Logger.WriteLine(LOG_IDENT, $"{limit} is not one of the launcher's frame rate options, ignoring it");
                return;
            }

            if (limit > HighestSupported && RemoteConfig.Disabled.FpsAbove240)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Frame rates above {HighestSupported} are switched off by Sonaria Software for now, using {HighestSupported}");
                limit = HighestSupported;
            }

            try
            {
                if (!File.Exists(FileLocation))
                {
                    // Roblox creates this the first time it runs, so it'll be picked up on the next launch
                    App.Logger.WriteLine(LOG_IDENT, "Roblox has not created its settings file yet");
                    return;
                }

                byte[] bytes = File.ReadAllBytes(FileLocation);
                bool hasBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
                int offset = hasBom ? 3 : 0;

                // if the file isn't valid UTF-8, leave it completely alone rather than risk damaging it
                string text = new UTF8Encoding(false, true).GetString(bytes, offset, bytes.Length - offset);

                Match match = FramerateCapPattern.Match(text);

                if (!match.Success)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Roblox's settings file has no frame rate entry yet");
                    return;
                }

                string value = limit.ToString(CultureInfo.InvariantCulture);

                if (match.Groups[2].Value == value)
                    return;

                string updated = FramerateCapPattern.Replace(text, m => m.Groups[1].Value + value + m.Groups[3].Value, 1);

                // write the new copy beside the original and swap it in, so the file is never left half-written
                string tempLocation = FileLocation + ".sonaria-tmp";

                File.WriteAllText(tempLocation, updated, new UTF8Encoding(hasBom));
                File.Move(tempLocation, FileLocation, true);

                App.Logger.WriteLine(LOG_IDENT, $"Set Roblox's maximum frame rate to {value} (was {match.Groups[2].Value})");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not set Roblox's maximum frame rate");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }
    }
}
