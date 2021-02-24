using Windows.UI.Xaml.Controls;

namespace SampleTest.Samples
{
    /// <summary>
    /// A sample for demonstrating features in the RoamingSettings namespace.
    /// </summary>
    public sealed partial class RoamingSettings : Page
    {
        private RoamingSettingsViewModel _vm => DataContext as RoamingSettingsViewModel;

        public RoamingSettings()
        {
            InitializeComponent();
            DataContext = new RoamingSettingsViewModel();
        }

        private void UpdateButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _vm.AddOrUpdateAdditionalData();
        }

        private void CreateButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _vm.CreateCustomRoamingSettings();
        }

        private void DeleteButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _vm.DeleteCustomRoamingSettings();
        }
    }
}
