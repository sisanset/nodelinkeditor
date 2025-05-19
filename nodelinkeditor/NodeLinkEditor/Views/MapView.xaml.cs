using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using NodeLinkEditor.ViewModels;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Windows.Media;
using NodeLinkEditor.Converters;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Reflection;
using NodeLinkEditor.Others;
using NodeLinkEditor.Models;

namespace NodeLinkEditor.Views
{
    public partial class MapView : UserControl
    {
        private NodeViewModel? _draggedNode;
        private Point _dragStartPoint;
        private DispatcherTimer _dragTimer;
        private bool _isTimerElapsed = false;

        public MapView()
        {
            InitializeComponent();

            _dragTimer = new DispatcherTimer();
            _dragTimer.Interval = TimeSpan.FromMilliseconds(200);
            _dragTimer.Tick += DragTimer_Tick;

            if (DataContext is MapEditorViewModel viewModel)
            {
                var adjSize = viewModel.NodeSize / 2;
                AdjNodePositionConverter.Adj = adjSize;
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = DataContext as MapEditorViewModel;
            if (viewModel == null || sender is not Canvas)
            { return; }

            Point point = GetCoordFromPixel(sender, e);
            // Node作成
            switch (viewModel.SelectedMode)
            {
                case Models.EditMode.NodeLink:
                    viewModel.CreateNodeCommand.Execute(point);
                    break;
                case Models.EditMode.HelperLine:
                    if (viewModel.IsDrawingHelperLine)
                    {
                        viewModel.DrawingHelperLine.EndX = point.X;
                        viewModel.DrawingHelperLine.EndY = point.Y;
                        viewModel.CreateHelperLineCommand.Execute(viewModel.DrawingHelperLine);
                    }
                    else
                    {
                        viewModel.DrawingHelperLine = new HelperLineViewModel(new Models.HelperLine
                        {
                            StartX = point.X,
                            StartY = point.Y,
                            EndX = point.X,
                            EndY = point.Y,
                        });
                        viewModel.IsDrawingHelperLine = true;
                    }
                    break;
                default:
                    break;
            }
            return;
        }

        private void DragTimer_Tick(object? sender, EventArgs e)
        {
            _dragTimer.Stop();
            _isTimerElapsed = true;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (DataContext is MapEditorViewModel viewModel)
            {
                Point point = GetCoordFromPixel(sender, e);
                if (e.LeftButton == MouseButtonState.Pressed && _draggedNode != null && _isTimerElapsed)
                {
                    _draggedNode.X = point.X;
                    _draggedNode.Y = point.Y;
                    // if point.XがCanvasの範囲外->MouseLeftButtonUp
                }
                if (e.LeftButton != MouseButtonState.Pressed && viewModel.SelectedMode == EditMode.HelperLine && viewModel.IsDrawingHelperLine)
                {
                    viewModel.DrawingHelperLine.EndX = point.X;
                    viewModel.DrawingHelperLine.EndY = point.Y;
                }
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _dragTimer.Stop();
            if (DataContext is MapEditorViewModel viewModel && _draggedNode != null && _isTimerElapsed)
            {
                Point point = GetCoordFromPixel(sender, e);
                viewModel.MoveNodeCommand.Execute((_draggedNode, point.X, point.Y, _dragStartPoint.X, _dragStartPoint.Y));
            }
            _draggedNode = null;
            _isTimerElapsed = false;
        }
        private Point GetCoordFromPixel(object sender, MouseEventArgs e) => CoordConv.PixelToCoord(e.GetPosition((Canvas)sender));

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            Canvas_MouseLeftButtonUp(sender, new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, new MouseButton()));//Canvas外に出ても呼び出されないときがある
        }

        private void Link_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MapEditorViewModel viewModel && sender is Path path)
            {
                e.Handled = true;
                viewModel.SelectedLink = path.DataContext as LinkViewModel;
            }
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = DataContext as MapEditorViewModel;
            var node = (sender as Ellipse)?.DataContext as NodeViewModel;

            if (viewModel == null || node == null)
            { return; }

            e.Handled = true;

