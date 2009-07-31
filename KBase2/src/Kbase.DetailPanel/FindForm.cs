/*
This file is part of TheKBase Desktop
A Multi-Hierarchical  Information Manager
Copyright (C) 2004-2007 Daniel Rosenstark

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
using Kbase.MainFrm;
using Kbase.LibraryWrap;

namespace Kbase.DetailPanel
{
	/// <summary>
	/// For finding in the DetailPane
	/// </summary>
	public class FindForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.TextBox textBox;
		private System.Windows.Forms.Button buttonFind;
		private System.Windows.Forms.CheckBox checkFromBeginning;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.CheckBox checkSearchOtherSnippets;

		public FindForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            this.SizeGripStyle = SizeGripStyle.Hide;
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
            this.textBox = new System.Windows.Forms.TextBox();
            this.buttonFind = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.checkFromBeginning = new System.Windows.Forms.CheckBox();
            this.checkSearchOtherSnippets = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(8, 11);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(280, 20);
            this.textBox.TabIndex = 1;
            this.textBox.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // buttonFind
            // 
            this.buttonFind.Location = new System.Drawing.Point(216, 59);
            this.buttonFind.Name = "buttonFind";
            this.buttonFind.Size = new System.Drawing.Size(72, 24);
            this.buttonFind.TabIndex = 3;
            this.buttonFind.Text = "&Find";
            this.buttonFind.Click += new System.EventHandler(this.buttonFind_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(144, 59);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(72, 24);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // checkFromBeginning
            // 
            this.checkFromBeginning.Checked = true;
            this.checkFromBeginning.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkFromBeginning.Location = new System.Drawing.Point(8, 35);
            this.checkFromBeginning.Name = "checkFromBeginning";
            this.checkFromBeginning.Size = new System.Drawing.Size(128, 24);
            this.checkFromBeginning.TabIndex = 2;
            this.checkFromBeginning.Text = "Start from beginning";
            // 
            // checkSearchOtherSnippets
            // 
            this.checkSearchOtherSnippets.Checked = true;
            this.checkSearchOtherSnippets.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkSearchOtherSnippets.Location = new System.Drawing.Point(8, 59);
            this.checkSearchOtherSnippets.Name = "checkSearchOtherSnippets";
            this.checkSearchOtherSnippets.Size = new System.Drawing.Size(128, 24);
            this.checkSearchOtherSnippets.TabIndex = 2;
            this.checkSearchOtherSnippets.Text = "Search all snippets";
            // 
            // FindForm
            // 
            this.AcceptButton = this.buttonFind;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(296, 115);
            this.ControlBox = false;
            this.Controls.Add(this.checkFromBeginning);
            this.Controls.Add(this.buttonFind);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.checkSearchOtherSnippets);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FindForm";
            this.Opacity = 0.9;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find Text";
            this.TopMost = true;
            this.LostFocus += new System.EventHandler(this.FindForm_LostFocus);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.FindForm_Closing);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private void buttonCancel_Click(object sender, System.EventArgs e)
		{
                this.Visible = false;
		}

		private void FindForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
	        this.Visible = false;
	        e.Cancel = true;
		}

		private void buttonFind_Click(object sender, System.EventArgs e)
		{
            Universe.Instance.detailPane.FindText(textBox.Text, checkFromBeginning.Checked, checkSearchOtherSnippets.Checked);
			if (Universe.Instance.detailPane.Enabled)
				checkFromBeginning.Checked = false;
		}

		private void textBox_TextChanged(object sender, System.EventArgs e)
		{
		  checkFromBeginning.Checked = true;
		}

        protected override void OnLoad(EventArgs e)
        {
            if (!(Universe.Instance.ModelGateway is Kbase.ModelInMemory.SnippetDictionary))
            {
                this.checkSearchOtherSnippets.Checked = false;
                this.checkSearchOtherSnippets.Enabled = false;
            }
        }

        void FindForm_LostFocus(object sender, EventArgs e)
        {
        }


        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible) {
                // this.TopMost = true;
                // this.Focus();
                this.Activate();
                //this.BringToFront();
                Logger.Log("visible changed to here a focus man.");
            }
        }


        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
        }

	}
}
