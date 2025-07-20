using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection.PortableExecutable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Linq;
using NodeLinkEditor.Models;
using NodeLinkEditor.ViewModels;

namespace NodeLinkEditor.Views
{
    /// <summary>
    /// NodeListView.xaml の相互作用ロジック
    /// </summary>
    public partial class NodeListView : Window
    {
        public class CollectionToStringConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value is string str ? $"{str}," : value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
        public NodeListView(MapEditorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            var gridView = new GridView();
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Name",
                DisplayMemberBinding = new System.Windows.Data.Binding("Name"),
                Width = 30
            });
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "X",
                DisplayMemberBinding = new System.Windows.Data.Binding("X") { StringFormat = "{0:F2}" },
                Width = 80
            });
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Y",
                DisplayMemberBinding = new System.Windows.Data.Binding("Y") { StringFormat = "{0:F2}" },
                Width = 80
            });
            Enum.GetValues<NodeAttribute>().ToList().ForEach(attr =>
            {
                var factory = new FrameworkElementFactory(typeof(CheckBox));
                factory.SetBinding(CheckBox.IsCheckedProperty, new System.Windows.Data.Binding($"AttributeOptions[{(int)attr}].IsSelected"));

                var gridColumn = new GridViewColumn
                {
                    Header = attr.ToString(),
                    Width = 80,
                    CellTemplate = new DataTemplate
                    {
                        VisualTree = factory
                    }
                };
                gridView.Columns.Add(gridColumn);
            });


            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding(".") { Converter = new CollectionToStringConverter() });
            textBlockFactory.SetValue(TextBlock.MarginProperty, new Thickness(2, 0, 2, 0));

            var itemsControlFactory = new FrameworkElementFactory(typeof(ItemsControl));
            itemsControlFactory.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("AssociatedNodes"));

            var itemsPanelTemplate = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(StackPanel)));
            itemsPanelTemplate.VisualTree.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            itemsControlFactory.SetValue(ItemsControl.ItemsPanelProperty, itemsPanelTemplate);

            var itemTemplate = new DataTemplate { VisualTree = textBlockFactory };
            itemsControlFactory.SetValue(ItemsControl.ItemTemplateProperty, itemTemplate);
            var dataTemplate = new DataTemplate { VisualTree = itemsControlFactory };
            var colAssociatedNodes = new GridViewColumn
            {
                Header = "Associated Nodes",
                CellTemplate = dataTemplate
            };
            gridView.Columns.Add(colAssociatedNodes);

            node_list.View = gridView;

            viewModel.PropertyChanged += ChangeSelectedItem;
        }
        private void ChangeSelectedItem(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(MapEditorViewModel.SelectedNode)) { return; }
            if (DataContext is not MapEditorViewModel viewModel) { return; }
            if (viewModel.SelectedNode == null) { return; }
            if (node_list.SelectedItem is NodeViewModel selectedNode && selectedNode.Name == viewModel.SelectedNode.Name) { return; }
            foreach (var (value, index) in node_list.Items.Cast<NodeViewModel>().ToList().Select((value, index) => (value, index)))
            {
                if (value.Name == viewModel.SelectedNode.Name)
                {
                    node_list.SelectedIndex = index;
                }
            }
        }

        private void node_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("SelectionChanged!");

            if (DataContext is not MapEditorViewModel viewModel) { return; }
            if (node_list.SelectedItem is not NodeViewModel selectedNode) { return; }

            viewModel.ClearSelectedNodes();
            viewModel.AddSelectedNode(selectedNode);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (DataContext is not MapEditorViewModel viewModel) { return; }

            viewModel.PropertyChanged -= ChangeSelectedItem;
        }
    }
}
