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
using Kbase.Model;
using System.Xml.Serialization;
using System.Collections.Generic;
using Kbase.Model.Search;

namespace Kbase.Serialization
{
	/// <summary>
	/// Summary description for SerializableSnippet.
	/// </summary>
	[XmlType("Snippet")]
    [Serializable()]
    public class SerializableSnippet
	{


		[XmlElement("Child",typeof(int))]
		public List<int> children = new List<int>();

		public string Text;
		public string Title;
		public DateTime Created;
        public DateTime Modified;
        public string Color;
		public string Icon;
		public int Id;
        public List<SerializableCriterion> Criteria = new List<SerializableCriterion>();

		public SerializableSnippet()
		{
		}


		public SerializableSnippet(Snippet snippet)
		{
			foreach (Snippet child in snippet.Children) 
			{
				children.Add(child.Id);
			}

			Text = snippet.TextReadOnce;
			Title = snippet.Title;
			Created = snippet.Created;
            Modified = snippet.Modified;
            Color = snippet.Color;
			Icon = snippet.Icon;
			Id = snippet.Id;
            if (snippet.HasSearchSaved) {
                foreach (SearchCriterion criterion in snippet.Criteria) {
                    if (!criterion.IsBlank())
                        Criteria.Add(new SerializableCriterion(criterion));                
                }
            }
		}

		public Snippet MakeSnippetInModel(Snippet where, Hashtable serializableSnippets,
            bool merge) 
		{
			// if we have no parents at all, then we need to be created in the parent where
			if (cachedSnippet == null) 
			{
				cachedSnippet = where.AddChildSnippet();
				cachedSnippet.Text = Text;
				cachedSnippet.Title = Title;
				cachedSnippet.Color = Color;
				cachedSnippet.Created = Created;
                cachedSnippet.Modified = Modified;
                cachedSnippet.Icon = Icon;
                if (Criteria != null && Criteria.Count > 0) {
                    List<SearchCriterion> searchCriteria = new List<SearchCriterion>();
                    foreach (SerializableCriterion criterion in Criteria) {
                        searchCriteria.Add(criterion.GetCriterion());                    
                    }
                    cachedSnippet.Criteria = searchCriteria;

                }
                if (!merge)
				    cachedSnippet.Id = Id;
				foreach (int child in children) {
					// get the serializable snippet and
					SerializableSnippet sChild = (SerializableSnippet)serializableSnippets[child];
					// make it in the model under cachedSnippet (or just add cachedSnippet as another parent)
					sChild.MakeSnippetInModel(cachedSnippet,serializableSnippets, merge);
				}
			} 
			// otherwise we just add another parent.  The data are already done.
			else {
				where.AddChildSnippet(cachedSnippet);
			}
            System.Diagnostics.Debug.Assert(cachedSnippet != null);
            return cachedSnippet;
		}
		
		// each snippet can have a cached version of it's InMemory counterpart.
        [NonSerialized()]Snippet cachedSnippet = null;
	
	}
}
