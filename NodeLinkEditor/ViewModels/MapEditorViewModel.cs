using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using NodeLinkEditor.Others;
using Microsoft.Win32;
using System.Runtime.CompilerServices;
using NodeLinkEditor.Models;
using System.Windows;
using Microsoft.VisualBasic;

namespace NodeLinkEditor.ViewModels
{
    public class MapEditorViewModel : INotifyPropertyChanged
    {
        public double NodeInterval { get; set; } = 50.0;
        public double IntersectionInterval { get; set; } = 10.0;
        private UndoRedoManager _undoRedoManager = new();
        public ObservableCollection<NodeViewModel> Nodes { get; set; } = [];
        public ObservableCollection<LinkViewModel> Links { get; set; } = [];
        private int _nodeSize = 20;
        public int NodeSize
        {
            get => _nodeSize;
            set { _nodeSize = value; OnPropertyChanged(); }
        }

        private NodeViewModel? _startNode;
        public NodeViewModel? StartNode
        {
            get => _startNode;
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
            get => _selectedNode;
            set
            {
                _selectedNode = value;
                OnPropertyChanged();

                if (_selectedNode == null)
                { return; }
                ClearSelectedLinks();
                SelectedHelperLine = null;
            }
        }
        public ObservableCollection<NodeViewModel> SelectedNodes { get; set; } = [];

        public ObservableCollection<LinkViewModel> SelectedLinks { get; set; } = [];
        private LinkViewModel? _selectedLink;
        public LinkViewModel? SelectedLink
        {
            get => _selectedLink;
            set
            {
                _selectedLink = value;
                OnPropertyChanged();

                if (_selectedLink == null)
                { return; }
                ClearSelectedNodes();
                SelectedHelperLine = null;
            }
        }

        private MapData _mapData = new() { Width = 800, Height = 450 };
        public MapData MapData
        {
            get => _mapData;
            set { _mapData = value; OnPropertyChanged(); }
        }

