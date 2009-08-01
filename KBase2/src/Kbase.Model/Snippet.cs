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
using System.Collections.Generic;
using System.Diagnostics;
using Kbase.Model.Search;
using System.ComponentModel;
namespace Kbase.Model
{

	public delegate void SnippetChildEventHandler(Snippet child);
	public delegate void SnippetParentEventHandler(Snippet parent);
	public delegate void SnippetEventHandler();


	/// <summary>
    /// Subclasses should be concerned with their data implementation
    /// (talking to a DB or all in memory or whatever).
    /// 
	/// CAREFUL! *Subclasses* should NOT interact with 
    /// SnippetInstance (nor with its subclasses) nor with  
    /// WinForms directly. Since the present class handles the UI 
    /// interaction (view updates). Subclasses should set the UI 
    /// variable and that's it.  See ModelInMemory for examples.
	/// </summary>
    [System.ComponentModel.DefaultProperty("Title")
    ]
    public abstract class Snippet
	{

		// cached local variables for the UI
		public SnippetUI UI = null;
        // if this is the actual TopLevel master snippet guy
		public bool TopLevel = false;

        [Browsable(true), ReadOnly(true),
            Bindable(false), DefaultValueAttribute(""), DesignOnly(false),
            DescriptionAttribute("Is this is a search snippet?"), System.ComponentModel.DisplayName("Is Search Snippet?")
        ]
        public abstract bool HasSearchSaved
        {
            get;
        }


        [Browsable(true), DisplayName("Search Criteria")]
        public string[] SearchCriteriaForDisplay
        {
            get {
                List<string> retVal = new List<string>();
                if (HasSearchSaved && Criteria.Count > 0)
                {
                    foreach (SearchCriterion criterion in Criteria) {
                        retVal.Add(new Kbase.Serialization.SerializableCriterion(criterion).GetAsText());
                    }
                }
                else {
                    retVal.Add("Not a search snippet");
                }
                return retVal.ToArray();
            
            }

            set {
                try
                {
                    // this is cool, the user can actually type in new search criteria...
                    // the really correct way to do this would be to pop up the search box,
                    // but I don't know how to do that yet
                    List<SearchCriterion> criteria = new List<SearchCriterion>();
                    foreach (string criterionText in value) { 
                        criteria.Add(new Kbase.Serialization.SerializableCriterion(criterionText).GetCriterion());
                    }
                    // made it, set it
                    Criteria = criteria;
                }
                catch { 
                    // do nothing, there's some problem deserializing from the text
                    // maybe the user screwed it up, 
                    // this catch block will force him to start over
                
                }            
            
            }
        }
        
        
        [Browsable(false)]
        public abstract IList<SearchCriterion> Criteria
        {
            get;
            set;
        }

        [Browsable(true), ReadOnly(true),
            Bindable(false), DefaultValueAttribute(""), DesignOnly(false),
            DescriptionAttribute("Change title for Snippet"), System.ComponentModel.DisplayName("Id")]
		public abstract int Id
		{
			get; 
			set;
		}

        [Browsable(true), ReadOnly(false),
            Bindable(false), DefaultValueAttribute(""), DesignOnly(false),
            DescriptionAttribute("Change created date for the Snippet"), System.ComponentModel.DisplayName("Created Date")
        ]
        public abstract DateTime Created 
		{
			get;
			set;
		}

        /// <summary>
        /// Implementers are responsible for making sure the modified date changes when the snippet is modified
        /// For instance, if the text is changed, if the title is changed
        /// </summary>
        [Browsable(true), ReadOnly(false),
            Bindable(false), DefaultValueAttribute(""), DesignOnly(false),
            DescriptionAttribute("Change modified date for the Snippet"), System.ComponentModel.DisplayName("Modified Date")
        ]
        public abstract DateTime Modified
        {
            get;
            set;
        }
		

        [Browsable(false)]
		public abstract int ChildrenCount
		{
			get;
		}

        [Browsable(false)]
        public abstract int ParentCount
		{
			get;
		}

        [Browsable(true), ReadOnly(false),
            Bindable(false), DefaultValueAttribute(""), DesignOnly(false),
            DescriptionAttribute("Single line of title text"), System.ComponentModel.DisplayName("Title")
        ]
        public abstract string Title
        {
            get;
            set;
        }


        /// <summary>
        /// implementers MUST call OnTextChange
        /// </summary>
        [Browsable(false)]
        public abstract string TextReadOnce
        {
            get;
        }
        
        /// <summary>
		/// implementers MUST call OnTextChange
		/// </summary>
        [Browsable(false)]
        public abstract string Text
        {
            get;
            set;
        }

