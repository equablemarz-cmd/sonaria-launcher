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
            Version = "4.0.1",
            DownloadUrl = $"https://github.com/{App.ProjectRepository}/releases/download/ticktracker-4.0.1/TickTracker-4.0.1.zip",
            Sha256 = "95cd322f93b81ce74f70b7858b37f0695700a95a36f3e519d50e6f93768cd027",
            EntryPoint = @"TickTracker\TickTracker.exe",
            SingleInstanceMutex = "TickTracker_single_instance"
        };
    }
}
