using NodeLinkEditor.ViewModels;
using System.Windows.Controls;

namespace NodeLinkEditor.Views
{
    public partial class AttributeEditor : UserControl
    {
        public AttributeEditor()
        {
            InitializeComponent();
        }

        private void TextBoxNodePos_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is not MapEditorViewModel viewModel) { return; }
            if (viewModel.SelectedNodes.Count != 1) { return; }
            var textBox = sender as TextBox;
            if (textBox == null || viewModel.SelectedNode == null) { return; }
            string text = textBox.Text;
            string name = textBox.Name;
            if (double.TryParse(text, out double value))
            {
                var node = viewModel.SelectedNodes[0];
                if (textBox.Name == "TextBoxNodePosX")
                { viewModel.MoveNodeCommand.Execute((node, value, node.Y)); }
                else if (textBox.Name == "TextBoxNodePosY")
                { viewModel.MoveNodeCommand.Execute((node, node.X, value)); }
                viewModel.SelectedNode = new NodeViewModel(viewModel.SelectedNodes[0].GetNodeCopy());
            }
        }

        private void AttributeCheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is not MapEditorViewModel viewModel) { return; }
            if (viewModel.SelectedNode == null) { return; }
            foreach (var node in viewModel.SelectedNodes)
            {
                foreach (var att in viewModel.SelectedNode.AttributeOptions.Zip(node.AttributeOptions, (sa, la) => (sa, la)))
                {
                    if (att.sa.IsSelected == null) { continue; }
                    att.la.IsSelected = att.sa.IsSelected;
                }
            }
        }

        private void NoButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is not MapEditorViewModel viewModel) { return; }
            if (viewModel.SelectedNodes.Count != 1) { return; }
            if (viewModel.SelectedNode == null) { return; }
            viewModel.SelectedNodes[0].ChangeNodeName(TextBoxNodeNo.Text);
            viewModel.SelectedNode = new NodeViewModel(viewModel.SelectedNodes[0].GetNodeCopy());
        }
    }
}
