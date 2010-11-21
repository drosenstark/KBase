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
using System.Windows.Forms;
using Kbase.Icon;
using System.Collections.Generic;
using Kbase.LibraryWrap;

namespace Kbase.SnippetTreeView
{
	/// <summary>
	/// The IconSelector form for user to choose custom icons.
	/// A lot of the stuff is in the package Kbase.Icon.
	/// </summary>
	public class IconSelector : Form
	{

		IconSelectorTreeView treeView = new IconSelectorTreeView ();

		public IconSelector ()
		{
			this.Height = 500;
			this.Width = 200;
			treeView.Size = this.ClientSize;
			this.Controls.Add (treeView);
			this.Text = "Select An Icon";
		}

		public string SelectedIcon {
			get {
				TreeNodeIconSelector node = treeView.SelectedNodeCached;
				
				// we should be able to use SelectedNode, but Mono changes it to something else
				if (node == null)
					node = treeView.SelectedNode as TreeNodeIconSelector;
				return node.iconSet.FancyName;
			}
			set {
				bool breakOut = false;
				foreach (TreeNode topLevelNode in treeView.Nodes) {
					foreach (TreeNodeIconSelector node in topLevelNode.Nodes) {
						// find the node to select (cannot be a top level node
						if (node.iconSet.FancyName == value) {
							treeView.ResetSelectedNode (node);
							breakOut = true;
							break;
						}
						if (breakOut)
							break;
					}
				}
			}
		}
		
		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
			treeView.Size = ClientSize;
		}
		
		
	}
	
}
