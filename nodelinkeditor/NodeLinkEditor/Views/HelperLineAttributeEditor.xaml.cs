using NodeLinkEditor.Models;
using NodeLinkEditor.ViewModels;
using System.Windows.Controls;

namespace NodeLinkEditor.Views
{
    public partial class HelperLineAttributeEditor : UserControl
    {
        public HelperLineAttributeEditor()
        {
            InitializeComponent();
        }

        private void TextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is MapEditorViewModel viewModel)
            {
                var textBox = sender as TextBox;
                if (textBox == null || viewModel.SelectedHelperLine == null) { return; }
                var text = textBox.Text;
                var name = textBox.Name;
                if (double.TryParse(text, out double value))
                {
                    var newLine = viewModel.SelectedHelperLine.GetHelperLineCopy();
                    switch (name)
                    {
                        case "TextBoxStartX":
                            newLine.StartX = value;
                            break;
                        case "TextBoxStartY":
                            newLine.StartY = value;
                            break;
                        case "TextBoxEndX":
                            newLine.EndX = value;
                            break;
                        case "TextBoxEndY":
                            newLine.EndY = value;
                            break;
                        case "TextBoxTransX":
                            newLine.StartX += value;
                            newLine.EndX += value;
                            textBox.Text = 0.ToString();
                            break;
                        case "TextBoxTransY":
                            newLine.StartY += value;
                            newLine.EndY += value;
                            textBox.Text = 0.ToString();
                            break;
                        default:
                            break;
                    }
                    if (!viewModel.SelectedHelperLine.GetHelperLineCopy().EqualCoord(newLine))
                    { viewModel.MoveHelperLineCommand.Execute((viewModel.SelectedHelperLine, newLine)); }
                }
            }
        }
    }
}
