using System.Security.Cryptography;

namespace Bloxstrap.Sonaria
{
    /// <summary>
    /// The font Creatures of Sonaria's own interface uses: Source Sans, in its regular and bold weights.
    /// Offered as a ready-made choice for the launcher's custom font mod, so that Roblox's own menus and
    /// chat match the game. It only changes how text looks.
    ///
    /// The files are "Sonaria UI": Source Sans 3 by Adobe (SIL Open Font Licence) drawn 12% larger, because
    /// Source Sans is a small-bodied font and would otherwise make menus and chat smaller than Roblox's
    /// own fonts do. See Resources/Mods/Fonts/README.md.
    ///
    /// Unlike a font picked by hand, it only stands in for Roblox's own interface fonts. Fonts a game
    /// chooses for its own look (its HUD, badges, titles) are left exactly as the game made them.
    /// </summary>
    public static class CosFont
    {
        /// <summary>
        /// What the settings window's font task is set to when this font is picked, in place of a file path
        /// </summary>
        public const string Marker = "<Creatures of Sonaria font>";

        private const string RegularResource = "Mods.Fonts.SonariaUIRegular.ttf";

        private const string BoldResource = "Mods.Fonts.SonariaUIBold.ttf";

        // SHA-256 of the files earlier versions put in place as the custom font
        private static readonly string[] OldDefaultSha256 =
        {
            "7f88db0b870a2719c0798fda0a8f19d3013f9d71fc5072713a1e846706b6de29", // 3.1.0: Fredoka One
            "4644c81b86ec9caaa76b634889968ed3c4f4f52f054855933acc7c2b21e53b0f"  // 3.1.1 and 3.1.2: Source Sans 3 Regular
        };

        // The fonts Roblox sets its own menus, chat and player list in, by the name of their
        // manifest in content\fonts\families (and by the family name inside it)
        private static readonly string[] RobloxInterfaceFamilies =
        {
            "BuilderSans", "GothamSSm", "Montserrat", "Arial", "LegacyArial", "Arimo", "LegacyArimo"
        };

        /// <summary>
        /// Whether the custom font in place is this one rather than a font picked by hand.
        /// Only this one comes with a separate file for bold text.
        /// </summary>
        public static bool IsInPlace => File.Exists(Paths.CustomFont) && File.Exists(Paths.CustomFontBold);

        /// <summary>
        /// Whether this font stands in for the given font family. It only does for Roblox's own interface fonts.
        /// </summary>
        public static bool StandsInFor(string manifestFileName, string? familyName)
        {
            string fileKey = Key(Path.GetFileNameWithoutExtension(manifestFileName));
            string nameKey = Key(familyName ?? "");

            return RobloxInterfaceFamilies.Any(x => Key(x) == fileKey || Key(x) == nameKey);
        }

        private static string Key(string text) => new string(text.Where(Char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

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
        /// Swaps a font that an earlier version installed for the current one. A font the user picked
        /// themselves, or no custom font at all, is left exactly as it is.
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

                if (!OldDefaultSha256.Any(x => String.Equals(hash, x, StringComparison.OrdinalIgnoreCase)))
                    return;

                Install();

                App.Logger.WriteLine(LOG_IDENT, "Replaced an earlier version's font with the current Creatures of Sonaria font");
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
