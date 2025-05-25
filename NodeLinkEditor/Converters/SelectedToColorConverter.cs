using NodeLinkEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace NodeLinkEditor.Converters
{
    class SelectedToColorConverter : IMultiValueConverter
    {
        // node, SelectedNode, SelectedNodes
        // クリックで選択されている(isSelected)なら黄色
        // Ctrlで選択されている(isReferenced)なら緑
        // それ以外は赤
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length != 2)
            { return Brushes.Red; }
            if (values[0] is bool isSelected)
            {
                if (isSelected)
                { return Brushes.Yellow; }
            }
            if (values[1] is bool isReferenced)
            {
                if (isReferenced)
                { return Brushes.Green; }
            }
            return Brushes.Red;
        }
        public object[] ConvertBack(object values, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
