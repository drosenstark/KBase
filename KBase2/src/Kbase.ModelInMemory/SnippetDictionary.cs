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
using System.Diagnostics;
using System.Collections.Generic;
using Kbase.Model.Search;
using Kbase.Model;
using Kbase.LibraryWrap;

namespace Kbase.ModelInMemory
{
	/// <summary>
	/// Implementation of the ModelGateway for all snippets in memory
	/// </summary>
	public class SnippetDictionary : Kbase.Model.ModelGateway
	{

        internal static PersistentIdGenerator idGenerator =
            new PersistentIdGenerator();

        // just to make sure it doesn't get serialized to disk
		public SnippetDictionary()
		{
		}


		private bool dirty = false;
		public override bool Dirty
		{
			get
			{
				return dirty;
			}

			set {
				dirty = value;
			}
		}

        internal IList<SnippetInMemory> snippets = new List<SnippetInMemory>();

		public void Add(SnippetInMemory snippet) {
			if (!snippets.Contains(snippet))
				snippets.Add(snippet);
            Dirty = true;
		}

		public void Remove(SnippetInMemory snippet) {
			// This assert is NOT always valid, because sometimes the UI wipes out
            // a parent and then wipes out children and all kinds of weird shit.
            // It's all ok.
            // Debug.Assert(snippets.Contains(snippet),"Removing non-existent snippet " + snippet + ".");
			snippets.Remove(snippet);
            Dirty = true;
		}


		static SnippetInMemory topLevelSnippet = null;

		public override Kbase.Model.Snippet TopLevelSnippet
		{
			get
			{
				if (topLevelSnippet == null) {
					topLevelSnippet = new SnippetInMemory(this);
					topLevelSnippet.TopLevel = true;
					topLevelSnippet.Title = "*** TOP TOP TOPPPP ***";
					// here is where we decide on WinForms
					Kbase.Model.SnippetInstance instance = new Kbase.Model.SnippetInstanceTopLevel(topLevelSnippet);
					topLevelSnippet.UI.ShowChildren();

                    // this is the first time, so the model is actually not dirty
                    // though we've added this toplevelsnip
                    Dirty = false;
				}
				Debug.Assert(topLevelSnippet.UI.SnippetInstances.Count == 1);			
				return topLevelSnippet;
			}
		}


	
		/// <summary>
		/// Searches after and before the startPoint for a snippet that contains the text (in titles or text)
		/// </summary>
		/// <param name="startPoint"></param>
		/// <param name="text"></param>
		/// <returns>The next snippet containing the text if there is one, otherwise null</returns>
		public override Kbase.Model.Snippet FindNextSnippetContaining(Kbase.Model.Snippet startPoint, string text) {
            int startIndex = snippets.IndexOf(startPoint as SnippetInMemory);

            // search after start 
            for (int i = startIndex + 1; i < snippets.Count; i++)
            {
                SnippetInMemory consider = (SnippetInMemory)snippets[i];
                if (consider.ContainsInTextOrTitle(false, text))
                    return consider;
            }

            // search before start (last chance!)
            for (int i = 0; i < startIndex; i++)
            {
                SnippetInMemory consider = (SnippetInMemory)snippets[i];
                if (consider.ContainsInTextOrTitle(false, text))
                    return consider;
            }

            // searching failed return null
            return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="title"></param>
		/// <returns>the Snippet if there is one, null otherwise</returns>
		public override Kbase.Model.Snippet FindSnippet(string title) {
			SnippetInMemory retVal = null;
			foreach (SnippetInMemory snippet in snippets) {
				if (snippet.Title.Equals(title)) {
					retVal = snippet;
					break;
				}
			}
			return retVal;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="title"></param>
		/// <returns>the Snippet if there is one, null otherwise</returns>
		public override Kbase.Model.Snippet FindSnippet(int id) 
		{
			SnippetInMemory retVal = null;
			foreach (SnippetInMemory snippet in snippets) 
			{
				if (snippet.Id == id) 
				{
					retVal = snippet;
					break;
				}
			}
			return retVal;
		}


		public override void PrintInfo()
		{
			int totalParents = 0;
			int totalChildren = 0;
			int[] snippetsShowingWithInstances = new int[100];
			foreach (SnippetInMemory snippet in snippets) 
			{
				totalParents += snippet.Parents.Count;
				totalChildren += snippet.Children.Count;
				snippetsShowingWithInstances[snippet.UI.SnippetInstances.Count]++;
			}

			
			float averageParents = 0;
			float averageChildren = 0;

			if (snippets.Count > 0) 
			{
				averageParents = (float)totalParents/(float)snippets.Count;
				averageChildren = (float)totalChildren/(float)snippets.Count;
			}

            Logger.Log("Dictionary contains " + snippets.Count + " snippets. Average parents per snippet " + averageParents + " average children " + averageChildren);
            Logger.Log("Top level node child counts " + TopLevelSnippet.Children.Count);
            Logger.Log("Snippets showing with instances: ");

			for (int i=0; i<snippetsShowingWithInstances.Length; i++) {
				if (snippetsShowingWithInstances[i] > 0) {
					Logger.Log("Snippets showing " + i + " instances: " + snippetsShowingWithInstances[i]);
				}				
			
			}
		}

		public override void StartRunawayThreads()
		{
            // does nothing, obviously, we have no runaway threads

		}

        public override void Search(IList<SearchCriterion> criteria)
        {
            SearchContainerInMemory sContainer = new SearchContainerInMemory(this);
            sContainer.SearchAndDisplay(criteria, null);
        }

        public override void SearchRepeat(Snippet oldSearch, IList<SearchCriterion> criteria) {
            SearchContainerInMemory sContainer = new SearchContainerInMemory(this);
            sContainer.SearchAndDisplay(criteria, oldSearch);
        }

        public void ConsistencyCheck() {
            Dictionary<int, SnippetInMemory> hash = new Dictionary<int, SnippetInMemory>();
            foreach (SnippetInMemory s in snippets) {
                if (hash.ContainsKey(s.Id))
                    throw new FatalErrorException("what the fuck!");
                hash.Add(s.Id, s);
            }
        }


        public override void Reset() {
            idGenerator.Reset();
        }

        public override bool IsResetable
        {
            get { return true; }
        }
        public override void PreFetchChildrenAndGrandChildren(Snippet snippet)
        {
        }

	}
}
