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
            Version = "1.0.2",
            DownloadUrl = $"https://github.com/{App.ProjectRepository}/releases/download/damagecounter-1.0.2/DamageCounter-1.0.2.zip",
            Sha256 = "fcaf905242437dfe7ed9af209c461f10a3a73c5f256e5d8b22258c742ab00c78",
            EntryPoint = @"DamageCounter\DamageCounter.exe",
            SingleInstanceMutex = "DamageCounter_single_instance"
        };
    }
}
