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
        public static string GetNodeName(bool isIdx)
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
        public static void RegisterNodename(string name)
        {
            if (double.TryParse(name, out double idx))
            { NodeNameSet.Add((int)idx); }
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
            RegisterNodename(Name);
            _x = node.X;
            _y = node.Y;
            var nodeAttributes = new List<NodeAttribute>(node.Attributes.Distinct());
            AssociatedNodes = [.. node.AssociatedNodes.Distinct()];
            AttributeOptions = [.. Enum.GetValues<NodeAttribute>().Cast<NodeAttribute>().Select(attr =>
                {
                    var option = new AttributeOption<NodeAttribute>(attr);
                    if (nodeAttributes.Contains(attr))
                    { option.IsSelected = true; }
                    return option;
                })];
        }
        public NodeViewModel(double x, double y, bool getNodeName = true) : this(new Node() { X = x, Y = y, Name = GetNodeName(getNodeName) })
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
                Attributes = [.. AttributeOptions.Where(a => a.IsSelected).Select(a => a.Attribute)],
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
