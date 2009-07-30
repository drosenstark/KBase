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
    /// This SnippetUI overrides the general behavior of collapsing and expanding: showing and hiding
    /// are now suppressed activities in all ways.  All instances are available on the tree at all 
    /// times.  In base class Kbase.Model.SnippetUI, this is not the case: instances are deleted to allow
    /// the Snippet instances (and their accompanying resources, including update checks and so forth) 
    /// to be freed.
    /// </summary>
    public class SnippetUINoCollapse : SnippetUI
    {

		public SnippetUINoCollapse(Snippet snippet) : base(snippet)
		{

        }



        public override bool ShowingChildren
        {
            get
            {
                return true;
            }
            set { 
            
            }
        }

        public override void ShowChildren()
        {
        }

        public override void ShowGrandChildren()
        {
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

        protected override void OnAfterAddChildShowDescendants(Kbase.Model.SnippetInstance instance, List<Kbase.Model.Snippet> children)
        {
            Kbase.Model.SnippetInstance childInstance;
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
