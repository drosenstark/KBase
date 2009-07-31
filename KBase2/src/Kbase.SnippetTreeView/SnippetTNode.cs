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
using System.Collections;
using System.Drawing;
using Kbase.MultipleSelectionTreeView;
using Kbase.Model;
using System.Diagnostics;
using Kbase.Icon;

namespace Kbase.SnippetTreeView
{
	/// <summary>
	/// Summary description for Snippet.
	/// </summary>
	
	public class SnippetTNode : TreeNodeMultipleSelect
	{
		public SnippetInstance SnippetInstance;
		public IList SearchCriteria;
		public static Color HighlightBack = Color.Cyan;

		public Snippet Snippet {
			get 
			{
				if (SnippetInstance != null)
					return SnippetInstance.Snippet;
				else
					return null;
			}
		}


        /// <summary>
        /// this is for cloning for the snippet tree view link selection thinger
        /// </summary>
        public SnippetTNode() 
        {
        }

		public SnippetTNode(string name, SnippetInstance snippetInstance) : base(name)
		{
			this.SnippetInstance = snippetInstance;
			this.ImageIndex = IconList.Instance.defaultIconIndex;
		}

		public void Highlight() 
		{
            Debug.Assert(!Dead && this.TreeView != null);

			// if it's not selected highlight it
			if (!Selected) 
			{
				this.BackColor = HighlightBack;
				Universe.Instance.snippetPane.highlightedNodes.Add(this);
			}
		}

		public void RemoveHighlight() 
		{
            // THIS SHOULD NOT HAPPEN, HOWEVER, if it does, when TheKBase is saved, this SnippetTNode
			// (which is not on a tree) will not get saved
			if (Dead || this.TreeView == null)
				return;
			// check if it's got a different back color
			if (!Selected)
				PaintAsUnselected();
		}


		public void RedoPaintjob() {
			SnippetPane pane = Universe.Instance.snippetPane;
			if (pane.SelectedNodes.Contains(this)) {
				if (pane.SelectedNodes.Count == 1)
					PaintAsSelectedUnique();
				else
					PaintAsSelected();
			} else
				PaintAsUnselected();
			if (pane.highlightedNodes.Contains(this))
				Highlight();
        }

        protected override void PaintAsSelected()
        {
            base.PaintAsSelected();
            SetColor(SnippetPane.ConvertToColor(Snippet.Color));
            ImageIndex = IconList.Instance.GetIconIndexUnselected(Snippet.Icon);
        }
	
		public override void PaintAsUnselected()
		{
			base.PaintAsUnselected();
			//SetColor(SnippetPane.ConvertToColor(Snippet.Color));
			ImageIndex = IconList.Instance.GetIconIndexUnselected(Snippet.Icon);
		}

		// the super doesn't actually use the SelectedImageIndex ever
		public override void PaintAsSelectedUnique()
		{
			base.PaintAsSelectedUnique ();
			ImageIndex = IconList.Instance.GetIconIndexSelected(Snippet.Icon);
			this.SnippetInstance.Highlight();
		}


		public void SetColor(System.Drawing.Color color) 
		{
			if (!Selected && !color.Equals(Color.Empty))
				BackColor = color;
		}


		
		public override string ToString()
		{
			return "SnippetTNode " + Text + " with snippet " + Snippet.GetHashCode();
		}

        public override int GetHashCode()
        {
            return base.GetHashCode(); // just to avoid the warning for overriding equals
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            return base.Equals(obj);
        }
		
	}
}
