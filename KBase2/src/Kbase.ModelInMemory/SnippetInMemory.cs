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
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using Kbase.Model.Search;


namespace Kbase.ModelInMemory
{
    /// <summary>
    /// The model is pretty OO. Snippets here are related with real containment,
    /// and their relationship is a LOT simpler than the relationship between
    /// SnippetInstances.
    /// </summary>
	public class SnippetInMemory : Kbase.Model.Snippet, IComparable
	{

		private int persistentId = -1;
        private List<SnippetInMemory> children = new List<SnippetInMemory>();
        private List<SnippetInMemory> parents = new List<SnippetInMemory>();
		private SnippetDictionary model = null;
		static bool collapsingAndExpanding = false;

        public static SnippetInMemory MakeLastXSnippet(SnippetDictionary model) {
            SnippetInMemory retVal = new SnippetInMemory(model);
            retVal.title = LAST_X_TEXT;
            model.TopLevelSnippet.AddChildSnippet(retVal);
            return retVal;
        }

		public SnippetInMemory(SnippetDictionary model)
		{
			this.model = (SnippetDictionary)model;
			if (collapsingAndExpanding)
				this.UI = new Model.SnippetUIFullCollapse(this);
			else
				this.UI = new Model.SnippetUINoCollapse(this);
            model.Dirty = true;
        }


        public override bool HasSearchSaved
        {
            get {
                return (Criteria != null);
            }
        }


        public override bool IsLastX
        {
            get {
                //if (ConfusionUtilities.Util.IsMono())
                //    return false;
                return (this.Parents.Contains(model.TopLevelSnippet) && LAST_X_TEXT == this.title);
            }
        }


        IList<SearchCriterion> criteria = null;
        public override IList<SearchCriterion> Criteria
        {
            get
            {
                return criteria;
            }
            set
            {
                this.criteria = value;
            }
        }

		private DateTime created = DateTime.Now;
		public override DateTime Created
		{
			get
			{
				return created;
			}
			set {
				created = value;
                model.Dirty = true;
            }
		}

        private DateTime modified; // this is essentially a blank datetime
        public override DateTime Modified
        {
            get
            {
                if (modified == new DateTime()) // if it's blank, we set it to the created for backwards compat
                    modified = Created; // we don't set dirty to true, since this is rule-based updating
                return modified;
            }
            set
            {
                modified = value;
                model.Dirty = true;
            }
        }


		public override int Id
		{
			get 
			{
				if (persistentId == -1)
					persistentId = SnippetDictionary.idGenerator.GetId();
				return persistentId;
			}
			set {
				persistentId = value;
				SnippetDictionary.idGenerator.InsistOnNewMax(value);
                // don't set the model to true, as this will no doubt be called only by the deserializer
            }
		}

		string title = Universe.Instance.Settings.DEFAULT_TEXT;

		public override string Title 
		{
			get 
			{
				return title;
			}

			set 
			{
                if (value != Universe.Instance.Settings.DEFAULT_TEXT)
                    AddToLastXSnippets();
                title = value;
                model.Dirty = true;
                Modified = DateTime.Now;
                UI.OnTitleChange();
            }
		}

		string text = "";


        public override string TextReadOnce
        {
            get
            {
                return Text;
            }
        }

		public override string Text
		{
			get 
			{
				return text;
			}

			set 
			{ 
                AddToLastXSnippets();
				text = value;
                Modified = DateTime.Now;
                model.Dirty = true;
			}
		}



		private string icon = Kbase.Icon.IconList.Instance.DefaultIcon;

		public override string Icon
		{
			get 
			{
                if (icon == null || icon.Length == 0)
                    return "Unknown Icon";
                else 
				    return icon;
			}

			set 
			{
                AddToLastXSnippets();
                icon = value;
                model.Dirty = true;
                UI.OnIconChange();
			}
		}

		string color = null;

		public override string Color
		{
			get 
			{
				return color;
			}

			set 
			{
                AddToLastXSnippets();
                color = value;
                model.Dirty = true;
                UI.OnColorChange();
			}
		}

		public override int ParentCount
		{
			get
			{
				return parents.Count;
			}
		}

		public override int ChildrenCount
		{
			get
			{
				return children.Count;
			}
		}

        public override List<Kbase.Model.Snippet> Children
		{
			get
			{
                List<Kbase.Model.Snippet> retVal = new List<Kbase.Model.Snippet>(children.Count);
                foreach (Kbase.Model.Snippet snippet in children) {
                    retVal.Add(snippet);
                }
				return retVal;
			}
		}

