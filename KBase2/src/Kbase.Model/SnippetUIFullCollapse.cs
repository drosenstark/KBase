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
using System.Text;

namespace Kbase.Model
{
    /// <summary>
    /// The most aggressive on-demand getting of data and releasing it when it is
    /// hidden. Unless the data on the server are many and changing constantly,
    /// this model is totally inefficient, because you throw away a lot of data.
    /// </summary>
    public class SnippetUIFullCollapse : SnippetUI
    {

		public SnippetUIFullCollapse(Snippet snippet) : base(snippet)
		{

        }

        bool showingChildren = false;
        public override bool ShowingChildren
        {
            get
            {
                return showingChildren;
            }
            set {
                showingChildren = value;
            }
        }

        public override void ShowGrandChildren()
        {
            foreach (Snippet child in snippet.Children)
            {
                child.UI.ShowChildren();
            }
        }

        /// <summary>
        /// thanks to UpdateUIAfterAddChildSnippet this can be called more than once with no negative effects
        /// </summary>
        public override void ShowChildren()
        {
            showingChildren = true;
            foreach (Snippet child in snippet.Children)
            {
                Show(child);
            }
        }


        public override void HideGrandChildren()
        {
            foreach (Snippet child in snippet.Children)
            {
                child.UI.HideChildren();
            }
        }

        public override void HideChildren()
        {
            if (!IsAnyInstanceVisible())
            {
                showingChildren = false;
                foreach (Snippet child in snippet.Children)
                {
                    Hide(child);
                }
            }
        }

        public override bool CascadeOnCollapse
        {
            get
            {
                return true;
            }
        }

        protected override void OnAfterAddChildShowDescendants(SnippetInstance instance, List<Snippet> children)
        {
            if (!instance.Visible)
                return;

            SnippetInstance childInstance;
            foreach (Snippet child in children)
            {
                childInstance = instance.AddChild(child);
                if (childInstance != null)
                {
                    OnAfterAddChildShowDescendants(childInstance, child.Children);
                }
            }
        }

        
    }
    
    
}
