namespace Bloxstrap.Sonaria
{
    /// <summary>
    /// Starts and stops one module's program. Only ever stops a copy that it started itself.
    /// </summary>
    public class ModuleRunner : IDisposable
    {
        private readonly ModuleInfo _module;

        private readonly object _lock = new();

        private Process? _process;

        private bool _wanted = false;

        public ModuleRunner(ModuleInfo module)
        {
            _module = module;
        }

        /// <summary>
        /// Fetches the module in the background if it isn't on disk yet, so it's ready by the time it's needed
        /// </summary>
        public void Prepare() => Task.Run(() => ModuleManager.EnsureInstalled(_module));

        public void Start()
        {
            lock (_lock)
                _wanted = true;

            Task.Run(StartAsync);
        }

        private async Task StartAsync()
        {
            string LOG_IDENT = $"ModuleRunner::StartAsync({_module.Id})";

            try
            {
                if (!await ModuleManager.EnsureInstalled(_module))
                    return;

                lock (_lock)
                {
                    // stop could have been requested while it was downloading
                    if (!_wanted)
                        return;

                    if (_process is not null && !_process.HasExited)
                        return;

                    if (!String.IsNullOrEmpty(_module.SingleInstanceMutex) && Utilities.DoesMutexExist(_module.SingleInstanceMutex))
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Already running outside of the launcher, leaving it alone");
                        return;
                    }

                    string path = ModuleManager.GetEntryPath(_module);

                    _process?.Dispose();

                    _process = Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        WorkingDirectory = Path.GetDirectoryName(path)!,
                        UseShellExecute = false
                    });

                    App.Logger.WriteLine(LOG_IDENT, $"Started (PID {_process?.Id})");
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to start");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public void Stop()
        {
            string LOG_IDENT = $"ModuleRunner::Stop({_module.Id})";

            lock (_lock)
            {
                _wanted = false;

                if (_process is null)
                    return;

                try
                {
                    if (!_process.HasExited)
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Closing (PID {_process.Id})");
                        _process.Kill();

                        // give it a moment to actually go, so starting it again straight away doesn't see the old copy
                        _process.WaitForExit(2000);
                    }
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Failed to close");
                    App.Logger.WriteException(LOG_IDENT, ex);
                }

                _process.Dispose();
                _process = null;
            }
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}
