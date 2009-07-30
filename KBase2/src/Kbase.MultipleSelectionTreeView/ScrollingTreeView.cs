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
using System.Drawing;

namespace Kbase.MultipleSelectionTreeView
{
	/// <summary>
	/// This code is loosely based on this code
    /// http://www.dotnet4all.com/dotnet-code/2004/10/how-to-scroll-treeview.html
	/// TreeView that knows how to handle the scrolling beyond its boundaries.
    /// Some modification was obviously necessary, but the main concepts about
    /// the clock ticks and that stuff actually work... sure looks like a hack :)
	/// </summary>
	public abstract class ScrollingTreeView : TreeViewMultipleSelect
	{

	
		/* all this is to handle the drag up and down beyond boundaries */
		long m_Ticks;
		protected override void OnDragOver(DragEventArgs drgevent)
		{
			try 
			{
				// call the super
				base.OnDragOver (drgevent);
				Point pt = PointToClient(new Point(drgevent.X, drgevent.Y));
				// Grab the node from where the cursor is. 
				TreeNode node = GetNodeAt(pt);
			
				// DRosenstark Addition 22-Jan-2005
				// if you don't do this, it will throw an "Exception" if the node is null
				// but not really, just shows the not-draggable-to-here-icon thinger
				if (node == null)
					return;
				// and how much time has past since we lsat passed through this method
				TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - m_Ticks);
				//scroll up
				if (pt.Y < ItemHeight) 
				{
					// if within one node of top, scroll quickly
					if (node.PrevVisibleNode!= null) 
					{
						node = node.PrevVisibleNode;
					}
					node.EnsureVisible();
					m_Ticks = DateTime.Now.Ticks;
				} 
				else if (pt.Y < (ItemHeight * 2)) 
				{
					// if within two nodes of the top, scroll slowly
					if (ts.TotalMilliseconds > 250)
					{
						node = node.PrevVisibleNode;
						if (node.PrevVisibleNode != null) 
						{
							node = node.PrevVisibleNode;
						}
						node.EnsureVisible();
						m_Ticks	= DateTime.Now.Ticks;
					}
				}

		
				//scroll down
				if (pt.Y > ItemHeight) 
				{
					// if within one node of top, scroll quickly
					if (node.NextVisibleNode!= null) 
					{
						node = node.NextVisibleNode;
					}
					node.EnsureVisible();
					m_Ticks = DateTime.Now.Ticks;
				} 
				else if (pt.Y > (ItemHeight * 2)) 
				{
					// if within two nodes of the top, scroll slowly
					if (ts.TotalMilliseconds > 250)
					{
						node = node.NextVisibleNode;
						if (node.NextVisibleNode != null) 
						{
							node = node.NextVisibleNode;
						}
						node.EnsureVisible();
						m_Ticks	= DateTime.Now.Ticks;
					}
				}
			} 
			catch (Exception e2) 
			{
				OnError(e2);
			}
				
		}



	}
}
