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
using System.Text;

namespace Kbase.Model
{
    /// <summary>
    /// Currently this is not being used, but in certain model implementations
    /// it has been interesting to expand ON-DEMAND but not to destroy data
    /// on collapse (i.e., release nodes from the UI).
    /// </summary>
    public class SnippetUIExpandNoCollapse : SnippetUI
    {

        public SnippetUIExpandNoCollapse(Snippet snippet)
            : base(snippet)
        {

        }

        bool showingChildren = false;
        public override bool ShowingChildren
        {
            get
            {
                return showingChildren;
            }
            set
            {
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


        public override void HideChildren()
        {
        }

        public override void HideGrandChildren()
        {
        }


        public override bool CascadeOnCollapse
        {
            get
            {
                return false;
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
