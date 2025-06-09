using NodeLinkEditor.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;

namespace NodeLinkEditor.Views
{
    public partial class LinkAttributeEditor : UserControl
    {
        public LinkAttributeEditor()
        {
            InitializeComponent();
        }

        private void TextBoxCost_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is not MapEditorViewModel viewModel) { return; }

            var textBox = sender as TextBox;
            if (textBox == null || viewModel.SelectedLink == null) { return; }
            var text = textBox.Text;
            var name = textBox.Name;
            if (double.TryParse(text, out double value))
            {
                if (name == "TextBoxStartToEndCost")
                {
                    viewModel.SelectedLink.StartToEndCost = value;
                    foreach (var l in viewModel.SelectedLinks)
                    { l.StartToEndCost = value; }
                }
                else if (name == "TextBoxEndToStartCost")
                {
                    viewModel.SelectedLink.EndToStartCost = value;
                    foreach (var l in viewModel.SelectedLinks)
                    { l.EndToStartCost = value; }
                }
            }
        }

        private void SwapNodes_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is MapEditorViewModel viewModel && viewModel.SelectedLink != null)
            {
                viewModel.SelectedLink.SwapNodes();
                foreach (var link in viewModel.SelectedLinks)
                { link.SwapNodes(); }
            }
        }

        private void AttributeCheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is not MapEditorViewModel viewModel) { return; }
            if (viewModel.SelectedLink == null) { return; }
            foreach (var link in viewModel.SelectedLinks)
            {
                foreach (var att in viewModel.SelectedLink.AttributeOptions.Zip(link.AttributeOptions, (sa, la) => (sa, la)))
                { att.la.IsSelected = att.sa.IsSelected; }
            }
        }

        private void IsTwoWayCheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is not MapEditorViewModel viewModel) { return; }
            if (viewModel.SelectedLink == null) { return; }
            foreach (var link in viewModel.SelectedLinks)
            { link.IsTwoWay = viewModel.SelectedLink.IsTwoWay; }

        }
    }
}
