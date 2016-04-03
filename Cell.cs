using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS
{
    /// <summary>
    /// The cell class represents a Cell within the spreadsheet
    /// . A spreadsheet contains infinetly many cells, and each 
    /// cell contains two pieces of vital information: the cells
    /// contents and the value of those contents. Contents may be 
    /// a double, a string, or a formula object. If it is a double
    /// or a string, the value is that double or string. However if 
    /// the contents is a Formula, then value of that formula is either
    /// a double, or a Formula Error. 
    /// </summary>
    class Cell
    {
        /// <summary>
        /// Represents the contents of a cell within the spreadsheet. 
        /// Again, its contents can be a double, a string, or a Formula
        /// </summary>
        internal Object contents { get; set;}

        /// <summary>
        /// Represents the value of a cell. If it is a string, the value is that 
        /// string or double. If its a Formula, that is valid, and where all 
        /// variables are defined, then the value is a double. Otherwise it's
        /// value is a Formula Error Object
        /// </summary>
        internal Object value { get; set; }

        /// <summary>
        /// Constructor for a cell that sets both the contents and the 
        /// value for a cell within the spreadsheet. In spreadsheet, the 
        /// contents and the value are both set each time a cell is added. 
        /// The only time a value can change, however, is if the content is
        /// a formula and its value is dependent on other cells. 
        /// </summary>
        /// <param name="Contents"></param>
        /// <param name="Value"></param>
        internal Cell(Object Contents = null, Object Value = null)
        {
            contents = Contents;
            value = Value;
        }
    }
}
