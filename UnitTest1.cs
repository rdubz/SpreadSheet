using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetGUI;

namespace ControllerTester
{
    /// <summary>
    /// The purpose of this class is to test the controller and make
    /// sure it handles all interactions between the view and the model
    /// correctly. It uses a stub (a fake GUI) to test the controller. 
    /// By doing this, we open up a new way to test the GUI form for 
    /// the spreadsheet. 
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// a test to make sure NewOpen gets called when the user hits the new file menu button
        /// </summary>
        [TestMethod]
        public void FireOpenEvent()
        {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);

            stub.FireOpenEvent();

            Assert.IsTrue(stub.CalledNewOpen);
        }

        /// <summary>
        /// a test to make sure DoClose() gets called from the controller
        /// </summary>
        [TestMethod]
        public void FireCloseEvent()
        {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);

            stub.FireCloseEvent();

            Assert.IsTrue(stub.CalledDoClose);
        }

        /// <summary>
        /// a tet to make sure the appopriate message gets sent to the message 
        /// box in the form when the help file menu strip item is hit
        /// </summary>
        [TestMethod]
        public void FireMessage()
        {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);

            stub.FireHelpMessage();

            Assert.AreEqual("To edit a cell, click on it and then type its contents in the text box above, then press enter to set it", stub.Message);
        }

        /// <summary>
        /// a test to make sure all the cells get updated in a new form when the 
        /// user opens a new spreadsheet window 
        /// </summary>
        [TestMethod]
        public void FireFileChosenEvent()
        {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);

            stub.FireFileChosenEvent();

            Assert.AreEqual(true, stub.CalledUpdatePanel);
        }

        /// <summary>
        /// a method that test that the save event works right
        /// </summary>
        [TestMethod]
        public void FireSaveEvent()
        {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);

            stub.FireSaveEvent();
        }

        /// <summary>
        /// when the user puts contents into a cell
        /// the it gets added in the model, then the update 
        /// method gets called to change all dependent cells. 
        /// we check to make sure it gets called here. 
        /// </summary>
        [TestMethod]
        public void FireSetCellContentsEvent()
        {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);

            stub.FireSetContentsInSpreadsheetEvent("A1", "3");

            Assert.IsTrue(stub.CalledUpdatePanel);    
        }

        /// <summary>
        /// same test as above, excpet this time we need to make sure 
        /// the right message gets displayed when the user tries to input 
        /// an incorrect formula
        /// </summary>
        [TestMethod]
        public void FireSetCellContentsEvent2()
        {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);

            stub.FireSetContentsInSpreadsheetEvent("A1", "3");
            stub.FireSetContentsInSpreadsheetEvent("B1", "=(((9+A5 - )");

            Assert.AreEqual("Sorry: This is an invalid Formula", stub.Message);
        }

        /// <summary>
        /// same test as above, but we check the correct message gets displayed 
        /// when the user creates a circular depenedency
        /// </summary>
        [TestMethod]
        public void FireSetCellWithCircularDependency()
        {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);

            stub.FireSetContentsWithCircularDependency();

            Assert.AreEqual("Sorry: Circular Dependencies are not allowed in the spreadsheet", stub.Message);
        }

        /// <summary>
        /// a test that makes sure the checksaved event 
        /// gives the right value of the property that determines 
        /// a saved spreadsheet or not
        /// </summary>
        [TestMethod]
        public void FireCheckSaved()
        {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);

            Assert.AreEqual(false, stub.FireCheckSaved());

             
        }

        /// <summary>
        /// same test as above, just make sure the 
        /// value is different when the cell hasn't been saved
        /// since last changed
        /// </summary>
        [TestMethod]
        public void FireCheckSaved2()
        {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);
            stub.FireSetContentsInSpreadsheetEvent("A1", "3");

            Assert.AreEqual(true, stub.FireCheckSaved());

        }

        /// <summary>
        /// a test to make sure the updatevalue box method gets the correct 
        /// value from the model
        /// </summary>
        [TestMethod]
        public void FireUpdateValueBox()
        {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);

            //inside the stub this method sets the contents of cell A1 to 3
            stub.FireSetContentsInSpreadsheetEvent("A1", "3");
            Assert.AreEqual((double) 3, stub.FireUpdateValueBox("A1"));
        }

        /// <summary>
        /// same test as above, but for a different value 
        /// this needed to be done to hit another code path 
        /// in the controller
        /// </summary>
        [TestMethod]
        public void FireUpdateValueBox2()
        {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);

            //inside the stub this method sets the contents of cell A1 to 3
            stub.FireSetContentsInSpreadsheetEvent("A1", "3");
            Assert.AreEqual("", stub.FireUpdateValueBox("A9"));
        }

        /// <summary>
        /// same test as above, but for a different value 
        /// this needed to be done to hit another code path 
        /// in the controller
        /// </summary>
        [TestMethod]
        public void FireUpdateValueBox3()
        {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);

            //inside the stub this method sets the contents of cell A1 to 3
            stub.FireSetContentsInSpreadsheetEvent("A1", "3");
            Assert.AreEqual("", stub.FireUpdateValueBox("9A0"));
        }

        /// <summary>
        /// a test to make sure the updatecontentsbox method gets the correct 
        /// contents from the model
        /// </summary>
        [TestMethod]
        public void FireUpdateContentsBox()
        {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);

            //inside the stub this method sets the contents of cell A1 to 3
            stub.FireSetContentsInSpreadsheetEvent("A1", "=B1 + 3");
            Assert.AreEqual("=B1+3", stub.FireUpdateContentsBox("A1"));
        }

        /// <summary>
        /// same test as above, but for a different value, 
        /// this will hit a different code path in the controller to give
        /// better code coverage
        /// </summary>
        [TestMethod]
        public void FireUpdateContentsBox2()
        {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);

            stub.FireSetContentsInSpreadsheetEvent("A1", "=B1 + 3");
            stub.FireSetContentsInSpreadsheetEvent("A6", "two");
            Assert.AreEqual("two", stub.FireUpdateContentsBox("A6"));
        }

        /// <summary>
        /// same test as above, but for a different value, 
        /// this will hit a different code path in the controller to give
        /// better code coverage
        /// </summary>
        [TestMethod]
        public void FireUpdateContentsBox3()
        {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);

            stub.FireSetContentsInSpreadsheetEvent("A1", "=B1 + 3");
            stub.FireSetContentsInSpreadsheetEvent("A6", "two");
            Assert.AreEqual("", stub.FireUpdateContentsBox("B6"));
        }

        /// <summary>
        /// same test as above, but for a different value, 
        /// this will hit a different code path in the controller to give
        /// better code coverage
        /// </summary>
        [TestMethod]
        public void FireUpdateContentsBox4()
         {
            Form1Stub stub = new Form1Stub();
            Controller controller = new Controller(stub);

            stub.FireSetContentsInSpreadsheetEvent("A1", "=B1 + 3");
            stub.FireSetContentsInSpreadsheetEvent("A6", "two");
            Assert.AreEqual("", stub.FireUpdateContentsBox("9bd89"));
        }
    }
}
