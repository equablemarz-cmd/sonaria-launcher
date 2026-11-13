using Bloxstrap.Integrations;

namespace Bloxstrap.Sonaria
{
    /// <summary>
    /// Runs the Sonaria tools while Roblox is open. Lives in the watcher process.
    /// It works out whether you're in Creatures of Sonaria purely from the place and universe IDs
    /// that the activity watcher reads out of Roblox's own log file.
    /// </summary>
    public class CosIntegration : IDisposable
    {
        // teleporting between CoS servers shows up as a leave followed by a join,
        // so wait a little before closing the tools to avoid them flickering off and on
        private static readonly TimeSpan LeaveGracePeriod = TimeSpan.FromSeconds(15);

        private readonly ActivityWatcher? _activityWatcher;

        private readonly ModuleRunner _tickTracker = new(ModuleCatalog.TickTracker);

        private readonly object _lock = new();

        private CancellationTokenSource? _pendingStop;

        private bool _inCos = false;

        public CosIntegration(ActivityWatcher? activityWatcher)
        {
            const string LOG_IDENT = "CosIntegration";

            _activityWatcher = activityWatcher;

            if (App.Settings.Prop.TickTrackerOnlyInCos && _activityWatcher is not null)
            {
                App.Logger.WriteLine(LOG_IDENT, "TickTracker will start when Creatures of Sonaria is joined");

                _activityWatcher.OnGameJoin += OnGameJoin;
                _activityWatcher.OnGameLeave += OnGameLeave;

                _tickTracker.Prepare();
            }
            else
            {
                App.Logger.WriteLine(LOG_IDENT, "TickTracker will stay open while Roblox is running");

                _tickTracker.Start();
            }
        }

        public static bool IsCos(ActivityData data) => data.PlaceId == App.CosPlaceId || data.UniverseId == App.CosUniverseId;

        private void OnGameJoin(object? sender, EventArgs e)
        {
            const string LOG_IDENT = "CosIntegration::OnGameJoin";

            if (_activityWatcher is null)
                return;

            CancelPendingStop();

            _inCos = IsCos(_activityWatcher.Data);

            if (_inCos)
            {
                App.Logger.WriteLine(LOG_IDENT, "Joined Creatures of Sonaria");
                _tickTracker.Start();
            }
            else
            {
                App.Logger.WriteLine(LOG_IDENT, "Joined a different experience");
                _tickTracker.Stop();
            }
        }

        private void OnGameLeave(object? sender, EventArgs e)
        {
            if (!_inCos)
                return;

            _inCos = false;

            CancellationTokenSource cts = new();

            lock (_lock)
            {
                _pendingStop?.Cancel();
                _pendingStop = cts;
            }

            Task.Delay(LeaveGracePeriod, cts.Token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;

                App.Logger.WriteLine("CosIntegration::OnGameLeave", "Left Creatures of Sonaria");
                _tickTracker.Stop();
            }, TaskScheduler.Default);
        }

        private void CancelPendingStop()
        {
            lock (_lock)
            {
                _pendingStop?.Cancel();
                _pendingStop = null;
            }
        }

        public void Dispose()
        {
            if (_activityWatcher is not null)
            {
                _activityWatcher.OnGameJoin -= OnGameJoin;
                _activityWatcher.OnGameLeave -= OnGameLeave;
            }

            CancelPendingStop();
            _tickTracker.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
