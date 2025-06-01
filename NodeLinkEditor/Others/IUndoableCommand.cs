namespace NodeLinkEditor.Others
{
    public interface IUndoableCommand
    {
        void Execute();
        void Undo();
    }
}
