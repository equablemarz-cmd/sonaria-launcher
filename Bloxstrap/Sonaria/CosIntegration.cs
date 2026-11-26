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

        private readonly ModuleRunner? _tickTracker;

        // the Damage Counter is only ever open inside Creatures of Sonaria
        private readonly ModuleRunner? _damageCounter;

        // counts kills for the kill leaderboard while you're in Creatures of Sonaria
        private KillTracker? _killTracker;

        private readonly object _lock = new();

        private CancellationTokenSource? _pendingStop;

        private bool _inCos = false;

        public CosIntegration(ActivityWatcher? activityWatcher)
        {
            const string LOG_IDENT = "CosIntegration";

            _activityWatcher = activityWatcher;

            if (App.Settings.Prop.TickTrackerEnabled && RemoteConfig.TickTrackerAllowed)
                _tickTracker = new(ModuleCatalog.TickTracker);
            else if (App.Settings.Prop.TickTrackerEnabled)
                App.Logger.WriteLine(LOG_IDENT, "TickTracker is switched off by Sonaria Software for now");

            if (App.Settings.Prop.DamageCounterEnabled && RemoteConfig.DamageCounterAllowed && _activityWatcher is not null)
            {
                _damageCounter = new(ModuleCatalog.DamageCounter);
                App.Logger.WriteLine(LOG_IDENT, "Damage Counter will start when Creatures of Sonaria is joined");
                _damageCounter.Prepare();
            }
            else if (App.Settings.Prop.DamageCounterEnabled && !RemoteConfig.DamageCounterAllowed)
            {
                App.Logger.WriteLine(LOG_IDENT, "Damage Counter is switched off by Sonaria Software for now");
            }

            if (_activityWatcher is not null)
            {
                _activityWatcher.OnGameJoin += OnGameJoin;
                _activityWatcher.OnGameLeave += OnGameLeave;
            }

            if (_tickTracker is null)
                return;

            if (App.Settings.Prop.TickTrackerOnlyInCos && _activityWatcher is not null)
            {
                App.Logger.WriteLine(LOG_IDENT, "TickTracker will start when Creatures of Sonaria is joined");
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

                // remembered so the launcher can offer to rejoin it later.
                // private and reserved servers can't be rejoined from a link, so those are left out
                if (_activityWatcher.Data.ServerType == ServerType.Public)
                    LastServer.Save(_activityWatcher.Data);
            }
            else
            {
                App.Logger.WriteLine(LOG_IDENT, "Joined a different experience");
            }

            if (_damageCounter is not null)
            {
                if (_inCos)
                    _damageCounter.Start();
                else
                    _damageCounter.Stop();
            }

            UpdateKillTracker();

            if (_tickTracker is null || !App.Settings.Prop.TickTrackerOnlyInCos)
                return;

            if (_inCos)
                _tickTracker.Start();
            else
                _tickTracker.Stop();
        }

        private void OnGameLeave(object? sender, EventArgs e)
        {
            if (!_inCos)
                return;

            _inCos = false;

            LastServer.MarkLeft();

            // the kill counting stops straight away: there are no kills to see outside CoS
            UpdateKillTracker();

            // the tools that close when you leave CoS
            var leaving = new List<ModuleRunner>();

            if (_tickTracker is not null && App.Settings.Prop.TickTrackerOnlyInCos)
                leaving.Add(_tickTracker);

            if (_damageCounter is not null)
                leaving.Add(_damageCounter);

            if (leaving.Count == 0)
                return;

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

                // a join can arrive just as the wait ends, so only stop if this is still the pending stop
                lock (_lock)
                {
                    if (cts.IsCancellationRequested || !ReferenceEquals(_pendingStop, cts))
                        return;

                    _pendingStop = null;

                    App.Logger.WriteLine("CosIntegration::OnGameLeave", "Left Creatures of Sonaria");

                    foreach (var runner in leaving)
                        runner.Stop();
                }
            }, TaskScheduler.Default);
        }

        /// <summary>
        /// Starts counting kills when you're in CoS (as whichever Roblox account joined) and stops otherwise
        /// </summary>
        private void UpdateKillTracker()
        {
            const string LOG_IDENT = "CosIntegration::UpdateKillTracker";

            lock (_lock)
            {
                long userId = _activityWatcher?.Data.UserId ?? 0;
                long placeId = _activityWatcher?.Data.PlaceId ?? 0;

                bool wanted = _inCos && userId > 0 && RemoteConfig.KillLeaderboardAllowed;

                if (!wanted)
                {
                    if (_killTracker is not null)
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Stopped counting kills");
                        _killTracker.Dispose();
                        _killTracker = null;
                    }

                    if (_inCos && userId <= 0)
                        App.Logger.WriteLine(LOG_IDENT, "Not counting kills: the Roblox account isn't known yet");

                    return;
                }

                // already counting for this account (a teleport between CoS servers, for example)
                if (_killTracker is not null && _killTrackerUserId == userId)
                    return;

                _killTracker?.Dispose();
                _killTracker = new KillTracker(userId, placeId);
                _killTrackerUserId = userId;
            }
        }

        private long _killTrackerUserId;

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

            lock (_lock)
            {
                _killTracker?.Dispose();
                _killTracker = null;
            }

            _tickTracker?.Dispose();
            _damageCounter?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
