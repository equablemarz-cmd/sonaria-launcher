namespace Bloxstrap.Sonaria
{
    public static class CosMods
    {
        private static readonly string[] FontAndCursorLocations =
        {
            @"content\fonts\CustomFont.ttf",
            @"content\fonts\families\",
            @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png",
            @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png"
        };

        private static readonly Dictionary<string, string> CursorFiles = new()
        {
            { @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png",    "Cursor.Sonaria.ArrowCursor.png"    },
            { @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png", "Cursor.Sonaria.ArrowFarCursor.png" }
        };

        /// <summary>
        /// Puts the Sonaria cursor in place, unless the mods folder already has a cursor in it
        /// </summary>
        public static void InstallCursorIfNoneChosen()
        {
            const string LOG_IDENT = "CosMods::InstallCursorIfNoneChosen";

            try
            {
                if (CursorFiles.Keys.Any(x => File.Exists(Path.Combine(Paths.Modifications, x))))
                    return;

                foreach (var entry in CursorFiles)
                {
                    string location = Path.Combine(Paths.Modifications, entry.Key);

                    Directory.CreateDirectory(Path.GetDirectoryName(location)!);

                    using var resourceStream = Resource.GetStream(entry.Value);
                    using var fileStream = File.Create(location);
                    resourceStream.CopyTo(fileStream);
                }

                App.Logger.WriteLine(LOG_IDENT, "Set the Sonaria cursor as Roblox's cursor");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not set the cursor, leaving Roblox's own in place");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        /// <summary>
        /// Whether a file in the mods folder (given relative to it) belongs to the font or cursor mods
        /// </summary>
        public static bool IsFontOrCursorFile(string relativeFile) =>
            FontAndCursorLocations.Any(x => relativeFile.StartsWith(x, StringComparison.OrdinalIgnoreCase));
    }
}
