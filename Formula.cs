// Skeleton written by Joe Zachary for CS 3500, January 2015
// Revised by Joe Zachary, January 2016
// JLZ Repaired pair of mistakes, January 23, 2016

//Skeleton Code implemented by Ryan Williams 
//u0931022

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// this namespace represents a formula that the spreadsheet will use. It determins if a formula is valid or not, and 
/// can also use evaluate() to compute the given formula. 
/// </summary>
namespace Formulas
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  Provides a means to evaluate Formulas.  Formulas can be composed of
    /// non-negative floating-point numbers, variables, left and right parentheses, and
    /// the four binary operator symbols +, -, *, and /.  (The unary operators + and -
    /// are not allowed.)
    /// </summary>
    public struct Formula
    {

        /// <summary>
        /// a formula is composed of multiple tokens, these tokens are stored here
        /// </summary>
        private IEnumerable<string> tokens;
        
        /// <summary>
        /// Creates a Formula from a string that consists of a standard infix expression composed
        /// from non-negative floating-point numbers (using C#-like syntax for double/int literals), 
        /// variable symbols (a letter followed by zero or more letters and/or digits), left and right
        /// parentheses, and the four binary operator symbols +, -, *, and /.  White space is
        /// permitted between tokens, but is not required.
        /// 
        /// Examples of a valid parameter to this constructor are:
        ///     "2.5e9 + x5 / 17"
        ///     "(5 * 2) + 8"
        ///     "x*y-2+35/9"
        ///     
        /// Examples of invalid parameters are:
        ///     "_"
        ///     "-5.3"
        ///     "2 5 + 3"
        /// 
        /// Calls the three parameter constructor
        /// </summary>
        public Formula(string formula = "0") : this (formula, s => s, s => true)
        {
            
        }

        /// <summary>
        /// Creates a formula from a sting that gets normalized, and then validated. 
        /// For one, the formula passed in needs to be valid. If it is, we can then 
        /// attempt to normalize it using the normalizer method. Once the token has been
        /// normalized we then need to check that it is valid. If either of these fail
        /// the formula cannot be properly formated.
        /// </summary>
        /// <param name="formula"></param> the string representing the formula
        /// <param name="normalizer"></param> delegate method that normalizeds a string to canonical form
        /// <param name="validator"></param> delegate method that checks if a normalized string is valid
        public Formula(string formula, Normalizer normalizer, Validator validator)
        {
            formula = formula ?? "0"; //if formula is null, initialize it to the string "0"
            int leftparen = 0;
            int rightparen = 0;
            double value;
            string previous_token = null;
            string normalizedFormula = null;

            //check to make sure we dont make a null formula
            if(formula == null)
            {
                throw new FormulaFormatException("Sorry, can't use null as a formula");
            }
            
            //our formula is comprised of tokens, we store them here 
            tokens = Formula.GetTokens(formula);


            //check to make sure the formula has at least one token in it
            if(!(tokens.Count() >= 1))
            {
                throw new FormulaFormatException("Sorry, this is an invalid formula");
            }

            //check to make sure the first token is either a number, a variable, or a opening paren; if not we throw the exception
            if(!(double.TryParse(tokens.ElementAt(0), out value)) && !(Regex.IsMatch(tokens.ElementAt(0), @"[a-zA-Z][0-9a-zA-Z]*")) && !(tokens.ElementAt(0) == "("))
            {
                throw new FormulaFormatException("Sorry, the first element in the formula is not a valid token");
            }

            //check to make sure the last token is either a number, a variable, or a closing paren; if not we throw the exception
            if(!(double.TryParse(tokens.ElementAt(tokens.Count() - 1), out value)) && !(Regex.IsMatch(tokens.ElementAt(tokens.Count() - 1), @"[a-zA-Z][0-9a-zA-Z]*")) && !(tokens.ElementAt(tokens.Count() - 1) == ")"))
            {
                throw new FormulaFormatException("Sorry, the last element in the formula is not a valid token");
            }

            foreach(string token in tokens)
            {
                //check to make sure none of the tokens are invalid
                if(!(double.TryParse(token, out value)) && !(Regex.IsMatch(token, @"[a-zA-Z][0-9a-zA-Z]*")) && token != "(" && token != ")" && token != "+" && token != "-" && token != "*" && token != "/")
                {
                    throw new FormulaFormatException("Sorry, this is an invalid formula");
                }

                //if at anytime we have more right parens than left as we read the tokens, we know we have an invalid formula
                if(rightparen > leftparen)
                {
                    throw new FormulaFormatException("Sorry this is an invalid formula: Detected too many closing parens to open parens");
                }

                //check to make sure that tokens following an open paren or an operator are valid
                if(Regex.IsMatch(token, @"[a-zA-Z][0-9a-zA-Z]*") || double.TryParse(token, out value) || token == "(")
                {
                    if (previous_token != "+" && previous_token != "-" && previous_token != "*" && previous_token != "/" && previous_token != "(" && previous_token != null)
                    {
                        throw new FormulaFormatException("Sorry invalid format");
                    }
                }

                //check to make sure that tokens following a number, a variable, or a closing paren are valid
                if(token == "+" || token == "-" || token == "/" || token == "*" || token == ")")
                {
                    if (!(Regex.IsMatch(previous_token, @"[a-zA-Z][0-9a-zA-Z]*")) && !(double.TryParse(previous_token, out value)) && !(previous_token == ")"))
                    {
                        throw new FormulaFormatException("Sorry, invalid format");
                    }
                }

                if (token == "(")
                {
                    leftparen++;
                }

                if (token == ")")
                {
                    rightparen++;
                }

                previous_token = token; //used to check the previous token, to make sure it follows the proper token
            }

            //check for a matched pair of parens, a complete formula has the same amount of left and right parens
            if (leftparen != rightparen)
            {
                throw new FormulaFormatException("Sorry this is an invalid formula: Number of open parans doesn't equal number of closing parans");
            }

            //now that we know it is a valid formula, we now try to normalize it
            foreach (string token in tokens)
            {
                if (Regex.IsMatch(token, @"^[a-zA-Z][0-9a-zA-Z]*$"))
                {
                    //we need a try-catch block just incase the normalizer throws an exception
                    try
                    {
                        if(normalizer(token) != null && validator(normalizer(token)))
                        {
                            normalizedFormula += normalizer(token);
                        }
                        else
                        {
                            throw new FormulaFormatException("Sorry this is an invalid formula: can't normalize the variable");
                        }
                    }
                    catch (Exception e)
                    {
                        throw new FormulaFormatException("Sorry this is an invalid formula: can't normalize the variable");
                    }
                }
                else
                {
                    normalizedFormula += token;
                }
            }

            //re-populate the tokens IEnumerable with the new normalized version, if it could normalize it
            tokens = Formula.GetTokens(normalizedFormula);
        }

        /// <summary>
        /// Evaluates this Formula, using the Lookup delegate to determine the values of variables.  (The
        /// delegate takes a variable name as a parameter and returns its value (if it has one) or throws
        /// an UndefinedVariableException (otherwise).  Uses the standard precedence rules when doing the evaluation.
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, its value is returned.  Otherwise, throws a FormulaEvaluationException  
        /// with an explanatory Message.
        /// </summary>
        public double Evaluate(Lookup lookup)
        {
            Stack<double> value = new Stack<double>();
            Stack<string> op = new Stack<string>();
            double temp1;
            double temp2;

            double number;//double just used for TryParse() method

            if(tokens == null)
            {
                return 0;
            }

            foreach(String token in tokens)
            {
                //if token is a double 
                if (double.TryParse(token, out number))
                {
                    if (op.Count() != 0) //make sure we don't try to pop or peek on any empty stack
                    {
                        if (op.Peek() == "*")
                        {
                            op.Pop();
                            value.Push(number * value.Pop());
                        }
                        else if (op.Peek() == "/")
                        {
                            op.Pop();

                            //make sure we catch any divide by zero exceptions
                            if(number == 0)
                            {
                                throw new FormulaEvaluationException("Sorry, can't divide by zero");
                            }


                            value.Push(value.Pop() / number);
                        }
                        else
                        {
                            value.Push(number);
                        }
                    }
                    else
                    {
                        value.Push(number);
                    }
                }
                else if (Regex.IsMatch(token, @"^[a-zA-Z][0-9a-zA-Z]*$")) //if token is a variable
                {
                    double variable;

                    //test to make sure the variable is defined by the method we pass in
                    try
                    {
                        variable = lookup.Invoke(token);
                    }
                    catch (UndefinedVariableException) //if its not, we cannot compute because we don't have values for all variables
                    {
                        throw new FormulaEvaluationException("Sorry, this variable is undefined.");
                    }

                    if (op.Count() != 0) //make sure we dont pop or peek from an empty stack
                    {
                        if(op.Peek() == "*")
                        {
                            op.Pop();
                            value.Push(lookup(token) * value.Pop());
                        }
                        else if (op.Peek() == "/")
                        {
                            op.Pop();

                            if (variable == 0) //make sure we dont divide by zero, if we do; throw an exception 
                            {
                                throw new FormulaEvaluationException("Sorry, we cannot divide by zero");
                            }

                            value.Push(value.Pop() / variable);
                        }
                        else
                        {
                            value.Push(lookup(token));
                        }
                    }
                    else
                    {
                        value.Push(lookup(token));
                    }
                }
                else if(token == "+" || token == "-") //if token is an operator (+ or -)
                {
                    if(op.Count() != 0)//make sure we don't try peeking or popping an empty stack
                    {
                        if(op.Peek() == "+")
                        {
                            op.Pop();
                            value.Push(value.Pop() + value.Pop());
                            op.Push(token);
                        }
                        else if(op.Peek() == "-")
                        {
                            op.Pop();
                            temp1 = value.Pop();
                            temp2 = value.Pop();

                            value.Push(temp2 - temp1);
                            op.Push(token);
                        }
                        else
                        {
                            op.Push(token);
                        }
                    }
                    else
                    {
                        op.Push(token);
                    }
                }
                else if(token == "*" || token == "/") //if token is an operator (* or /)
                {
                    op.Push(token);
                }

                //if token is an open paren
                if (token == "(")
                {
                    op.Push(token);
                }
                else if(token == ")") //if token is a closing paren
                {
                    if(op.Count() != 0) //check to make sure the stack isnt empty
                    {
                        if (op.Peek() == "+")
                        {
                            op.Pop();
                            value.Push(value.Pop() + value.Pop());
                        }
                        else if (op.Peek() == "-")
                        {
                            op.Pop();
                            temp1 = value.Pop();
                            temp2 = value.Pop();
                            value.Push(temp2 - temp1);
                        }
                    }
                    
                    op.Pop();


                    if(op.Count() != 0) //make sure stack isn't empty
                    {
                        if (op.Peek() == "*")
                        {
                            op.Pop();
                            value.Push(value.Pop() * value.Pop());
                        }
                        else if (op.Peek() == "/")
                        {
                            op.Pop();
                            temp1 = value.Pop();
                            temp2 = value.Pop();
                            if (temp1 == 0) //make sure we don't try to divide by zero
                            {
                                throw new FormulaEvaluationException("Sorry, we cannot divide by zero.");
                            }

                            value.Push(temp2 / temp1);
                        }
                    }
                }
            }

            if(op.Count == 0)
            {
                return value.Pop();
            }
            else
            {
               if(op.Peek() == "+")
               {
                   return value.Pop() + value.Pop();
               }
               else
               {
                    temp1 = value.Pop();
                    temp2 = value.Pop();
                    return temp2 - temp1;
               } 
            }
        }

        /// <summary>
        /// Given a formula, enumerates the tokens that compose it.  Tokens are left paren,
        /// right paren, one of the four operator symbols, a string consisting of a letter followed by
        /// zero or more digits and/or letters, a double literal, and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z][0-9a-zA-Z]*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: e[\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if(!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }

        /// <summary>
        /// This method finds all the strings in the formula, 
        /// places them all in a ISet<string> and then returns it 
        /// </summary>
        /// <returns></returns>
        public ISet<string> GetVariables()
        {
            HashSet<string> setOfVariables = new HashSet<string>();

            //loop through the formula looking for variables 
            foreach(string token in tokens)
            {
                if(Regex.IsMatch(token, @"^[a-zA-Z][0-9a-zA-Z]*$"))
                {
                    setOfVariables.Add(token);
                }
            }

            return setOfVariables; //return the ISet<string> with the variables, if there are non it returns an empty Set
        }

        /// <summary>
        /// This ToString() method overrides the current built-in ToString()
        /// method. Here, we write it to return a string version of our formula
        /// in its normalized form
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if(tokens == null)
            {
                return "0";
            }

            string tempString = "";

            foreach(string token in tokens)
            {
                tempString += token;
            }

            return tempString; 
        }

    }

    /// <summary>
    /// A Lookup method is one that maps some strings to double values.  Given a string,
    /// such a function can either return a double (meaning that the string maps to the
    /// double) or throw an UndefinedVariableException (meaning that the string is unmapped 
    /// to a value. Exactly how a Lookup method decides which strings map to doubles and which
    /// don't is up to the implementation of the method.
    /// </summary>
    public delegate double Lookup(string s);

    /// <summary>
    /// A Normalizer method is one that normalizes a variable by putting it into
    /// canonical form. If it can't, an exception will be thrown. Exaclty how it 
    /// normalizes it is completely up to the implementation of the method. 
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public delegate string Normalizer(string s);

    /// <summary>
    /// A Validator method is one that validates a variable after it has been 
    /// normalized. If it is valid, it returns true. Otherwise, it returns false. 
    /// The validity of the normalized variable is completely up to the implementation
    /// of the method. 
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public delegate bool Validator(string s);

    /// <summary>
    /// Used to report that a Lookup delegate is unable to determine the value
    /// of a variable.
    /// </summary>
    public class UndefinedVariableException : Exception
    {
        /// <summary>
        /// Constructs an UndefinedVariableException containing whose message is the
        /// undefined variable.
        /// </summary>
        /// <param name="variable"></param>
        public UndefinedVariableException(String variable)
            : base(variable)
        {
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the parameter to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>  
        public FormulaFormatException(String message) : base(message)
        {
        }
    }

    /// <summary>
    /// Used to report errors that occur when evaluating a Formula.
    /// </summary>
    public class FormulaEvaluationException : Exception
    {
        /// <summary>
        /// Constructs a FormulaEvaluationException containing the explanatory message.
        /// </summary>
        public FormulaEvaluationException(String message) : base(message)
        {
        }
    }
}