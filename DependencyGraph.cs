// Skeleton implementation written by Joe Zachary for CS 3500, January 2015.
// Revised for CS 3500 by Joe Zachary, January 29, 2016

// Skeleton Code filled in by Ryan Williams 
//u0931022

using System;
using System.Linq;
using System.Collections.Generic;

namespace Dependencies
{

    /// <summary>
    /// A DependencyGraph can be modeled as a set of dependencies, where a dependency is an ordered 
    /// pair of strings.  Two dependencies (s1,t1) and (s2,t2) are considered equal if and only if 
    /// s1 equals s2 and t1 equals t2.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that the dependency (s,t) is in DG 
    ///    is called the dependents of s, which we will denote as dependents(s).
    ///        
    ///    (2) If t is a string, the set of all strings s such that the dependency (s,t) is in DG 
    ///    is called the dependees of t, which we will denote as dependees(t).
    ///    
    /// The notations dependents(s) and dependees(s) are used in the specification of the methods of this class.
    ///
    /// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    ///     dependents("a") = {"b", "c"}
    ///     dependents("b") = {"d"}
    ///     dependents("c") = {}
    ///     dependents("d") = {"d"}
    ///     dependees("a") = {}
    ///     dependees("b") = {"a"}
    ///     dependees("c") = {"a"}
    ///     dependees("d") = {"b", "d"}
    ///     
    /// All of the methods below require their string parameters to be non-null.  This means that 
    /// the behavior of the method is undefined when a string parameter is null.  
    ///
    /// IMPORTANT IMPLEMENTATION NOTE
    /// 
    /// The simplest way to describe a DependencyGraph and its methods is as a set of dependencies, 
    /// as discussed above.
    /// 
    /// However, physically representing a DependencyGraph as, say, a set of ordered pairs will not
    /// yield an acceptably efficient representation.  DO NOT USE SUCH A REPRESENTATION.
    /// 
    /// You'll need to be more clever than that.  Design a representation that is both easy to work
    /// with as well acceptably efficient according to the guidelines in the PS3 writeup. Some of
    /// the test cases with which you will be graded will create massive DependencyGraphs.  If you
    /// build an inefficient DependencyGraph this week, you will be regretting it for the next month.
    /// </summary>
    public class DependencyGraph
    {
        /// <summary>
        /// This dependents dictionary will hold a dependee for a key, and a list following this 
        /// key will contain all the dependents for that key. To maintain the functionality of 
        /// this graph, when this dictionary changes, the dependees dictionary must change as well
        /// </summary>
        private Dictionary<string, HashSet<string>> dependents;

        /// <summary>
        /// This dependees dictionary will hold dependents as keys, and 
        /// following that key will be all the dependess for that key. To maintain the functionality of
        /// this dependency graph, when this dictionary changes, the dependents dictionary changes as well
        /// </summary>
        private Dictionary<string, HashSet<string>> dependees;

        /// <summary>
        /// This global varialbe will hold represent the number of dependencies in the
        /// Dependency graph. Everytime we add a dependency we increment it, and everytime
        /// we remove a dependency we decrement it. 
        /// </summary>
        private int size;

        /// <summary>
        /// Creates a DependencyGraph containing no dependencies.
        /// </summary>
        public DependencyGraph()
        {
            dependents = new Dictionary<string, HashSet<string>>();
            dependees = new Dictionary<string, HashSet<string>>();
            size = 0;
        }

        /// <summary>
        /// One parameter constructor that takes a currently 
        /// existing dependency graph and creates the same dependency graph 
        /// with out references pointing to the same object (so when you change
        /// one it doesn't change the other). 
        /// </summary>
        /// <param name="dg"></param>
        public DependencyGraph(DependencyGraph dg)
        {
            //make sure the dependencie graph we passed in was actually created
            if(dg == null)
            {
                throw new ArgumentNullException();
            }

            //we must create new dictionaries before we start adding dependencies from the old dependency graph that was passed in
            dependents = new Dictionary<string, HashSet<string>>();
            dependees = new Dictionary<string, HashSet<string>>();
            size = 0;

            //populate our dictionaries by adding dependencies from the graph that we passed in
            foreach(string dependee in dg.dependents.Keys)
            {
                foreach(string s in dg.GetDependents(dependee))
                {
                    AddDependency(dependee, s);
                }
            }
        }

        /// <summary>
        /// The number of dependencies in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get { return size; }
        }

