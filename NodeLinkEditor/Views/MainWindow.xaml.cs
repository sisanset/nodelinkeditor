using NodeLinkEditor.Others;
using NodeLinkEditor.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace NodeLinkEditor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (DataContext is MainWindowViewModel viewModel)
            {
                var settings = FileIO.LoadSettingsYaml("config.yaml");
                viewModel.MapEditor.IntersectionInterval = settings.IntersectionInterval;
                viewModel.MapEditor.NodeInterval = settings.NodeInterval;
                viewModel.MapEditor.MqttClient.BrokerAddress = settings.MqttBroker;
                viewModel.MapEditor.MqttClient.BrokerPort = settings.MqttPort;
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (DataContext is MainWindowViewModel viewModel)
                {
                    viewModel.ClearSelectionCommand?.Execute(null);
                }
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var mainWindowViewModel = (ViewModels.MainWindowViewModel)DataContext;
            var settingsWindow = new SettingsWindow(mainWindowViewModel.MapEditor);
            settingsWindow.Show();
        }
    }
}
