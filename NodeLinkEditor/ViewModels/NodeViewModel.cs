using NodeLinkEditor.Models;
using NodeLinkEditor.Others;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Collections.ObjectModel;


namespace NodeLinkEditor.ViewModels
{
    public class NodeViewModel : INotifyPropertyChanged
    {
        private static int NodeIdx = 0;
        private static HashSet<int> NodeNameSet = new HashSet<int>();
        private static string GetNodeName(bool isIdx)
        {
            if (!isIdx) { return ""; }
            if (NodeNameSet.Contains(NodeIdx))
            {
                NodeIdx++;
                return GetNodeName(isIdx);
            }
            NodeNameSet.Add(NodeIdx);
            return $"{NodeIdx++}";
        }
        private static string GetNodeName(string offset)
        {
            if (string.IsNullOrEmpty(offset)) { return GetNodeName(true); }
            if (!int.TryParse(offset, out int idx)) { return GetNodeName(true); }
            while (NodeNameSet.Contains(idx))
            { idx++; }
            NodeNameSet.Add(idx);
            return $"{idx}";
        }
        private static void RegisterNodeName(string name)
        {
            if (int.TryParse(name, out int idx))
            { NodeNameSet.Add(idx); }
        }
        public static void ResetNodeName()
        {
            NodeIdx = 0;
            NodeNameSet.Clear();
        }
        public bool ChangeNodeName(string name)
        {
            if (!int.TryParse(name, out int newIdx)) { return false; }
            if (NodeNameSet.Contains(newIdx)) { return false; }
            var oldIdx = int.Parse(Name);
            NodeNameSet.Remove(oldIdx);
            NodeNameSet.Add(newIdx);
            Name = name;
            OnPropertyChanged(nameof(Name));
            NodeIdx = new[] { oldIdx, newIdx + 1, NodeIdx }.Min();
            return true;
        }

        public Guid ID { get; init; }
        public string Name { get; set; } = "";
        public double _x;
        public double X
        {
            get => _x;
            set
            {
                _x = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Point));
            }
        }
        public double _y;
        public double Y
        {
            get => _y;
            set
            {
                _y = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Point));
            }
        }
        public Point Point
        {
            get => new(X, Y);
        }
        public ObservableCollection<string> AssociatedNodes { get; set; } = [];
        public ObservableCollection<AttributeOption<NodeAttribute>> AttributeOptions { get; }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }
        private bool _isReferenced = false;
        public bool IsReferenced
        {
            get => _isReferenced;
            set { _isReferenced = value; OnPropertyChanged(); }
        }
        private bool _isStartNode = false;
        public bool IsStartNode
        {
            get => _isStartNode;
            set { _isStartNode = value; OnPropertyChanged(); }
        }

        public NodeViewModel(Node node)
        {
            ID = node.ID;
            Name = node.Name;
            RegisterNodeName(Name);
            _x = node.X;
            _y = node.Y;
            var nodeAttributes = new List<NodeAttribute>(node.Attributes.Distinct());
            AssociatedNodes = [.. node.AssociatedNodes.Distinct()];
            AttributeOptions = [.. Enum.GetValues<NodeAttribute>().Cast<NodeAttribute>().Select(attr =>
                    new AttributeOption<NodeAttribute>(attr){ IsSelected=nodeAttributes.Contains(attr)}
                )];
        }

        public NodeViewModel(double x, double y, bool getNodeName = true) : this(new Node() { X = x, Y = y, Name = GetNodeName(getNodeName) })
        {
        }
        public NodeViewModel(double x, double y, string name) : this(new Node() { X = x, Y = y, Name = GetNodeName(name) })
        {
        }
        public Node GetNodeCopy()
        {
            return new Node
            {
                ID = ID,
                Name = Name,
                X = X,
                Y = Y,
                AssociatedNodes = [.. AssociatedNodes],
                Attributes = [.. AttributeOptions.Where(a => a.IsSelected == true).Select(a => a.Attribute)],
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