        /// <summary>
        /// </summary>
        [Browsable(false), Description("Search Snippet Parent?")]
        public bool IsSearchSnippetParent
        {
            get {
                return (this.Title.Equals(SearchContainer.SEARCH_RESULTS) && this.Parents.Contains(Universe.Instance.ModelGateway.TopLevelSnippet));
            }
        }




        [Browsable(false)]
        public abstract string Color
        {
            get;
            set;
        }

        [Browsable(true), DisplayName("Icon Text"), ReadOnly(true)]
        public abstract string Icon
        {
            get;
            set;
        }

        [Browsable(false)]
		public abstract List<Snippet> Children {get;}

        [Browsable(false)]
        public abstract List<Snippet> Parents { get;}



		#region Methods to avoid loops in the hierarchy OnBeforeAdd
		/// <summary>
		/// this is too costly to make public.
		/// </summary>
		/// <param name="to"></param>
		/// <returns></returns>
		protected virtual void OnBeforeAdd(Snippet child) {
            if (Children.Contains(child))
                throw new NewChildIsAncestorException(child, this);
			
			// checking if THIS is an ancestor of TO
			// if it is we throw an exception
            child.IsAncestorOf(this);
		}

        public virtual void OnBeforeExpand() { 
            
        }


		/// <summary>
		/// here we go up through the ancestors of TO to see if THIS exists
		/// Can be overriden if the model has a better way to do it.
		/// </summary>
		/// <param name="snippet"></param>
		/// <throws></throws>
		protected virtual void IsAncestorOf(Snippet to) 
		{
			if (this == to)
				throw new NewChildIsAncestorException(this,to);
			foreach (Snippet parent in to.Parents) {
				IsAncestorOf(parent);
			}
		}
		#endregion


		/// This is now overridable because moves are inherently
        /// transactional (kbase can be left in an inconsistent state).
        /// Overriding methods must:
        /// 1) call UI.OnBeforeMove(from)
        /// 2) call UI.OnAfterMove(to)
		/// 3) throw IllegalMoveOrCopyException.
		public virtual void MoveSnippet(Snippet from, Snippet to) 
		{
            UI.OnBeforeMove(from);
			to.AddChildSnippet(this);
			from.RemoveChildSnippet(this);
            UI.OnAfterMove(to);
		}

		/// NOT overridable.
		/// Throws IllegalMoveOrCopyException.
		public void CopySnippet(Snippet to) 
		{
			to.AddChildSnippet(this);
			to.UI.ShowChildren();
			to.UI.ShowGrandChildren();
		}

        public Snippet GetPropertyValue(Snippet property) {
            Snippet retVal = null;
            foreach (Snippet parent in Parents) { 
                if(parent.Parents.Contains(property)) {
                    // we could jump out for efficiency, but we continue because we want exclusive properties
                    if (retVal != null)
                    {
                        throw new Kbase.Properties.MultiplePropertiesSelectedException();
                    }
                    retVal = parent;                    
                }
            }
            return retVal;
        }

        public List<Snippet> GetMultiplePropertyValues(Snippet property)
        {
            List<Snippet> retVal = new List<Snippet>();
            foreach (Snippet parent in Parents)
            {
                if (parent.Parents.Contains(property))
                {
                    retVal.Add(parent);
                }
            }
            return retVal;
        }



		public bool WillDeleteRemoveSnippet() {
			return (ParentCount == 1);
		}
		
		/// <summary>
		/// deletes from one parent
		/// NOT overridable.
		/// </summary>
		/// <param name="from"></param>
		public void DeleteSnippet(Snippet from) 
		{
			from.RemoveChildSnippet(this);
		}

		/// <summary>
		/// deletes from all parents.  
		/// NOT overridable.
		/// </summary>
		public void DeleteSnippet() 
		{
			foreach (Snippet parent in Parents) 
			{
				parent.RemoveChildSnippet(this);
			}
		}
		
		public abstract void RemoveChildSnippet(Snippet child);
	
		public abstract void RemoveAllChildSnippets();

		/// <returns>the child</returns>
		public abstract Snippet AddChildSnippet(); 

		public abstract void AddChildSnippet(Snippet child); 

		public abstract void SortChildrenByTitle();
        public abstract void SortChildrenByIcon();
        public abstract void SortChildrenByCreateDate();
        public abstract void SortChildrenByModifiedDate();

		public abstract void MoveUpChild(Snippet child);
		public abstract void MoveDownChild(Snippet child);


		public abstract void RemoveFromMemory();


        public Kbase.DetailPanel.ExternalSnippet WatchSnippet = null;

        public bool WatchingExternalFile {
            get {
                return (WatchSnippet != null);
            }
        }


	}
}
