using NodeLinkEditor.Models;
using NodeLinkEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeLinkEditor.Others
{
    public class HelperLinePositionChangeCommand : IUndoableCommand
    {
        private HelperLineViewModel _line;
        private HelperLine _oldLine;
        private HelperLine _newLine;
        public HelperLinePositionChangeCommand(HelperLineViewModel line, HelperLine newLine)
        {
            _line = line;
            _oldLine = line.GetHelperLineCopy();
            _newLine = newLine;
        }
        public void Execute()
        {
            _line.StartX = _newLine.StartX;
            _line.StartY = _newLine.StartY;
            _line.EndX = _newLine.EndX;
            _line.EndY = _newLine.EndY;
        }

        public void Undo()
        {
            _line.StartX = _oldLine.StartX;
            _line.StartY = _oldLine.StartY;
            _line.EndX = _oldLine.EndX;
            _line.EndY = _oldLine.EndY;
        }
    }
}
