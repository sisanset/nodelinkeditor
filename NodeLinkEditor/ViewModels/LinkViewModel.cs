using NodeLinkEditor.Models;
using NodeLinkEditor.Others;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace NodeLinkEditor.ViewModels
{
    public class LinkViewModel : INotifyPropertyChanged
    {

        public Guid ID { get; set; }
        public ObservableCollection<AttributeOption<LinkAttribute>> AttributeOptions { get; }

        private double _startToEndCost = 1.0;
        public double StartToEndCost
        {
            get => _startToEndCost;
            set { _startToEndCost = value; OnPropertyChanged(); }
        }
        private double _endToStartCost = 1.0;
        public double EndToStartCost
        {
            get => _endToStartCost;
            set { _endToStartCost = value; OnPropertyChanged(); }
        }

        private NodeViewModel _startNode;
        public NodeViewModel StartNode
        {
            get => _startNode;
            set { _startNode = value; OnPropertyChanged(); }
        }

        private NodeViewModel _endNode;
        public NodeViewModel EndNode
        {
            get => _endNode;
            set { _endNode = value; OnPropertyChanged(); }
        }
        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }
        private bool _isTwoWay = false;
        public bool IsTwoWay
        {
            get => _isTwoWay;
            set
            {
                _isTwoWay = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EndThickness));
            }
        }
        private double _startThickness = 10;
        public double StartThickness
        {
            get => _startThickness;
            set { _startThickness = value; OnPropertyChanged(); }
        }
        private double _endThickness = 2;
        public double EndThickness
        {
            get => _isTwoWay ? _startThickness : _endThickness;
            set { _endThickness = value; OnPropertyChanged(); }
        }
        public Point StartPoint
        {
            get => new(StartNode.X, StartNode.Y);
        }

        public Point EndPoint
        {
            get => new(EndNode.X, EndNode.Y);
        }
        public LinkViewModel(Link link, NodeViewModel start, NodeViewModel end)
        {
            ID = link.ID;
            var linkAttributes = new List<LinkAttribute>(link.Attributes.Distinct());
            AttributeOptions = [.. Enum.GetValues<LinkAttribute>().Cast<LinkAttribute>().Select(attr =>
                {
                    var option=new AttributeOption<LinkAttribute>(attr);
                    if(linkAttributes.Contains(attr))
                    { option.IsSelected=true; }
                    return option;
                })];
            StartToEndCost = link.StartToEndCost;
            EndToStartCost = link.EndToStartCost;
            _startNode = start;
            _endNode = end;
            IsTwoWay = link.IsTwoWay;
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
                Attributes = [.. AttributeOptions.Where(a => a.IsSelected).Select(a => a.Attribute)],
                StartToEndCost = StartToEndCost,
                EndToStartCost = EndToStartCost,
                IsTwoWay = IsTwoWay,
            };
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
