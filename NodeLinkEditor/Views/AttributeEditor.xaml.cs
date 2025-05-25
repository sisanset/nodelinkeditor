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
            if (DataContext is MapEditorViewModel viewModel)
            {
                var textBox = sender as TextBox;
                if (textBox == null || viewModel.SelectedNode == null)
                { return; }
                string text = textBox.Text;
                string name = textBox.Name;
                if (double.TryParse(text, out double value))
                {
                    if (textBox.Name == "TextBoxNodePosX")
                    {
                        viewModel.MoveNodeCommand.Execute((viewModel.SelectedNode, value, viewModel.SelectedNode.Y));
                    }
                    else if (textBox.Name == "TextBoxNodePosY")
                    {
                        viewModel.MoveNodeCommand.Execute((viewModel.SelectedNode, viewModel.SelectedNode.X, value));
                    }
                }
            }
        }
    }
}
