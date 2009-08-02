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
using Kbase.Model;
using System.Windows.Forms;
using Kbase.MainFrm;

namespace Kbase.Properties
{

	public class SnippetTitleBox : TextBox
	{
		private Snippet currentSnippet = null;

		public SnippetTitleBox() 
		{
            InitializeContextMenu();
		}

        ToolTip tip = new ToolTip();
		public void Edit(Snippet snippet) {
            if (snippet == null)
                Text = "";
            else
            {
                currentSnippet = snippet;
                Text = currentSnippet.Title;
                tip.SetToolTip(this, "Click to edit");//\nId " + currentSnippet.Id);//" - Created On " + currentSnippet.Created);
            }
		}

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            UpdateTitle();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyData == Keys.Return)
                UpdateTitle();
        }

        protected void UpdateTitle()
        {
            try
            {
                if (currentSnippet != null && Text.Length > 0 && !Text.Equals(currentSnippet.Title))
                {
                    currentSnippet.Title = Text;
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }

        void InitializeContextMenu() {
            if (ContextMenu == null)
                ContextMenu = new ContextMenu();

            MenuItem menuItem;

            // Define the MenuItem objects to display for the TextBox.
            menuItem = new MenuItem("Cu&t");
            menuItem.Click += new EventHandler(ClickCut);
            ContextMenu.MenuItems.Add(menuItem);

            // Define the MenuItem objects to display for the TextBox.
            menuItem = new MenuItem("&Copy");
            menuItem.Click += new EventHandler(ClickCopy);
            ContextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("&Paste");
            menuItem.Click += new EventHandler(ClickPaste);
            ContextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("Insert &Date");
            menuItem.Click += new System.EventHandler(ClickInsertDate);
            ContextMenu.MenuItems.Add(menuItem);
        }


        
        public void ClickCopy(object sender, System.EventArgs e)
        {
            if (!Enabled)
                return;
            Copy();
        }

        public void ClickCut(object sender, System.EventArgs e)
        {
            if (!Enabled)
                return;
            Cut();
        }

        public void ClickPaste(object sender, System.EventArgs e)
        {
            if (!Enabled)
                return;
            Paste();
        }

        public void ClickInsertDate(object sender, System.EventArgs e)
        {
            if (!Enabled)
                return;
            InsertDate();
        }

        static string dateTimeFormat = "yyyy-MM-dd HH:mm tt";
        private void InsertDate()
        {
            if (!Enabled)
                return;
            Paste(DateTime.Now.ToString(dateTimeFormat));
        }



	}
}
