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
            Version = "4.3.0",
            DownloadUrl = $"https://github.com/{App.ProjectRepository}/releases/download/ticktracker-4.3.0/TickTracker-4.3.0.zip",
            Sha256 = "672cdb32a07663fc6e4fa04ad182f07d825e81876426212af8348a693db13d70",
            EntryPoint = @"TickTracker\TickTracker.exe",
            SingleInstanceMutex = "TickTracker_single_instance"
        };

        public static readonly ModuleInfo DamageCounter = new()
        {
            Id = "DamageCounter",
            Name = "Damage Counter",
            Version = "1.0.4",
            DownloadUrl = $"https://github.com/{App.ProjectRepository}/releases/download/damagecounter-1.0.4/DamageCounter-1.0.4.zip",
            Sha256 = "664f6649f559603e6b72ff4b5999e0f90ecf7a2a6791cbf5100b79fdb80f6456",
            EntryPoint = @"DamageCounter\DamageCounter.exe",
            SingleInstanceMutex = "DamageCounter_single_instance"
        };
    }
}
