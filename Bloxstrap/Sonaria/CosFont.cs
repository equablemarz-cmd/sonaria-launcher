namespace Bloxstrap.Sonaria
{
    /// <summary>
    /// The rounded font Creatures of Sonaria uses for its own interface (Fredoka One, SIL Open Font Licence),
    /// offered as a ready-made choice for the launcher's existing custom font mod. It only changes how
    /// Roblox's text looks.
    /// </summary>
    public static class CosFont
    {
        private const string ResourceName = "Mods.Fonts.FredokaOne.ttf";

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

                Directory.CreateDirectory(Path.GetDirectoryName(Paths.CustomFont)!);

                using var resourceStream = Resource.GetStream(ResourceName);
                using var fileStream = File.Create(Paths.CustomFont);
                resourceStream.CopyTo(fileStream);

                App.Logger.WriteLine(LOG_IDENT, "Set the Creatures of Sonaria font as Roblox's font");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not set the font, leaving Roblox's own in place");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        /// <summary>
        /// Writes the font out to a temporary file, for the settings window to hand to the custom font mod
        /// </summary>
        public static string ExtractToTemp()
        {
            string location = Path.Combine(Path.GetTempPath(), "SonariaSoftwareLauncher-FredokaOne.ttf");

            using var resourceStream = Resource.GetStream(ResourceName);
            using var fileStream = File.Create(location);
            resourceStream.CopyTo(fileStream);

            return location;
        }
    }
}
