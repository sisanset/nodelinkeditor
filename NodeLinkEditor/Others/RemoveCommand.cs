using System.Collections.ObjectModel;

namespace NodeLinkEditor.Others
{
    internal class RemoveCommand<T> : IUndoableCommand
    {
        private readonly ObservableCollection<T> _items;
        private readonly T _item;
        public RemoveCommand(ObservableCollection<T> items, T item)
        {
            _items = items;
            _item = item;

        }
        public void Execute() => _items.Remove(_item);
        public void Undo() => _items.Add(_item);
    }
}
