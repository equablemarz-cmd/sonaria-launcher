using System.Windows;
using System.Windows.Input;
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

        public LaunchMenuViewModel()
        {
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

            try
            {
                // the clipboard can be briefly held by another app, so try a few times
                Clipboard.SetDataObject(_currentServerLink, true, 5, 100);
                CopyServerLinkTitle = "Copied! Paste it in #find-a-fight";
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not copy the server link");
                App.Logger.WriteException(LOG_IDENT, ex);
                CopyServerLinkTitle = "Couldn't copy, try again";
            }

            int version = Interlocked.Increment(ref _copyServerLinkResetVersion);

            Task.Delay(TimeSpan.FromSeconds(3)).ContinueWith(_ =>
            {
                // only the latest click puts the title back
                if (version == _copyServerLinkResetVersion)
                    CopyServerLinkTitle = CopyServerLinkDefaultTitle;
            });
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
