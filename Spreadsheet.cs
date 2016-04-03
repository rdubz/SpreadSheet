using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Formulas;
using SS;
using System.Text.RegularExpressions;
using Dependencies;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Text.RegularExpressions;

namespace SS
{
    /// <summary>
    /// This class implements Joe Zachary's Abstract SpreadSheet 
    /// Its current functionality is to create cells and populate it with 
    /// its contents whether it is a formula, a double, or some text. 
    /// It also gets all the names of all non empty cells by returning
    /// an ISet<> of them all. And it can retreieve the contents of a cell, 
    /// provided the name of the cell. 
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        /// <summary>
        /// The cells dictionary is what is used to hold all the cells in the spreadsheet
        /// Its key is the name of the cell, and the value is whats in the cell itself, 
        /// such as it's contents and the value of those contents 
        /// </summary>
        private Dictionary<string, Cell> cells;

        /// <summary>
        /// The dependency graph from our past assignment is what keeps track of the relationship 
        /// of cells in terms of dependents and dependencies. 
        /// </summary>
        private DependencyGraph dg;

        /// <summary>
        /// The spreadsheet keeps track of the saved state of the spreadsheet
        /// If the spreadsheet has been changed since the last time it was savedf
        /// or created, the it is true. Otherwise it is false
        /// </summary>
        private bool changed;

        /// <summary>
        /// A second regular expression that is used to validate whether a cell name is 
        /// valid or not, on top of the regular expression that only accepts cell names 
        /// of the correct format
        /// </summary>
        private Regex IsValid;

        /// <summary>
        /// zero parameter constructor that creates an empty spreadsheet
        /// </summary>
        public Spreadsheet()
        {
            cells = new Dictionary<string, Cell>();
            dg = new DependencyGraph();
            Changed = false; //nothing has changed in the spreadsheet so the changed property is false 
            IsValid = new Regex("^[a-zA-Z][1-9][0-9]?$"); //initialze RegEx to accept all strings
        }

        // Creates an empty Spreadsheet whose IsValid regular expression is provided as the parameter
        public Spreadsheet(Regex isValid)
        {
            cells = new Dictionary<string, Cell>();
            dg = new DependencyGraph();
            Changed = false;
            IsValid = isValid; //create another RegEx to even further valid cell names
        }

