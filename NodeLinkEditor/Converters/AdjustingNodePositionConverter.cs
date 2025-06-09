using NodeLinkEditor.Others;
using System.Windows.Data;

namespace NodeLinkEditor.Converters
{
    public class AdjNodePositionConverter : IValueConverter
    {
        static public int Adj { get; set; } = 0;
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                if (parameter is string axis)
                {
                    if (axis == "X")
                    { return CoordConv.CoordXToPixelX(doubleValue) - Adj; }
                    else if (axis == "Y")
                    { return CoordConv.CoordYToPixelY(doubleValue) - Adj; }
                    if (axis == "NX")
                    { return CoordConv.CoordXToPixelX(doubleValue) - 55 - Adj; }
                    else if (axis == "NY")
                    { return CoordConv.CoordYToPixelY(doubleValue) - 1.5 * Adj; }
                }
                return doubleValue - Adj;
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                if (parameter is string axis)
                {
                    if (axis == "X")
                    { return CoordConv.CoordXToPixelX(doubleValue); }
                    else if (axis == "Y")
                    { return CoordConv.CoordYToPixelY(doubleValue); }
                }
                return doubleValue;
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}