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

        /// <summary>
        /// Whether a file in the mods folder (given relative to it) belongs to the font or cursor mods
        /// </summary>
        public static bool IsFontOrCursorFile(string relativeFile) =>
            FontAndCursorLocations.Any(x => relativeFile.StartsWith(x, StringComparison.OrdinalIgnoreCase));
    }
}
