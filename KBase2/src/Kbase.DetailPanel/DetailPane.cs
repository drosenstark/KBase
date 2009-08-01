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
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using Kbase.SnippetTreeView;
using Kbase.Model;
using Kbase.MainFrm;
using System.Drawing;

namespace Kbase.DetailPanel
{

	/// <summary>
	/// The text pane or detail pane... it's a RichTextBox but a lot of stuff is 
    /// done using the RtfConverter, so the end results end up being plain text
    /// (well, mostly)
	/// </summary>
	public partial class DetailPane : RichTextBox
	{

		internal Snippet snippet = null;

		public DetailPane()
		{
			Universe.Instance.detailPane = this;
			Name = "detailPane";
			Multiline = true;
			AcceptsTab = true;
            AllowDrop = true;
			Reset();
		}
	
		public bool IsEditingSnippet(Snippet snippet) 
		{
			if (this.snippet == null)
				return false;
			return this.snippet.Equals(snippet);
		}

        public bool IsEditingSnippet(int id)
        {
            if (this.snippet == null)
                return false;
            return this.snippet.Id == id;
        }

        /// <summary>
        /// We use this AND enabled... enabled off is only for EditNone.
        /// </summary>
        public bool Editable {
            get {
                return !ReadOnly;
            }

            set {
                ReadOnly = !value;
                if (value)
                {
                    BackColor = Color.White; // bug in winforms, this doesn't reset correctly even if we're enabled AND readonly
                }
            }
        }


		public void EditNone() 
		{
			Save();
			Reset();
            Enabled = false;
		}

        /// <summary>
        /// caller must make sure it's the right snippet
        /// </summary>
        public void UpdateTextFromExternal() {
            Invoke(new ZeroArgumentEventHandler(UpdateTextFromExternalInner));
        }

        private void UpdateTextFromExternalInner()
        {
            Edit(snippet);
        }

		public void Edit(Snippet snippet) 
		{
			float oldFactor = ZoomFactor;
			// save the current snippet
			Save();
			Reset();

            Enabled = true;

			this.snippet = snippet;

            bool activate = Load(false);

            if (activate)
            {
                Universe.Instance.mainForm.AnnounceEditing(snippet);

                this.Editable = true;
                ZoomFactor = oldFactor;
                if (ZoomFactor != oldFactor)
                {
                    // God, please help me and tell me why I need to set this 
                    // variable twice. Worse, a while loop locks up!
                    // Must have to do with the updates to the thinger. A while
                    // doesn't allow the screen to update.
                    ZoomFactor = oldFactor;
                }
            }
		}

		public void RedoFormatting() 
		{
			if (snippet == null)
				return;
			Save();
            // blow away the Rtf
            snippet.UI.Rtf = null;
			Load(false);
		}

		public bool Dirty 
		{
			get 
			{
                return (this.Rtf != snippet.UI.Rtf);
			}
		}


		
		public void Reset() 
		{
			Load(true);
			snippet = null;
			Universe.Instance.mainForm.AnnounceEditing(null);
			this.Editable = false;
		}

		bool handlingInKeyDown = false;
		protected override void OnLinkClicked(LinkClickedEventArgs e)
		{
			try 
			{
				base.OnLinkClicked (e);
			if (!Editable)
				return;
				// could cache this, but how often are they going to hit it?
				new HyperlinkUtil(Universe.Instance.Path).Open(e.LinkText);
			} 
			catch (Exception ex) 
			{
				MainForm.ShowError(ex,"Could not open hyperlink " + e.LinkText);			
			}
		}


		protected override void OnKeyDown(KeyEventArgs e)
		{
			try 
			{
				if (!Editable)
					return;
				base.OnKeyDown (e);
				// if there is text selected and tab is pressed, this is an indent
				if ((e.KeyCode == Keys.Tab)  && SelectedText.Length > 0) 
				{
					handlingInKeyDown = true;
					bool reverse = (e.Shift);
					IndentSelection(reverse);
				} 
				else if (e.KeyCode == Keys.T && e.Control) 
				{
					handlingInKeyDown = true;
					bool reverse = e.Shift;
					IndentSelection(reverse);
				}
				else if (e.KeyCode == Keys.B && e.Control) 
				{
					handlingInKeyDown = true;
					bool reverse = e.Shift;
					BulletSelection();
				}

				else if (e.KeyCode == Keys.F && e.Control)  
				{
					ClickFind(null, null);
					e.Handled = true;
				}			
				else if ((e.KeyCode == Keys.V && e.Control && e.Shift))  
				{
					ClickPastePlainText(null, null);
					e.Handled = true;
				}
				else if ((e.KeyCode == Keys.V && e.Control) || (e.KeyCode == Keys.Insert && e.Shift))  
				{
					ClickPaste(null, null);
					e.Handled = true;
				}
				Universe.Instance.mainForm.OnKeyDownPublic(e);
			} 
			catch (Exception e2) 
			{
				MainForm.ShowError(e2);
			}
		
		}



