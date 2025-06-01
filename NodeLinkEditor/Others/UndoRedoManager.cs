namespace NodeLinkEditor.Others
{
    public class UndoRedoManager
    {
        private Stack<IUndoableCommand> _undoStack = new();
        private Stack<IUndoableCommand> _redoStack = new();
        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void Execute(IUndoableCommand command)
        {
            _undoStack.Push(command);
            _redoStack.Clear();
            command.Execute();
        }

        public void Undo()
        {
            if (_undoStack.Count == 0)
            { return; }
            var command = _undoStack.Pop();
            _redoStack.Push(command);
            command?.Undo();
        }

        public void Redo()
        {
            if (_redoStack.Count == 0)
            { return; }
            var command = _redoStack.Pop();
            _undoStack.Push(command);
            command?.Execute();
        }
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
