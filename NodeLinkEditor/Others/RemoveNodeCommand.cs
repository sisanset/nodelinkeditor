using NodeLinkEditor.ViewModels;
using System.Collections.ObjectModel;

namespace NodeLinkEditor.Others
{
    internal class RemoveNodeCommand : IUndoableCommand
    {
        private readonly ObservableCollection<NodeViewModel> _nodes;
        private readonly NodeViewModel _removedNode;
        private readonly ObservableCollection<LinkViewModel> _links;
        private readonly List<LinkViewModel> _removedLinks;
        public RemoveNodeCommand(ObservableCollection<NodeViewModel> nodes, NodeViewModel removedNode, ObservableCollection<LinkViewModel> links)
        {
            _nodes = nodes;
            _removedNode = removedNode;
            _links = links;
            _removedLinks = [.. _links.Where(l => l.StartNode == _removedNode || l.EndNode == _removedNode)];
        }
        public void Execute()
        {
            _removedNode.IsSelected = false;
            _removedNode.IsReferenced = false;
            _nodes.Remove(_removedNode);
            foreach (var l in _removedLinks)
            {
                l.IsSelected = false;
                _links.Remove(l);
            }

        }
        public void Undo()
        {
            _nodes.Add(_removedNode);
            foreach (var l in _removedLinks)
            { _links.Add(l); }
        }
    }
}