        public override List<Kbase.Model.Snippet> Parents
		{
			get
			{
                List<Kbase.Model.Snippet> retVal = new List<Kbase.Model.Snippet>(parents.Count);
                foreach (Kbase.Model.Snippet snippet in parents)
                {
                    retVal.Add(snippet);
                }
                return retVal;
            }
		}




		public override Kbase.Model.Snippet AddChildSnippet()
		{
			// make a new one
			SnippetInMemory retVal = new SnippetInMemory(this.model);
			if (UI.ShowingChildren)
				retVal.UI.ShowChildren();
			else
				retVal.UI.HideChildren();
			// add it as a child
			AddChildSnippet(retVal);
			// and add it to the model
			model.Add((SnippetInMemory)retVal);
			return retVal;
		}

		public override void AddChildSnippet(Kbase.Model.Snippet child)
		{
            AddChildSnippet((SnippetInMemory)child);
		}

        void AddChildSnippet(SnippetInMemory child) {
            OnBeforeAdd(child);
            // this should only happen using the old serializers, not using the UI
            Debug.Assert(!children.Contains(child), "Parent " + this + ", Child " + child + "." +
                    "Trying to add child that already exists. This should ONLY happen from the old serializer.");

            children.Add((SnippetInMemory)child);
            ((SnippetInMemory)child).parents.Add(this);
            model.Dirty = true;
            UI.Show(child);
        
        }

        public override void RemoveChildSnippet(Kbase.Model.Snippet child)
        {
            RemoveChildSnippet((SnippetInMemory)child);
        }

		void RemoveChildSnippet(SnippetInMemory child)
		{
			children.Remove(child);
			((SnippetInMemory)child).parents.Remove(this);
            model.Dirty = true;
            if (child.Parents.Count == 0) 
			{
				child.RemoveAllChildSnippets();
				model.Remove((SnippetInMemory)child);
			}

            UI.Hide(child);
		}


		public override void RemoveAllChildSnippets()
		{
			// gotta copy the collection because we'll be modifying it
			ArrayList childrenCopy = new ArrayList(children);
			foreach (SnippetInMemory child in childrenCopy) {
				RemoveChildSnippet(child);							
			}
		}

		public override string ToString()
		{
			return "\"" + Title + "\" (" + Id + ") [children=" + ChildrenCount + ", parents=" + ParentCount + ", SnippetInstances " + UI.SnippetInstances.Count + "]";
		}

		public override void MoveUpChild(Kbase.Model.Snippet child)
		{
            MoveUpChild((SnippetInMemory)child);
        }

        void MoveUpChild(SnippetInMemory child) {
			Debug.Assert(children.Contains(child));
			int i = children.IndexOf(child);
			if (i > 0) 
			{
				children.Remove(child);
				children.Insert(i-1, child);
                model.Dirty = true;
                UI.OnAfterReorder();
			}
		}

        public override void MoveDownChild(Kbase.Model.Snippet child)
        {
            MoveDownChild((SnippetInMemory)child);
        }

		void MoveDownChild(SnippetInMemory child)
		{
			Debug.Assert(children.Contains(child));
			int i = children.IndexOf(child);
			if (i < children.Count - 1) 
			{
				children.Remove(child);
				children.Insert(i+1, child);
                model.Dirty = true;
                UI.OnAfterReorder();
			}
		}


		public override void RemoveFromMemory()
		{
            // do nothing 
		}

		public override void SortChildrenByTitle()
		{
			children.Sort();
			UI.OnAfterReorder();
		}

        public override void SortChildrenByCreateDate()
        {
            children.Sort(new ComprarerCreateDate());
            UI.OnAfterReorder();
        }

        public override void SortChildrenByModifiedDate()
        {
            children.Sort(new ComprarerModifiedDate());
            UI.OnAfterReorder();
        }
 
        public override void SortChildrenByIcon()
        {
            children.Sort(new ComprarerIcon());
            UI.OnAfterReorder();
        }



		public int CompareTo(object other) 
		{
            Debug.Assert(other is SnippetInMemory);
            string otherTitle = null; 
			if (other is SnippetInMemory)
				otherTitle = ((SnippetInMemory)other).Title;
			return Title.CompareTo(otherTitle);
        }


        public const string LAST_X_TEXT = "Recent Snippets";


        #region Search Support *****************
        public bool Matches(SearchCriterion criterion)
        {
            // where are we searching?
            // do we need to search the self?
            if ((criterion.Where & SearchTypeWhere.Own) > 0)
            {
                if (ThisMatches(criterion))
                    return true;
            }

            if ((criterion.Where & SearchTypeWhere.Parent) > 0 || (criterion.Where & SearchTypeWhere.Ancestor) > 0)
            {
                if (ParentMatches(criterion))
                    return true;
            }

            return false;
        }

