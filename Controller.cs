//Written by Ryan Williams 
//CS 3500, Spring 2016

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS;
using Formulas;
using System.IO;
using System.Text.RegularExpressions;

namespace SpreadsheetGUI
{
    /// <summary>
    /// This class controlls all interaction between the view (GUI) and the model (spreadsheet). 
    /// This is done by giving the controller a reference to the Interface that the GUI is built on, 
    /// and a reference to the model. Once this has been acomplished, we use events and methods that
    /// are registered to methods in the controller which handle all interactions between the view and model. 
    /// </summary>
    public class Controller
    {
        // The window being controlled
        private ISpreadsheet window;

        // The model being used
        private Spreadsheet model;

        /// <summary>
        /// Begins controlling window.
        /// </summary>
        public Controller(ISpreadsheet window, StreamReader filename = null)
        {
            //register all the events in the interface to the methods here in the controller
            //this is how the controller communicates with the view
            this.window = window;

            //check to see if we are openning a new spreadsheet with a filename or a brand new one
            if(filename != null)
            {
                this.model = new Spreadsheet(filename);

                //now that we have read in the XML, we need to go and populate all the cells in the grid with the data
                foreach(string name in model.GetNamesOfAllNonemptyCells())
                {
                    UpdateCells(name);
                }
            }
            else
            {
                this.model = new Spreadsheet();
            }
            
            //hook methods to events getting fired in the GUI
            window.FileChosenEvent += HandleFileChosen; //event fired when the user wants to open an existing spreadsheet
            window.CloseEvent += HandleClose; //event that we fire when the user wants to close the form
            window.NewEvent += HandleNew; //event fired when they want to open a new spreadsheet
            window.HelpMessage += HandleHelpMessage; //event fired when the user wants to display the help message
            window.SetContentsInSpreadSheet += HandleSetContents; //event fired when the user sets a cells contents
            window.UpdateContextBox += HandleUpdateContentsBox; //event fired when we need to update contents textbox
            window.UpdateValueBox += HandleUpdateValueBox; //event fired when we need to updaste value textbox
            window.SaveEvent += HandleSaveEvent; //event fired when the user wants to save a spreadsheet
            window.CheckSaved += HandleCheckSaved; //event fired when the user closes, needed to check if saved or not
        }

        /// <summary>
        /// a method that checks to see the value of the changed property in the 
        /// spreadhseet, which determines if we can close without saving or not
        /// </summary>
        /// <returns></returns>
        private bool HandleCheckSaved()
        {
            if (model.Changed) //check the value in the spreadsheet model
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Handles the save event that was triggered in the view. 
        /// Passes the file name to the model so it can do the saving
        /// </summary>
        /// <param name="file"></param>
        private void HandleSaveEvent(string file)
        {
            try
            {
                model.Save(new StreamWriter(file)); //save the contents of the spreadsheet in an XML
            }
            catch (Exception e)
            {
                window.Message = "Sorry, couldn't save the file"; //if something goes wrong, let the user know
            }
        }

        /// <summary>
        /// Handles the SetContents event that gets fired in the view. 
        /// Here we give the cell name and its contents to the model
        /// </summary>
        /// <param name="cellName"></param>
        /// <param name="contents"></param>
        private void HandleSetContents(string cellName, string contents)
        {
            try
            {
                HashSet<string> recalculate = (HashSet<string>)model.SetContentsOfCell(cellName, contents); //put them in the model
                foreach (string cell in recalculate)
                {
                    UpdateCells(cell); //update all the cells whos value changed once we added a cell (if any)
                }
            }
            catch (Exception e)
            {
                //if a circular dependency happens, the model throws it and we must display that this happened to the user
                if (e is CircularException)
                {
                    window.Message = "Sorry: Circular Dependencies are not allowed in the spreadsheet";
                }

                //if the user gave an invalid formula, Formula (model) will throw it; we catch it and let the user know
                if (e is FormulaFormatException)
                {
                    window.Message = "Sorry: This is an invalid Formula";
                }
            }
        }

        /// <summary>
        /// Calls the updatePanel method in the view that goes and changes the value of all cells that must change
        /// </summary>
        /// <param name="name"></param>
        private void UpdateCells(string name)
        {
            window.UpdatePanel(GoBackCol(name), GoBackRow(name), model.GetCellValue(name).ToString());
        }

        /// <summary>
        /// A helper method that takes the name of the cell and gives back the column number 
        /// that is specific to that name, so that way we can upgrade all the cells on the grid
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private object GoBackCol(string name)
        {
            char[] column = name.ToCharArray();
            return char.ToUpper(column[0]) - 64; //the first char gives the column, -64 to reverse ascii value
        }

        /// <summary>
        /// A helper method that takes the name of the cell and gives back the row number 
        /// that is specific to that name, so that way we can upgrade all the cells on the grid
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private object GoBackRow(string name)
        {
            int val;
            Int32.TryParse(name.Substring(1), out val); //parses everything after the 0 index char into an int
            return val; //now we have the int for the row... return it 
        }

        /// <summary>
        /// Handles the event that gets fired in the moedl that updates the value of the value textbox
        /// </summary>
        /// <param name="cellName"></param>
        /// <returns></returns>
        private object HandleUpdateValueBox(string cellName)
        {
            try
            {
                return model.GetCellValue(cellName); //give the value of that cell, given the cell name
            }
            catch (Exception)
            {
                return ""; //if it doesn't have a value for some reason, we display nothing
            }
        }

        /// <summary>
        /// Sets ContentsTextBox
        /// </summary>
        private object HandleUpdateContentsBox(string cellName)
        {
            try
            {
                if (model.GetCellContents(cellName) is Formula)
                {
                    //if the contents is a formula, we must append the equal sign, to denote such a content
                    //we don't have to worry about ToString() failing either, because we overrode it in Formula()
                    return "=" + model.GetCellContents(cellName).ToString();
                }
                else
                {
                    return model.GetCellContents(cellName).ToString(); //if its not a formula, just give it the double 
                }
            }
            catch(Exception e)
            {
                return ""; //if the cell doesn't exist in the spreadsheet then we want to just output an empty string in the contents box
            }
            
        }

        /// <summary>
        /// Handles a request to open a file.
        /// </summary>
        private void HandleFileChosen(String filename)
        {
            try
            {
                window.OpenNew(filename); //open an existing spreadsheet in a new window
            }
            catch(Exception e)
            {
                window.Message = "Unable to open file\n" + e.Message; //if something goes wrong, let the user know
            }
        }

        /// <summary>
        /// Handles a request to close the window
        /// </summary>
        private void HandleClose()
        {
            window.DoClose(); //call the method in the view that closes the form
        }

        /// <summary>
        /// Handles a request to open a new window.
        /// </summary>
        private void HandleNew()
        {
            window.OpenNew(); //call the method in the view that closes the form
        }

        /// <summary>
        /// Handles the event that is fired when the user clicks the Help menu strip item
        /// </summary>
        private void HandleHelpMessage()
        {
            //set the value of the Message property in the view that displays the message to the user
            window.Message = "To edit a cell, click on it and then type its contents in the text box above, then press enter to set it";
        }
    }
}
