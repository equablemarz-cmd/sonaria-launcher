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
            Version = "4.1.5",
            DownloadUrl = $"https://github.com/{App.ProjectRepository}/releases/download/ticktracker-4.1.5/TickTracker-4.1.5.zip",
            Sha256 = "47ec0b092060b9823928f2dceb9d7a8102b99c5866840fefc33ca05a24ee5722",
            EntryPoint = @"TickTracker\TickTracker.exe",
            SingleInstanceMutex = "TickTracker_single_instance"
        };

        public static readonly ModuleInfo DamageCounter = new()
        {
            Id = "DamageCounter",
            Name = "Damage Counter",
            Version = "1.0.1",
            DownloadUrl = $"https://github.com/{App.ProjectRepository}/releases/download/damagecounter-1.0.1/DamageCounter-1.0.1.zip",
            Sha256 = "d7ea5bc961576fdb91858435125afbce6f05b73d67c2e0ad33d6a20e454f243f",
            EntryPoint = @"DamageCounter\DamageCounter.exe",
            SingleInstanceMutex = "DamageCounter_single_instance"
        };
    }
}
