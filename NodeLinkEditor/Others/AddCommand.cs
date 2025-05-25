using NodeLinkEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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
