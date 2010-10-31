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
using System.Windows.Forms;
using Kbase.Model;

namespace Kbase.Properties.ParentPane
{
    class ParentPaneNode : TreeNode
    {


        public Snippet snippet;
        SnippetInstance instance = null;
        public SnippetInstance Instance {
            get {
                if (instance == null)
                    instance = snippet.UI.GetAnyInstance();
                return instance;
            }
            set {
                instance = value;
            }
        }

        public ParentPaneNode(Snippet snippet) {
            this.snippet = snippet;
            Redecorate();
        }

        public void Redecorate() {
            Text = snippet.Title;
            ImageIndex = Icon.IconList.Instance.GetIconIndexUnselected(snippet.Icon);
            SelectedImageIndex = Icon.IconList.Instance.GetIconIndexSelected(snippet.Icon);
            BackColor = Kbase.SnippetTreeView.SnippetPane.ConvertToColor(snippet.Color);
        }


    }
}
