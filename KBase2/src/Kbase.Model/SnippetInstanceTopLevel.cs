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
using System.Collections.Generic;
using System.Text;

namespace Kbase.Model
{
    /// <summary>
    /// The TopLevel is really special due to WinForms... its children
    /// are actually the children of the Tree
    /// </summary>
    class SnippetInstanceTopLevel : SnippetInstance
    {

		public SnippetInstanceTopLevel(Snippet topLevelSnippet) 
		{
			this.Snippet = topLevelSnippet;
			this.parent = null;
			this.Snippet.UI.SnippetInstances.Add(this);
		}


		#region top of tree stuff, allows the topOfTree to be substituted out for speed
        // in tests, this works about a 35% savings over using BeginUpdate and EndUpdate
		private static IList topOfTheTreeNodes = null;

		/// <summary>
		/// put the top of the tree back in its place!
		/// </summary>
		public static void ReplaceTopOfTree() {
			if (topOfTheTreeNodes != null) 
			{
				foreach (System.Windows.Forms.TreeNode node in topOfTheTreeNodes) 
				{
					Universe.Instance.snippetPane.Nodes.Add(node);
				}
			}
			topOfTheTreeNodes = Universe.Instance.snippetPane.Nodes;
		}

		public static void ReplaceTopOfTree(System.Collections.IList list) {
			topOfTheTreeNodes = list;	
		}
		#endregion methods for top of tree optimization stuff


        public override SnippetInstance AddChild(Snippet child)
		{
			SnippetInstance retVal = new SnippetInstance(child, this);

			children.Add(retVal);

			// we allow the topOfTheTree to be substituted out for speed
			if (topOfTheTreeNodes == null)
				ReplaceTopOfTree();
			topOfTheTreeNodes.Add(retVal.node);
			retVal.node.RedoPaintjob();
			return retVal;
		}

		public override void ReorderChildren() 
		{
			Hashtable childrenHashed = new Hashtable(children.Count);			
			foreach (SnippetInstance instance in children) {
				childrenHashed.Add(instance.Snippet,instance);
			}
			// wipe the children clear (now that we've got a reference to them)
			children.Clear();
			topOfTheTreeNodes.Clear();
			foreach (Snippet child in Snippet.Children) 
			{
				SnippetInstance instance = (SnippetInstance)childrenHashed[child];
				children.Add(instance);
                topOfTheTreeNodes.Add(instance.node);
			}
		}


        /// <param name="child"></param>
        /// <returns>Return the SnippetInstance, but might return null if this Snippet doesn't have an instance</returns>
        public override void RemoveChild(Snippet child)
        {
            SnippetInstance removeInstance = null;
            // gotta find the correct child which was representing this snippet
            foreach (SnippetInstance childInstance in children)
            {
                if (childInstance.Snippet == child)
                {
                    removeInstance = childInstance;
                    break;
                }
            }

            if (removeInstance != null)
            {
                Universe.Instance.snippetPane.Nodes.Remove(removeInstance.node);
                Universe.Instance.snippetPane.SelectedNodes.Remove(removeInstance.node);
                removeInstance.WipeInstanceAndAllDescendants();
            }
        }




    }
}
