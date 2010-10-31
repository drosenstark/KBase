/*
This file is part of TheKBase Desktop
A Multi-Hierarchical  Information Manager
Copyright (C) 2004-2010 Daniel Rosenstark

TheKBase Desktop is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
("GPL") version 2 as published by the Free Software Foundation.
See the file LICENSE.TXT for the full text of the GNU GPL, or see
http://www.gnu.org/licenses/gpl.txt

For using TheKBase Desktop with software that can not be combined with 
the GNU GPL or any other queries, please contact Daniel Rosenstark 
(license@thekbase.com).
*/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Kbase.SnippetTreeView;
using System.Diagnostics;
using Kbase.LibraryWrap;

namespace Kbase.MainFrm
{
	/// <summary>
	/// Summary description for TestForm.
	/// </summary>
	public class TestForm : System.Windows.Forms.Form
	{
        private System.Windows.Forms.Button button3;
        private Button buttonSaveOnServer;
        private Button buttonTest;
        private Button buttonRefresh;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public TestForm()
		{

            //
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			Universe.Instance.testForm = this;

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.button3 = new System.Windows.Forms.Button();
            this.buttonSaveOnServer = new System.Windows.Forms.Button();
            this.buttonTest = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(16, 16);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(128, 23);
            this.button3.TabIndex = 3;
            this.button3.Text = "Show Info";
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // buttonSaveOnServer
            // 
            this.buttonSaveOnServer.Location = new System.Drawing.Point(160, 16);
            this.buttonSaveOnServer.Name = "buttonSaveOnServer";
            this.buttonSaveOnServer.Size = new System.Drawing.Size(128, 23);
            this.buttonSaveOnServer.TabIndex = 4;
            this.buttonSaveOnServer.Text = "Test1";
            this.buttonSaveOnServer.Click += new System.EventHandler(this.buttonSaveOnServer_Click);
            // 
            // buttonTest
            // 
            this.buttonTest.Location = new System.Drawing.Point(305, 16);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(128, 23);
            this.buttonTest.TabIndex = 5;
            this.buttonTest.Text = "Test2";
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Location = new System.Drawing.Point(460, 16);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(128, 23);
            this.buttonRefresh.TabIndex = 6;
            this.buttonRefresh.Text = "Test3";
            // 
            // TestForm
            // 
            this.ClientSize = new System.Drawing.Size(644, 54);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.buttonTest);
            this.Controls.Add(this.buttonSaveOnServer);
            this.Controls.Add(this.button3);
            this.Name = "TestForm";
            this.Text = "TestForm";
            this.ResumeLayout(false);

		}
		#endregion


		private string rtf = null;
		private void button2_Click(object sender, System.EventArgs e)
		{
			rtf = Universe.Instance.detailPane.Rtf;
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			CountNodes();
			//Kbase.Logging.DebugForm.GetInstance();
		}

		public void CountNodes() {
			try 
			{
				Logger.Log("*************** Snippet Pane Info ***********");
				totalCount = 0;
				GetInfo(Universe.Instance.snippetPane.Nodes, 1);
				Universe.Instance.ModelGateway.PrintInfo();
                Logger.Log("Total Nodes Visible on Snippet Pane " + totalCount);
			} 
			catch (Exception ex) {
				MainForm.ShowError(ex);
			}
		}
		
		int totalCount;

		void GetInfo(TreeNodeCollection nodes, int depth) {
			foreach (TreeNode node in nodes) {
				totalCount++;
				GetInfo(node.Nodes,depth+1);
			}
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
		}

		private void back_Click(object sender, System.EventArgs e)
		{
			Universe.Instance.snippetPane.NavigateBackward();
		}

		private void forward_Click(object sender, System.EventArgs e)
		{
			Universe.Instance.snippetPane.NavigateForward();
		}

		private void listBox1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		}

        private void buttonSaveOnServer_Click(object sender, EventArgs e)
        {
            //Kbase.ServerInMemory.ServerFactory.Write();
            //MessageBox.Show(Kbase.ServerInMemory.ServerFactory.GetInfo());
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            //Kbase.Server.Server server = Kbase.ServerInMemory.ServerFactory.GetServer();
            //if (testId == -1)
            //testId = server.GetTopLevelSnippet().Id;
            //Kbase.Server.PSnippet snip = server.MakeNewSnippet();
            //server.AddChild(testId, snip.Id);
            //snip.Title = "blah hoooooooo!";
            //server.UpdateSnippet(snip);
            //testId = snip.Id;
        }




	}
}
