using NodeLinkEditor.Models;
using NodeLinkEditor.Others;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Linq;

namespace NodeLinkEditor.ViewModels
{
    public class LinkViewModel : INotifyPropertyChanged
    {

        public Guid ID { get; set; }
        public ObservableCollection<LinkAttribute> Attributes { get; set; } = [];

        private double _startToEndCost = 1.0;
        public double StartToEndCost
        {
            get { return _startToEndCost; }
            set { _startToEndCost = value; OnPropertyChanged(); }
        }
        private double _endToStartCost = 1.0;
        public double EndToStartCost
        {
            get { return _endToStartCost; }
            set { _endToStartCost = value; OnPropertyChanged(); }
        }

        private NodeViewModel _startNode;
        public NodeViewModel StartNode
        {
            get { return _startNode; }
            set
            {
                _startNode = value;
                OnPropertyChanged();
            }
        }

        private NodeViewModel _endNode;
        public NodeViewModel EndNode
        {
            get { return _endNode; }
            set
            {
                _endNode = value;
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
        public double StartThickness { get; set; } = 10;
        public double EndThickness { get; set; } = 2;
        public Point StartPoint
        {
            get { return new Point(StartNode.X, StartNode.Y); }
        }

        public Point EndPoint
        {
            get { return new Point(EndNode.X, EndNode.Y); }
        }
        public LinkViewModel(Link link, NodeViewModel start, NodeViewModel end)
        {
            ID = link.ID;
            Attributes = [.. link.Attributes];
            StartToEndCost = link.StartToEndCost;
            EndToStartCost = link.EndToStartCost;
            _startNode = start;
            _endNode = end;
        }
        public LinkViewModel(NodeViewModel start, NodeViewModel end) : this(new Link(), start, end)
        {
        }
        public Link GetLinkCopy()
        {
            return new Link
            {
                ID = ID,
                StartNodeID = StartNode.ID,
                EndNodeID = EndNode.ID,
                Attributes = [.. Attributes],
                StartToEndCost = StartToEndCost,
                EndToStartCost = EndToStartCost,
            };
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