		static string dateTimeFormat = "yyyy-MM-dd HH:mm tt";
		private void InsertDate() 
		{
			if (!Editable)
				return;
			Paste(DateTime.Now.ToString(dateTimeFormat));
		}


		private void InsertHyperlink() 
		{
			if (!Editable)
				return;
			OpenFileDialog fileDialog = new OpenFileDialog();
			DialogResult result = fileDialog.ShowDialog();

			if (result == DialogResult.OK) 
			{
				string path = fileDialog.FileName;
                insertFileLink(path);
			}
		}

        private void insertFileLink(string path) {
            if (path != null)
            {
                path = new HyperlinkUtil(Universe.Instance.Path).Relativize(path) + "";
                path = System.Web.HttpUtility.UrlPathEncode(path);
                path = "<file://" + path + ">";
                Paste(path);
            }
        }

        private void InsertSnippetLink() 
		{
            Universe.Instance.snippetPane.SelectSnippetLinkStart(new SomeSnippetEventHandler(this.PasteSnippetLink), this);
		} 

		public void PasteSnippetLink(Snippet snippet) 
		{
			if (!Editable)
				return;
			string link = snippet.Title + " <file://this#" + snippet.Id + ">";
			Paste(link);
        }

		private void Paste(string text) 
		{
			SaveClipboard();
			Clipboard.SetDataObject(text);
			Paste();
			RestoreClipboard();
		}

		private void PasteSpecial() 
		{
			string text = (string)Clipboard.GetDataObject().GetData(DataFormats.StringFormat);
			SaveClipboard();
			RtfConverter.LoadAndCopyToClipboard(text,Font);
			Paste();
			RestoreClipboard();
		}
	

		protected override void OnMouseDown(MouseEventArgs e)
		{
			try 
			{
				base.OnMouseDown (e);
				if (!Editable)
					return;
				if (e.Button == MouseButtons.Right)
					OnRightClick(e);

			} 
			catch (Exception e2) 
			{
				MainForm.ShowError(e2);
			}
		}


		ContextMenu rightClickMenu = null;
		protected void OnRightClick(MouseEventArgs e)
		{
			if (rightClickMenu == null) 
			{
				rightClickMenu = new ContextMenu();
				MenuItem menuItem;

                // Define the MenuItem objects to display for the TextBox.
                menuItem = new MenuItem("Cu&t");
                menuItem.Click += new EventHandler(ClickCut);
                rightClickMenu.MenuItems.Add(menuItem);
                
                // Define the MenuItem objects to display for the TextBox.
				menuItem = new MenuItem("&Copy");
				menuItem.Click +=new EventHandler(ClickCopy);
				rightClickMenu.MenuItems.Add(menuItem);

				menuItem = new MenuItem("&Paste");
				menuItem.Click +=new EventHandler(ClickPaste);
				rightClickMenu.MenuItems.Add(menuItem);

				rightClickMenu.MenuItems.Add(new MenuItem("-"));

				menuItem = new MenuItem("Copy &Plain Text");
				menuItem.Click +=new EventHandler(ClickCopyPlainText);
				rightClickMenu.MenuItems.Add(menuItem);

				menuItem = new MenuItem("Paste &Plain Text");
				menuItem.Click +=new EventHandler(ClickPastePlainText);
				rightClickMenu.MenuItems.Add(menuItem);
				
				rightClickMenu.MenuItems.Add(new MenuItem("-"));

				menuItem = new MenuItem("Insert Link to &File");
				menuItem.Click +=new EventHandler(ClickInsertHyperlink);
				rightClickMenu.MenuItems.Add(menuItem);

				menuItem = new MenuItem("Insert Link to &Snippet");
				menuItem.Click +=new EventHandler(ClickInsertSnippetlink);
				rightClickMenu.MenuItems.Add(menuItem);


				menuItem = new MenuItem("Insert &Date");
				menuItem.Click += new System.EventHandler(ClickInsertDate);
				rightClickMenu.MenuItems.Add(menuItem);

				
				rightClickMenu.MenuItems.Add(new MenuItem("-"));

				menuItem = new MenuItem("Zoom &In");
				menuItem.Click +=new EventHandler(ClickZoomIn);
				rightClickMenu.MenuItems.Add(menuItem);

				menuItem = new MenuItem("Zoom &Out");
				menuItem.Click +=new EventHandler(ClickZoomOut);
				rightClickMenu.MenuItems.Add(menuItem);

				rightClickMenu.MenuItems.Add(new MenuItem("-"));

				menuItem = new MenuItem("Redo &Formatting");
				menuItem.Click +=new EventHandler(ClickRedoFormatting);
				rightClickMenu.MenuItems.Add(menuItem);
			}

			Cursor.Current = Cursors.Arrow;
			rightClickMenu.Show(Universe.Instance.mainForm, Universe.Instance.mainForm.PointToClient(PointToScreen(new System.Drawing.Point(e.X, e.Y)))); 
		}

