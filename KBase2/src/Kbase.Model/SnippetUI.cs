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
using System.Diagnostics;
using System.Collections.Generic;

namespace Kbase.Model
{
	/// <summary>
	/// This handles all User Interface functions for a Snippet.
    /// Note: this class is independent from WinForms and from 
    /// the SnippetPane. The SnippetInstances actually handle 
    /// the final UI (and encapsulate the TreeViewNodes, etc.).
    /// 
    /// Originally there were several models and needs for different 
    /// expand-collapse schemes. If one is to use only the ModelInMemory,
    /// this class could be combined with SnippetUINoCollapse
	/// </summary>
	public abstract class SnippetUI
	{

		protected Snippet snippet = null;
		public List<SnippetInstance> SnippetInstances = new List<SnippetInstance>();
        string rtf = null;

		public SnippetUI(Snippet snippet)
		{
			this.snippet = snippet;
		}


		
        bool OnBeforeMoveSelected = false;
        public void OnBeforeMove(Snippet oldParent) {
            SnippetInstance instance = GetInstanceUnder(oldParent);
            Debug.Assert(instance != null);
            OnBeforeMoveSelected = instance.Selected;
        
        }
        public void OnAfterMove(Snippet newParent)
        {
            newParent.UI.ShowChildren();
            newParent.UI.ShowGrandChildren();

            SnippetInstance instance = GetInstanceUnder(newParent);
            Debug.Assert(instance != null);
            if (OnBeforeMoveSelected)
            {
                instance.Select();
            }
            OnLocationInfoChange();
        }

		public void OnAfterRemoveChildSnippet(Snippet child) 
		{
			foreach (SnippetInstance instance in SnippetInstances) 
			{
				// NOTE that this doesn't just remove the child, but also the child's 
                // children in terms of the Snippet instances.
				instance.RemoveChild(child);
			}

			// this will leave some Snippets as loaded that are actually
			// not visible.  In some future, it should be possible to 
			// clean these up.  The problem is that you cannot trace down the 
			// Snippets.children because that would inadvertendly load the whole 
			// model!
			
			// if the child is not being shown anywhere, it
			// can be cleared out from local memory
			// HOWEVER, what that means depends on the implementation
			// of the Snippet of RemoveFromMemory. 
			// For the InMemoryModel it probably won't do anything
			if (child.UI.SnippetInstances.Count == 0)
				child.RemoveFromMemory();
        }


		public bool IsShowingChild(Snippet child) 
		{
			foreach (SnippetInstance instance in SnippetInstances) 
			{
				if (instance.HasChild(child))
					return true;
			}
			return false;
		}


		public void OnAfterAddChildSnippet(Snippet child) 
		{
            if (this.snippet.TopLevel)
                child.UI.ShowChildren();
            SnippetInstance childInstance;
			foreach (SnippetInstance instance in SnippetInstances) 
			{
				if (!instance.HasChild(child))
				{
					// Add a child to the UI (and the snippet) for
					// the current INSTANCE in the tree
					childInstance = instance.AddChild(child);

					// If child comes wih children AND IT IS SHOWING CHILDREN, 
					// those need to be added to the UI as well.
					if (child.UI.ShowingChildren)
						OnAfterAddChildShowDescendants(childInstance,child.Children);
				}
			}
		}


		/// <summary>
		/// For the current INSTANCE in the tree, add a SnippetInstance for each of the CHILDREN snippets
		/// </summary>
        protected abstract void OnAfterAddChildShowDescendants(SnippetInstance instance, List<Snippet> children);

        public void OnAfterReorder() 
		{
			foreach (SnippetInstance instance in SnippetInstances) {
				instance.ReorderChildren();
			}
		}


		public virtual SnippetInstance GetAnyInstance() 
		{
			Debug.Assert(SnippetInstances.Count > 0);

            SnippetInstance any = (SnippetInstance)SnippetInstances[0];
            // now we are trying to get a visible instance, if possible
            foreach (SnippetInstance instance in SnippetInstances) {
                if (instance.Visible)
                {
                    any = instance;
                    break;
                }
            }
			return any;
		}

		/// <summary>
		/// </summary>
		/// <param name="parent"></param>
		/// <returns>SnippetInstance, or null if none found</returns>
		public SnippetInstance GetInstanceUnder(SnippetInstance parent) 
		{
			foreach (SnippetInstance instance in SnippetInstances) 
			{
				if (instance.parent == parent)
					return instance;
			}
			return null;
		}


