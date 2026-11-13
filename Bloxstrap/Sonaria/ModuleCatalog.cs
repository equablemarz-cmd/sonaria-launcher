namespace Bloxstrap.Sonaria
{
    /// <summary>
    /// The modules this version of the launcher knows about, pinned by version and SHA-256.
    /// </summary>
    public static class ModuleCatalog
    {
        public static readonly ModuleInfo TickTracker = new()
        {
            Id = "TickTracker",
            Name = "TickTracker",
            Version = "4.0.0",
            DownloadUrl = $"https://github.com/{App.ProjectRepository}/releases/download/ticktracker-4.0.0/TickTracker.4.0.zip",
            Sha256 = "408aa52fb491357e250a1e84b6e75af29d33996fc46f6f60c6cdb9365f3b4b2b",
            EntryPoint = @"TickTracker\TickTracker.exe",
            SingleInstanceMutex = "TickTracker_single_instance"
        };
    }
}
