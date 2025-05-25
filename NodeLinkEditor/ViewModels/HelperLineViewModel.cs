using NodeLinkEditor.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NodeLinkEditor.ViewModels
{
    public class HelperLineViewModel : INotifyPropertyChanged
    {
        public Guid ID { get; init; }
        private double _startX;
        public double StartX
        {
            get => _startX;
            set { _startX = value; OnPropertyChanged(); }
        }
        private double _endX;
        public double EndX
        {
            get => _endX;
            set { _endX = value; OnPropertyChanged(); }
        }
        private double _startY;
        public double StartY
        {
            get => _startY;
            set { _startY = value; OnPropertyChanged(); }
        }
        private double _endY;
        public double EndY
        {
            get => _endY;
            set { _endY = value; OnPropertyChanged(); }
        }
        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }
        private bool _isIntersection = true;
        public bool IsIntersection
        {
            get => _isIntersection;
            set { _isIntersection = value; OnPropertyChanged(); }
        }
        public HelperLineViewModel(HelperLine helperLine)
        {
            ID = helperLine.ID;
            StartX = helperLine.StartX;
            StartY = helperLine.StartY;
            EndX = helperLine.EndX;
            EndY = helperLine.EndY;
        }
        public HelperLineViewModel() : this(new HelperLine
        {
            StartX = 0,
            StartY = 0,
            EndX = 0,
            EndY = 0
        })
        { }
        public HelperLine GetHelperLineCopy()
        {
            return new HelperLine()
            {
                ID = ID,
                StartX = StartX,
                StartY = StartY,
                EndX = EndX,
                EndY = EndY,
            };
        }
        public System.Windows.Point? Intersect(HelperLineViewModel other)
        {
            double x1 = StartX, y1 = StartY, x2 = EndX, y2 = EndY;
            double x3 = other.StartX, y3 = other.StartY, x4 = other.EndX, y4 = other.EndY;

            var line1_p3 = (x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1);
            var line1_p4 = (x2 - x1) * (y4 - y1) - (x4 - x1) * (y2 - y1);
            if (0 <= line1_p3 * line1_p4)
            { return null; }
            var line2_p1 = (x4 - x3) * (y1 - y3) - (x1 - x3) * (y4 - y3);
            var line2_p2 = (x4 - x3) * (y2 - y3) - (x2 - x3) * (y4 - y3);
            if (0 <= line2_p1 * line2_p2)
            { return null; }

            double denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (denominator == 0)
            { return null; }

            double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denominator;
            double u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / denominator;

            if (t > 0 && t < 1 && u > 0 && u < 1)
            {
                double intersectionX = x1 + t * (x2 - x1);
                double intersectionY = y1 + t * (y2 - y1);
                return new System.Windows.Point(intersectionX, intersectionY);
            }

            return null; // Lines do not intersect within the segments
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
