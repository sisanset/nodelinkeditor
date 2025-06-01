using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NodeLinkEditor.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brushes = new List<SolidColorBrush> { Brushes.Yellow,Brushes.Black };//true,false
            if (value is bool b)
            {
                if (parameter is string param)
                {
                    foreach ((string v, int i) in param.Split(',').Select((v, i) => (v, i)))
                    {
                        brushes[i] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(v));
                    }
                }
                return b ? brushes[0] : brushes[1];
            }
            return brushes[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
