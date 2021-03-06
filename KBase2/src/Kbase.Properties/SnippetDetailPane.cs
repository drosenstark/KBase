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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Kbase.SnippetTreeView;
using Kbase.Model;
using Kbase.MainFrm;
using System.Diagnostics;
using Kbase.DetailPanel;



namespace Kbase.Properties
{
	/// <summary>
	/// Houses the snippettitle and snippetdate, properties panes and 
    /// location and parent panes and all the stuff in the 
    /// Kbase.Properties namespace
	/// </summary>
	public class SnippetDetailPane : System.Windows.Forms.Panel
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.Container components = null;
        const int standardLabelHeight = 50;

        ParentPane.ParentPane parentPane;
        LocationPane2.LocationPane locationPane;
        protected TabPage locationPanesPageLocations;
        protected TabPage locationPanesPageParents;

		SnippetTitleBox snippetTitle;
        SnippetDateBox snippetDate;
        ExternalEditButton externalEditButton;
        ExternalEditStopButton externalEditStopButton;

		Panel topPanel;
        PropertiesPaneHolder propertiesPaneHolder;
        internal List<Snippet> selectedSnippets = null;


		public SnippetDetailPane()
		{
			Init();
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

		private void Init()
		{
			this.SuspendLayout();

            // this thing is set up as follows: you have a topPanel that holds the snippet date and the 
            // snippet title. Then under that, filling up the rest of the space, is the propertiesPaneHolder

			

            // 
            // snippetDate
            // 
            this.snippetDate = new SnippetDateBox();
            this.snippetDate.Location = new System.Drawing.Point(0, 0);
            this.snippetDate.Width = this.ClientSize.Width;
            this.snippetDate.Height = standardLabelHeight;
            this.snippetDate.AutoSize = true;
            this.snippetDate.Dock = DockStyle.Left;
            this.snippetDate.Text = new DateTime().ToString();
            this.snippetDate.BorderStyle = BorderStyle.Fixed3D;

            // 
            // snippetTitle
            // 
            this.snippetTitle = new SnippetTitleBox();
            this.snippetTitle.Location = new System.Drawing.Point(0, 0);
            //this.snippetTitle.Width = this.ClientSize.Width;
            this.snippetTitle.Height = standardLabelHeight;
            this.snippetTitle.Multiline = false;
            this.snippetTitle.Name = "Snippet Title";
            this.snippetTitle.TabIndex = 0;
            this.snippetTitle.ReadOnly = false;
            this.snippetTitle.Dock = DockStyle.Left;

            
            // 
            // externalEditButton
            // 
            externalEditButton = new ExternalEditButton();
            externalEditButton.Location = new System.Drawing.Point(0, 0);
            externalEditButton.Height = standardLabelHeight - 10;
            externalEditButton.AutoSize = true;
            externalEditButton.Dock = DockStyle.Right;


            // 
            // externalEditStopButton
            // 
            externalEditStopButton = externalEditButton.externalEditStopButton;
            externalEditStopButton.Location = new System.Drawing.Point(0, 0);
            externalEditStopButton.Height = standardLabelHeight - 10;
            externalEditStopButton.AutoSize = true;
            externalEditStopButton.Dock = DockStyle.Right;

			// Top Panel
			topPanel = new Panel();
			topPanel.Dock = DockStyle.Top;
            topPanel.Controls.Add(externalEditStopButton);
            topPanel.Controls.Add(this.externalEditButton);
            topPanel.Controls.Add(this.snippetTitle);
            topPanel.Controls.Add(this.snippetDate);

            // ParentPane which is the tree backwards
            this.parentPane = new ParentPane.ParentPane();
            this.parentPane.Location = new System.Drawing.Point(0, 15);
            this.parentPane.Dock = DockStyle.Fill;
            this.parentPane.AutoSize = true;

            // LocationPane is another version of the ParentPane
            this.locationPane = new LocationPane2.LocationPane();
            this.locationPane.Location = new System.Drawing.Point(0, 15);
            this.locationPane.Dock = DockStyle.Fill;
            this.locationPane.AutoSize = true;


            locationPanesPageParents = new TabPage("Parents");
            locationPanesPageParents.Controls.Add(parentPane);
            locationPanesPageLocations = new TabPage("Locations");
            locationPanesPageLocations.Controls.Add(locationPane);
            List<TabPage> pages = new List<TabPage>(2);
            pages.Add(locationPanesPageParents);
            pages.Add(locationPanesPageLocations);

            propertiesPaneHolder = new PropertiesPaneHolder(pages);

			
			// 
			// SnippetDetailPane
			// 
			// this.AllowDrop = true;
			this.AutoScroll = true;
			this.VScroll = true;
            this.Controls.Add(propertiesPaneHolder);
            this.Controls.Add(topPanel);
            this.Cursor = System.Windows.Forms.Cursors.Default;
			this.Name = "PropertiesPane";
			this.Text = "Properties";
            // does nothing bottomHalfSplitContainer.Location = new Point(0, 0);
            // does nothing bottomHalfSplitContainer.Height = 1000;
			this.ResumeLayout(false);
		}






		public void EditNone() 
		{
            if (this.selectedSnippets != null)
                this.selectedSnippets.Clear();
            snippetTitle.Edit(null);
            snippetDate.Edit(null);
            externalEditButton.Edit(null);
            parentPane.Clear();
            locationPane.Clear();
            LayoutProperties();
            this.Enabled = false;
		}

		internal void Edit(SnippetInstance instance) 
		{
			if (instance == null) 
			{
				EditNone();
				return;
			}
			this.Enabled = true;
            selectedSnippets = new List<Snippet>(1);
            selectedSnippets.Add(instance.Snippet);


            propertiesPaneHolder.Edit(selectedSnippets);
            externalEditButton.Edit(instance.Snippet);
            snippetDate.Edit(instance.Snippet);
            snippetTitle.Edit(instance.Snippet);
            parentPane.Edit(instance);

            locationPane.Edit(instance);

			LayoutProperties();
		}

		public void Edit(List<Snippet> selectedSnippets) 
		{
			if (selectedSnippets == null)
				return;
            this.selectedSnippets = selectedSnippets;
            this.Enabled = true;

            propertiesPaneHolder.Edit(selectedSnippets);

			Text = "- Multiple Selection ("+ selectedSnippets.Count +" Snippets) -\n";
            parentPane.Clear();
            locationPane.Clear();
            LayoutProperties();
		}

		public bool IsEditingSnippet(Snippet snippet) 
		{
			bool retVal = false;
			if (this.selectedSnippets != null)
				return (this.selectedSnippets.Contains(snippet));
			return retVal;
		}



		protected override void OnResize(EventArgs e)
		{
			try 
			{
				base.OnResize (e);
				LayoutProperties();
			} 
			catch (Exception e2) 
			{
				MainForm.ShowError(e2);
			}

		}




        internal void BeginEdit()
        {
            if (selectedSnippets.Count == 1) {
                snippetTitle.SelectAll();
                snippetTitle.Focus();
            
            }
        }



        public void OnAfterRestoreProperties() {
            propertiesPaneHolder.OnAfterRestoreProperties();
        }


        internal void LayoutProperties() // some of this gets pushed down to the properties pane, of coures
        {
            SuspendLayout();

            int y = 0;
            int tabIndex = 0;
            topPanel.Width = ClientRectangle.Width;
            topPanel.TabIndex = tabIndex++;

            if (selectedSnippets != null && selectedSnippets.Count == 1)
            {
                snippetTitle.Location = new System.Drawing.Point(0, y);
                y += snippetTitle.Height;
                // at some point I'll redo this, just leaving space to the right
                // and in between
                snippetTitle.Width = topPanel.ClientRectangle.Width - snippetDate.Width - externalEditButton.Width - externalEditStopButton.Width - 5;
                snippetTitle.TabIndex = tabIndex++;
                snippetTitle.Visible = true;

                snippetDate.Location = new System.Drawing.Point(snippetTitle.Width + 3, y - snippetTitle.Height);
                snippetDate.Visible = true;
                topPanel.Height = snippetTitle.Height + 5 ;

            }
            else
            {
                snippetTitle.Visible = false;
                snippetDate.Visible = false;
                topPanel.Height = 0;
            }

            propertiesPaneHolder.LayoutProperties();

            ResumeLayout(true);

        }



    }
}
