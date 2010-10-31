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
using System.Collections;
using System.Text;
using System.Collections.Generic;
using Kbase.Model.Search;

namespace Kbase.ModelInMemory 
{

    /// <summary>
    /// Motor for searches and the wrapper for the search results as they are 
    /// going.
    /// </summary>
	public class SearchContainerInMemory : Kbase.Model.SearchContainer
	{

        SnippetDictionary model = null;
        internal SearchContainerInMemory(SnippetDictionary model) {
            this.model = model;
        }

        protected override List<Kbase.Model.Snippet> Search(IList<SearchCriterion> criteria)
        {
            foreach (SearchCriterion criterion in criteria)
            {
                if (IsValid(criterion))
                    Search(criterion);
            }
            // I don't know how to do this yet, there must be some kind of cast, like ConvertAll maybe
            List<Kbase.Model.Snippet> retVal = new List<Kbase.Model.Snippet>(lastResults.Count);
            foreach (SnippetInMemory snippet in lastResults) {
                retVal.Add(snippet);
            }
            return retVal;
        }

        List<SnippetInMemory> lastResults = new List<SnippetInMemory>();
        protected void Search(SearchCriterion criterion)
        {
            IList<SnippetInMemory> searchIn = null;
            // we only search in the last results if it's an And or a Not
            if (criterion.ConcatWithLast == SearchTypeConcat.And || criterion.ConcatWithLast == SearchTypeConcat.Not)
                searchIn = lastResults;
            // otherwsie we search in the original universe of snippets
            else if (criterion.ConcatWithLast == SearchTypeConcat.Or || criterion.ConcatWithLast == SearchTypeConcat.None)
                searchIn = model.snippets;
            else
                throw new NotSupportedException("Unsupported search type, sorry!");

            List<SnippetInMemory> results = new List<SnippetInMemory>();

            // if it's an OR or a NOT, bring the old results forward
            if (criterion.ConcatWithLast == SearchTypeConcat.Or || criterion.ConcatWithLast == SearchTypeConcat.Not)
                results.AddRange(lastResults);

            foreach (SnippetInMemory snippet in searchIn)
            {
                // AND and OR both need the snippet to not be present already (to not double up) and to match the criterion.
                if (criterion.ConcatWithLast != SearchTypeConcat.Not && !results.Contains(snippet) && snippet.Matches(criterion))
                {
                    results.Add(snippet);
                }
                // the not needs the snippet to already be there to remove it, and then if it matches the criteria we remove it
                else if (criterion.ConcatWithLast == SearchTypeConcat.Not && results.Contains(snippet) && snippet.Matches(criterion))
                {
                    results.Remove(snippet);
                }
            }

            lastResults = results;
        }

	}
	

}
