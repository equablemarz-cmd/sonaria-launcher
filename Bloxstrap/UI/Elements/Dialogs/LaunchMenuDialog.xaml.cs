using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Bloxstrap.UI.ViewModels.Dialogs;
using Bloxstrap.UI.ViewModels.Installer;
using Wpf.Ui.Mvvm.Interfaces;

namespace Bloxstrap.UI.Elements.Dialogs
{
    /// <summary>
    /// Interaction logic for LaunchMenuDialog.xaml
    /// </summary>
    public partial class LaunchMenuDialog
    {
        public NextAction CloseAction = NextAction.Terminate;

        public LaunchMenuDialog()
        {
            var viewModel = new LaunchMenuViewModel();
            viewModel.CloseWindowRequest += (_, closeAction) =>
            {
                CloseAction = closeAction;
                Close();
            };

            viewModel.ThemeChanged += (_, _) => ApplyTheme();

            DataContext = viewModel;

            InitializeComponent();

            Closed += (_, _) => viewModel.StopMusic();

            LoadBackground();
        }

        /// <summary>
        /// Shows the picture or GIF the user chose in settings, dimmed, behind the menu. Nothing is shown if none was chosen.
        /// </summary>
        private void LoadBackground()
        {
            const string LOG_IDENT = "LaunchMenuDialog::LoadBackground";

            string location = App.Settings.Prop.LaunchMenuBackground;

            if (String.IsNullOrEmpty(location))
                return;

            try
            {
                if (!System.IO.File.Exists(location))
                {
                    App.Logger.WriteLine(LOG_IDENT, "The chosen background is no longer where it was, so none is shown");
                    return;
                }

                var uri = new Uri(location, UriKind.Absolute);

                if (location.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    XamlAnimatedGif.AnimationBehavior.SetRepeatBehavior(BackgroundImage, System.Windows.Media.Animation.RepeatBehavior.Forever);
                    XamlAnimatedGif.AnimationBehavior.SetSourceUri(BackgroundImage, uri);
                }
                else
                {
                    // read fully now, so the file isn't held open while the menu is up
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = uri;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    BackgroundImage.Source = bitmap;
                }

                BackgroundCanvas.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not show the chosen background");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }
    }
}