            // 複数Node選択
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (viewModel.SelectedNodes.Contains(node))
                {
                    viewModel.SelectedNodes.Remove(node);
                    node.IsReferenced = false;
                }
                else
                {
                    viewModel.SelectedNodes.Add(node);
                    node.IsReferenced = true;
                }
                return;
            }

            // Line作成
            viewModel.SelectedNode = node;
            var startNode = viewModel.StartNode;
            var endNode = viewModel.EndNode;
            if (startNode == null)
            { viewModel.StartNode = node; }
            else if (endNode == null)
            {
                // 同node、または既存のLinkがある場合は、Line作成しない
                if (startNode.ID == node.ID)
                { }
                else if (viewModel.Links.Any(l =>
                    (startNode.ID == l.StartNode.ID && node.ID == l.EndNode.ID) ||
                    (startNode.ID == l.EndNode.ID && node.ID == l.StartNode.ID)))
                { viewModel.StartNode = node; }
                else
                {
                    viewModel.EndNode = node;
                    viewModel.CreateLinkCommand.Execute(null);
                }
            }

            // ドラッグ開始
            _draggedNode = node;
            _dragStartPoint = new Point(node.X, node.Y);
            _isTimerElapsed = false;
            _dragTimer.Stop();
            _dragTimer.Start();
        }

        private void ScrollViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (DataContext is MapEditorViewModel viewModel)
                { viewModel.ClearSelection(); }
            }
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is MapEditorViewModel viewModel)
            {
                DrawGridLines();
            }
        }

        private void DrawGridLines(int gridSpacing = 1/*[m]*/, double thickness = 0.5)
        {
            GridCanvas.Children.Clear();
            double width = BackgroundImage.ActualWidth;
            double height = BackgroundImage.ActualHeight;

            var minX = (int)CoordConv.PixelXToCoordX(0);
            var maxX = (int)CoordConv.PixelXToCoordX(width);
            for (int x = minX; x <= maxX; x += gridSpacing)
            {
                var pixelX = CoordConv.CoordXToPixelX(x);
                var line = new Line
                {
                    X1 = pixelX,
                    Y1 = 0,
                    X2 = pixelX,
                    Y2 = height,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = thickness,
                };
                GridCanvas.Children.Add(line);
                if (x % 10 == 0)
                {
                    var label = new TextBlock
                    {
                        Text = x.ToString(),
                        FontSize = 10,
                        Foreground = Brushes.Gray,
                    };
                    Canvas.SetLeft(label, pixelX + 2);
                    Canvas.SetTop(label, height - 14);
                    GridCanvas.Children.Add(label);
                }
            }

            var maxY = (int)CoordConv.PixelYToCoordY(0);
            var minY = (int)CoordConv.PixelYToCoordY(height);
            for (double y = minY; y <= maxY; y += gridSpacing)
            {
                var pixelY = CoordConv.CoordYToPixelY(y);
                var line = new Line
                {
                    X1 = 0,
                    Y1 = pixelY,
                    X2 = width,
                    Y2 = pixelY,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = thickness,
                };
                GridCanvas.Children.Add(line);
                if (y % 10 == 0)
                {
                    var label = new TextBlock
                    {
                        Text = y.ToString(),
                        FontSize = 10,
                        Foreground = Brushes.Gray,
                    };
                    Canvas.SetLeft(label, 2);
                    Canvas.SetTop(label, pixelY + 2);
                    GridCanvas.Children.Add(label);
                }
            }

            var lineB = new Line { X1 = 0, Y1 = 0, X2 = 0, Y2 = height, Stroke = Brushes.LightGray, StrokeThickness = thickness, };
            GridCanvas.Children.Add(lineB);
            lineB = new Line { X1 = width, Y1 = 0, X2 = width, Y2 = height, Stroke = Brushes.LightGray, StrokeThickness = thickness, };
            GridCanvas.Children.Add(lineB);
            lineB = new Line { X1 = 0, Y1 = 0, X2 = width, Y2 = 0, Stroke = Brushes.LightGray, StrokeThickness = thickness, };
            GridCanvas.Children.Add(lineB);
            lineB = new Line { X1 = 0, Y1 = height, X2 = width, Y2 = height, Stroke = Brushes.LightGray, StrokeThickness = thickness, };
            GridCanvas.Children.Add(lineB);
        }

        private void Line_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MapEditorViewModel viewModel && sender is Line line)
            {
                if (viewModel.SelectedMode != EditMode.HelperLine)
                { return; }
                e.Handled = true;
                viewModel.SelectedHelperLine = line.DataContext as HelperLineViewModel;
            }
        }
    }
}

