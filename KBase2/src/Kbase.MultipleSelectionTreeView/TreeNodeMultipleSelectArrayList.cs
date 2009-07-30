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
using System.Collections;

namespace Kbase.MultipleSelectionTreeView
{
	/// <summary>
	/// Like most of my collections, this exists just to override the equals and hashcode methods.
	/// If they have the same contents, two collections are the same.
	/// </summary>
	public class TreeNodeMultipleSelectArrayList : ArrayList
	{
		public TreeNodeMultipleSelectArrayList() 
		{
		}
		public TreeNodeMultipleSelectArrayList(int i) : base(i) 
		{
		}

		public TreeNodeMultipleSelectArrayList(System.Windows.Forms.TreeNode[] array) 
		{
			foreach (System.Windows.Forms.TreeNode node in array) {
				this.Add(node);
			}
		}


		public override int GetHashCode()
		{
			int retVal = 0;
			foreach (TreeNodeMultipleSelect node in this) 
			{
				retVal += node.GetHashCode();
			}
			return retVal;
		}

		/// <summary>
		/// If they contain the same Snippets, they're equivalent
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>

		public override bool Equals(object obj)
		{
			if (!(obj is TreeNodeMultipleSelectArrayList))
				return false;
			TreeNodeMultipleSelectArrayList compareTo = (TreeNodeMultipleSelectArrayList)obj;
			if (Count != compareTo.Count) 
				return false;
			foreach (TreeNodeMultipleSelect node in this) 
			{
				if (!compareTo.Contains(node))
					return false;
			}
			return true;
		}

	}
}
