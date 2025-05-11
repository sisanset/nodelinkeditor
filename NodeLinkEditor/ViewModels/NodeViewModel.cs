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
            get { return _x; }
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
            get { return _y; }
            set
            {
                _y = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Point));
            }
        }
        public Point Point
        {
            get { return new Point(X, Y); }
        }
        public ObservableCollection<Guid> AssociatedNodes { get; set; } = [];
        public ObservableCollection<NodeAttribute> Attributes { get; set; } = [];

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
        private bool _isReferenced = false;
        public bool IsReferenced
        {
            get { return _isReferenced; }
            set
            {
                _isReferenced = value;
                OnPropertyChanged();
            }
        }

        public NodeViewModel(Node node)
        {
            ID = node.ID;
            _x = node.X;
            _y = node.Y;
            Attributes = [.. node.Attributes.Distinct()];
            AssociatedNodes = [.. node.AssociatedNodes.Distinct()];
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
                Attributes = [.. Attributes],
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
