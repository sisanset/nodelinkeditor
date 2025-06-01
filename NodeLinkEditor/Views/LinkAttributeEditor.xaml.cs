using NodeLinkEditor.ViewModels;
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
            if (DataContext is MapEditorViewModel viewModel)
            {
                var textBox = sender as TextBox;
                if (textBox == null || viewModel.SelectedLink == null) { return; }
                var text = textBox.Text;
                var name = textBox.Name;
                if (double.TryParse(text, out double value))
                {
                    if (name == "TextBoxStartToEndCost")
                    {
                        viewModel.SelectedLink.StartToEndCost = value;
                    }
                    else if (name == "TextBoxEndToStartCost")
                    {
                        viewModel.SelectedLink.EndToStartCost = value;
                    }
                }
            }

        }

        private void SwapNodes_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is MapEditorViewModel viewModel && viewModel.SelectedLink != null)
            {
                (viewModel.SelectedLink.StartNode, viewModel.SelectedLink.EndNode) =
                (viewModel.SelectedLink.EndNode, viewModel.SelectedLink.StartNode);
            }
        }
    }
}
