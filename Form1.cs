//Written by Ryan Williams
//CS 3500, Spring 2016

using SSGui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    /// <summary>
    /// This class represents the Graphical User Interface for the spreadsheet. 
    /// The spreadsheet is represented by a spreadsheet panel which has a grid 
    /// like appearance. There are three textboxes (one editable, and two non-editable)
    /// that represent the cells name, contents, and value. Whats displayed in the boxes 
    /// is the values that corelate to that cell selected in the grid.
    /// 
    /// Also has a help menu, along with a file menu that can open a new spreadsheet, open an
    /// existing spreadsheet, save the current spreadsheet, or close the currently open 
    /// spreadsheet. 
    /// </summary>
    public partial class Form1 : Form, ISpreadsheet
    {
        public Form1()
        {
            InitializeComponent();

            // This an example of registering a method so that it is notified when
            // an event happens.  The SelectionChanged event is declared with a
            // delegate that specifies that all methods that register with it must
            // take a SpreadsheetPanel as its parameter and return nothing.  So we
            // register the displaySelection method below.
            spreadsheetPanel2.SelectionChanged += displaySelection;
            spreadsheetPanel2.SelectionChanged += View_UpdateContentsBox;
            spreadsheetPanel2.SelectionChanged += View_UpdateNameBox;
            spreadsheetPanel2.SelectionChanged += View_UpdateValueBox;
           
            //when the GUI opens, we want the selected cell to be at A1, which is col 0 row 0 in the panel
            spreadsheetPanel2.SetSelection(0, 0);
        }

        public event Action CloseEvent; //event that we fire when the user wants to close the form
        public event Action<string> FileChosenEvent; //event fired when the user wants to open an existing spreadsheet
        public event Action<string> SaveEvent; //event fired when the user wants to save a spreadsheet
        public event Action NewEvent; //event fired when they want to open a new spreadsheet
        public event Action<string, string> SetContentsInSpreadSheet; //event fired when the user sets a cells contents
        public event Action HelpMessage; //event fired when the user wants to display the help message
        public event Func<string, object> UpdateContextBox; //event fired when we need to update contents textbox
        public event Func<string, object> UpdateValueBox; //event fired when we need to updaste value textbox
        public event Func <bool>CheckSaved; //event fired when the user closes, needed to check if saved or not

        /// <summary>
        /// Every time the selection changes, this method is called with the
        /// Spreadsheet as its parameter. We display the value of the cell 
        /// that is selected
        /// </summary>
        private void displaySelection(SpreadsheetPanel ss)
        {
            int row, col;
            String value;
            ss.GetSelection(out col, out row); //get value of the col and row in the grid
            ss.GetValue(col, row, out value); //now that we have the col and row, we can get the value at that cell
            
            ss.SetValue(col, row, UpdateValueBox(ss.GetCellName(col, row)).ToString()); // put the value in the box
        }

        /// <summary>
        /// A property that displays a message box on the GUI
        /// whenever a message is needed to be delivered to the user
        /// </summary>
        public string Message
        {
            set
            {
                MessageBox.Show(value); //display the message box with the desired string
            }
        }

        /// <summary>
        /// Updates the Contents textbox to display the contents
        /// of whatever cell is selected on the GUI
        /// </summary>
        /// <param name="sender"></param>
        private void View_UpdateContentsBox(SpreadsheetPanel ss)
        {
            if(UpdateContextBox != null)
            {
                int col, row;
                ss.GetSelection(out col , out row);

                //places the contents in the contents textbox according to the cell name given
                ContentsTextBox.Text = UpdateContextBox(ss.GetCellName(col, row)).ToString();
            }
        }

        /// <summary>
        /// Updates the Value textbox to display the value
        /// of whatever cell is selected on the GUI. The value 
        /// may be a double, a string, or a FormulaError
        /// </summary>
        /// <param name="sender"></param>
        private void View_UpdateValueBox(SpreadsheetPanel ss)
        {
            if(UpdateValueBox != null)
            {
                int col, row;
                ss.GetSelection(out col, out row);

                //places the value in the value textbox according to the cell name given
                ValueTextBox.Text = UpdateValueBox(ss.GetCellName(col, row)).ToString();
            }
        }

        /// <summary>
        /// Updates the Name textbox to display the name 
        /// off the current cell selected in the cell. 
        /// The name is determined by the Cell column and row
        /// </summary>
        /// <param name="sender"></param>
        private void View_UpdateNameBox(SpreadsheetPanel ss)
        {
            int col, row;
            spreadsheetPanel2.GetSelection(out col, out row);

            //places the name in the cellname textbox according to the cell position on the grid
            cellname.Text = ss.GetCellName(col, row);
        }

        /// <summary>
        /// Opens a new blank Spreadsheet GUI form
        /// </summary>
        public void OpenNew()
        {
            SpreadsheetApplicationContext.GetContext().RunNew();
        }

        public void OpenNew(string filename)
        {
            SpreadsheetApplicationContext.GetContext().RunNew(filename);
        }

        /// <summary>
        /// Load method for the GUI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Calls an event to open a new Spreadsheet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if(NewEvent != null)
            {
                NewEvent(); //fire the new event that will be handled in the controller
            }
        }

        /// <summary>
        /// Saves the current state of the spreadsheet into an XML using 
        /// a savefiledialog in a windows GUI form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            DialogResult result = saveFileDialog1.ShowDialog(); //open the save dialog
            if(result == DialogResult.Yes || result == DialogResult.OK)
            {
                if(SaveEvent != null)
                {
                    SaveEvent(saveFileDialog1.FileName);//fire the save event and give it the file name to save to 
                }
            }
        }

        /// <summary>
        /// Opens an XML file using a openfiledialog, then takes that 
        /// information and dispalys it in the cells on the panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog4.ShowDialog(); //open the open file dialog
            if (result == DialogResult.Yes || result == DialogResult.OK)
            {
                if(FileChosenEvent != null)
                {
                    FileChosenEvent(openFileDialog4.FileName); //fire the FileChosenEvent which gets handled in the controller
                }
            }
        }

        /// <summary>
        /// Closes the current spreadsheet window. If the user tries to close 
        /// without saving, it prompts if they want to continue; otherwise they 
        /// may lose data. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            //first have to check to see if the file has been modified prior to closing
            if(CheckSaved())
            {
                DialogResult result = MessageBox.Show("Closing the file before saving could result in loss of data, do you want to continue?", "Possible Data Loss", MessageBoxButtons.YesNo);
                if(result == DialogResult.Yes) //if they want to continue with the close, we fire the event; otherwise do nothing
                {
                    if(CloseEvent != null)
                    {
                        CloseEvent();
                    }
                }
            }
            else if(CloseEvent != null) //if it has been saved we can go ahead with closing
            {
                CloseEvent();
            }

        }

        /// <summary>
        /// Displays a help message to help the user understand 
        /// how to use the spreadsheet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if(HelpMessage != null)
            {
                HelpMessage();//Fire the help message
            }
        }

        private void spreadsheetPanel2_Load(object sender, EventArgs e)
        {
            ContentsTextBox.Focus();
        }

        /// <summary>
        /// A method called from the Controller to close the GUI
        /// </summary>
        public void DoClose()
        {
            Close();//close the form
        }

        /// <summary>
        /// An event that sets the contents of a cell in the spreadsheet
        /// This event is fired when the user press the enter key after entering
        /// the contents. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentsBox_KeyDown(object sender, KeyEventArgs e)
        {
            //check to see if enter was hit
            if(e.KeyValue == 13)
            {
                int col, row;
                spreadsheetPanel2.GetSelection(out col, out row);
                SetContentsInSpreadSheet(spreadsheetPanel2.GetCellName(col, row), ContentsTextBox.Text); //set the contents in the spreadsheet
                ValueTextBox.Text = UpdateValueBox(spreadsheetPanel2.GetCellName(col, row)).ToString(); //update the value textbox, now that the cell has a value
                spreadsheetPanel2.SetValue(col, row, UpdateValueBox(spreadsheetPanel2.GetCellName(col, row)).ToString().ToString()); //place the value in the cell on the grid

            }
        }

        /// <summary>
        /// When a cells contents changes, it has the potential to 
        /// change the value of all other cells. This method updates 
        /// all the cells that need to be changed
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <param name="val"></param>
        public void UpdatePanel(object col, object row, string val)
        {
            this.spreadsheetPanel2.SetValue((int)col - 1, (int)row - 1, val); //put the value at the desired spot on the grid
        }
    }
}
