using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;

using Bloxstrap.UI.Elements.About;

namespace Bloxstrap.UI.ViewModels.Installer
{
    public class LaunchMenuViewModel : NotifyPropertyChangedViewModel
    {
        public string Version => string.Format(Strings.Menu_About_Version, App.Version);

        public ICommand LaunchSettingsCommand => new RelayCommand(LaunchSettings);

        public ICommand LaunchRobloxCommand => new RelayCommand(LaunchRoblox);

        public ICommand LaunchCosCommand => new RelayCommand(LaunchCos);

        public ICommand RejoinLastServerCommand => new RelayCommand(RejoinLastServer);

        public ICommand ToggleThemeCommand => new RelayCommand(ToggleTheme);

        public Visibility RejoinVisibility => Sonaria.LastServer.Load() is null ? Visibility.Collapsed : Visibility.Visible;

        private const string CopyServerLinkDefaultTitle = "Copy current server link";

        // worked out once when the menu opens
        private readonly string? _currentServerLink = Sonaria.LastServer.GetCurrentServerLink();

        private string _copyServerLinkTitle = CopyServerLinkDefaultTitle;

        private int _copyServerLinkResetVersion = 0;

        public ICommand CopyServerLinkCommand => new RelayCommand(CopyServerLink);

        public Visibility CopyServerLinkVisibility => _currentServerLink is null ? Visibility.Collapsed : Visibility.Visible;

        public string CopyServerLinkTitle
        {
            get => _copyServerLinkTitle;
            private set
            {
                _copyServerLinkTitle = value;
                OnPropertyChanged(nameof(CopyServerLinkTitle));
            }
        }

        public event EventHandler? ThemeChanged;

        /// <summary>
        /// A notice from Sonaria Software, followed by anything that has been switched off for now
        /// </summary>
        public string NoticeText => String.Join(" ", new[] { Sonaria.RemoteConfig.NoticeText, Sonaria.RemoteConfig.DisabledSummary }.Where(x => x.Length > 0));

        public Visibility NoticeVisibility => NoticeText.Length > 0 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility NoticeLinkVisibility => Sonaria.RemoteConfig.NoticeLink.Length > 0 ? Visibility.Visible : Visibility.Collapsed;

        public ICommand OpenNoticeLinkCommand => new RelayCommand(OpenNoticeLink);

        // music controls: what Windows says is playing, shown only while there is something to show
        private readonly DispatcherTimer? _musicTimer;

        private Sonaria.NowPlaying.Track? _track;

        private bool _musicRefreshing = false;

        public Visibility MusicVisibility => _track is null ? Visibility.Collapsed : Visibility.Visible;

        public string MusicTitle => _track?.Title ?? "";

        public string MusicArtist => _track?.Artist ?? "";

        public Visibility MusicArtistVisibility => String.IsNullOrEmpty(_track?.Artist) ? Visibility.Collapsed : Visibility.Visible;

        public Wpf.Ui.Common.SymbolRegular MusicPlayPauseIcon => _track?.IsPlaying == true ? Wpf.Ui.Common.SymbolRegular.Pause24 : Wpf.Ui.Common.SymbolRegular.Play24;

        public ICommand MusicPreviousCommand => new RelayCommand(() => SendMusic(Sonaria.NowPlaying.Previous));

        public ICommand MusicPlayPauseCommand => new RelayCommand(() => SendMusic(Sonaria.NowPlaying.TogglePlayPause));

        public ICommand MusicNextCommand => new RelayCommand(() => SendMusic(Sonaria.NowPlaying.Next));

        public LaunchMenuViewModel()
        {
            if (App.Settings.Prop.ShowMusicControls)
            {
                _musicTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                _musicTimer.Tick += (_, _) => RefreshMusic();
                _musicTimer.Start();

                RefreshMusic();
            }

            // shows the saved notice straight away, then swaps in the current one if it has changed
            Sonaria.RemoteConfig.Refresh().ContinueWith(_ =>
            {
                OnPropertyChanged(nameof(NoticeText));
                OnPropertyChanged(nameof(NoticeVisibility));
                OnPropertyChanged(nameof(NoticeLinkVisibility));
            });
        }

