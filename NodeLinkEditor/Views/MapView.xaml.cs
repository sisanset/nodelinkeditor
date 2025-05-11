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

            Point point = e.GetPosition((Canvas)sender);
            // Node作成
            viewModel.CreateNodeCommand.Execute(point);
            viewModel.StartNode = null;
            viewModel.EndNode = null;
            return;
        }

        private void DragTimer_Tick(object? sender, EventArgs e)
        {
            _dragTimer.Stop();
            _isTimerElapsed = true;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (DataContext is MapEditorViewModel viewModel && _draggedNode != null && e.LeftButton == MouseButtonState.Pressed && _isTimerElapsed)
            {
                Point point = e.GetPosition((Canvas)sender);
                _draggedNode.X = point.X;
                _draggedNode.Y = point.Y;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _dragTimer.Stop();
            if (DataContext is MapEditorViewModel viewModel && _draggedNode != null && _isTimerElapsed)
            {
                Point point = e.GetPosition((Canvas)sender);
                viewModel.MoveNodeCommand.Execute((_draggedNode, point.X, point.Y, _dragStartPoint.X, _dragStartPoint.Y));
            }
            _draggedNode = null;
            _isTimerElapsed = false;
        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            Canvas_MouseLeftButtonUp(sender, new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, new MouseButton()));
        }

        private void Line_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
                { viewModel.StartNode = null; }
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
    }
}

