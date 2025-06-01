using NodeLinkEditor.Others;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NodeLinkEditor.Converters
{
    internal class TaperedLineConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length != 4) { return null; }
            if (values[0] is Point startPoint && values[1] is Point endPoint && values[2] is double startThickness && values[3] is double endThicness)
            {
                var startPointPixel = CoordConv.CoordToPixel(startPoint);
                var endPointPixel = CoordConv.CoordToPixel(endPoint);
                Vector direction = endPointPixel - startPointPixel;
                direction.Normalize();
                Vector normal = new Vector(-direction.Y, direction.X);

                Point startPoint1 = startPointPixel + normal * (startThickness / 2);
                Point startPoint2 = startPointPixel - normal * (startThickness / 2);
                Point endPoint1 = endPointPixel + normal * (endThicness / 2);
                Point endPoint2 = endPointPixel - normal * (endThicness / 2);
                var geometry = new StreamGeometry();
                using (var context = geometry.Open())
                {
                    context.BeginFigure(startPoint1, true, false);
                    context.LineTo(endPoint1, true, false);
                    context.LineTo(endPoint2, true, false);
                    context.LineTo(startPoint2, true, false);
                    context.Close();
                }
                geometry.Freeze();
                return geometry;
            }
            return null;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
