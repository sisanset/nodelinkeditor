using System.ComponentModel;
using System.Windows.Input;

namespace NodeLinkEditor.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MapEditorViewModel MapEditor { get; } = new MapEditorViewModel();

        public ICommand SaveNodeLinkCommand => MapEditor.SaveNodeLinkCommand;
        public ICommand LoadNodeLinkCommand => MapEditor.LoadNodeLinkCommand;
        public ICommand LoadMapCommand => MapEditor.LoadMapCommand;

        public ICommand AlignHorizontalCommand => MapEditor.AlignHorizontalCommand;
        public ICommand AlignVerticalCommand => MapEditor.AlignVerticalCommand;
        public ICommand AlignLineEqualCommand => MapEditor.AlignLineEqualCommand;

        public ICommand UndoCommand => MapEditor.UndoCommand;
        public ICommand RedoCommand => MapEditor.RedoCommand;
        public ICommand ClearSelectionCommand => MapEditor.ClearSelectionCommand;

        public ICommand CreateNodeAtIntersectionCommand => MapEditor.CreateNodeAtIntersectionCommand;
        public ICommand CreateNodesBetweenCommand => MapEditor.CreateNodesBetweenCommand;
        public ICommand SetNodesDistanceCommand => MapEditor.SetNodesDistanceCommand;

        public ICommand ConnectMQTTCommand => MapEditor.ConnectMQTTCommand;
        public ICommand DisconnectMQTTCommand => MapEditor.DisconnectMQTTCommand;


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
