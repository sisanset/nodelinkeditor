using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using NodeLinkEditor.Others;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Text;
using System.Linq;
using NodeLinkEditor.Models;

namespace NodeLinkEditor.ViewModels
{
    public class MapEditorViewModel : INotifyPropertyChanged
    {
        private UndoRedoManager _undoRedoManager = new();
        private int _nodeSize = 20;
        public int NodeSize
        {
            get { return _nodeSize; }
            set
            {
                _nodeSize = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<NodeViewModel> Nodes { get; set; } = [];
        public ObservableCollection<LinkViewModel> Links { get; set; } = [];

        public NodeViewModel? StartNode { get; set; }
        public NodeViewModel? EndNode { get; set; }
        private NodeViewModel? _selectedNode;
        public NodeViewModel? SelectedNode
        {
            get
            {
                return _selectedNode;
            }
            set
            {
                if (_selectedNode != null)
                {
                    _selectedNode.IsSelected = false;
                }
                _selectedNode = value;
                OnPropertyChanged();
            }
        }

        private MapData _mapData = new() { Width = 800, Height = 450 };
        public MapData MapData
        {
            get { return _mapData; }
            set
            {
                _mapData = value;
                OnPropertyChanged();
            }
        }

        public ICommand CreateNodeCommand { get; }
        public ICommand CreateLinkCommand { get; }
        public ICommand SaveNodeLinkCommand { get; }
        public ICommand LoadNodeLinkCommand { get; }
        public ICommand LoadMapCommand { get; }
        public ICommand AlignHorizontalCommand { get; }
        public ICommand AlignVerticalCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand RemoveNodeCommand { get; }
        public ICommand RemoveLinkCommand { get; }
        public ICommand MoveNodeCommand { get; }


        public MapEditorViewModel()
        {
            CreateNodeCommand = new Others.RelayCommand(CreateNode);
            CreateLinkCommand = new Others.RelayCommand(CreateLink, CanCreateLink);
            SaveNodeLinkCommand = new Others.RelayCommand(SaveNodeLink);
            LoadNodeLinkCommand = new Others.RelayCommand(LoadNodeLink);
            LoadMapCommand = new Others.RelayCommand(LoadMap);
            AlignHorizontalCommand = new Others.RelayCommand(AlignHorizontal);
            AlignVerticalCommand = new Others.RelayCommand(AlignVertical);
            UndoCommand = new Others.RelayCommand(Undo);
            RedoCommand = new Others.RelayCommand(Redo);
            RemoveNodeCommand = new Others.RelayCommand(RemoveNode);
            RemoveLinkCommand = new Others.RelayCommand(RemoveLink);
            MoveNodeCommand = new Others.RelayCommand(MoveNode);
        }

        private void MoveNode(object? parameter)
        {
            if (parameter is (NodeViewModel node, double newX, double newY))
            {
                _undoRedoManager.Execute(new NodePositionChangeCommand(node, newX, newY));
            }
            else if (parameter is (NodeViewModel _node, double _newX, double _newY, double _oldX, double _oldY))
            {
                _undoRedoManager.Execute(new NodePositionChangeCommand(_node, _newX, _newY, _oldX, _oldY));
            }
        }
        private void RemoveNode(object? parameter)
        {
            if (parameter is NodeViewModel node)
            {
                _undoRedoManager.Execute(new RemoveCommand<NodeViewModel>(Nodes, node));
            }
        }
        private void RemoveLink(object? parameter)
        {
            if (parameter is LinkViewModel link)
            {
                _undoRedoManager.Execute(new RemoveCommand<LinkViewModel>(Links, link));
            }
        }

        private void CreateNode(object? parameter)
        {
            if (parameter is System.Windows.Point point)
            {
                _undoRedoManager.Execute(new AddCommand<NodeViewModel>(Nodes, new NodeViewModel(point.X, point.Y)));
            }
        }

        private void CreateLink(object? parameter)
        {
            if (StartNode != null && EndNode != null)
            {
                _undoRedoManager.Execute(new AddCommand<LinkViewModel>(Links, new LinkViewModel(StartNode, EndNode)));
                StartNode = null;
                EndNode = null;
            }
        }

        private bool CanCreateLink(object? parameter) => StartNode != null && EndNode != null;

        private void SaveNodeLink(object? parameter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                FileIO.SaveNodeLinkToJson(saveFileDialog.FileName, _mapData, Nodes, Links);
            }
        }

        private void LoadNodeLink(object? parameter)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() != true)
            { return; }
            _undoRedoManager.Clear();
            (MapData, Nodes, Links) = FileIO.LoadNodeLinkFromJson(openFileDialog.FileName);
            OnPropertyChanged(nameof(Nodes));
            OnPropertyChanged(nameof(Links));
        }

        private void LoadMap(object? parameter)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "YAML files (*.yaml)|*.yaml|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() != true)
            { return; }
            _undoRedoManager.Clear();
            Links.Clear();
            Nodes.Clear();
            var mapData = FileIO.LoadMapFromYaml(openFileDialog.FileName);
            MapData = mapData;
        }

        private void AlignHorizontal(object? parameter)
        {
            var selectedNodes = Nodes.Where(n => n.IsSelected).ToList();
            if (selectedNodes.Count == 0) return;
            double averageY = selectedNodes.Average(node => node.Y);
            _undoRedoManager.Execute(new NodePositionChangeCommand([.. selectedNodes.Select(node => (node, node.X, averageY))]));
        }

        private void AlignVertical(object? parameter)
        {
            var selectedNodes = Nodes.Where(n => n.IsSelected).ToList();
            if (selectedNodes.Count == 0) return;
            double averageX = selectedNodes.Average(node => node.X);
            _undoRedoManager.Execute(new NodePositionChangeCommand([.. selectedNodes.Select(node => (node, averageX, node.Y))]));
        }

        private void Undo(object? parameter) => _undoRedoManager.Undo();

        private void Redo(object? parameter) => _undoRedoManager.Redo();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
