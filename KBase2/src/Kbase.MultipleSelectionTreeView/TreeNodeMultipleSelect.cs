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

namespace Kbase.MultipleSelectionTreeView
{
	/// <summary>
	/// Summary description for TreeNodeMultipleSelect.
	/// </summary>
	public class TreeNodeMultipleSelect : TreeNode
	{

		public static int DefaultImageIndexSelected = 0;
		public static int DefaultImageIndexUnselected = 1;

		public TreeNodeMultipleSelect()
		{
			ImageIndex = DefaultImageIndexUnselected;
		}

		public TreeNodeMultipleSelect(string text) : base(text)
		{
			ImageIndex = DefaultImageIndexUnselected;
		}

		public new void Remove() 
		{
			if (!Dead) 
			{
				// and remove it from the Selection if necessary
				((TreeViewMultipleSelect)TreeView).SelectedNodes.Remove(this);
				base.Remove();
			}
		}

		public bool Dead
		{
			get 
			{
				return (TreeView == null);
			}
		}

		public virtual void PaintAsSelectedUnique() 
		{
			if (Dead)
				return;
			PaintAsSelected();
			ImageIndex = DefaultImageIndexSelected;
		}

		protected virtual void PaintAsSelected() 
		{
			// shouldn't we be worried that this means there's a memory leak?  No.
			// The selection changes all the time, so the references to these nodes in the selected
			// group will be released very soon.  I.e., it's not a problem here.
			if (!Dead) 
			{
				BackColor = System.Drawing.SystemColors.Highlight;
				ForeColor = System.Drawing.SystemColors.HighlightText;
			}
		}

		public virtual void PaintAsUnselected() {
			// shouldn't we be worried that this means there's a memory leak?  No.
			// The selection changes all the time, so the references to these nodes in the selected
			// group will be released very soon.  I.e., it's not a problem here.
			if (!Dead) 
			{
				ImageIndex = DefaultImageIndexUnselected;
				BackColor = TreeView.BackColor;
				ForeColor = TreeView.ForeColor;
			}
		}
		
		private bool selected = false;

		/// <summary>
		/// These paint the node correctly and everything.
		/// And you don't need to worry about whether the node is not attached
		/// to a tree, in which case it skips the painting.
		/// </summary>
		public bool Selected {
			get {
				return selected;				
			}
			set {
				selected = value;
				if (selected)
					PaintAsSelected();
				else
					PaintAsUnselected();
			}
		
		}
	}
}
