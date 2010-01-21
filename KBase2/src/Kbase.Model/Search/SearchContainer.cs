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
using System.Text;
using System.Windows.Forms;
using Kbase.SnippetTreeView;
using System.Collections.Generic;
using Kbase.Search;
using Kbase.Icon;
using Kbase.MainFrm;
using Kbase.Model.Search;


namespace Kbase.Model
{
    
    /// <summary>
    /// this contains all the UI pieces, subclasses of the present class
    /// (for the distinct model types) contain the actual searching mechanism 
    /// (only the Search method is abstract, really).
    /// </summary>
	public abstract class SearchContainer
	{
		
		protected TimeSpan timer;
    
			
		public SearchContainer() 
		{
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <param name="oldSearch">The oldSearch is used if we are repèating a search. It will
        /// be the parent of the new Search Results.</param>
        public void SearchAndDisplay(IList<SearchCriterion> searchCriteria, Snippet oldSearch) {
            DateTime StartTime = DateTime.Now;

            if (oldSearch != null)
            {
                // clean out the oldSearch of all children
                oldSearch.RemoveAllChildSnippets();
            }

            List<Snippet> results = Search(searchCriteria);
            DateTime StopTime = DateTime.Now;
            timer = StopTime - StartTime;
            string seconds = Convert.ToString(timer.TotalSeconds);
            if (results.Count == 0)
            {
                Universe.Instance.mainForm.SetStatus("Search completed in " + seconds + " seconds. No results found.");
                return;
            }
            AddToPane(searchCriteria, results, oldSearch);
            Universe.Instance.mainForm.SetStatus("Search completed in " + seconds + " seconds. " + results.Count + " results found.");
            
        }

        protected abstract List<Snippet> Search(IList<SearchCriterion> searchCriteria);

        protected void AddToPane(IList<SearchCriterion> criteria, List<Snippet> results,
            Snippet oldSearch)
        {
            if (oldSearch != null)
            {
                // clean the results if they contain the oldSearch
                if (results.Contains(oldSearch))
                    results.Remove(oldSearch);
            }

            Snippet resultsSnippet = null;
            resultsSnippet = MakeResultsSnippet(criteria, results, oldSearch);
            
            foreach (Snippet result in results)
            {
                resultsSnippet.AddChildSnippet(result);
            }
            
            // change the selection to him
            resultsSnippet.UI.GetAnyInstance().SelectExclusive();
        }

        Snippet GetSearchParent() {

            // find the search parent if it's there, otherwise make it
            Snippet searchParent = Universe.Instance.ModelGateway.FindTopLevelSnippet(SEARCH_RESULTS);
            if (searchParent == null)
            {
                searchParent = Universe.Instance.ModelGateway.TopLevelSnippet.AddChildSnippet();
                searchParent.Title = SEARCH_RESULTS;
                searchParent.Icon = Icon.IconList.Instance.searchIcon;
            }
            return searchParent;
        }

        Snippet MakeResultsSnippet(IList<SearchCriterion> criteria, 
            List<Snippet> results, Snippet oldSearch) 
        {

            StringBuilder searchResultsText = new StringBuilder();
            StringBuilder searchResultsTitle = new StringBuilder();
            foreach (SearchCriterion criterion in criteria)
            {
                if (IsValid(criterion))
                {
                    searchResultsText.Append(GetDescription(criterion));
                    searchResultsTitle.Append(GetShortDescription(criterion));
                }
            }
            searchResultsTitle.Append(" (" + results.Count + ")");

            string seconds = Convert.ToString(timer.TotalSeconds);
            searchResultsText.Append("\n" + results.Count + " snippets found (" + seconds + " seconds).");

            Snippet snippet = null;
            if (oldSearch == null)
            {
                snippet = GetSearchParent().AddChildSnippet();
                snippet.Icon = IconList.Instance.searchIcon;
                snippet.Title = "Search for " + searchResultsTitle.ToString();
            }
            else
                snippet = oldSearch;
            snippet.Text = DateTime.Now + "\n" + searchResultsText.ToString();
            snippet.Criteria = criteria;
            return snippet;
        }

		public const string SEARCH_RESULTS = "Search Results";


        protected string GetDescription(SearchCriterion criterion)
        {
			System.Text.StringBuilder builder = new System.Text.StringBuilder(20);
			if (criterion.ConcatWithLast != SearchTypeConcat.None) 
			{
				builder.Append("\n");
				builder.Append(criterion.ConcatWithLast);
				builder.Append("\n");
			}	
			builder.Append("Snippets whose ");
			if (criterion.Where != SearchTypeWhere.Own) 
			{
				builder.Append(criterion.Where.ToString().Replace("_"," "));
				if ((criterion.Where & SearchTypeWhere.Own) == 0)
					builder.Append("'s");
				builder.Append(" ");
			}
			builder.Append(criterion.TextTitle.ToString().ToLower());
            if (criterion.TextTitle == SearchTypeTextTitle.Id)
            {
                try
                {
                    builder.Append("=");
                    builder.Append(criterion.Word);
                    string title = Universe.Instance.ModelGateway.FindSnippet(Int32.Parse(criterion.Word)).Title;
                    builder.Append(" (");
                    builder.Append(title);
                    builder.Append(")");
                }
                catch (Exception) { 
                    // no problem, just hide this, snippet not found or id not numeric or 
                    // whatever
                }

            } else {
                builder.Append(" ");
                builder.Append(criterion.IsContains.ToString().ToLower());
                builder.Append(" the text \"");
                builder.Append(criterion.Word);
                builder.Append("\"");
            }


			return builder.ToString();
		}


        protected string GetShortDescription(SearchCriterion criterion) 
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder(10);
			if (criterion.ConcatWithLast != SearchTypeConcat.None) 
			{
				builder.Append(" ");
				builder.Append(criterion.ConcatWithLast);
				builder.Append(" ");
			}

            if (criterion.TextTitle == SearchTypeTextTitle.Id)
            {
                builder.Append("id=");
                builder.Append(criterion.Word);
            }
            else
            {
                builder.Append("\"");
                builder.Append(criterion.Word);
                builder.Append("\"");
            }
			return builder.ToString();
		}

        protected bool IsValid(SearchCriterion criterion) 
		{
			return (criterion.Word != null && criterion.Word.Length > 0);
		}
	}
	

}
