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
using Kbase.Model;
using System.Diagnostics;
using System.Collections;
using Kbase.SnippetTreeView;

namespace Kbase.Model
{
    /// <summary>
    /// The SnippetInstance is used by the SnippetUI (one SnippetUI per Snippet).
    /// 
    /// Since TheKBase Desktop is multiple-hierarchical, each Snippet may appear
    /// more than once in the UI. Each visual node is represented by a SnippetInstance.
    /// This IS a facade for a SnippetTNode: each SnippetInstance contains one.
    /// However, sometimes we even substitute the SnippetTNode (meaning that we
    /// replace the SnippetTNode with which we're "showing" the SnippetInstance), 
    /// so this is logically independent.
    /// </summary>
	public class SnippetInstance
	{
		public SnippetTNode node = null;
		Snippet snippet = null;
		public SnippetInstance parent = null;

		public ArrayList children = new ArrayList();

        protected SnippetInstance() { }

		public SnippetInstance(Snippet snippet, SnippetInstance parent) 
		{
			Debug.Assert(!snippet.IsTopLevel);
			this.Snippet = snippet;
			this.Snippet.UI.SnippetInstances.Add(this);
			this.parent = parent;
			this.node = new SnippetTNode(snippet.Title, this);
		}	
	
		public Snippet Snippet {
			get { return snippet; }
			set { snippet = value;}
		}

		public void Rename(string name)
		{
            node.Text = name;
            node.RedoPaintjob();
		}

		public void Highlight()
		{
			node.Highlight();
		}

		public void Unhighlight()
		{
			node.RemoveHighlight();			
		}


		public void OnAfterSelectExclusive()
		{
			foreach (SnippetInstance instancee in Snippet.UI.SnippetInstances) 
			{
				if (instancee != this) 
				{
					instancee.Highlight();
				}
			}
		}

        public void SelectExclusive()
        {
            Universe.Instance.snippetPane.ReplaceSelectionWith(this.node);
        }


        public bool Selected
        {
            get
            {
                return node.Selected;
            }
        }

        public void Select()
        {
            if (node.Selected)
                return;
            else
            {
                Universe.Instance.snippetPane.AddToSelection(node);
            }

        }



        public virtual SnippetInstance AddChild(Snippet child)
		{
			SnippetInstance retVal = new SnippetInstance(child, this);

			children.Add(retVal);
		    node.Nodes.Add(retVal.node);
		    retVal.node.RedoPaintjob();
			return retVal;
		}

		public bool HasChild(Snippet child)
		{
			foreach (SnippetInstance childInstance in children) {
				if (childInstance.Snippet == child)
					return true;
			}
			return false;
		}


		public virtual void ReorderChildren() 
		{
			Hashtable childrenHashed = new Hashtable(children.Count);			
			foreach (SnippetInstance instance in children) {
				childrenHashed.Add(instance.Snippet,instance);
			}
			// wipe the children clear (now that we've got a reference to them)
			children.Clear();
			this.node.Nodes.Clear();
			foreach (Snippet child in Snippet.Children) 
			{
				SnippetInstance instance = (SnippetInstance)childrenHashed[child];
				children.Add(instance);
				this.node.Nodes.Add(instance.node);
			}
		}


		/// <param name="child"></param>
		/// <returns>Return the SnippetInstance, but might return null if this Snippet doesn't have an instance</returns>
		public void RemoveAllChildren()
		{
			ArrayList childrenCopy = new ArrayList(children);

			foreach (SnippetInstance childInstance in childrenCopy) 
			{
				if (Snippet.IsTopLevel) 
				{
					Universe.Instance.snippetPane.Nodes.Remove(childInstance.node);
				} 
				else 
				{
					node.Nodes.Remove(childInstance.node);
				}
				childInstance.WipeInstanceAndAllDescendants();
				children.Remove(childInstance);
			}
		}


		/// <param name="child"></param>
		/// <returns>Return the SnippetInstance, but might return null if this Snippet doesn't have an instance</returns>
		public virtual void RemoveChild(Snippet child)
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
    			node.Nodes.Remove(removeInstance.node);
                Universe.Instance.snippetPane.SelectedNodes.Remove(removeInstance.node);
				removeInstance.WipeInstanceAndAllDescendants();
			}
		}


		public void WipeInstanceAndAllDescendants()
		{
		
			this.parent.children.Remove(this);
			this.Snippet.UI.SnippetInstances.Remove(this);
			ArrayList childrenCopy = new ArrayList(children);
			foreach (SnippetInstance child in childrenCopy) {
				child.WipeInstanceAndAllDescendants();
			}
		}

		public bool Visible
		{
			get
			{
				if (this.Snippet.IsTopLevel)
					return true;
				else
					return this.node.IsVisible;
			}
		}


		public void Repaint()
		{
			node.RedoPaintjob();
		}

    

        public string PathInfo {
            get {
                string retVal = node.FullPath.Replace("\\","/");
                int lastSlash = retVal.LastIndexOf("/");
                if (lastSlash == -1)
                    retVal = "* TOP LEVEL *";
                else
                    retVal = retVal.Substring(0, lastSlash);
                return retVal;
            }
        }

		public override string ToString()
		{
			string retVal;
			retVal = "Instance INFO for " + this.Snippet.Title + ": ";
            Debug.Assert(!node.Dead, "Cannot ask for ToString for an instance whose node is dead. The instance should be dead too!");
            retVal += node.FullPath;
			return retVal;
		}



	}


}
