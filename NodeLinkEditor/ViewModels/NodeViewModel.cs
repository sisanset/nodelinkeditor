using NodeLinkEditor.Models;
using NodeLinkEditor.Others;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace NodeLinkEditor.ViewModels
{


    public class NodeViewModel : INotifyPropertyChanged
    {
        private Node _node;
        public Guid ID { get { return _node.ID; } }
        public double X
        {
            get { return _node.X; }
            set
            {
                _node.X = value;
                OnPropertyChanged();
            }
        }

        public double Y
        {
            get { return _node.Y; }
            set
            {
                _node.Y = value;
                OnPropertyChanged();
            }
        }

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

        public NodeViewModel(Node node)
        {
            _node = node;
        }
        public NodeViewModel(double x, double y)
        {
            _node = new Node { X = x, Y = y };
        }
        public Node GetNodeCopy()
        {
            return new Node
            {
                ID = ID,
                X = X,
                Y = Y,
                AssociatedNodes = [.. _node.AssociatedNodes.Select(n => n)],
                Attributes = [.. _node.Attributes.Select(a => a)],
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
