using System.Security.Cryptography;

namespace Bloxstrap.Sonaria
{
    /// <summary>
    /// The font Creatures of Sonaria's own interface uses: Source Sans, in its regular and bold weights
    /// (Source Sans 3 by Adobe, SIL Open Font Licence). Offered as a ready-made choice for the launcher's
    /// custom font mod, so the rest of Roblox's text matches the game. It only changes how text looks.
    /// </summary>
    public static class CosFont
    {
        /// <summary>
        /// What the settings window's font task is set to when this font is picked, in place of a file path
        /// </summary>
        public const string Marker = "<Creatures of Sonaria font>";

        private const string RegularResource = "Mods.Fonts.SourceSans3Regular.ttf";

        private const string BoldResource = "Mods.Fonts.SourceSans3Bold.ttf";

        // the look-alike font that version 3.1.0 put in place
        private const string OldDefaultSha256 = "7f88db0b870a2719c0798fda0a8f19d3013f9d71fc5072713a1e846706b6de29";

        /// <summary>
        /// Puts the font in place as the custom font, replacing whatever custom font is there
        /// </summary>
        public static void Install()
        {
            Write(RegularResource, Paths.CustomFont);
            Write(BoldResource, Paths.CustomFontBold);
        }

        /// <summary>
        /// Puts the font in place as the custom font, unless a custom font has already been chosen
        /// </summary>
        public static void InstallIfNoneChosen()
        {
            const string LOG_IDENT = "CosFont::InstallIfNoneChosen";

            try
            {
                if (File.Exists(Paths.CustomFont))
                    return;

                Install();

                App.Logger.WriteLine(LOG_IDENT, "Set the Creatures of Sonaria font as Roblox's font");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not set the font, leaving Roblox's own in place");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        /// <summary>
        /// Swaps the font that version 3.1.0 installed for the real one. A font the user picked themselves,
        /// or no custom font at all, is left exactly as it is.
        /// </summary>
        public static void ReplaceOldDefault()
        {
            const string LOG_IDENT = "CosFont::ReplaceOldDefault";

            try
            {
                if (!File.Exists(Paths.CustomFont))
                    return;

                string hash;

                using (var stream = File.OpenRead(Paths.CustomFont))
                using (var sha256 = SHA256.Create())
                    hash = Convert.ToHexString(sha256.ComputeHash(stream));

                if (!String.Equals(hash, OldDefaultSha256, StringComparison.OrdinalIgnoreCase))
                    return;

                Install();

                App.Logger.WriteLine(LOG_IDENT, "Replaced the 3.1.0 font with the Creatures of Sonaria font");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not replace the font, leaving it as it is");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private static void Write(string resourceName, string location)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(location)!);

            if (File.Exists(location))
                Filesystem.AssertReadOnly(location);

            using var resourceStream = Resource.GetStream(resourceName);
            using var fileStream = File.Create(location);
            resourceStream.CopyTo(fileStream);
        }
    }
}
