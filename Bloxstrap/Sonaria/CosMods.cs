using System.Security.Cryptography;

namespace Bloxstrap.Sonaria
{
    public static class CosMods
    {
        private static readonly string[] FontAndCursorLocations =
        {
            @"content\fonts\CustomFont",
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

        // SHA-256 of the cursor picture earlier versions put in place (3.1.0 to 3.1.2: the first, smaller Sonaria cursor)
        private static readonly string[] OldCursorSha256 =
        {
            "73ad1b92845255003ce43d1ec5c26ade29b3979fefd1a182762509bba1c23b26"
        };

        /// <summary>
        /// Swaps a Sonaria cursor that an earlier version installed for the current one. A cursor the user
        /// picked themselves, or Roblox's own, is left exactly as it is.
        /// </summary>
        public static void ReplaceOldDefaultCursor()
        {
            const string LOG_IDENT = "CosMods::ReplaceOldDefaultCursor";

            try
            {
                bool replaced = false;

                foreach (var entry in CursorFiles)
                {
                    string location = Path.Combine(Paths.Modifications, entry.Key);

                    if (!File.Exists(location))
                        continue;

                    string hash;

                    using (var stream = File.OpenRead(location))
                    using (var sha256 = SHA256.Create())
                        hash = Convert.ToHexString(sha256.ComputeHash(stream));

                    if (!OldCursorSha256.Any(x => String.Equals(hash, x, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    Filesystem.AssertReadOnly(location);

                    using var resourceStream = Resource.GetStream(entry.Value);
                    using var fileStream = File.Create(location);
                    resourceStream.CopyTo(fileStream);

                    replaced = true;
                }

                if (replaced)
                    App.Logger.WriteLine(LOG_IDENT, "Replaced an earlier version's Sonaria cursor with the current one");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not replace the cursor, leaving it as it is");
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
