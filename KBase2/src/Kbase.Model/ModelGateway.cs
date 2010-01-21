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

namespace Kbase.Model
{
	/// <summary>
	/// This is the Model, if you will, or the only known gateway to 
    /// an OO model. Every model implements a ModelGateway.
    /// Basically, it is the main handle for the UI to get a TopLevelSnippet
    /// from one can navigate anywhere.
	/// </summary>
	public abstract class ModelGateway
	{

        public abstract void Search(IList<Search.SearchCriterion> criteria);
        public abstract void SearchRepeat(Snippet oldSearch, IList<Search.SearchCriterion> criteria);

		public abstract bool Dirty {
			get;
			set;
		}

        public bool SuspendEvents;

		public abstract Snippet TopLevelSnippet 
		{
			get;
		}

        public abstract Snippet LastXSnippet
        {
            get;
        }

        public abstract void AddSnippetToLastXSnippets(Snippet snippet);

        public object UnknownPieceToReserialize = null;

        public abstract void PreFetchChildrenAndGrandChildren(Snippet snippet);
		
		public abstract Snippet FindNextSnippetContaining(Snippet startPoint, string text);
 
		/// <summary>
		/// 
		/// </summary>
		/// <param name="title"></param>
		/// <returns>the Snippet if there is one, null otherwise</returns>
		public abstract Snippet FindSnippet(string title);

        public Snippet FindTopLevelSnippet(string exactTitleText) {
            Snippet retVal = null;
            // the top level children are always visible and therefore loaded in every model
            foreach (Snippet child in TopLevelSnippet.Children) {
                if (child.Title.Equals(exactTitleText))
                    return child;
            }
            return retVal;
        }

		/// <summary>
		/// necessary for the hyperlinks between snippets
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public abstract Snippet FindSnippet(int id); 

		public abstract void PrintInfo();

		public abstract void StartRunawayThreads();

        public abstract void Reset();

        public abstract bool IsResetable
        {
            get;
        }
	}
}