		public void ClickZoomIn(object sender, System.EventArgs e) 
		{
			if (!Editable)
				return;
			ZoomIn();
		}

		public void ClickZoomOut(object sender, System.EventArgs e) 
		{
			if (!Editable)
				return;
			ZoomOut();
		}



		public void ClickCopy(object sender, System.EventArgs e) 
		{
			if (!Editable)
				return;
			Copy();
		}

		public void ClickCut(object sender, System.EventArgs e) 
		{
			if (!Editable)
				return;
			Cut();
		}


		public void ClickCopyPlainText(object sender, System.EventArgs e) 
		{
			if (!Editable)
				return;
			CopySpecial();
		}

		public void ClickPastePlainText(object sender, System.EventArgs e) 
		{
			if (!Editable)
				return;
			PasteSpecial();
		}


		public void ClickUndo(object sender, System.EventArgs e) 
		{
			if (!Editable)
				return;
			Undo();
		}

		public void ClickRedo(object sender, System.EventArgs e) 
		{
			if (!Editable)
				return;
			Redo();
		}

		public void ClickRedoFormatting(object sender, System.EventArgs e) 
		{
			if (!Editable)
				return;
			RedoFormatting();
		}
		

		FindForm findForm = null;
		public void ClickFind(object sender, System.EventArgs e) 
		{
			if (findForm == null)
				findForm = new FindForm();
			findForm.Show();
		}


		internal void Select(Selection selection) 
		{
			Select(selection.Start, selection.Length);		
		}



		public void CopySpecial() 
		{
			if (!Editable)
				return;
			Selection oldSelection = new Selection(SelectionStart, SelectionLength);
			string copyThis = RtfConverter.Serialize(Rtf, SelectionStart,SelectionLength);
			Clipboard.SetDataObject(new DataObject(DataFormats.StringFormat, copyThis));
			Select(oldSelection);
		}
		
		public void ClickPaste(object sender, System.EventArgs e) 
		{
			if (!Editable)
				return;
			SaveClipboard();
			RtfConverter.FormatClipboard(Font);
			Paste();
			RestoreClipboard();
		}


		public void ClickInsertDate(object sender, System.EventArgs e) 
		{
			if (!Editable)
				return;
			InsertDate();
		}

		public void ClickInsertHyperlink(object sender, System.EventArgs e) 
		{
			if (!Editable)
				return;
			InsertHyperlink();
		}


		public void ClickInsertSnippetlink(object sender, System.EventArgs e) 
		{
			if (!Editable)
				return;
			InsertSnippetLink();
		}
	
		protected override void OnKeyUp(KeyEventArgs e)
		{
			try 
			{
				base.OnKeyUp (e);
				handlingInKeyDown = false;

			} 
			catch (Exception e2) 
			{
				MainForm.ShowError(e2);
			}
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			try 
			{
				base.OnKeyPress (e);
				if (!Editable)
					return;
				// this is necessary because the tab, for instance, doesn't get handled in
				// key down but rather here.  Must avoid this hence this logic.
				// it also avoids the beeping.
				if (handlingInKeyDown)
					e.Handled = true;
			} 
			catch (Exception e2) 
			{
				MainForm.ShowError(e2);
			}

		}

		public void BulletSelection() 
		{
			BulletSelection(SelectionBullet);
		}

		public void BulletSelection(bool reverse) 
		{
			if (reverse) 
			{
				SelectionBullet = false;
				SelectionHangingIndent = 0;
			} 
			else
			{
				SelectionBullet = true;
				SelectionHangingIndent = RtfConverter.IndentPerTab;
			}
		}


		public void IndentSelection(bool reverse) 
		{
			if (!reverse) 
			{
				SelectionIndent += RtfConverter.IndentPerTab;
			} 
			else 
			{
				// go back to a tab stop first
				// then start minusing
				if (SelectionIndent > RtfConverter.IndentPerTab) 
				{
					int mod = SelectionIndent % RtfConverter.IndentPerTab;
					if (mod > 0)
						SelectionIndent -= mod;
					else
						SelectionIndent -= RtfConverter.IndentPerTab;
				}
				else
					SelectionIndent = 0;
			}
		}

		public void Save() 
		{
            if (!Editable)
                return;
			if (this.snippet != null) 
			{
				if (Dirty) 
				{
					string newText = RtfConverter.GetTextFromRtf(Rtf);
					snippet.Text = newText;
					snippet.UI.Rtf = Rtf;
				}
			}		
		}

        public void Reload() {
            Load(false);
        }

