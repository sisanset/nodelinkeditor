using NodeLinkEditor.Models;
using NodeLinkEditor.ViewModels;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

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
                            newLine = CreateMovedLine(viewModel.SelectedHelperLine, LineMoveDirection.StartX, value);
                            break;
                        case "TextBoxStartY":
                            newLine = CreateMovedLine(viewModel.SelectedHelperLine, LineMoveDirection.StartY, value);
                            break;
                        case "TextBoxEndX":
                            newLine = CreateMovedLine(viewModel.SelectedHelperLine, LineMoveDirection.EndX, value);
                            break;
                        case "TextBoxEndY":
                            newLine = CreateMovedLine(viewModel.SelectedHelperLine, LineMoveDirection.EndY, value);
                            break;
                        case "TextBoxTransX":
                            newLine = CreateMovedLine(viewModel.SelectedHelperLine, LineMoveDirection.TransX, value);
                            break;
                        case "TextBoxTransY":
                            newLine = CreateMovedLine(viewModel.SelectedHelperLine, LineMoveDirection.TransY, value);
                            break;
                        default:
                            break;
                    }
                    if (!viewModel.SelectedHelperLine.GetHelperLineCopy().HasEqualCoordinates(newLine))
                    { viewModel.MoveHelperLineCommand.Execute((viewModel.SelectedHelperLine, newLine)); }
                }
            }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is MapEditorViewModel viewModel)
            {
                var button = sender as Button;
                if (button == null || viewModel.SelectedHelperLine == null)
                { return; }
                var name = button.Name;
                var text = string.Empty;
                if (name == "ButtonTransXP" || name == "ButtonTransXM")
                { text = TextBoxTransX.Text; }
                if (name == "ButtonTransYP" || name == "ButtonTransYM")
                { text = TextBoxTransY.Text; }
                if (name == "ButtonTransHP" || name == "ButtonTransHM")
                { text = TextBoxTransH.Text; }
                if (name == "ButtonTransVP" || name == "ButtonTransVM")
                { text = TextBoxTransV.Text; }
                if (double.TryParse(text, out double value))
                {
                    var newLine = viewModel.SelectedHelperLine.GetHelperLineCopy();
                    switch (name)
                    {
                        case "ButtonTransXP":
                            newLine = CreateMovedLine(viewModel.SelectedHelperLine, LineMoveDirection.TransX, value);
                            break;
                        case "ButtonTransXM":
                            newLine = CreateMovedLine(viewModel.SelectedHelperLine, LineMoveDirection.TransX, -value);
                            break;
                        case "ButtonTransYP":
                            newLine = CreateMovedLine(viewModel.SelectedHelperLine, LineMoveDirection.TransY, value);
                            break;
                        case "ButtonTransYM":
                            newLine = CreateMovedLine(viewModel.SelectedHelperLine, LineMoveDirection.TransY, -value);
                            break;
                        case "ButtonTransHP":
                            newLine = CreateMovedLine(viewModel.SelectedHelperLine, LineMoveDirection.TransH, value);
                            break;
                        case "ButtonTransHM":
                            newLine = CreateMovedLine(viewModel.SelectedHelperLine, LineMoveDirection.TransH, -value);
                            break;
                        case "ButtonTransVP":
                            newLine = CreateMovedLine(viewModel.SelectedHelperLine, LineMoveDirection.TransV, value);
                            break;
                        case "ButtonTransVM":
                            newLine = CreateMovedLine(viewModel.SelectedHelperLine, LineMoveDirection.TransV, -value);
                            break;
                    }
                    if (!viewModel.SelectedHelperLine.GetHelperLineCopy().HasEqualCoordinates(newLine))
                    { viewModel.MoveHelperLineCommand.Execute((viewModel.SelectedHelperLine, newLine)); }
                }
            }
        }

        private enum LineMoveDirection
        {
            StartX,
            StartY,
            EndX,
            EndY,
            TransX,
            TransY,
            TransH,
            TransV
        }

        private static HelperLine CreateMovedLine(HelperLineViewModel line, LineMoveDirection lineDirection, double value)
        {
            var newLine = line.GetHelperLineCopy();
            switch (lineDirection)
            {
                case LineMoveDirection.StartX:
                    newLine.StartX = value;
                    break;
                case LineMoveDirection.StartY:
                    newLine.StartY = value;
                    break;
                case LineMoveDirection.EndX:
                    newLine.EndX = value;
                    break;
                case LineMoveDirection.EndY:
                    newLine.EndY = value;
                    break;
                case LineMoveDirection.TransX:
                    newLine.StartX += value;
                    newLine.EndX += value;
                    break;
                case LineMoveDirection.TransY:
                    newLine.StartY += value;
                    newLine.EndY += value;
                    break;
                case LineMoveDirection.TransH:
                    var vecH = new Vector2((float)(newLine.EndX - newLine.StartX), (float)(newLine.EndY - newLine.StartY));
                    vecH = Vector2.Normalize(vecH) * (float)value;
                    if (Math.Abs(vecH.Y) < Math.Abs(vecH.X))
                    { if (value * vecH.X < 0) { vecH = -vecH; } }
                    else
                    { if (value * vecH.Y < 0) { vecH = -vecH; } }
                    newLine.StartX += vecH.X;
                    newLine.EndX += vecH.X;
                    newLine.StartY += vecH.Y;
                    newLine.EndY += vecH.Y;
                    break;
                case LineMoveDirection.TransV:
                    var vecV = new Vector2((float)(newLine.EndY - newLine.StartY), -(float)(newLine.EndX - newLine.StartX));
                    vecV = Vector2.Normalize(vecV) * (float)value;
                    if (Math.Abs(vecV.Y) < Math.Abs(vecV.X))
                    { if (value * vecV.X < 0) { vecV = -vecV; } }
                    else
                    { if (value * vecV.Y < 0) { vecV = -vecV; } }
                    newLine.StartX += vecV.X;
                    newLine.EndX += vecV.X;
                    newLine.StartY += vecV.Y;
                    newLine.EndY += vecV.Y;
                    break;
                default:
                    break;
            }
            return newLine;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[0-9.]+$"))
            { e.Handled = true; }
        }
    }
}
