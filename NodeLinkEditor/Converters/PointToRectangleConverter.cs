using NodeLinkEditor.Others;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NodeLinkEditor.Converters
{
    internal class PointToRectangleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length != 2) { return null; }
            if (values[0] is not Point startPoint || values[1] is not Point endPoint) { return null; }
            if (parameter is not string axis) { return null; }

            var left = Math.Min(CoordConv.CoordXToPixelX(startPoint.X), CoordConv.CoordXToPixelX(endPoint.X));
            var top = Math.Min(CoordConv.CoordYToPixelY(startPoint.Y), CoordConv.CoordYToPixelY(endPoint.Y));
            var width = Math.Abs(CoordConv.CoordXToPixelX(startPoint.X) - CoordConv.CoordXToPixelX(endPoint.X));
            var height = Math.Abs(CoordConv.CoordYToPixelY(startPoint.Y) - CoordConv.CoordYToPixelY(endPoint.Y));
            return axis switch
            {
                "Width" => width,
                "Height" => height,
                "Left" => left,
                "Top" => top,
                _ => null
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
