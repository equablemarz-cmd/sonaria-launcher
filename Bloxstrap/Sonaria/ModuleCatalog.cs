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
            Version = "1.0.0",
            DownloadUrl = $"https://github.com/{App.ProjectRepository}/releases/download/damagecounter-1.0.0/DamageCounter-1.0.0.zip",
            Sha256 = "a18eaaa239e51d9d912e35925f6141eba610fc45acbfca7a5087c3ac0713f6ba",
            EntryPoint = @"DamageCounter\DamageCounter.exe",
            SingleInstanceMutex = "DamageCounter_single_instance"
        };
    }
}
