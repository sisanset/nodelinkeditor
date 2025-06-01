using System.Collections.ObjectModel;

namespace NodeLinkEditor.Others
{
    internal class AddCommand<T> : IUndoableCommand
    {
        private readonly ObservableCollection<T> _items;
        private readonly T _item;
        public AddCommand(ObservableCollection<T> items, T item)
        {
            _items = items;
            _item = item;

        }
        public void Execute() => _items.Add(_item);
        public void Undo() => _items.Remove(_item);
    }
}