        /// <summary>
        /// Reports whether dependents(s) is non-empty.  Requires s != null.
        /// If it is null, throw an ArgumentNullException
        /// </summary>
        public bool HasDependents(string s)
        {
            //first check to make sure a null string wasn't passed in. 
            //if they do, and because the method is undefined if it is, I just return false
            if(s == null)
            {
                throw new ArgumentNullException();
            }

            if(dependents.ContainsKey(s))
            {
                if(dependents[s].Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Reports whether dependees(s) is non-empty.  Requires s != null.
        /// If it is null, throw an ArgumentNullException
        /// </summary>
        public bool HasDependees(string s)
        {
            //first check to make sure a null string wasn't passed in. 
            //if they do, and because the method is undefined if it is, I just return false
            if(s == null)
            {
                throw new ArgumentNullException();
            }

            //check to see if there is such a dependent
            if(dependees.ContainsKey(s))
            {
                if(dependees[s].Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else //if not, we return false, saying it't not existent
            {
                return false;
            }
        }

        /// <summary>
        /// Enumerates dependents(s).  Requires s != null.
        /// If it is null, throw an ArgumentNullException
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            //check to see if null gets passed 
            //if it does, and because the function is undefined if so, I just retrun an empty list
            if(s == null)
            {
                throw new ArgumentNullException();
            }

            if(dependents.ContainsKey(s)) //if the string is existent in the dependents dictionary, then it has dependents, this is due to the way I add dependencies 
            {
                return dependents[s];
            }

            return new HashSet<string>(); //if s doesn't have any dependents, then return an empty list
        }

        /// <summary>
        /// Enumerates dependees(s).  Requires s != null. 
        /// If it is null, throw an ArgumentNullException
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            //check to see if null gets passed 
            //if it does, and because the function is undefined if so, I just retrun an empty list
            if(s == null)
            {
                throw new ArgumentNullException();
            }

            if(dependees.ContainsKey(s)) //if the string is existent in the dependees dictionary, then it has dependees. Again, this is due to the way I add dependencies
            {
                return dependees[s];
            }

            return new HashSet<string>(); //if s doesn't have any dependees, then return an empty list
        }

        /// <summary>
        /// Adds the dependency (s,t) to this DependencyGraph.
        /// This has no effect if (s,t) already belongs to this DependencyGraph.
        /// Requires s != null and t != null.
        /// If either is null, throw an ArgumentNullException
        /// </summary>
        public void AddDependency(string s, string t)
        {
            //first check to see if either s or t are null. If just one of them is
            //, and becuase the method is undefined here, I just return and do nothing
            if(s == null || t == null)
            {
                throw new ArgumentNullException();
            }

            //check to see if the dependency is already in our graph 
            if(dependents.ContainsKey(s))
            { 
                //if it is, then we just return to ensure we do nothing, because we don't want to add an already existing dependencies. They must stay unique
                if(dependents[s].Contains(t))
                {
                    return;
                }
            }

            //check to see if we already have a key with this string 
            //if we don't, we create a new list to hold all the dependent data for that string (I add it to the list at the end of the method)
            if(!(dependents.ContainsKey(s)))
            {
                dependents.Add(s, new HashSet<string>());
            }

            //same as above, but for dependees. Notice the t and the s are swapped, this is because if a is a dependent of b, then b is a dependee of a
            if(!(dependees.ContainsKey(t)))
            {
                dependees.Add(t, new HashSet<string>());
                
            }

            dependents[s].Add(t);
            dependees[t].Add(s);

            size++;
        }

        /// <summary>
        /// Removes the dependency (s,t) from this DependencyGraph.
        /// Does nothing if (s,t) doesn't belong to this DependencyGraph.
        /// Requires s != null and t != null.
        /// If either is null, throw an ArgumentNullException
        /// </summary>
        public void RemoveDependency(string s, string t)
        {
            //first check to see if either s or t are null. If just one of them is
            //null, and becuase the method is undefined here, I just return and do nothing
            if (s == null || t == null)
            {
                throw new ArgumentNullException();
            }

            //check to see if we have such a pair in our graph
            if(dependents.ContainsKey(s))
            {
                if(dependents[s].Contains(t))
                {
                    //see if we can remove the key, or if we can only remove just the value from the list at that key
                    if (dependents[s].Count == 1) 
                    {
                        dependents.Remove(s);
                    }
                    else
                    {
                        dependents[s].Remove(t);
                    }

                    //same thing as above, but this time for dependees
                    if (dependees[t].Count == 1)
                    {
                        dependees.Remove(t);
                    }
                    else
                    {
                        dependees[t].Remove(s);
                    }

                    size--;
                }
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (s,r).  Then, for each
        /// t in newDependents, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// If either is null, throw an ArgumentNullException
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            //check to see if the dependent of s that gets passed in is null
            //because the method is undefined if it is null, I just return from the method and do nothing
            if(s == null)
            {
                throw new ArgumentNullException();
            }

            //check to see if any of the new dependents passed in is null
            //because the method is undefined if just one is null, I just return from the method and do nothing
            foreach (string t in newDependents)
            {
                if(t == null)
                {
                    throw new ArgumentNullException();
                }
            }

            //first check to see if s is already a dependent. If not, we need to add it; otherwise we replace it
            if(dependents.ContainsKey(s))
            {

                foreach(string t in dependents[s].ToList<string>())
                {
                    RemoveDependency(s, t);
        }

                foreach(string t in newDependents)
                {
                    AddDependency(s, t); //now add all the new dependents
                }
            }
            else
            {
                //if s isn't already a dependent, we make it one here, and give it all of its dependents
                foreach(string t in newDependents)
                {
                    AddDependency(s, t);
                }
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (r,t).  Then, for each 
        /// s in newDependees, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// If either is null, throw an ArgumentNullException
        /// </summary>
        public void ReplaceDependees(string t, IEnumerable<string> newDependees)
        {
            //check to see if the dependent of s that gets passed in is null
            //because the method is undefined if it is null, I just return from the method and do nothing
            if(t == null)
            {
                throw new ArgumentNullException();
            }

            //check to see if any of the new dependees passed in is null
            //because the method is undefined if just one is null, I just return from the method and do nothing
            foreach(string s in newDependees)
            {
                if(s == null)
                {
                    throw new ArgumentNullException();
                }
            }

            //check to see if t is already a dependee. If it is, we replace all the dependecies of the current form that have that dependee with the new ones
            if (dependees.ContainsKey(t))
            {
                foreach(string s in dependees[t].ToList<string>())
                {
                    RemoveDependency(s, t); //removea all the pairs of the form (r,t)
                }

                foreach(string s in newDependees)
                {
                    AddDependency(s, t); //now make all the new dependencies with our new list we are given, with the form (s,t)
                }
            }
            else
            {
                //if t isn't already a dependee in our graph, we add it here
                foreach(string s in newDependees)
                {
                    AddDependency(s, t);
                }
            }
        }
    }
}