        /// <summary>
        /// returns true if text was loaded, false otherwise
        /// </summary>
        /// <param name="clearAllText"></param>
        /// <returns></returns>
		public bool Load(bool clearAllText) 
		{
			if (clearAllText) {
				Clear();
				return true;
			}

            string text = null;
            text = snippet.Text;

            if (text == null)
            {
                this.Reset();
                Universe.Instance.mainForm.ShowMessage("Text could not be retrieved from server; Snippet may have been deleted.");
                return false;
            }

            /* we used to cache the Rtf but with external editing that's sometimes impossible 
			if (snippet.UI.Rtf != null) 
				this.Rtf = snippet.UI.Rtf;
             */
			Rtf = RtfConverter.Load(text, Font);
			snippet.UI.Rtf = Rtf;
            return !snippet.WatchingExternalFile;
		}


		#region OPERATIONS FOR CLIPBOARD CACHING so we can play with the clipboard 
		private object ClipboardCacheObj = null;
		private string ClipboardCacheFormat = null;

		private void RestoreClipboard() 
		{
            try
            {
                if (ClipboardCacheFormat != null && ClipboardCacheObj != null)
                {
                    DataObject popClip = new DataObject(ClipboardCacheFormat, ClipboardCacheObj);
                    Clipboard.SetDataObject(popClip, true);
                }
            }
            catch (Exception e) { 
                // this causes all kinds of weird errors, sometimes
                MainForm.ShowErrorSilent(e);
            }
		}

		/// <summary>
		/// This method sucks, but whatever
		/// </summary>
		private void SaveClipboard() 
		{
			try 
			{
				IDataObject obj = Clipboard.GetDataObject();
				ClipboardCacheFormat = obj.GetFormats()[0];
				ClipboardCacheObj = obj.GetData(ClipboardCacheFormat);
			} 
			catch (Exception e) 
			{
				MainForm.ShowErrorSilent(e);
				ClipboardCacheFormat = null;
				ClipboardCacheObj = null;
			}
		}
		#endregion 


        public void FindText(string text, bool startFromBeginning, bool searchOtherSnippets)
        {
            int found = -1;
            Selection oldSelection = null;
            if (Editable)
            {
                oldSelection = new Selection(SelectionStart, SelectionLength);
                int startFrom = 0;
                if (!startFromBeginning)
                    startFrom = SelectionStart + SelectionLength;
                found = Find(text, startFrom, RichTextBoxFinds.None);
            }
            if (found == -1 || (found == oldSelection.Start && text.Length == oldSelection.Length))
            {
                // careful!  this actually does the find in other snippets if 
                // necessary
                if (!searchOtherSnippets || !FindInOtherSnippets(text))
                {
                    MessageBox.Show("Text '" + text + "' not found.", MainForm.DialogCaption, MessageBoxButtons.OK);
                }
            }
            else
            {
                Select(found, text.Length);
                this.Focus();
                this.ScrollToCaret();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns>true if found, false if not</returns>
        private bool FindInOtherSnippets(string text)
        {
            bool retVal = false;
            Snippet next = Universe.Instance.ModelGateway.FindNextSnippetContaining(snippet, text);
            if (next != null)
            {
                // this selects one of the nodes that has a ref to the snippet,
                // thereby making our selected snippet next (so it's like edit(next))
                next.UI.GetAnyInstance().SelectExclusive();

                // is it in the title? Otherwise search to the first instance
                // you might say this is costly, we're searching twice,
                // but we already know we've found it so it's not that expensive
                if (this.snippet.Title.ToLower().IndexOf(text.ToLower()) == -1)
                    FindText(text, true, true);
                retVal = true;
            }
            return retVal;
        }


		public void ZoomIn() 
		{
			if (ZoomFactor < 5)
				ZoomFactor += .1F;
		}

		public void ZoomOut() 
		{
			if (ZoomFactor > .5)
				ZoomFactor -= .1F;
		}

        private const string NO_SNIPPET = "No Snippet Selected";

        /// <summary>
        /// Gets the description for the announce editing function of the mainform
        /// </summary>
        /// <returns></returns>
        internal string GetSnippetName()
        {
            string retVal = null;
            if (snippet == null)
                retVal =  NO_SNIPPET;
            else {
                retVal = snippet.Title;// +" [" + serialize(snippet.Created) + "]";
            }
            return retVal;

        }

        static string createdDateTimeFormat = "yyyy-MM-dd";
        private static String serialize(DateTime dateTime)
        {
            return dateTime.ToString(createdDateTimeFormat);
        }

    }

	/// <summary>
	/// Convenience class to save the current selected text
	/// </summary>
	public class Selection 
	{
		public Selection(int Start, int Length) 
		{
			this.Start = Start;
			this.Length = Length;
		}
		public int Start;
		public int Length;
	}
}
