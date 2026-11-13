namespace Bloxstrap.Sonaria
{
    /// <summary>
    /// A Sonaria Software tool that the launcher downloads, verifies and runs alongside Roblox.
    /// </summary>
    public class ModuleInfo
    {
        /// <summary>
        /// Folder-safe identifier, e.g. "TickTracker"
        /// </summary>
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";

        public string Version { get; init; } = "";

        /// <summary>
        /// Must be a release asset of this launcher's own GitHub repository
        /// </summary>
        public string DownloadUrl { get; init; } = "";

        /// <summary>
        /// Lowercase hex SHA-256 of the zip at <see cref="DownloadUrl"/>. The download is rejected if it doesn't match.
        /// </summary>
        public string Sha256 { get; init; } = "";

        /// <summary>
        /// Path of the program to start, relative to the folder the zip is extracted to
        /// </summary>
        public string EntryPoint { get; init; } = "";

        /// <summary>
        /// Name of the mutex the tool holds while it is running, if it has one.
        /// Used to avoid starting a second copy when the user already has it open.
        /// </summary>
        public string SingleInstanceMutex { get; init; } = "";
    }
}
