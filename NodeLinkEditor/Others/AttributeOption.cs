using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NodeLinkEditor.Others
{
    public class AttributeOption<T> : INotifyPropertyChanged
    {
        public T Attribute { get; }
        private bool? _isSelected = false;
        public bool? IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler? SelectionChanged;
        public AttributeOption(T attribute)
        {
            Attribute = attribute;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
