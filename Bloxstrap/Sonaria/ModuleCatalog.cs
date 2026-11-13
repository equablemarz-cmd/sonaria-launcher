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
            Version = "4.1.2",
            DownloadUrl = $"https://github.com/{App.ProjectRepository}/releases/download/ticktracker-4.1.2/TickTracker-4.1.2.zip",
            Sha256 = "8fd1bddf0dc59840e3689154879cca39d7a1793f4290467976512a9497114165",
            EntryPoint = @"TickTracker\TickTracker.exe",
            SingleInstanceMutex = "TickTracker_single_instance"
        };
    }
}
