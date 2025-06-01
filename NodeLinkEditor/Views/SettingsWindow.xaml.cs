using System.Windows;
using NodeLinkEditor.Others;
using NodeLinkEditor.ViewModels;
using static NodeLinkEditor.Others.FileIO;

namespace NodeLinkEditor.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(MapEditorViewModel mapEditorViewModel)
        {
            InitializeComponent();
            DataContext = mapEditorViewModel;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var mapEditorViewModel = (MapEditorViewModel)DataContext;
            FileIO.SaveSettingsYaml("config.yaml",
                new SettingsYaml
                {
                    NodeInterval = mapEditorViewModel.NodeInterval,
                    IntersectionInterval = mapEditorViewModel.IntersectionInterval,
                    MqttBroker = mapEditorViewModel.MqttClient.BrokerAddress,
                    MqttPort = mapEditorViewModel.MqttClient.BrokerPort
                });
            this.Close();
        }
    }
}
