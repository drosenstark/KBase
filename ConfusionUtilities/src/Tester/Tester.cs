/*
This file is part of Confusion Utilities
Copyright (C) 2004-2007 Daniel Rosenstark
license@confusionists.com
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ConfusionUtilities.Tester
{
    public partial class Tester : Form
    {
        public Tester()
        {
            InitializeComponent();
        }

        /// <summary>
        /// run before each test
        /// </summary>
        public virtual void Setup() { 
        
        }

        /// <summary>
        ///  run after each test
        /// </summary>
        public virtual void Teardown() { 
        
        
        }

        private void Tester_Load(object sender, EventArgs e)
        {
            Setup();
            List<MethodWrapper> methodInfoWrappers = new List<MethodWrapper>();
            Type thisType = Type.GetTypeFromHandle(Type.GetTypeHandle(this));
            foreach (MethodInfo m in thisType.GetMethods()) {
                MethodWrapper wrapper = new MethodWrapper(m);
                if (wrapper.IsTest())
                    methodInfoWrappers.Add(wrapper);
            }
            methodInfoWrappers.Sort();
            comboBoxTests.Items.Add("*** TEST ALL ***");
            comboBoxTests.Items.AddRange(methodInfoWrappers.ToArray());            
        }

        void Print(IDbCommand command)
        {
            IDataReader reader = command.ExecuteReader();
            DumpReader(reader);
        }

        public void PrintToGrid(IDbCommand commandGeneric)
        {
            //SqlCommand command = null;
            //if (!(commandGeneric is SqlCommand))
            //    return;
            //command = commandGeneric as SqlCommand;
            //SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

            //// Populate a new data table and bind it to the BindingSource.
            //DataTable table = new DataTable();
            //table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            //dataAdapter.Fill(table);
            //BindingSource dbBindSource = new BindingSource();

            //dbBindSource.DataSource = table;

            //// Resize the DataGridView columns to fit the newly loaded content.
            //dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
            //// you can make it grid readonly.
            //dataGridView1.ReadOnly = true;
            //// finally bind the data to the grid
            //dataGridView1.DataSource = dbBindSource;

        }


        public void DumpReader(System.Data.IDataReader reader)
        {
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    try
                    {
                        Print(reader.GetValue(i) + ", ");
                    }
                    catch (Exception ex)
                    {
                        Print(ex.Message);
                    }
                }
                PrintLine("");
            }
            reader.Close();
        }

        public void Assert(string description, bool condition) {
            string result = null;
            if (condition)
                result = "PASSED";
            else
                result = "FAILED";
             PrintLine(result + " " + description);
             success = success & condition;
        }

        public void Fail() {
            success = false;
        }

        public void PrintLine(object o)
        {
            PrintLine(o.ToString());
        }

        public void Print(object o)
        {
            PrintLine(o.ToString());
        }

        public TreeNode PrintLine(string text) {
            TreeNode retVal = null;
            if (currentTest != null)
            {
                retVal = PrintLine(text, currentTest.Nodes);
                if (!currentTest.IsExpanded)
                    currentTest.Expand();
            }
            else
            {
                retVal = PrintLine(text, treeViewOutput.Nodes);
            }
            return retVal;
        }

        TreeNode PrintLine(string text, TreeNodeCollection where)
        {
            TreeNode retVal = null;
            if (text.Contains("\r\n") || text.Contains("\n"))
            {
                text = text.Replace("\r\n", "\n");
                string[] texts = text.Split('\n');
                foreach (string textLine in texts)
                {
                    retVal = PrintLine(textLine, where);
                }
            }
            else
            {
                TreeNode node = new TreeNode(text);
                where.Add(node);
                retVal = node;
            }
            return retVal;
        }

        public void PrintLine(string message, object multiLineMessage) {
            PrintLine(message, multiLineMessage.ToString());
        }


        public void PrintLine(string message, string multiLineMessage)
        {
            TreeNode messageNode = PrintLine(message);
            Debug.WriteLine(message);
            PrintLine(multiLineMessage, messageNode.Nodes);
            Debug.WriteLine(multiLineMessage);
        }

        public void PrintLine(Exception ex)
        {
            string message = Type.GetTypeFromHandle(Type.GetTypeHandle(ex)).Name + " - " + ex.Message.Replace("\r\n", " ");
            PrintLine(message, ex.StackTrace);
            if (ex.InnerException != null)
                PrintLine(ex.InnerException);
        }

        TreeNode currentTest = null;
        bool success = false;

        private void buttonGo_Click(object sender, EventArgs e)
        {
            object o = comboBoxTests.SelectedItem;
            if (o is MethodWrapper)
            {
                MethodWrapper method = o as MethodWrapper;
                Test(method);
            }
            else {
                PerformAllTests();
            }
        }

        public void PerformAllTests()
        {
            // it's the all one
            foreach (object item in comboBoxTests.Items)
            {
                if (item is MethodWrapper)
                {
                    MethodWrapper method = item as MethodWrapper;
                    Test(method);
                }
            }
        }

        private void Test(MethodWrapper method)
        {
            this.Invoke(new MethodWrapperEventHandler(new MethodWrapperEventHandler(TestInner)),method);
        }

        private void TestInner(MethodWrapper method) {
            treeViewOutput.CollapseAll();
            treeViewOutput.BeginUpdate();
            currentTest = new TreeNode(method.ToString());
            treeViewOutput.Nodes.Add(currentTest);
            success = true;
            try
            {
                Setup();
                method.MethodInfo.Invoke(this, null);
                Teardown();
            }
            catch (Exception ex)
            {
                success = false;
                if (ex.InnerException != null)
                    PrintLine(ex.InnerException);
                else
                    PrintLine(ex);
            }
            if (success)
            {
                currentTest.Text += " PASSED";
                currentTest.BackColor = Color.LightGreen;
            }
            else
            {
                currentTest.Text += " FAILED";
                currentTest.BackColor = Color.LightSalmon;
            }
            treeViewOutput.EndUpdate();
        }


        private void Tester_Resize(object sender, EventArgs e)
        {
            buttonGo.Top = comboBoxTests.Top;
            // figure out button width
            buttonGo.Left = ClientSize.Width - buttonGo.Width - 5;
            comboBoxTests.Left = 5;
            comboBoxTests.Width = buttonGo.Left - 10;
            treeViewOutput.Width = ClientSize.Width - 10;
            treeViewOutput.Left = 5;
            treeViewOutput.Height = ClientSize.Height - comboBoxTests.Height - 15;
        }



    }

    public delegate void MethodWrapperEventHandler(MethodWrapper wrapper);

}