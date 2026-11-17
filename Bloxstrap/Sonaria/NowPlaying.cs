using Windows.Media.Control;

namespace Bloxstrap.Sonaria
{
    /// <summary>
    /// What is playing on this PC, as Windows itself reports it (the same song and buttons Windows
    /// shows beside its volume slider), for the launch menu's music controls. Play, pause and skip are
    /// passed to whichever app is playing. Nothing is played, downloaded or sent anywhere by the launcher.
    /// </summary>
    public static class NowPlaying
    {
        public record Track(string Title, string Artist, bool IsPlaying);

        private static GlobalSystemMediaTransportControlsSessionManager? _manager;

        private static bool _failureLogged = false;

        private static async Task<GlobalSystemMediaTransportControlsSession?> GetSession()
        {
            _manager ??= await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            return _manager.GetCurrentSession();
        }

        /// <summary>
        /// The song Windows says is current, or null when nothing is playing or Windows can't say
        /// </summary>
        public static async Task<Track?> GetCurrent()
        {
            try
            {
                var session = await GetSession();

                if (session is null)
                    return null;

                var properties = await session.TryGetMediaPropertiesAsync();

                if (properties is null || String.IsNullOrWhiteSpace(properties.Title))
                    return null;

                var status = session.GetPlaybackInfo()?.PlaybackStatus;

                return new Track(
                    properties.Title.Trim(),
                    (properties.Artist ?? "").Trim(),
                    status == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing);
            }
            catch (Exception ex)
            {
                LogFailure("NowPlaying::GetCurrent", ex);
                return null;
            }
        }

        public static Task TogglePlayPause() => Send("NowPlaying::TogglePlayPause", async session => await session.TryTogglePlayPauseAsync());

        public static Task Next() => Send("NowPlaying::Next", async session => await session.TrySkipNextAsync());

        public static Task Previous() => Send("NowPlaying::Previous", async session => await session.TrySkipPreviousAsync());

        private static async Task Send(string logIdent, Func<GlobalSystemMediaTransportControlsSession, Task<bool>> action)
        {
            try
            {
                var session = await GetSession();

                if (session is not null)
                    await action(session);
            }
            catch (Exception ex)
            {
                LogFailure(logIdent, ex);
            }
        }

        // older Windows 10 builds don't have this, and it is asked for every couple of seconds, so say so once
        private static void LogFailure(string logIdent, Exception ex)
        {
            if (_failureLogged)
                return;

            _failureLogged = true;

            App.Logger.WriteLine(logIdent, "Windows couldn't say what is playing, so the music controls stay hidden");
            App.Logger.WriteException(logIdent, ex);
        }
    }
}