        /// Creates a Spreadsheet that is a duplicate of the spreadsheet saved in source.
        /// See the AbstractSpreadsheet.Save method and Spreadsheet.xsd for the file format 
        /// specification.  If there's a problem reading source, throws an IOException
        /// If the contents of source are not consistent with the schema in Spreadsheet.xsd, 
        /// throws a SpreadsheetReadException. If there is an invalid cell name, or a 
        /// duplicate cell name, or an invalid formula in the source, throws a SpreadsheetReadException.
        /// If there's a Formula that causes a circular dependency, throws a SpreadsheetReadException. 
        public Spreadsheet(TextReader source)
        {
            cells = new Dictionary<string, Cell>();
            dg = new DependencyGraph();
            IsValid = new Regex("^(.*)$"); //initialze RegEx to accept all strings

            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add(null, "Spreadsheet.xsd");

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = schema;

            try
            {
                //create the reader
                using (XmlReader reader = XmlReader.Create(source, settings))
                {   
                    while (reader.Read()) //read in the file
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "spreadsheet":
                                    IsValid = new Regex(reader["IsValid"]);
                                    break;

                                case "cell":
                                    SetContentsOfCell(reader["name"], reader["contents"]);
                                    break;

                            }
                        }
                    }
                }
            }//if something happens, catch one of these exceptions and handle it
            catch(XmlSchemaValidationException)
            {
                source.Close();
                throw new SpreadsheetReadException("The XML file that was just read in does not match the schema definition");
            }
            catch(FormulaFormatException)
            {
                source.Close();
                throw new SpreadsheetReadException("There is an invalid formula in the file that was just tried to be read in");
            }
            catch(CircularException)
            {
                source.Close();
                throw new SpreadsheetReadException("A circular Dependency can't exist in the spreadsheet");
            }
            catch(InvalidNameException)
            {
                source.Close();
                throw new SpreadsheetReadException("Invalid name of a cell");
            }
            catch(Exception)
            {
                source.Close();
                throw new IOException();
            }

            source.Close();
            Changed = false;
        }

        // ADDED FOR PS6
        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed
        {
            get
            {
                return changed;  
            }

            protected set
            {
                changed = value;
            }
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            if(name == null || !(IsCellNameValid(name.ToUpper())))
            {
                throw new InvalidNameException();
            }

            if(!(cells.ContainsKey(name.ToUpper())))
            {
                return "";
            }

            //get the contents at that cell
            return cells[name.ToUpper()].contents;
        }

        // ADDED FOR PS6
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a FormulaError.
        /// </summary>
        public override object GetCellValue(string name)
        {
            name = name?.ToUpper();
            if(name == null || !(IsCellNameValid(name)))
            {
                throw new InvalidNameException();
            }

            if (!(cells.ContainsKey(name)))
            {
                return "";
            }

            return cells[name].value;
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            //create a HashSet to hold all the names of non empty cells
            HashSet<string> namesOfNonEmptyCells = new HashSet<string>();

            //gets all the keys in the cells dictionary, which represent the names of the cells
            foreach(string name in cells.Keys)
            {
                //make sure the cell isn't empty before we actually add it
                if(cells[name].contents != string.Empty)
                {
                    namesOfNonEmptyCells.Add(name);
                }
            }

            return namesOfNonEmptyCells;
        }

        // ADDED FOR PS6
        /// <summary>
        /// Writes the contents of this spreadsheet to dest using an XML format.
        /// The XML elements should be structured as follows:
        ///
        /// <spreadsheet IsValid="IsValid regex goes here">
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        /// </spreadsheet>
        ///
        /// The value of the isvalid attribute should be IsValid.ToString()
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.
        /// If the cell contains a string, the string (without surrounding double quotes) should be written as the contents.
        /// If the cell contains a double d, d.ToString() should be written as the contents.
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        ///
        /// If there are any problems writing to dest, the method should throw an IOException.
        /// </summary>
        public override void Save(TextWriter dest)
        {
            try
            {
                using (XmlWriter writer = XmlWriter.Create(dest))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("spreadsheet");
                    writer.WriteAttributeString("IsValid", IsValid.ToString());
                    foreach (string CellName in cells.Keys)
                    {
                        writer.WriteStartElement("cell");
                        writer.WriteAttributeString("name", CellName);
                        if (cells[CellName].contents is double)
                        {
                            writer.WriteAttributeString("contents", cells[CellName].contents.ToString());

                        }
                        else if (cells[CellName].contents is Formula)
                        {
                            writer.WriteAttributeString("contents", "=" + cells[CellName].contents.ToString());
                        }
                        else
                        {
                            writer.WriteAttributeString("contents", cells[CellName].contents.ToString());
                        }

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            catch(Exception)
            {
                dest.Close();
                throw new IOException();
            }
            dest.Close();

            Changed = false;
        }

        // MODIFIED PROTECTION FOR PS6
        /// <summary>
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, Formula formula)
        {
            //see if name is null, if it is throw exception, otherwise ToLower it to use through out the whole method 
            name = name?.ToUpper();
            //we can't have a cell who's contents is null, if the user tries we throw an exception 
            if(formula.Equals(null))
            {
                throw new ArgumentNullException();
            }

            //if the name provided is null or doesn't match what is exceptable for a cell name, then throw the exception
            if(name == null || !(IsCellNameValid(name)))
            {
                throw new InvalidNameException();
            }

            //make a copy of the current graph to hold the old state of the spreadsheet, just incase we have a circular exception
            DependencyGraph copyOfDg = new DependencyGraph(dg);
            bool addedCell = false; 

            Cell oldCell;
            cells.TryGetValue(name, out oldCell);

            //try to add it, and if we can't then we want to throw the exception and then go back to the state before we added it 
            try
            {
                //if the dictionary already contains the key, we just need to 
                //replace its dependents, remove the old key, and create a new one
                //with whatever formula they passed in
                if(cells.ContainsKey(name))
                {
                    List<String> Variables = new List<string>();

                    foreach (var item in formula.GetVariables())
                    {
                        Variables.Add(item);
                    }
                    //replace all the dependents with the new dependencies
                    dg.ReplaceDependents(name, Variables);

                    //remove the old key and add the new one
                    cells.Remove(name);
                    cells.Add(name, new Cell(formula, null));

                    GetCellsToRecalculate(name);
                }
                else
                {

                    //if no exceptions have been thrown yet, we are okay to add
                    //so we add the cell to the spreadsheet which is backed by our dictionary
                    cells.Add(name, new Cell(formula, null));
                    addedCell = true;
                    //create a dependency for name and each cell name that 
                    //is contained in the variable that is passed in
                    foreach (string variable in formula.GetVariables())
                    {
                        if (IsCellNameValid(variable))
                        {
                            dg.AddDependency(name, variable);
                        }
                    }

                    GetCellsToRecalculate(name);
                }
            }
            catch(CircularException)
            {
                dg = new DependencyGraph(copyOfDg);
                if(addedCell)
                {
                    cells.Remove(name);
                }
                else
                {
                    cells[name].contents = oldCell.contents;
                }
                throw new CircularException();
             }

            //get all the cells that are dependent of the cell we just changed
            //and chagned their value if needed
            foreach (string cellName in GetCellsToRecalculate(name))
            {
                if(cells[cellName].contents is Formula)
                {
                    try
                    {
                        cells[cellName].value = ((Formula)cells[cellName].contents).Evaluate(Lookup);
                    }
                    catch(UndefinedVariableException) //if a cell name doesn't have a value, then we can compute the value of the cell that needs that value 
                    {
                        cells[cellName].value = new FormulaError();
                    }
                    catch(FormulaEvaluationException)
                    {
                        cells[cellName].value = new FormulaError();
                    }
                }
            }

            return new HashSet<string>(GetCellsToRecalculate(name));
        }

        /// <summary>
        /// Helper method that is used to lookup values of variables
        /// which are cell names, in order to evaluate the value of a 
        /// formula 
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        private double Lookup(string variable)
        {
            //try to get the value of the cell name, in order to evaluate the current cells value 
            try
            {
                if (cells[variable].value == "")
                {
                    throw new UndefinedVariableException("This cell is empty");
                }
                return (double)cells[variable].value;
            }
            catch(FormulaEvaluationException)//if it throws any of these exceptions we catch it and throw another so the value of the cell then becomes FormulaError
            {
                throw new UndefinedVariableException("There is an undefined variable");
            }
            catch(KeyNotFoundException)
            {
                throw new UndefinedVariableException("There is an undefined variable");
            }
            catch(InvalidCastException)
            {
                throw new UndefinedVariableException("There is an undefined variable");
            }
            
        }

        // MODIFIED PROTECTION FOR PS6
        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, string text)
        {
            //we can't have a cell who's contents is null, if the user tries we throw an exception 
            if(text == null)
            {
                throw new ArgumentNullException();
            }

            //if the name provided is null or doesn't match what is exceptable for a cell name, then throw the exception
            if(name == null || !(IsCellNameValid(name)))
            {
                throw new InvalidNameException();
            }

            //see if our cells dictionary already contains the name
            //if so, we remove the current one and add the new one
            if(cells.ContainsKey(name))
            {
                cells.Remove(name);
                cells.Add(name, new Cell(text, text));

                //get all the cells that are dependent of the cell we just changed
                //and chagned their value if needed
                foreach (string cellName in GetCellsToRecalculate(name))
                {
                    if (cells[cellName].contents is Formula)
                    {
                        try
                        {
                            cells[cellName].value = ((Formula)cells[cellName].contents).Evaluate(Lookup);
                        }
                        catch (UndefinedVariableException)
                        {
                            cells[cellName].value = new FormulaError();
                        }
                        catch (FormulaEvaluationException)
                        {
                            cells[cellName].value = new FormulaError();
                        }
                    }
                }

                return new HashSet<string>(GetCellsToRecalculate(name));
            }

            //add the cell to the spreadsheet which is backed by our dictionary
            cells.Add(name, new Cell(text, text));

            //get all the cells that are dependent of the cell we just changed
            //and chagned their value if needed
            foreach (string cellName in GetCellsToRecalculate(name))
            {
                if(cells[cellName].contents is Formula)
                {
                    try
                    {
                        cells[cellName].value = ((Formula)cells[cellName].contents).Evaluate(Lookup);
                    }
                    catch(UndefinedVariableException)
                    {
                        cells[cellName].value = new FormulaError();
                    }
                    catch(FormulaEvaluationException)
                    {
                        cells[cellName].value = new FormulaError();
                    }
                }
            }

            return new HashSet<string>(GetCellsToRecalculate(name));
        }

        // MODIFIED PROTECTION FOR PS6
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, double number)
        {
            //if the name provided is null or doesn't match what is exceptable for a cell name, then throw the exception
            if(name == null || !(IsCellNameValid(name)))
            {
                throw new InvalidNameException();
            }

            //see if our cells dictionary already contains the name
            //if so, we remove the current one and add the new one
            if(cells.ContainsKey(name))
            {
                cells.Remove(name);
                cells.Add(name, new Cell(number, number));

                //get all the cells that are dependent of the cell we just changed
                //and chagned their value if needed
                foreach (string cellName in GetCellsToRecalculate(name))
                {
                    if (cells[cellName].contents is Formula)
                    {
                        try
                        {
                            cells[cellName].value = ((Formula)cells[cellName].contents).Evaluate(Lookup);
                        }
                        catch (UndefinedVariableException)
                        {
                            cells[cellName].value = new FormulaError();
                        }
                        catch (FormulaEvaluationException)
                        {
                            cells[cellName].value = new FormulaError();
                        }
                    }
                }

                return new HashSet<string>(GetCellsToRecalculate(name));
            }

            //add the cell to the spreadsheet which is backed by our dictionary
            cells.Add(name, new Cell(number, number));

            //get all the cells that are dependent of the cell we just changed
            //and chagned their value if needed
            foreach (string cellName in GetCellsToRecalculate(name))
            {
                if (cells[cellName].contents is Formula)
                {
                    try
                    {
                        cells[cellName].value = ((Formula)cells[cellName].contents).Evaluate(Lookup);
                    }
                    catch (UndefinedVariableException)
                    {
                        cells[cellName].value = new FormulaError();
                    }
                    catch(FormulaEvaluationException)
                    {
                        cells[cellName].value = new FormulaError();
                    }
                }
            }

            return new HashSet<string>(GetCellsToRecalculate(name));
        }

        // ADDED FOR PS6
        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        ///
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        ///
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor with s => s.ToUpper() as the normalizer and a validator that
        /// checks that s is a valid cell name as defined in the AbstractSpreadsheet
        /// class comment.  There are then three possibilities:
        ///
        ///   (1) If the remainder of content cannot be parsed into a Formula, a
        ///       Formulas.FormulaFormatException is thrown.
        ///
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///
        ///   (3) Otherwise, the contents of the named cell becomes f.
        ///
        /// Otherwise, the contents of the named cell becomes content.
        ///
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        ///
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        /// </summary>
        public override ISet<string> SetContentsOfCell(string name, string content)
        {
            name = name?.ToUpper(); //all names of cells are to be normalized to uppercase when brought in; if its not null
            double number; // a varibale to hold the number if it can parse it to a double 

            if(content == null)
            {
                throw new ArgumentNullException();
            }

            if(name == null || !(IsCellNameValid(name)))
            {
                throw new InvalidNameException();
            }

            //if we can parse the string, then we know it is a double, therefore its contents and value is that double 
            if(double.TryParse(content, out number))
            {
                Changed = true;
                return SetCellContents(name, number);
            }
            else if(content.StartsWith("=")) //if the string starts with the '=' char, we know it is a formula
            {
                string replace = "";
                Regex equal = new Regex(@"(=)");
                string formula = equal.Replace(content, replace); //get the new string with the '=' char ommited, to create the formula for the cells contents

                try
                {
                    Changed = true;
                    return SetCellContents(name, new Formula(formula, s => s.ToUpper(), IsCellNameValid));
                }
                catch(FormulaFormatException)
                {
                    Changed = true;
                    throw new FormulaFormatException("Sorry, invalid formula format");
                }
                catch(CircularException)
                {
                    Changed = true;
                    throw new CircularException();
                }
            }
            else
            {
                Changed = true;
                return SetCellContents(name, content);
            }
        }

        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            //if the name provided is null, then throw the exception
            if(name == null)
            {
                throw new ArgumentNullException();
            }

            //if the name provided doesn't match what is exceptable for a cell name, then throw the exception
            if(!(IsCellNameValid(name)))
            {
                throw new InvalidNameException();
            }
       
            return dg.GetDependees(name);
        }

        /// <summary>
        /// Helper method that takes a possible name for a cell and determines if it is a valid name 
        /// A valid cell name is one or more letters, followed by one non zero digit, followed
        /// by zero or more digits. This method uses a regular expression to determine this. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool IsCellNameValid(string name)
        {
            //uses two regular expressions to evaluate if a cell name is valid or not
            return Regex.IsMatch(name.ToUpper(), "^([a-zA-Z]+[1-9]{1}[0-9]*)$")  && IsValid.IsMatch(name);
        }
    }
}
