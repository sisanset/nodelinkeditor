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
        public ObservableCollection<NodeViewModel> Nodes { get; set; } = [];
        public ObservableCollection<LinkViewModel> Links { get; set; } = [];
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
                { _selectedNode.IsSelected = false; }
                _selectedNode = value;
                if (_selectedNode != null)
                { _selectedNode.IsSelected = true; }
                OnPropertyChanged();

                if (_selectedNode == null)
                { return; }
                if (_selectedLink != null)
                { _selectedLink.IsSelected = false; }
                SelectedLink = null;
            }
        }
        public ObservableCollection<NodeViewModel> SelectedNodes { get; set; } = [];

        private LinkViewModel? _selectedLink;
        public LinkViewModel? SelectedLink
        {
            get
            {
                return _selectedLink;
            }
            set
            {
                if (_selectedLink != null)
                { _selectedLink.IsSelected = false; }
                _selectedLink = value;
                if (_selectedLink != null)
                { _selectedLink.IsSelected = true; }
                OnPropertyChanged();

                if (_selectedLink == null)
                { return; }
                if (_selectedNode != null)
                { _selectedNode.IsSelected = false; }
                SelectedNode = null;
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

        public IEnumerable<NodeAttribute> AllAttributes => Enum.GetValues(typeof(NodeAttribute)).Cast<NodeAttribute>();
        public ICommand AddAttributeCommand { get; }
        public ICommand RemoveAttributeCommand { get; }
        public ICommand AddAssociatedCommand { get; }
        public ICommand RemoveAssociatedCommand { get; }
        public IEnumerable<LinkAttribute> AllLinkAttributes => Enum.GetValues(typeof(LinkAttribute)).Cast<LinkAttribute>();
        public ICommand AddLinkAttributeCommand { get; }
        public ICommand RemoveLinkAttributeCommand { get; }
        public ICommand ClearSelectionCommand { get; }

        public void ClearSelection()
        {
            SelectedNode = null;
            SelectedLink = null;
            foreach (var node in SelectedNodes.Where(n => n.IsReferenced))
            { node.IsReferenced = false; }
            SelectedNodes.Clear();
        }

        public MapEditorViewModel()
        {
            CreateNodeCommand = new Others.RelayCommand(CreateNode);
            CreateLinkCommand = new Others.RelayCommand(CreateLink, CanCreateLink);
            SaveNodeLinkCommand = new Others.RelayCommand(SaveNodeLink);
            LoadNodeLinkCommand = new Others.RelayCommand(LoadNodeLink);
            LoadMapCommand = new Others.RelayCommand(LoadMap);
            AlignHorizontalCommand = new Others.RelayCommand(AlignHorizontal);
            AlignVerticalCommand = new Others.RelayCommand(AlignVertical);
            UndoCommand = new Others.RelayCommand(Undo, CanUndo);
            RedoCommand = new Others.RelayCommand(Redo, CanRedo);
            RemoveNodeCommand = new Others.RelayCommand(RemoveNode);
            RemoveLinkCommand = new Others.RelayCommand(RemoveLink);
            MoveNodeCommand = new Others.RelayCommand(MoveNode);

            AddAttributeCommand = new Others.RelayCommand(attr =>
            {
                if (SelectedNode == null) { return; }
                if (attr is not NodeAttribute) { return; }
                var attribute = (NodeAttribute)attr;
                if (!SelectedNode.Attributes.Contains(attribute))
                {
                    SelectedNode.Attributes.Add(attribute);
                }
            });
            RemoveAttributeCommand = new Others.RelayCommand(attr =>
            {
                if (SelectedNode == null) { return; }
                if (attr is not NodeAttribute) { return; }
                var attribute = (NodeAttribute)attr;
                SelectedNode.Attributes.Remove(attribute);
            });
            AddAssociatedCommand = new Others.RelayCommand((_) =>
            {
                if (SelectedNode == null) { return; }
                foreach (var node in SelectedNodes)
                {
                    if (node == SelectedNode) { continue; }
                    if (SelectedNode.AssociatedNodes.Contains(node.ID)) { continue; }
                    SelectedNode.AssociatedNodes.Add(node.ID);
                }
            });
            RemoveAssociatedCommand = new Others.RelayCommand((nodeId) =>
            {
                if (SelectedNode == null) { return; }
                if (nodeId is not Guid) { return; }
                SelectedNode.AssociatedNodes.Remove((Guid)nodeId);
            });

            AddLinkAttributeCommand = new Others.RelayCommand(attr =>
            {
                if (SelectedLink == null) { return; }
                if (attr is not LinkAttribute) { return; }
                var attribute = (LinkAttribute)attr;
                if (!SelectedLink.Attributes.Contains(attribute))
                {
                    SelectedLink.Attributes.Add(attribute);
                }
            });
            RemoveLinkAttributeCommand = new Others.RelayCommand(attr =>
            {
                if (SelectedLink == null) { return; }
                if (attr is not LinkAttribute) { return; }
                var attribute = (LinkAttribute)attr;
                SelectedLink.Attributes.Remove(attribute);
            });
            ClearSelectionCommand = new Others.RelayCommand((_) =>
            {
                ClearSelection();
            });


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
                SelectedNodes.Remove(node);
                if (SelectedNode == node)
                { SelectedNode = null; }
                if (Links.Any(link => (link.StartNode == node || link.EndNode == node) && link == SelectedLink))
                { SelectedLink = null; }
                if (StartNode == node)
                { StartNode = null; }
                _undoRedoManager.Execute(new RemoveNodeCommand(Nodes, node, Links));
            }
        }
        private void RemoveLink(object? parameter)
        {
            if (parameter is LinkViewModel link)
            {
                if (SelectedLink == link)
                { SelectedLink = null; }
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
            (var mapData, Nodes, Links) = FileIO.LoadNodeLinkFromJson(openFileDialog.FileName);
            if (mapData.YamlFilePath != string.Empty)
            { MapData = mapData; }
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
            if (SelectedNodes.Count == 0) return;
            double firstY = SelectedNodes[0].Y;
            _undoRedoManager.Execute(new NodePositionChangeCommand([.. SelectedNodes.Select(node => (node, node.X, firstY))]));
        }

        private void AlignVertical(object? parameter)
        {
            if (SelectedNodes.Count == 0) return;
            double firstX = SelectedNodes[0].X;
            _undoRedoManager.Execute(new NodePositionChangeCommand([.. SelectedNodes.Select(node => (node, firstX, node.Y))]));
        }

        private void Undo(object? parameter) => _undoRedoManager.Undo();
        private bool CanUndo(object? parameter) => _undoRedoManager.CanUndo;

        private void Redo(object? parameter) => _undoRedoManager.Redo();
        private bool CanRedo(object? parameter) => _undoRedoManager.CanRedo;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