        private EditMode _selectedMode = EditMode.Node;
        public EditMode SelectedMode
        {
            get => _selectedMode;
            set
            {
                _selectedMode = value;
                OnPropertyChanged();
                ClearSelection();
            }
        }
        public bool IsNodeMode
        {
            get => SelectedMode == EditMode.Node;
            set
            {
                if (value)
                {
                    SelectedMode = EditMode.Node;
                    OnPropertyChanged(nameof(SelectedMode));
                }
            }
        }
        public bool IsLinkMode
        {
            get => SelectedMode == EditMode.Link;
            set
            {
                if (value)
                {
                    SelectedMode = EditMode.Link;
                }
            }
        }
        public bool IsHelperLineMode
        {
            get => SelectedMode == EditMode.HelperLine;
            set
            {
                if (value)
                {
                    SelectedMode = EditMode.HelperLine;
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
                ClearSelectedNodes();
                ClearSelectedLinks();
            }
        }
        private bool _isDrawingHelperLine = false;
        public bool IsDrawingHelperLine
        {
            get => _isDrawingHelperLine;
            set { _isDrawingHelperLine = value; OnPropertyChanged(); }
        }
        private HelperLineViewModel _DrawingHelperLine = new();
        public HelperLineViewModel DrawingHelperLine
        {
            get => _DrawingHelperLine;
            set { _DrawingHelperLine = value; OnPropertyChanged(); }
        }

        public AMRMqttClient MqttClient { get; }
        public AMRViewModel AMRModel { get; set; } = new();

        private bool _isMqttConnected = false;
        public bool IsMqttConnected
        {
            get => _isMqttConnected;
            set { _isMqttConnected = value; OnPropertyChanged(); }
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
        public ICommand DuplicateHelperLineCommand { get; }
        public ICommand RemoveHelperLineCommand { get; }
        public ICommand MoveHelperLineCommand { get; }
        public ICommand CreateNodeAtIntersectionCommand { get; }
        public ICommand CreateNodesBetweenCommand { get; }
        public ICommand SetNodesDistanceCommand { get; }

        public ICommand ConnectMQTTCommand { get; }
        public ICommand DisconnectMQTTCommand { get; }

        public void ClearSelection()
        {
            ClearSelectedLinks();
            ClearSelectedNodes();
            StartNode = null;
            EndNode = null;
            SelectedHelperLine = null;
            IsDrawingHelperLine = false;
        }
        public void AddSelectedLink(LinkViewModel link)
        {
            if (link.IsSelected) { return; }
            link.IsSelected = true;
            SelectedLinks.Add(link);
            if (SelectedLinks.Count == 1)
            { SelectedLink = new LinkViewModel(link.GetLinkCopy(), link.StartNode, link.EndNode); }
            else
            { SelectedLink = new LinkViewModel(link.StartNode, link.EndNode); }
        }
        public void RemoveSelectedLink(LinkViewModel link)
        {
            if (!link.IsSelected) { return; }
            link.IsSelected = false;
            SelectedLinks.Remove(link);
            if (SelectedLinks.Count == 0)
            { SelectedLink = null; }
            else if (SelectedLinks.Count == 1)
            { SelectedLink = new LinkViewModel(SelectedLinks[0].GetLinkCopy(), SelectedLinks[0].StartNode, SelectedLinks[0].EndNode); }
        }
        public void ClearSelectedLinks()
        {
            foreach (var link in SelectedLinks)
            { link.IsSelected = false; }
            SelectedLinks.Clear();
            SelectedLink = null;
        }

        public void AddSelectedNode(NodeViewModel node)
        {
            if (node.IsSelected) { return; }
            node.IsSelected = true;
            SelectedNodes.Add(node);
            if (SelectedNodes.Count == 1)
            { SelectedNode = new NodeViewModel(node.GetNodeCopy()); }
            else
            { SelectedNode = new NodeViewModel(0, 0, false); }
        }
        public void RemoveSelectedNode(NodeViewModel node)
        {
            if (!node.IsSelected) { return; }
            node.IsSelected = false;
            SelectedNodes.Remove(node);
            if (SelectedNodes.Count == 0)
            { SelectedNode = null; }
            else if (SelectedNodes.Count == 1)
            { SelectedNode = new NodeViewModel(SelectedNodes[0].GetNodeCopy()); }
        }
        public void ClearSelectedNodes()
        {
            foreach (var node in SelectedNodes)
            { node.IsSelected = false; }
            SelectedNodes.Clear();
            SelectedNode = null;
        }

        public MapEditorViewModel()
        {
            CreateNodeCommand = new Others.RelayCommand(CreateNode, CanCreateNode);
            CreateLinkCommand = new Others.RelayCommand(CreateLink, CanCreateLink);
            SaveNodeLinkCommand = new Others.RelayCommand(SaveNodeLink);
            LoadNodeLinkCommand = new Others.RelayCommand(LoadNodeLink);
            LoadMapCommand = new Others.RelayCommand(LoadMap);
            AlignHorizontalCommand = new Others.RelayCommand(AlignHorizontal, CanAlignLineHorizontalVertical);
            AlignVerticalCommand = new Others.RelayCommand(AlignVertical, CanAlignLineHorizontalVertical);
            AlignLineEqualCommand = new Others.RelayCommand(AlignLineEqual, CanAlignLineEqual);
            UndoCommand = new Others.RelayCommand(Undo, CanUndo);
            RedoCommand = new Others.RelayCommand(Redo, CanRedo);
            RemoveNodeCommand = new Others.RelayCommand(RemoveNode, CanRemoveNode);
            RemoveLinkCommand = new Others.RelayCommand(RemoveLink, CanRemoveLink);
            MoveNodeCommand = new Others.RelayCommand(MoveNode);
            CreateHelperLineCommand = new Others.RelayCommand(CreateHelperLine);
            DuplicateHelperLineCommand = new Others.RelayCommand(DuplicateHelperLine);
            RemoveHelperLineCommand = new Others.RelayCommand(RemoveHelperLine);
            MoveHelperLineCommand = new Others.RelayCommand(MoveHelperLine, CanMoveHelperLine);
            CreateNodeAtIntersectionCommand = new Others.RelayCommand(CreateNodeAtIntersection, CanCreateNodeAtIntersection);

            AddAssociatedCommand = new Others.RelayCommand((_) =>
            {
                for (var i = 0; i < SelectedNodes.Count; i++)
                {
                    for (var j = i + 1; j < SelectedNodes.Count; j++)
                    {
                        if (!SelectedNodes[i].AssociatedNodes.Contains(SelectedNodes[j].Name))
                        { SelectedNodes[i].AssociatedNodes.Add(SelectedNodes[j].Name); }
                        if (!SelectedNodes[j].AssociatedNodes.Contains(SelectedNodes[i].Name))
                        { SelectedNodes[j].AssociatedNodes.Add(SelectedNodes[i].Name); }
                    }
                }
            });
            RemoveAssociatedCommand = new Others.RelayCommand((nodeName) =>
            {
                if (nodeName is not string name) { return; }
                if (SelectedNodes.Count != 1) { return; }
                var node = SelectedNodes[0];
                node.AssociatedNodes.Remove(name);
                Nodes.SingleOrDefault(n => n.Name == name)?.AssociatedNodes.Remove(node.Name);
                SelectedNode = new NodeViewModel(node.GetNodeCopy());
            });

            ClearSelectionCommand = new Others.RelayCommand((_) => ClearSelection());

            CreateNodesBetweenCommand = new Others.RelayCommand(
                (param) =>
                {
                    if (param is string betweenType)
                    {
                        if (SelectedNodes.Count != 2) return;
                        if (betweenType == "NodeInterval")
                        { CreateNodesBetween(SelectedNodes[0], SelectedNodes[1], 0.0, 0.0, NodeInterval); }
                        else if (betweenType == "StartEndInterval")
                        { CreateNodesBetween(SelectedNodes[0], SelectedNodes[1], IntersectionInterval, IntersectionInterval, NodeInterval); }
                        else if (betweenType == "StartInterval")
                        { CreateNodesBetween(SelectedNodes[0], SelectedNodes[1], IntersectionInterval, 0.0, NodeInterval); }
                        else if (betweenType == "EndInterval")
                        { CreateNodesBetween(SelectedNodes[0], SelectedNodes[1], 0.0, IntersectionInterval, NodeInterval); }
                        else
                        { return; }
                    }
                },
                CanCreateNodesBetween);
            SetNodesDistanceCommand = new Others.RelayCommand(SetNodesDistance, CanSetNodesDistance);

            MqttClient = new AMRMqttClient();
            MqttClient.SetMessageReceivedEvent((x, y, z) => Application.Current.Dispatcher.Invoke(() => AMRModel.SetAMR(x, y, z)));
            MqttClient.SetConnectedEvent(() => Application.Current.Dispatcher.Invoke(() => IsMqttConnected = true));
            MqttClient.SetDisconnectedEvent(() => Application.Current.Dispatcher.Invoke(() => { IsMqttConnected = false; AMRModel.IsConnected = false; }));

            ConnectMQTTCommand = new Others.RelayCommand(async (_) =>
            {
                await MqttClient.Connect();
                await MqttClient.Subscribe();
            });
            DisconnectMQTTCommand = new Others.RelayCommand(async (_) =>
            {
                AMRModel.IsConnected = false;
                await MqttClient.Unsubscribe();
                await MqttClient.Disconnect();
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
                RemoveSelectedNode(node);
                foreach (var l in SelectedLinks.Where(link => link.StartNode == node || link.EndNode == node))
                { RemoveSelectedLink(l); }
                foreach (var n in Nodes.Where(n => n.AssociatedNodes.Contains(node.Name)))
                {
                    n.AssociatedNodes.Remove(node.Name);
                    if (SelectedNodes.Count == 1 && n.Name == SelectedNode?.Name)
                    { SelectedNode = new NodeViewModel(n.GetNodeCopy()); }
                }
                if (StartNode == node)
                { StartNode = null; }
                _undoRedoManager.Execute(new RemoveNodeCommand(Nodes, node, Links));
            }
        }
        private bool CanRemoveNode(object? parameter) => parameter is NodeViewModel;
        private void RemoveLink(object? parameter)
        {
            if (parameter is LinkViewModel link)
            {
                RemoveSelectedLink(link);
                _undoRedoManager.Execute(new RemoveCommand<LinkViewModel>(Links, link));
            }
        }
        private bool CanRemoveLink(object? parameter) => parameter is LinkViewModel;

        private void CreateNode(object? parameter)
        {
            if (parameter is System.Windows.Point point)
            {
                _undoRedoManager.Execute(new AddCommand<NodeViewModel>(Nodes, new NodeViewModel(point.X, point.Y)));
            }
            if (parameter is AMRViewModel amrModel)
            {
                _undoRedoManager.Execute(new AddCommand<NodeViewModel>(Nodes, new NodeViewModel(amrModel.X, amrModel.Y)));
                Nodes.Last().AttributeOptions.First(a => a.Attribute == NodeAttribute.Working).IsSelected = true;
            }
        }
        private bool CanCreateNode(object? parameter) => parameter is System.Windows.Point || parameter is AMRViewModel;

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
            ClearMapDate();
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
            ClearMapDate();
            var mapData = FileIO.LoadMapFromYaml(openFileDialog.FileName);
            MapData = mapData;
            SetCoordConv();
        }
        private void ClearMapDate()
        {
            _undoRedoManager.Clear();
            ClearSelectedLinks();
            Links.Clear();
            Nodes.Clear();
            HelperLines.Clear();
            NodeViewModel.ResetNodeName();
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
            var nodes = SelectedNodes.Where(n => n != null).ToList();
            var nodeNum = nodes.Count;
            if (nodeNum < 2) return;
            double firstY = nodes[0].Y;
            _undoRedoManager.Execute(new NodePositionChangeCommand([.. nodes.Select(node => (node, node.X, firstY))]));
        }

        private void AlignVertical(object? parameter)
        {
            var nodes = SelectedNodes.Where(n => n != null).ToList();
            var nodeNum = nodes.Count;
            if (nodeNum < 2) return;
            double firstX = nodes[0].X;
            _undoRedoManager.Execute(new NodePositionChangeCommand([.. nodes.Select(node => (node, firstX, node.Y))]));
        }
        private bool CanAlignLineHorizontalVertical(object? parameter) => 1 < SelectedNodes.Where(n => n != null).Count();
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
        private bool CanAlignLineEqual(object? parameter) => 2 < SelectedNodes.Where(n => n != null).Count();

        private void Undo(object? parameter) => _undoRedoManager.Undo();
        private bool CanUndo(object? parameter) => _undoRedoManager.CanUndo;

        private void Redo(object? parameter) => _undoRedoManager.Redo();
        private bool CanRedo(object? parameter) => _undoRedoManager.CanRedo;

        private void CreateHelperLine(object? parameter)
        {
            if (parameter is HelperLineViewModel line)
            {
                _undoRedoManager.Execute(new AddCommand<HelperLineViewModel>(HelperLines, line));
            }
        }
        private void DuplicateHelperLine(object? parameter)
        {
            if (parameter is HelperLineViewModel line)
            {
                var newLine = new HelperLineViewModel(line.GetHelperLineCopy());
                _undoRedoManager.Execute(new AddCommand<HelperLineViewModel>(HelperLines, newLine));
            }
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
        private bool CanMoveHelperLine(object? parameter) => parameter is (HelperLineViewModel, HelperLine);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CreateNodeAtIntersection(object? parameter)
        {
            var intersections = new List<NodeViewModel>();
            foreach (var (intersection, isIntersection) in FindIntersections())
            {
                if (Nodes.Any(node => double.Hypot(node.X - intersection.X, node.Y - intersection.Y) < 0.1))
                { continue; }
                CreateNode(intersection);
                if (isIntersection)
                {
                    var newNode = Nodes.Last();
                    newNode.AttributeOptions.First(a => a.Attribute == NodeAttribute.Intersection).IsSelected = true;
                    intersections.Add(newNode);
                }
            }
            for (var i = 0; i < intersections.Count - 1; i++)
            {
                for (var j = i + 1; j < intersections.Count; j++)
                {
                    if (
                        Math.Abs(intersections[i].X - intersections[j].X) < 3 &&
                        Math.Abs(intersections[i].Y - intersections[j].Y) < 3
                        )
                    {
                        intersections[i].AssociatedNodes.Add(intersections[j].Name);
                        intersections[j].AssociatedNodes.Add(intersections[i].Name);
                    }
                }

            }
        }

        private bool CanCreateNodeAtIntersection(object? parameter) => 1 < HelperLines.Count;

        public List<(System.Windows.Point, bool)> FindIntersections()
        {
            var intersections = new List<(System.Windows.Point, bool)>();
            for (int i = 0; i < HelperLines.Count; i++)
            {
                for (int j = i + 1; j < HelperLines.Count; j++)
                {
                    var line1 = HelperLines[i];
                    var line2 = HelperLines[j];

                    System.Windows.Point? intersection = line1.Intersect(line2);
                    if (intersection != null)
                    {
                        intersections.Add((intersection.Value, line1.IsIntersection && line2.IsIntersection));
                    }
                }
            }
            return intersections;
        }

        private bool CanCreateNodesBetween(object? parameter) => SelectedNodes.Count == 2;

        // 次のようにnodeを置く
        // startNode-startInterval-node-interval-…-node-endInterval-endNode
        private void CreateNodesBetween(NodeViewModel startNode, NodeViewModel endNode, double startInterval, double endInterval, double interval)
        {
            double distance = Math.Sqrt(Math.Pow(endNode.X - startNode.X, 2) + Math.Pow(endNode.Y - startNode.Y, 2));

            if (distance - startInterval - endInterval <= 0.0)
            { return; }
            int numberOfNodes = (int)((distance - startInterval - endInterval) / interval);

            var startX = startNode.X + startInterval / distance * (endNode.X - startNode.X);
            var startY = startNode.Y + startInterval / distance * (endNode.Y - startNode.Y);
            var endX = endNode.X - endInterval / distance * (endNode.X - startNode.X);
            var endY = endNode.Y - endInterval / distance * (endNode.Y - startNode.Y);

            double deltaX = (endX - startX) / (numberOfNodes + 1);
            double deltaY = (endY - startY) / (numberOfNodes + 1);

            List<NodeViewModel> newNodes = [];
            List<LinkViewModel> newLinks = [];

            NodeViewModel prevNode = startNode;

            if (startInterval != 0.0)
            {
                var newNode = new NodeViewModel(startX, startY);
                newNodes.Add(newNode);
                newLinks.Add(new LinkViewModel(startNode, newNode));
                prevNode = newNode;
            }

            for (int i = 1; i <= numberOfNodes; i++)
            {
                double newX = startX + deltaX * i;
                double newY = startY + deltaY * i;
                var newNode = new NodeViewModel(newX, newY);
                newNodes.Add(newNode);
                newLinks.Add(new LinkViewModel(prevNode, newNode));
                prevNode = newNode;
            }
            if (endInterval != 0.0)
            {
                var newNode = new NodeViewModel(endX, endY);
                newNodes.Add(newNode);
                newLinks.Add(new LinkViewModel(prevNode, newNode));
                prevNode = newNode;
            }
            if (newNodes.Count == 0)
            { return; }
            newLinks.Add(new LinkViewModel(prevNode, endNode));
            foreach (var node in newNodes)
            { _undoRedoManager.Execute(new AddCommand<NodeViewModel>(Nodes, node)); }
            foreach (var link in newLinks)
            { _undoRedoManager.Execute(new AddCommand<LinkViewModel>(Links, link)); }
        }
        private void SetNodesDistance(object? parameter)
        {
            if (SelectedNodes.Count != 2) { return; }
            string? input = Interaction.InputBox("ノード間隔を入力してください:", "ノード間隔設定", NodeInterval.ToString());
            if (string.IsNullOrEmpty(input))
            {
                return;
            }

            if (double.TryParse(input, out double distance))
            {
                double diffX = SelectedNodes[1].X - SelectedNodes[0].X;
                double diffY = SelectedNodes[1].Y - SelectedNodes[0].Y;
                double currentDistance = Math.Sqrt(diffX * diffX + diffY * diffY);
                double newX = SelectedNodes[0].X + diffX * (distance / currentDistance);
                double newY = SelectedNodes[0].Y + diffY * (distance / currentDistance);
                MoveNode((SelectedNodes[1], newX, newY));
            }
            else
            {
                MessageBox.Show("無効な入力です。数値を入力してください。", "エラー");
            }
        }
        private bool CanSetNodesDistance(object? parameter) => SelectedNodes.Count == 2;

    }
}
