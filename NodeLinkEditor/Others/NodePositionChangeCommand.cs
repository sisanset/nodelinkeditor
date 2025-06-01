using NodeLinkEditor.ViewModels;

namespace NodeLinkEditor.Others
{
    public class NodePositionChangeCommand : IUndoableCommand
    {
        private List<(NodeViewModel node, double newX, double newY, double oldX, double oldY)> _points = [];// 変更後位置、変更前位置
        public NodePositionChangeCommand(NodeViewModel node, double newX, double newY) : this([(node, newX, newY, node.X, node.Y)])
        {
        }
        public NodePositionChangeCommand(NodeViewModel node, double newX, double newY, double oldX, double oldY) : this([(node, newX, newY, oldX, oldY)])
        {
        }
        public NodePositionChangeCommand(List<(NodeViewModel node, double newX, double newY)> nodes) : this(nodes.Select(n => (n.node, n.newX, n.newY, n.node.X, n.node.Y)).ToList())
        {
        }
        public NodePositionChangeCommand(List<(NodeViewModel node, double newX, double newY, double oldX, double oldY)> nodes)
        {
            foreach (var (node, newX, newY, oldX, oldY) in nodes)
            {
                _points.Add((node, newX, newY, oldX, oldY));
            }
        }
        public void Execute()
        {
            foreach (var (node, newX, newY, _, _) in _points)
            {
                node.X = newX;
                node.Y = newY;
            }
        }
        public void Undo()
        {
            foreach (var (node, _, _, oldX, oldY) in _points)
            {
                node.X = oldX;
                node.Y = oldY;
            }
        }
    }
}
