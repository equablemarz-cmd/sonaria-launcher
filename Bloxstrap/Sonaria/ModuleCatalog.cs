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
            Version = "4.2.0",
            DownloadUrl = $"https://github.com/{App.ProjectRepository}/releases/download/ticktracker-4.2.0/TickTracker-4.2.0.zip",
            Sha256 = "8fff65b4f197ad3b21332befbdeedb5cfb479f67489ab41718bd04312d530d30",
            EntryPoint = @"TickTracker\TickTracker.exe",
            SingleInstanceMutex = "TickTracker_single_instance",

            // TickTracker.exe only starts the app when given no arguments, so its settings go in as environment values
            Environment = () => new Dictionary<string, string>
            {
                ["TICKTRACKER_ALWAYS_SHOW"] = App.Settings.Prop.TickTrackerAutoHide ? "0" : "1"
            }
        };
    }
}