        private bool ThisMatches(SearchCriterion criterion)
        {
            // we do not allow searching on the Search Snippet Parent
            if (this.IsSearchSnippetParent)
                return false;

            // We use the & for the ifs so that you can then add things, like 
            // icon OR title contains

            // search the create 
            if ((criterion.TextTitle & SearchTypeTextTitle.Created) > 0)
            {
                if (Matches(criterion.IsContains, criterion.Word, this.Created))
                    return true;
            }

            // search the create 
            if ((criterion.TextTitle & SearchTypeTextTitle.Modified) > 0)
            {
                if (Matches(criterion.IsContains, criterion.Word, this.Modified))
                    return true;
            }



            // search the icon if necessary
            if ((criterion.TextTitle & SearchTypeTextTitle.Icon) > 0)
            {
                if (Matches(criterion.IsContains, criterion.Word, this.Icon, criterion.IgnoreCase))
                    return true;
            }

            // search the text if necessary
            if ((criterion.TextTitle & SearchTypeTextTitle.Text) > 0)
            {
                if (Matches(criterion.IsContains, criterion.Word, this.Text, criterion.IgnoreCase))
                    return true;
            }
            // search the title
            if ((criterion.TextTitle & SearchTypeTextTitle.Title) > 0)
            {
                if (Matches(criterion.IsContains, criterion.Word, this.Title, criterion.IgnoreCase))
                    return true;
            }
            // search the id
            if ((criterion.TextTitle & SearchTypeTextTitle.Id) > 0)
            {
                if (this.Id == Int32.Parse(criterion.Word))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// searches for date within a string (is or contains).  Not really an instance method.
        /// </summary>
        /// <param name="isOrContains"></param>
        /// <param name="searchFor"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool Matches(SearchTypeIsContains isOrContains, string searchFor, DateTime snippetVal)
        {

            // pull off the time
            DateTime searchForDate = DateTime.Parse(searchFor).Date;
            snippetVal = snippetVal.Date; // get just the date part
            int beforeOnAfter = snippetVal.CompareTo(searchForDate);
            if (isOrContains == SearchTypeIsContains.Before)
            {
                if (beforeOnAfter == -1)
                    return true;
            }
            else if (isOrContains == SearchTypeIsContains.After)
            {
                if (beforeOnAfter == 1)
                    return true;
            }
            if (beforeOnAfter == 0)
                return true; // jackpot, the values are equal, it hits all three conditions, 
            // before after and IS
            return false;
        }


        /// <summary>
        /// searches for text within a string (is or contains).  Not an instance method (per se),
        /// but private.
        /// </summary>
        /// <param name="isOrContains"></param>
        /// <param name="searchFor"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool Matches(SearchTypeIsContains isOrContains, string searchFor, string text, bool ignoreCase)
        {
            text = text.ToLower();
            searchFor = searchFor.ToLower();
            if (isOrContains == SearchTypeIsContains.Contains)
            {
                int where = text.IndexOf(searchFor);
                if (where != -1)
                    return true;
            }
            else
            {
                if (text.Equals(searchFor))
                    return true;
            }
            return false;
        }

        private bool ParentMatches(SearchCriterion criterion)
        {
            bool continueToAncestors = ((criterion.Where & SearchTypeWhere.Ancestor) > 0);
            foreach (SnippetInMemory parent in Parents)
            {
                // it is okay to skip the search (and assume false) because it's already 
                // been searched and these are ORs within one SearchCriterion
                if (parent.ThisMatches(criterion))
                    return true;
                if (continueToAncestors)
                {
                    if (parent.ParentMatches(criterion))
                        return true;
                }
            }
            return false;
        }

        public bool ContainsInTextOrTitle(bool caseSensitive, string what)
        {
            string compare;

            if (!caseSensitive)
                what = what.ToLower();

            if (!caseSensitive)
                compare = Title.ToLower();
            else
                compare = Title;

            if (compare.IndexOf(what) != -1)
                return true;

            if (!caseSensitive)
                compare = Text.ToLower();
            else
                compare = Text;

            if (compare.IndexOf(what) != -1)
                return true;

            return false;

        }


        #endregion

        void AddToLastXSnippets() {
            if (ConfusionUtilities.Util.IsMono())
                return; // problems still with this on mono
           if (this.title != LAST_X_TEXT && !this.IsTopLevel && !this.IsLastX)
                model.AddSnippetToLastXSnippets(this);
        }























    }



}
