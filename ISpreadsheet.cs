using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetGUI
{
    public interface ISpreadsheet
    {
        event Func<string , object> UpdateContextBox;

        event Func<string, object> UpdateValueBox;

        event Action<string, string> SetContentsInSpreadSheet;

        event Action<string> FileChosenEvent;

        event Action CloseEvent;

        event Action<string> SaveEvent;

        event Action NewEvent;

        event Action HelpMessage;

        void OpenNew(string filename);

        event Func <bool>CheckSaved;

        string Message { set; }

        void DoClose();

        void UpdatePanel(object col, object row, string value);

        void OpenNew();

    }
}
