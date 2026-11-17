using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for BloxstrapPage.xaml
    /// </summary>
    public partial class BloxstrapPage
    {
        public BloxstrapPage()
        {
            DataContext = new BloxstrapViewModel();
            InitializeComponent();
        }

        // the list of apps to start with Roblox lives under Advanced, and reads from the shared integrations view model
        public void CustomIntegrationSelection(object sender, SelectionChangedEventArgs e)
        {
            IntegrationsViewModel viewModel = IntegrationsViewModel.Shared;
            viewModel.SelectedCustomIntegration = (CustomIntegration)((ListBox)sender).SelectedItem;
            viewModel.OnPropertyChanged(nameof(viewModel.SelectedCustomIntegration));
            viewModel.OnPropertyChanged(nameof(viewModel.IsCustomIntegrationSelected));
        }
    }
}
