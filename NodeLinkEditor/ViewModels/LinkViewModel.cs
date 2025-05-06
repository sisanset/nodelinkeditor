using NodeLinkEditor.Models;
using NodeLinkEditor.Others;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Linq;

namespace NodeLinkEditor.ViewModels
{
    public class LinkViewModel : INotifyPropertyChanged
    {
        private Link _link;
        private NodeViewModel _startNode;
        private NodeViewModel _endNode;

        public NodeViewModel StartNode
        {
            get { return _startNode; }
            set
            {
                _startNode = value;
                _link.StartNodeID = value.ID;
                OnPropertyChanged();
            }
        }

        public NodeViewModel EndNode
        {
            get { return _endNode; }
            set
            {
                _endNode = value;
                _link.EndNodeID = value.ID;
                OnPropertyChanged();
            }
        }

        public Point StartPoint
        {
            get { return new Point(StartNode.X, StartNode.Y); }
        }

        public Point EndPoint
        {
            get { return new Point(EndNode.X, EndNode.Y); }
        }
        public LinkViewModel(NodeViewModel start, NodeViewModel end)
        {
            _link = new Link { StartNodeID = start.ID, EndNodeID = end.ID };
            _startNode = start;
            _endNode = end;
        }
        public Link GetLinkCopy()
        {
            return new Link
            {
                ID = _link.ID,
                StartNodeID = StartNode.ID,
                EndNodeID = EndNode.ID,
                Attributes = [.. _link.Attributes.Select(a => a)],
            };
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
