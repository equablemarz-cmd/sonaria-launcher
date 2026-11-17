using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for DiscordPage.xaml
    /// </summary>
    public partial class DiscordPage
    {
        public DiscordPage()
        {
            DataContext = IntegrationsViewModel.Shared;
            InitializeComponent();
        }
    }
}
