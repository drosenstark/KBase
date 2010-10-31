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
using System.Collections;

namespace Kbase.MultipleSelectionTreeView
{
	/// <summary>
	/// Holds the Selections made in the TreeViewMultipleSelect for 
    /// forward/back navigation
	/// </summary>
	public class NavigationHistory
	{

        internal NavigationToolBar toolBar = null;

		// holds the history but each new place blows away everything in front of whereAreWe
		ArrayList sequentialHistory = new ArrayList();
		// holds the total history, backs and forwards don't affect it
		ArrayList totalHistory = new ArrayList();

		int whereAreWe = 0;
		TreeViewMultipleSelect tree = null;
		bool supressSelectionAdd = false;

		public NavigationHistory(TreeViewMultipleSelect tree)
		{
			this.tree = tree;
		}

		public void Forward() {
			MoveForwardBackward(true);
		}

		public void Backward() {
			MoveForwardBackward(false);
		}

        public bool CanMoveForward() {
            return (whereAreWe < sequentialHistory.Count - 1);
        }

        public bool CanMoveBackward() {
            return whereAreWe > 0;
        }

		private void MoveForwardBackward(bool forward) {
			// if we can move forward in the list, move forward, blocking the AddSelection method
			TreeNodeMultipleSelectArrayList next = null;
			if (forward && CanMoveForward()) 
				// increment and get
				next = (TreeNodeMultipleSelectArrayList)sequentialHistory[++whereAreWe];
			else if (!forward && CanMoveBackward())
				// decrement and get
				next = (TreeNodeMultipleSelectArrayList)sequentialHistory[--whereAreWe];
			
			if (next != null) 
			{
				// this can get sticky because the elements might have 
				// been deleted, so we adjust the selection 
				ArrayList deleteUs = new ArrayList();
				deleteUs.AddRange(next);
				foreach (TreeNodeMultipleSelect node in deleteUs) {
					if (node.Dead) {
						next.Remove(node);
					}
				}
				
				// suppress the behavior if next has no elements
				if (next.Count > 0) 
				{
					supressSelectionAdd = true;			
					tree.ReplaceSelectionWith(next);
				} 
				else {
					// and remove it from the list
					sequentialHistory.Remove(next);				
					totalHistory.Remove(next);
					// and try to redo the gesture if possible
					MoveForwardBackward(forward);
				}
			}
            UpdateToolBar();
		
		}

		public void ClearHistory() {
			sequentialHistory.Clear();		
			totalHistory.Clear();
            UpdateToolBar();
        }

		public void AddSelection(TreeNodeMultipleSelectArrayList selectionIn) {
			if (!supressSelectionAdd) {
				// Must copy the collection, otherwise we're using a collection that's being modified by the 
				// multiple selection pane all the time!
				TreeNodeMultipleSelectArrayList selection = new TreeNodeMultipleSelectArrayList(selectionIn.Count);
				selection.AddRange(selectionIn);
				// kill everything after where we are
				if (sequentialHistory.Count > whereAreWe + 1)
					sequentialHistory.RemoveRange(whereAreWe+1, sequentialHistory.Count-(whereAreWe+1));
				if (!totalHistory.Contains(selection))
					totalHistory.Add(selection);
				whereAreWe = sequentialHistory.Add(selection);
			} else
				// shut this off in any case
				supressSelectionAdd = false;
            UpdateToolBar();
        }

		public void AddSelection(TreeNodeMultipleSelect node) 
		{
			TreeNodeMultipleSelectArrayList list = new TreeNodeMultipleSelectArrayList(1);
			list.Add(node);
			AddSelection(list);
            UpdateToolBar();
		}

        void UpdateToolBar() {
            if (toolBar != null)
                toolBar.OnAfterNavigation();
        }
	}
}
