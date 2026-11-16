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
            Version = "4.1.3",
            DownloadUrl = $"https://github.com/{App.ProjectRepository}/releases/download/ticktracker-4.1.3/TickTracker-4.1.3.zip",
            Sha256 = "09f3233ef8b04a0b216f7519e09a3ff5c5bf94cd2c3f06a852654fa73635c372",
            EntryPoint = @"TickTracker\TickTracker.exe",
            SingleInstanceMutex = "TickTracker_single_instance"
        };
    }
}
