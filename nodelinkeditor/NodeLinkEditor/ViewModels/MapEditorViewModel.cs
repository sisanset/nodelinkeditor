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
using System.Collections.Generic;
using NodeLinkEditor.Views;

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

        private NodeViewModel? _startNode;
        public NodeViewModel? StartNode
        {
            get
            {
                return _startNode;
            }
            set
            {
                if (_startNode != null)
                { _startNode.IsStartNode = false; }
                _startNode = value;
                if (_startNode != null)
                { _startNode.IsStartNode = true; }
                OnPropertyChanged();
            }
        }
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
                if (_selectedHelperLine != null)
                { _selectedHelperLine.IsSelected = false; }
                SelectedHelperLine = null;
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
                SelectedHelperLine = null;
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

        private EditMode _selectedMode;
        public EditMode SelectedMode
        {
            get { return _selectedMode; }
            set
            {
                _selectedMode = value;
                OnPropertyChanged();
            }
        }
        public bool IsNodeLinkMode
        {
            get { return SelectedMode == EditMode.NodeLink; }
            set
            {
                if (value)
                {
                    SelectedMode = EditMode.NodeLink;
                    OnPropertyChanged(nameof(SelectedMode));
                }
            }
        }
        public bool IsHelperLineMode
        {
            get { return SelectedMode == EditMode.HelperLine; }
            set
            {
                if (value)
                {
                    SelectedMode = EditMode.HelperLine;
                    OnPropertyChanged(nameof(SelectedMode));
                }
            }
        }
        public ObservableCollection<HelperLineViewModel> HelperLines { get; set; } = [];
        public HelperLineViewModel? _selectedHelperLine;
        public HelperLineViewModel? SelectedHelperLine
        {
            get => _selectedHelperLine;
            set
            {
                if (_selectedHelperLine != null)
                { _selectedHelperLine.IsSelected = false; }
                _selectedHelperLine = value;
                if (_selectedHelperLine != null)
                { _selectedHelperLine.IsSelected = true; }
                OnPropertyChanged();

                if (_selectedHelperLine == null)
                { return; }
                SelectedNode = null;
                SelectedLink = null;
            }
        }
        private bool _isDrawingHelperLine;
        public bool IsDrawingHelperLine
        {
            get => _isDrawingHelperLine;
            set
            {
                _isDrawingHelperLine = value;
                OnPropertyChanged();
            }
        }
        private HelperLineViewModel _DrawingHelperLine = new HelperLineViewModel();
        public HelperLineViewModel DrawingHelperLine
        {
            get => _DrawingHelperLine;
            set
            {
                _DrawingHelperLine = value;
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
        public ICommand AlignLineEqualCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand RemoveNodeCommand { get; }
        public ICommand RemoveLinkCommand { get; }
        public ICommand MoveNodeCommand { get; }
        public ICommand AddAssociatedCommand { get; }
        public ICommand RemoveAssociatedCommand { get; }
        public ICommand ClearSelectionCommand { get; }
        public ICommand CreateHelperLineCommand { get; }
        public ICommand RemoveHelperLineCommand { get; }
        public ICommand MoveHelperLineCommand { get; }
        public ICommand CreateNodeAtIntersectionCommand { get; }

        public void ClearSelection()
        {
            SelectedNode = null;
            SelectedLink = null;
            foreach (var node in SelectedNodes.Where(n => n.IsReferenced))
            { node.IsReferenced = false; }
            SelectedNodes.Clear();
            StartNode = null;
            EndNode = null;
            IsDrawingHelperLine = false;
            SelectedHelperLine = null;
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
            AlignLineEqualCommand = new Others.RelayCommand(AlignLineEqual, CanAlignLineEqual);
            UndoCommand = new Others.RelayCommand(Undo, CanUndo);
            RedoCommand = new Others.RelayCommand(Redo, CanRedo);
            RemoveNodeCommand = new Others.RelayCommand(RemoveNode);
            RemoveLinkCommand = new Others.RelayCommand(RemoveLink);
            MoveNodeCommand = new Others.RelayCommand(MoveNode);
            CreateHelperLineCommand = new Others.RelayCommand(CreateHelperLine);
            RemoveHelperLineCommand = new Others.RelayCommand(RemoveHelperLine);
            MoveHelperLineCommand = new Others.RelayCommand(MoveHelperLine);
            CreateNodeAtIntersectionCommand = new Others.RelayCommand(CreateNodeAtIntersection, CanCreateNodeAtIntersection);

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
                FileIO.SaveNodeLinkToJson(saveFileDialog.FileName, _mapData, Nodes, Links, HelperLines);
            }
        }

        private void LoadNodeLink(object? parameter)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() != true)
            { return; }
            _undoRedoManager.Clear();
            ClearSelection();
            (var mapData, Nodes, Links, HelperLines) = FileIO.LoadNodeLinkFromJson(openFileDialog.FileName);
            if (mapData.YamlFilePath != string.Empty)
            {
                MapData = mapData;
                SetCoordConv();
            }
            OnPropertyChanged(nameof(Nodes));
            OnPropertyChanged(nameof(Links));
            OnPropertyChanged(nameof(HelperLines));
        }

        private void LoadMap(object? parameter)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "YAML files (*.yaml)|*.yaml|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() != true)
            { return; }
            _undoRedoManager.Clear();
            ClearSelection();
            Links.Clear();
            Nodes.Clear();
            HelperLines.Clear();
            var mapData = FileIO.LoadMapFromYaml(openFileDialog.FileName);
            MapData = mapData;
            SetCoordConv();
        }
        private void SetCoordConv()
        {
            CoordConv.Height = MapData.Height;
            CoordConv.Width = MapData.Width;
            CoordConv.Resolution = MapData.Resolution;
            CoordConv.OriginX = MapData.Origin[0];
            CoordConv.OriginY = MapData.Origin[1];
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
        private void AlignLineEqual(object? parameter)
        {
            var nodes = SelectedNodes.Where(n => n != null).ToList();
            var nodeNum = nodes.Count;
            if (nodeNum <= 2) return;
            var firstX = nodes[0].X;
            var lastX = nodes.Last().X;
            var firstY = nodes[0].Y;
            var lastY = nodes.Last().Y;
            double intervalX = (lastX - firstX) / (nodeNum - 1);
            double intervalY = (lastY - firstY) / (nodeNum - 1);
            List<(NodeViewModel, double, double)> alignedNodes = [];
            for (var i = 1; i < nodeNum - 1; i++)
            {
                alignedNodes.Add((nodes[i], firstX + i * intervalX, firstY + i * intervalY));
            }
            _undoRedoManager.Execute(new NodePositionChangeCommand(alignedNodes));
        }
        private bool CanAlignLineEqual(object? parameter) => SelectedNodes.Where(n => n != null).Count() > 2;

        private void Undo(object? parameter) => _undoRedoManager.Undo();
        private bool CanUndo(object? parameter) => _undoRedoManager.CanUndo;

        private void Redo(object? parameter) => _undoRedoManager.Redo();
        private bool CanRedo(object? parameter) => _undoRedoManager.CanRedo;

        private void CreateHelperLine(object? parameter)
        {
            if (IsDrawingHelperLine && parameter is HelperLineViewModel line)
            {
                _undoRedoManager.Execute(new AddCommand<HelperLineViewModel>(HelperLines, line));
            }
            IsDrawingHelperLine = false;
        }
        private void RemoveHelperLine(object? parameter)
        {
            if (parameter is HelperLineViewModel line)
            {
                if (SelectedHelperLine == line)
                { SelectedHelperLine = null; }
                _undoRedoManager.Execute(new RemoveCommand<HelperLineViewModel>(HelperLines, line));
            }
        }
        private void MoveHelperLine(object? parameter)
        {
            if (parameter is (HelperLineViewModel line, HelperLine newLine))
            {
                _undoRedoManager.Execute(new HelperLinePositionChangeCommand(line, newLine));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CreateNodeAtIntersection(object? parameter)
        {
            foreach (var intersection in FindIntersections())
            {
                if (Nodes.Any(node => double.Hypot(node.X - intersection.X, node.Y - intersection.Y) < 0.1))
                { continue; }
                CreateNode(intersection);
            }
        }

        private bool CanCreateNodeAtIntersection(object? parameter) => 1 < HelperLines.Count;

        public List<System.Windows.Point> FindIntersections()
        {
            var intersections = new List<System.Windows.Point>();
            for (int i = 0; i < HelperLines.Count; i++)
            {
                for (int j = i + 1; j < HelperLines.Count; j++)
                {
                    var line1 = HelperLines[i];
                    var line2 = HelperLines[j];

                    System.Windows.Point? intersection = line1.Intersect(line2);
                    if (intersection != null)
                    {
                        intersections.Add(intersection.Value);
                    }
                }
            }
            return intersections;
        }
    }
}
