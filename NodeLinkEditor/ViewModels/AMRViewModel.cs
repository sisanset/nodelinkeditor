using Newtonsoft.Json;
using NodeLinkEditor.Others;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace NodeLinkEditor.ViewModels
{
    public class AMRViewModel : INotifyPropertyChanged
    {
        public class AmrData
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Yaw { get; set; }
        }
        private PointCollection _amrPoints = [new Point(50, 50), new Point(150, 50), new Point(100, 150)];
        public PointCollection AmrPoints
        {
            get => _amrPoints;
            set { _amrPoints = value; OnPropertyChanged(); }
        }
        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(); }
        }

        private double _x;
        public double X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(); }
        }
        private double _y;
        public double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(); }
        }
        private double _yaw;
        public double Yaw
        {
            get => _yaw;
            set { _yaw = value; OnPropertyChanged(); }
        }
        public void SetAMR(double x, double y, double yaw)
        {
            var pixelX = CoordConv.CoordXToPixelX(x);
            var pixelY = CoordConv.CoordYToPixelY(y);
            AmrPoints = [
                    new Point(pixelX + 15 * Math.Cos(yaw) , pixelY - 15 * Math.Sin(yaw)),
                        new Point(pixelX + 10 * Math.Cos(2 * Math.PI/3.0+ yaw), pixelY - 10 * Math.Sin(2 * Math.PI / 3.0 + yaw )),
                        new Point(pixelX + 10 * Math.Cos(- 2 * Math.PI / 3.0+yaw ), pixelY - 10 * Math.Sin(-2 * Math.PI / 3.0 + yaw))
                ];
            IsConnected = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
