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
            Version = "4.1.4",
            DownloadUrl = $"https://github.com/{App.ProjectRepository}/releases/download/ticktracker-4.1.4/TickTracker-4.1.4.zip",
            Sha256 = "ffe20bffb3f7cd327e7e7bdb94edfc56ac93e89c1add3ae477601bc21fb0860b",
            EntryPoint = @"TickTracker\TickTracker.exe",
            SingleInstanceMutex = "TickTracker_single_instance"
        };
    }
}