        private void CopyServerLink()
        {
            const string LOG_IDENT = "LaunchMenuViewModel::CopyServerLink";

            if (_currentServerLink is null)
                return;

            // the clipboard can be briefly held by another app, so try a few times
            bool copied = false;

            for (int attempt = 1; attempt <= 5 && !copied; attempt++)
            {
                try
                {
                    Clipboard.SetDataObject(_currentServerLink, true);
                    copied = true;
                }
                catch (Exception ex)
                {
                    if (attempt == 5)
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Could not copy the server link");
                        App.Logger.WriteException(LOG_IDENT, ex);
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }

            CopyServerLinkTitle = copied ? "Copied! Paste it in #find-a-fight" : "Couldn't copy, try again";

            int version = Interlocked.Increment(ref _copyServerLinkResetVersion);

            Task.Delay(TimeSpan.FromSeconds(3)).ContinueWith(_ =>
            {
                // only the latest click puts the title back
                if (version == _copyServerLinkResetVersion)
                    CopyServerLinkTitle = CopyServerLinkDefaultTitle;
            });
        }

        /// <summary>
        /// Called when the launch menu closes, so nothing keeps asking Windows what is playing
        /// </summary>
        public void StopMusic() => _musicTimer?.Stop();

        private async void RefreshMusic()
        {
            if (_musicRefreshing)
                return;

            _musicRefreshing = true;

            try
            {
                var track = await Sonaria.NowPlaying.GetCurrent();

                if (track != _track)
                {
                    _track = track;

                    OnPropertyChanged(nameof(MusicVisibility));
                    OnPropertyChanged(nameof(MusicTitle));
                    OnPropertyChanged(nameof(MusicArtist));
                    OnPropertyChanged(nameof(MusicArtistVisibility));
                    OnPropertyChanged(nameof(MusicPlayPauseIcon));
                }
            }
            catch (Exception)
            {
                // GetCurrent already reports its own failures; the controls just stay as they were
            }
            finally
            {
                _musicRefreshing = false;
            }
        }

        private async void SendMusic(Func<Task> action)
        {
            try
            {
                await action();

                // give the music app a moment to act before asking what it's doing now
                await Task.Delay(300);
            }
            catch (Exception)
            {
            }

            RefreshMusic();
        }

        private void OpenNoticeLink()
        {
            string link = Sonaria.RemoteConfig.NoticeLink;

            if (link.Length > 0)
                Utilities.ShellExecute(link);
        }

        public ICommand LaunchRobloxStudioCommand => new RelayCommand(LaunchRobloxStudio);

        public ICommand LaunchAboutCommand => new RelayCommand(LaunchAbout);

        public event EventHandler<NextAction>? CloseWindowRequest;

        private void LaunchSettings() => CloseWindowRequest?.Invoke(this, NextAction.LaunchSettings);

        private void LaunchRoblox() => CloseWindowRequest?.Invoke(this, NextAction.LaunchRoblox);

        private void LaunchCos() => CloseWindowRequest?.Invoke(this, NextAction.LaunchCos);

        private void RejoinLastServer() => CloseWindowRequest?.Invoke(this, NextAction.RejoinLastServer);

        private void ToggleTheme()
        {
            App.Settings.Prop.Theme = App.Settings.Prop.Theme.GetFinal() == Theme.Dark ? Theme.Light : Theme.Dark;
            App.Settings.Save();

            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void LaunchRobloxStudio() => CloseWindowRequest?.Invoke(this, NextAction.LaunchRobloxStudio);

        private void LaunchAbout() => new MainWindow().ShowDialog();
    }
}
