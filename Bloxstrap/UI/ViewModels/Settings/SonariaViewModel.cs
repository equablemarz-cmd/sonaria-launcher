using System.Windows;
using System.Windows.Input;

using Microsoft.Win32;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Settings
{
    /// <summary>
    /// Settings Sonaria Software Launcher adds itself: the launch menu's background
    /// </summary>
    public class SonariaViewModel : NotifyPropertyChangedViewModel
    {
        // a GIF is decoded as it plays, but a huge one still makes the menu slow to open
        private const long MaxBackgroundBytes = 25 * 1024 * 1024;

        public ICommand ChooseBackgroundCommand => new RelayCommand(ChooseBackground);

        public ICommand RemoveBackgroundCommand => new RelayCommand(RemoveBackground);

        public bool HasBackground => !String.IsNullOrEmpty(App.Settings.Prop.LaunchMenuBackground);

        public string BackgroundName => HasBackground ? Path.GetFileName(App.Settings.Prop.LaunchMenuBackground) : "";

        public Visibility BackgroundChosenVisibility => HasBackground ? Visibility.Visible : Visibility.Collapsed;

        private void ChooseBackground()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Pictures and GIFs|*.gif;*.png;*.jpg;*.jpeg;*.bmp"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                if (new FileInfo(dialog.FileName).Length > MaxBackgroundBytes)
                {
                    Frontend.ShowMessageBox("That file is bigger than 25 MB, which would make the launch menu slow to open. Pick a smaller picture or GIF.", MessageBoxImage.Warning);
                    return;
                }
            }
            catch (Exception)
            {
                Frontend.ShowMessageBox("That file couldn't be read. Pick another picture or GIF.", MessageBoxImage.Error);
                return;
            }

            App.Settings.Prop.LaunchMenuBackground = dialog.FileName;
            BackgroundChanged();
        }

        private void RemoveBackground()
        {
            App.Settings.Prop.LaunchMenuBackground = "";
            BackgroundChanged();
        }

        private void BackgroundChanged()
        {
            OnPropertyChanged(nameof(HasBackground));
            OnPropertyChanged(nameof(BackgroundName));
            OnPropertyChanged(nameof(BackgroundChosenVisibility));
        }
    }
}