        /// <summary>
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>SnippetInstance, or null if none found</returns>
        public SnippetInstance GetInstanceUnder(Snippet parent)
        {
            foreach (SnippetInstance instance in SnippetInstances)
            {
                if (instance.parent.Snippet == parent)
                    return instance;
            }
            return null;
        }

		public bool IsAnyInstanceVisible() {
			foreach (SnippetInstance instance in SnippetInstances) {
				if (instance.Visible)
					return true;
			}
			return false;
		}

		public bool IsJustOneInstanceVisible() {
			int instancesVisible = 0;
			foreach (SnippetInstance instance in SnippetInstances) {
				if (instance.Visible)
					instancesVisible++;
			}
			return (instancesVisible == 1);
		}

		/// <summary>
		/// Gets the instance of THIS underneath the top level
		/// </summary>
		/// <param name="parent"></param>
		/// <returns>SnippetInstance, or null if none found</returns>
		public SnippetInstance GetInstanceUnderTopLevel() 
		{
			SnippetInstance topInstance = Universe.Instance.ModelGateway.TopLevelSnippet.UI.GetAnyInstance();
			return GetInstanceUnder(topInstance);
		}

        public abstract void ShowGrandChildren();

        /// <summary>
        /// thanks to UpdateUIAfterAddChildSnippet this can be called more than once with no negative effects
        /// </summary>
        public abstract void ShowChildren();


        public abstract void HideGrandChildren();

        public abstract void HideChildren();




		public virtual void Show(Kbase.Model.Snippet child)
		{
			if (ShowingChildren) 
			{
				OnAfterAddChildSnippet(child);
			}
		}

		public virtual void Hide(Kbase.Model.Snippet child)
		{
			if (IsShowingChild(child)) 
			{
				OnAfterRemoveChildSnippet(child);
			}
		}



        public abstract bool ShowingChildren
        {
            get;
            set;
        }

		public void ForceShowingChildren(bool showingChildren) {
			this.ShowingChildren = showingChildren;
		}

		/// <summary>
		/// should the UI cascade on collapse to allow for the freeing up of resources?
		/// </summary>
        public abstract bool CascadeOnCollapse
        {
            get;
        }

        public virtual string LocationInfo
        {
            get {
                System.Text.StringBuilder retVal = new System.Text.StringBuilder();
                retVal.Append("--- Other Locations ---\n");
                System.Text.StringBuilder firstPath = new System.Text.StringBuilder(); 
                firstPath.Append("--- Selected Location ---\n");
                foreach (SnippetInstance instance in SnippetInstances) {
                    if (instance.Selected)
                    {
                        firstPath.Append(instance.PathInfo);
                        firstPath.Append("\n");
                    }
                    else { 
                        retVal.Append(instance.PathInfo);                    
                        retVal.Append("\n");                    
                    }                    
                }
                return firstPath + retVal.ToString();
            }
        }


        public event SnippetEventHandler TitleChange = null;
        public virtual void OnTitleChange()
        {
            if (TitleChange != null)
                TitleChange();
            foreach (SnippetInstance instance in SnippetInstances)
            {
                instance.Rename(snippet.Title);
            }
            Universe.Instance.OnAfterSelectOnCachedNodeChanged(snippet);
        }



        public event SnippetEventHandler ColorChange = null;
        public virtual void OnColorChange()
        {
            if (ColorChange != null)
                ColorChange();
            foreach (SnippetInstance instance in SnippetInstances)
            {
                instance.Repaint();
            }

            Universe.Instance.OnAfterSelectOnCachedNodeChanged(snippet);
        }

        public event SnippetEventHandler IconChange = null;
        public virtual void OnIconChange()
        {
            if (IconChange != null)
                IconChange();
            foreach (SnippetInstance instance in SnippetInstances)
            {
                instance.Repaint();
            }

            Universe.Instance.OnAfterSelectOnCachedNodeChanged(snippet);
        
        }

        /// <summary>
        /// implementers MUST call OnTextChange
        /// </summary>
        public virtual string Rtf
        {
            get { return rtf; }
            set
            {
                this.rtf = value;
            }
        }

        // clients register interest with the LocationInfoChange for a snippet
        // public event SnippetEventHandler LocationInfoChange = null;
        public virtual void OnLocationInfoChange()
        {
            // if (LocationInfoChange != null)
                // LocationInfoChange();
            Universe.Instance.OnAfterSelectOnCachedNodeChanged(snippet);
        }


	}
}
