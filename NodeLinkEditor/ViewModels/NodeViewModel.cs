using NodeLinkEditor.Models;
using NodeLinkEditor.Others;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using System;

namespace NodeLinkEditor.ViewModels
{
    public class NodeViewModel : INotifyPropertyChanged
    {
        public Guid ID { get; init; }
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
        public ObservableCollection<Guid> AssociatedNodes { get; set; } = [];
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
        public NodeViewModel(double x, double y) : this(new Node() { X = x, Y = y })
        {
        }
        public Node GetNodeCopy()
        {
            return new Node
            {
                ID = ID,
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
