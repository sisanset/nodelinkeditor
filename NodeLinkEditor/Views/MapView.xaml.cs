using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using NodeLinkEditor.ViewModels;
using System.Windows.Shapes;
using System.Windows.Media;
using NodeLinkEditor.Converters;
using System.Windows.Threading;
using NodeLinkEditor.Others;
using NodeLinkEditor.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NodeLinkEditor.Views
{
    public partial class MapView : UserControl, INotifyPropertyChanged
    {
        private Point _rectangleStartPoint;
        public Point RectangleStartPoint
        {
            get => _rectangleStartPoint;
            set { _rectangleStartPoint = value; OnPropertyChanged(); }
        }
        private Point _rectangleEndPoint;
        public Point RectangleEndPoint
        {
            get => _rectangleEndPoint;
            set { _rectangleEndPoint = value; OnPropertyChanged(); }
        }
        private bool _isRectangleDragging = false;
        public bool IsRectangleDragging
        {
            get => _isRectangleDragging;
            set { _isRectangleDragging = value; OnPropertyChanged(); }
        }

        private double _scale = 1.0;
        public double Scale
        {
            get => _scale;
            set
            {
                _scale = Math.Max(_minScale, Math.Min(_maxScale, value));
                OnPropertyChanged();
            }
        }
        private double _zoomFactor = 1.1;
        private double _minScale = 0.25;
        private double _maxScale = 5.0;

        private NodeViewModel? _draggedNode;
        private Point _dragStartPoint;
        private DispatcherTimer _dragTimer;
        private bool _isTimerElapsed = false;
        private DispatcherTimer _scrollTimer;
        private Point _lastMousePosition;
        private bool _isMouseInside = false;

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

            // スクロールタイマー
            _scrollTimer = new DispatcherTimer
            { Interval = TimeSpan.FromMilliseconds(100) };
            _scrollTimer.Tick += ScrollTimer_Tick;
            _scrollTimer.Start();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer) return;
            _isMouseInside = true;
            _lastMousePosition = e.GetPosition(scrollViewer);
        }
        private void ScrollViewer_MouseLeave(object sender, MouseEventArgs e) => _isMouseInside = false;

        private void ScrollTimer_Tick(object? sender, EventArgs e)
        {
            if (DataContext is not MapEditorViewModel viewModel) { return; }
            if (!_isMouseInside) return;
            if (!viewModel.IsDrawingHelperLine && _draggedNode == null && !IsRectangleDragging) { return; }

            var scrollViewer = MainScrollViewer;
            if (scrollViewer == null) return;
            // ScrollViewerのサイズを取得
            double viewportWidth = scrollViewer.ViewportWidth;
            double viewportHeight = scrollViewer.ViewportHeight;

            // スクロール量
            double scrollAmount = 15;

            // 縁にマウスがあるかどうかを判断し、スクロールを実行
            if (_lastMousePosition.X < 20) // 左端
            {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - scrollAmount);
            }
            else if (_lastMousePosition.X > viewportWidth - 20) // 右端
            {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + scrollAmount);
            }

            if (_lastMousePosition.Y < 20) // 上端
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - scrollAmount);
            }
            else if (_lastMousePosition.Y > viewportHeight - 20) // 下端
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + scrollAmount);
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
                case Models.EditMode.Node:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        IsRectangleDragging = true;
                        UpdateRectangle(point, point);
                    }
                    else
                    { viewModel.CreateNodeCommand.Execute(point); }
                    break;
                case Models.EditMode.Link:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        IsRectangleDragging = true;
                        UpdateRectangle(point, point);
                    }
                    break;
                case Models.EditMode.HelperLine:
                    if (viewModel.IsDrawingHelperLine)
                    {
                        viewModel.IsDrawingHelperLine = false;
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


        private Point? GetClosestPointFromLines(Point point, MapEditorViewModel viewModel)
        {
            double threshold = 30.0;
            Point? closestPoint = null;
            double minDistance = double.MaxValue;
            foreach (var line in viewModel.HelperLines)
            {
                var cp = GetClosestPointFromLine(point, line);
                var pixelDiffX = CoordConv.CoordXToPixelX(cp.X) - CoordConv.CoordXToPixelX(point.X);
                var pixelDiffY = CoordConv.CoordYToPixelY(cp.Y) - CoordConv.CoordYToPixelY(point.Y);
                var powDist = Math.Pow(pixelDiffX, 2) + Math.Pow(pixelDiffY, 2);
                if (powDist < minDistance)
                {
                    minDistance = powDist;
                    closestPoint = cp;
                }
            }
            if (closestPoint == null || threshold * threshold < minDistance)
            { return null; }
            return closestPoint;
        }
        private Point GetClosestPointFromLine(Point p, HelperLineViewModel line)
        {
            var spX = p.X - line.StartX;
            var spY = p.Y - line.StartY;
            var seX = line.EndX - line.StartX;
            var seY = line.EndY - line.StartY;
            var esX = line.StartX - line.EndX;
            var esY = line.StartY - line.EndY;
            var epX = p.X - line.EndX;
            var epY = p.Y - line.EndY;
            if (spX * seX + spY * seY < 0)
            { return new Point(line.StartX, line.StartY); }
            if (epX * esX + epY * esY < 0)
            { return new Point(line.EndX, line.EndY); }
            var seNorm = Math.Sqrt(seX * seX + seY * seY);
            var cpNorm = (spX * seX + spY * seY) / seNorm;
            var cpX = line.StartX + seX * cpNorm / seNorm;
            var cpY = line.StartY + seY * cpNorm / seNorm;
            return new Point(cpX, cpY);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (DataContext is not MapEditorViewModel viewModel) { return; }

            Point point = GetCoordFromPixel(sender, e);
            MousePositionText.Text = $"Mouse Position: X = {point.X:N3}, Y = {point.Y:N3}";

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_draggedNode != null && _isTimerElapsed)
                {
                    _draggedNode.X = point.X;
                    _draggedNode.Y = point.Y;
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        var closestPoint = GetClosestPointFromLines(point, viewModel);
                        if (closestPoint != null)
                        {
                            _draggedNode.X = closestPoint.Value.X;
                            _draggedNode.Y = closestPoint.Value.Y;
                        }
                    }
                    if (viewModel.SelectedNodes.Count == 1 && viewModel.SelectedNode?.ID == _draggedNode.ID)
                    {
                        viewModel.SelectedNode.X = _draggedNode.X;
                        viewModel.SelectedNode.Y = _draggedNode.Y;
                    }
                }
                if (IsRectangleDragging)
                { UpdateRectangle(_rectangleStartPoint, point); }
            }
            else
            {
                if (viewModel.SelectedMode == EditMode.HelperLine && viewModel.IsDrawingHelperLine)
                {
                    viewModel.DrawingHelperLine.EndX = point.X;
                    viewModel.DrawingHelperLine.EndY = point.Y;
                }
                if (IsRectangleDragging)
                {
                    IsRectangleDragging = false;
                    SelectInsideRectangle();
                }
            }
        }
        private void SelectInsideRectangle()
        {
            if (DataContext is not MapEditorViewModel viewModel) { return; }
            var rect = new Rect(
                Math.Min(_rectangleStartPoint.X, _rectangleEndPoint.X),
                Math.Min(_rectangleStartPoint.Y, _rectangleEndPoint.Y),
                Math.Abs(_rectangleStartPoint.X - _rectangleEndPoint.X),
                Math.Abs(_rectangleStartPoint.Y - _rectangleEndPoint.Y));

            var nodes = viewModel.Nodes.Where(n => rect.Contains(n.Point)).ToList();
            if (EditMode.Node == viewModel.SelectedMode)
            { nodes.ForEach(n => viewModel.AddSelectedNode(n)); }
            else if (EditMode.Link == viewModel.SelectedMode)
            {
                foreach (var l in viewModel.Links.Where(l => nodes.Contains(l.StartNode) && nodes.Contains(l.EndNode)))
                { viewModel.AddSelectedLink(l); }
            }
        }
        private void UpdateRectangle(Point startPoint, Point endPoint)
        {
            RectangleStartPoint = startPoint;
            RectangleEndPoint = endPoint;
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _dragTimer.Stop();
            if (DataContext is not MapEditorViewModel viewModel) { return; }
            Point point = GetCoordFromPixel(sender, e);
            if (_draggedNode != null && _isTimerElapsed)
            {
                _draggedNode.X = point.X;
                _draggedNode.Y = point.Y;
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    var closestPoint = GetClosestPointFromLines(point, viewModel);
                    if (closestPoint != null)
                    {
                        _draggedNode.X = closestPoint.Value.X;
                        _draggedNode.Y = closestPoint.Value.Y;
                    }
                }
                viewModel.MoveNodeCommand.Execute((_draggedNode, _draggedNode.X, _draggedNode.Y, _dragStartPoint.X, _dragStartPoint.Y));
            }
            _draggedNode = null;
            _isTimerElapsed = false;
            if (IsRectangleDragging)
            {
                IsRectangleDragging = false;
                SelectInsideRectangle();
            }
        }

        private Point GetCoordFromPixel(object sender, MouseEventArgs e) => CoordConv.PixelToCoord(e.GetPosition((Canvas)sender));

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            Canvas_MouseLeftButtonUp(sender, new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, new MouseButton()));//Canvas外に出ても呼び出されないときがある
        }

        private void Link_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MapEditorViewModel viewModel && sender is Path path && path.DataContext is LinkViewModel link)
            {
                e.Handled = true;
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    // ctl押しながらの場合は追加・削除
                    // 選択されたものを削除
                    if (link.IsSelected)
                    { viewModel.RemoveSelectedLink(link); }
                    else//追加
                    { viewModel.AddSelectedLink(link); }
                    return;
                }
                // ctl無しの場合は一つのみ
                if (link.IsSelected)
                { return; }
                viewModel.ClearSelectedLinks();
                viewModel.AddSelectedLink(link);
            }
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not MapEditorViewModel viewModel) { return; }
            if (sender is not Ellipse || ((Ellipse)sender).DataContext is not NodeViewModel node) { return; }

            e.Handled = true;

            // 複数Node選択
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (node.IsSelected)
                { viewModel.RemoveSelectedNode(node); }
                else
                { viewModel.AddSelectedNode(node); }
            }
            else
            {
                if (!node.IsSelected)
                {
                    viewModel.ClearSelectedNodes();
                    viewModel.AddSelectedNode(node);
                }
            }

            // Line作成
            if (viewModel.SelectedMode == EditMode.Link)
            {
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
            }

            // ドラッグ開始
            _draggedNode = node;
            _dragStartPoint = new Point(node.X, node.Y);
            _isTimerElapsed = false;
            _dragTimer.Stop();
            _dragTimer.Start();
        }
        private void Ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            if (DataContext is not MapEditorViewModel viewModel) { return; }
            if (sender is not Ellipse || ((Ellipse)sender).DataContext is not NodeViewModel node) { return; }
            if (node.AssociatedNodes.Count == 0) { return; }
            node.IsReferenced = true;
            foreach (var a in node.AssociatedNodes)
            {
                var associatedNode = viewModel.Nodes.FirstOrDefault(n => n.Name == a);
                if (associatedNode != null)
                { associatedNode.IsReferenced = true; }
            }
        }

        private void Ellipse_MouseLeave(object sender, MouseEventArgs e)
        {
            if (DataContext is not MapEditorViewModel viewModel) { return; }
            foreach (var n in viewModel.Nodes)
            { n.IsReferenced = false; }
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
            int interval = 5;
            double ordThick = thickness;
            double originTchick = thickness + 2.0;
            double intervalThick = thickness + 0.5;
            var originBrush = Brushes.DimGray;
            var intervalBrush = Brushes.Gray;
            var ordBrush = Brushes.LightGray;

            var minX = (int)(CoordConv.PixelXToCoordX(0) / gridSpacing);
            var maxX = (int)(CoordConv.PixelXToCoordX(width) / gridSpacing);
            for (int x = minX; x <= maxX; x += gridSpacing)
            {
                var brush = x == 0 ? originBrush : x % interval == 0 ? intervalBrush : ordBrush;
                var thicknessX = x == 0 ? originTchick : x % interval == 0 ? intervalThick : ordThick;
                var pixelX = CoordConv.CoordXToPixelX(x);
                var line = new Line
                {
                    X1 = pixelX,
                    Y1 = 0,
                    X2 = pixelX,
                    Y2 = height,
                    Stroke = brush,
                    StrokeThickness = thicknessX,
                };
                GridCanvas.Children.Add(line);
                if (x % interval == 0)
                {
                    var label = new TextBlock
                    {
                        Text = x.ToString(),
                        FontSize = 10,
                        Foreground = brush,
                    };
                    Canvas.SetLeft(label, pixelX + 10);
                    Canvas.SetTop(label, CoordConv.CoordYToPixelY(0) + 3);
                    GridCanvas.Children.Add(label);
                }
            }

            var maxY = (int)(CoordConv.PixelYToCoordY(0) / gridSpacing);
            var minY = (int)(CoordConv.PixelYToCoordY(height) / gridSpacing);
            for (double y = minY; y <= maxY; y += gridSpacing)
            {
                var brush = y == 0 ? originBrush : y % interval == 0 ? intervalBrush : ordBrush;
                var thicknessY = y == 0 ? originTchick : y % interval == 0 ? intervalThick : ordThick;
                var pixelY = CoordConv.CoordYToPixelY(y);
                var line = new Line
                {
                    X1 = 0,
                    Y1 = pixelY,
                    X2 = width,
                    Y2 = pixelY,
                    Stroke = brush,
                    StrokeThickness = thicknessY,
                };
                GridCanvas.Children.Add(line);
                if (y % interval == 0)
                {
                    var label = new TextBlock
                    {
                        Text = y.ToString(),
                        FontSize = 10,
                        Foreground = brush,
                        TextAlignment = TextAlignment.Right,
                        Width = 20,
                    };
                    Canvas.SetLeft(label, CoordConv.CoordXToPixelX(0) - 30);
                    Canvas.SetTop(label, pixelY - 17);
                    GridCanvas.Children.Add(label);
                }
            }

            var lineB = new Line { X1 = 0, Y1 = 0, X2 = 0, Y2 = height, Stroke = Brushes.Gray, StrokeThickness = thickness, };
            GridCanvas.Children.Add(lineB);
            lineB = new Line { X1 = width, Y1 = 0, X2 = width, Y2 = height, Stroke = Brushes.Gray, StrokeThickness = thickness, };
            GridCanvas.Children.Add(lineB);
            lineB = new Line { X1 = 0, Y1 = 0, X2 = width, Y2 = 0, Stroke = Brushes.Gray, StrokeThickness = thickness, };
            GridCanvas.Children.Add(lineB);
            lineB = new Line { X1 = 0, Y1 = height, X2 = width, Y2 = height, Stroke = Brushes.Gray, StrokeThickness = thickness, };
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

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl)) { return; }
            if (DataContext is MapEditorViewModel viewModel)
            {
                var point = e.GetPosition((IInputElement)sender);
                var offsetX = MainScrollViewer.HorizontalOffset;
                var offsetY = MainScrollViewer.VerticalOffset;
                var scaleRatio = 0 < e.Delta ? _zoomFactor : 1.0 / _zoomFactor;
                var oldScale = Scale;
                Scale *= scaleRatio;
                if (oldScale == Scale) { e.Handled = true; return; }

                MainScrollViewer.ScrollToHorizontalOffset((point.X * scaleRatio) - (point.X - offsetX));
                MainScrollViewer.ScrollToVerticalOffset((point.Y * scaleRatio) - (point.Y - offsetY));
            }
        }
    }
}
