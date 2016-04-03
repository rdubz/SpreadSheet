//Ryan Williams 
//CS 3500
//This stub was written by Ryan Williams
//to properly test the controller 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    /// <summary>
    /// This stub acts like a fake SpreadSheet GUI Windows Form.
    /// Instead of watching what happens to the GUI uisng coded UI 
    /// tests. This class has methods that call events just like buttons, 
    /// textbox, and such things of those in a GUI do.
    /// </summary>
    public class Form1Stub : ISpreadsheet
    {

        public event Action CloseEvent; //event that we fire when the user wants to close the form
        public event Action<string> FileChosenEvent; //event fired when the user wants to open an existing spreadsheet
        public event Action HelpMessage; //event fired when the user wants to display the help message
        public event Action NewEvent; //event fired when they want to open a new spreadsheet
        public event Action<string> SaveEvent; //event fired when the user wants to save a spreadsheet
        public event Action<string> SelectionChanged; //event fired when the user clicks on a different cell
        public event Action<string, string> SetContentsInSpreadSheet; //event fired when the user sets a cells contents
        public event Func<string, object> UpdateContextBox; //event fired when we need to update contents textbox 
        public event Func<string, object> UpdateValueBox; //event fired when we need to updaste value textbox
        public event Func<bool> CheckSaved; //event fired when the user closes, needed to check if saved or not

        /// <summary>
        /// a string used to check the values that are sent to 
        /// a message box via a message property within the GUI
        /// </summary>
        private string check;
       
        /// <summary>
        /// a property that checks to see if the method that closes 
        /// the form in the GUI gets called from the controller 
        /// </summary>
        public bool CalledDoClose
        {
            get; private set;
        }

        /// <summary>
        /// a property that checks to see if the method that opens 
        /// the form in the GUI gets called from the controller
        /// </summary>
        public bool CalledNewOpen
        {
            get; private set;
        }

        /// <summary>
        /// A property used to see if the UpdatePanel method gets called from 
        /// the controller
        /// </summary>
        public bool CalledUpdatePanel
        {
            get; private set;
        }

        /// <summary>
        /// The message property used in the GUI to set message box
        /// prompts. Here we use the check string to make sure the 
        /// right string is getting output from the GUI 
        /// </summary>
        public string Message
        {
            set
            {
                check = value;
            }
            get
            {
                return check;
            }
        }

        /// <summary>
        /// Method that acts like a an eventhandler in the GUI that 
        /// calls the CloseEvent that is registered in the controller 
        /// </summary>
        public void FireCloseEvent()
        {
            if(CloseEvent != null)
            {
                CloseEvent();
            }
        }

        /// <summary>
        /// a method that simulates an event in the GUI that fires the checksaved method to see 
        /// if the file needs to be saved for closing or not
        /// </summary>
        /// <returns></returns>
        public bool FireCheckSaved()
        {
            return CheckSaved();
        }

        /// <summary>
        /// a method that simulates an event in the GUI that fires the UpdateValueBox, 
        /// which goes to the model (through the controller) to get the value of a cell 
        /// in the spreadsheet
        /// </summary>
        /// <param name="cellName"></param>
        /// <returns></returns>
        public object FireUpdateValueBox(string cellName)
        {
            return UpdateValueBox(cellName);
        }

        /// <summary>
        /// a method that simulates an event in the GUI that fires the UpdateContentsBox, 
        /// which goes to the model (through the controller) to get the contents of a cell 
        /// in the spreadsheet
        /// </summary>
        /// <param name="cellName"></param>
        /// <returns></returns>
        public object FireUpdateContentsBox(string cellName)
        {
            return UpdateContextBox(cellName);
        }

        /// <summary>
        /// Method that acts like a an eventhandler in the GUI that 
        /// calls the NewEvent event that is registered in the controller 
        /// </summary>
        public void FireOpenEvent()
        {
            if(NewEvent != null)
            {
                NewEvent();
            }
        }

        /// <summary>
        /// a method that simulates the file menu event click for the 
        /// help menu
        /// </summary>
        public void FireHelpMessage()
        {
            if(HelpMessage != null)
            {
                HelpMessage();
            }
        }

        /// <summary>
        /// a method that simulates an event in the GUI that fires the FileChosenEvent, 
        /// which goes to the model (through the controller) to get the xml of a file to 
        /// load into a spreadsheet 
        /// </summary>
        /// <param name="cellName"></param>
        /// <returns></returns>
        public void FireFileChosenEvent()
        {
            OpenFileDialog file = new OpenFileDialog();
            DialogResult result = file.ShowDialog(); //open the open file dialog
            if (result == DialogResult.Yes || result == DialogResult.OK)
            {
                if (FileChosenEvent != null)
                {
                    FileChosenEvent(file.FileName); //fire the FileChosenEvent which gets handled in the controller
                }
            }
        }

        /// <summary>
        /// a method that simulates an event in the GUI that fires the SaveEvent, 
        /// which goes to the model (through the controller) to write a xml file to 
        /// a destination 
        /// </summary>
        /// <param name="cellName"></param>
        /// <returns></returns>
        public void FireSaveEvent()
        {
            SaveFileDialog save = new SaveFileDialog();
            DialogResult result = save.ShowDialog(); //open the save dialog
            if (result == DialogResult.Yes || result == DialogResult.OK)
            {
                if (SaveEvent != null) //make sure the controller was created
                {
                    SaveEvent(save.FileName);//fire the save event and give it the file name to save to 
                }
            }
        }

        /// <summary>
        /// a method in the stub that simulates putting items into the spreadsheet 
        /// ,the event is noramally fired when the user hits enter in the contents box
        /// after typing in the contents 
        /// </summary>
        /// <param name="cellName"></param>
        /// <param name="contents"></param>
        public void FireSetContentsInSpreadsheetEvent(string cellName, string contents)
        {    
           SetContentsInSpreadSheet(cellName, contents); //set the contents in the spreadsheet
        }

        /// <summary>
        /// similar to the method above, but this one creates a circular dependency
        /// </summary>
        public void FireSetContentsWithCircularDependency()
        {
            SetContentsInSpreadSheet("A1", "=B1");
            SetContentsInSpreadSheet("B1", "=A1 ");
        }

        /// <summary>
        /// the method that normally gets called in the form to close the 
        /// GUI when the user hits close. We just set a property here though
        /// for testing purposes 
        /// </summary>
        public void DoClose()
        {
            CalledDoClose = true; //set the property value 
        }

        /// <summary>
        /// the method that normally gets called in the GUI form when the user 
        /// hits open in the file menu strip. We just set a property value to 
        /// check for testing
        /// </summary>
        public void OpenNew()
        {
            CalledNewOpen = true; //set the property value 
        }

        /// <summary>
        /// this method gets called in the GUI when all the cells in the 
        /// grid need to be updated. Again, we just set a proprty value to 
        /// check later for testing to make sure this method got called 
        /// from the controller when appropriate
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <param name="value"></param>
        public void UpdatePanel(object col, object row, string value)
        {
            CalledUpdatePanel = true; //set the property value 
        }

        /// <summary>
        /// this method gets called when the user opens a file 
        /// and we want to create a new form with what the spreadsheet just read in
        /// </summary>
        /// <param name="filename"></param>
        public void OpenNew(string filename)
        {
            CalledNewOpen = true;
        }
    }
}
