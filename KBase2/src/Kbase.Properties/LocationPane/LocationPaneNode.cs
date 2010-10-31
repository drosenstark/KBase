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

namespace Kbase.Properties.LocationPane2
{
    class LocationPaneNode : TreeNode
    {
        // later we lose the reference to the real instance, but we need to know it here
        public bool SelectMe { 
            get {
                return OriginalInstance.Selected;
            }
        }

        public SnippetInstance OriginalInstance = null;
        public SnippetInstance Instance = null;
        public LocationPaneNode(SnippetInstance instance) {
            this.Instance = instance;
            // later we lose the reference to the real instance, but we need to know it here
            this.OriginalInstance = instance;
            ChopOffLastPath();
        }

        public void ChopOffLastPath() {
            SnippetInstance parentInstance = Instance.parent;
            if (parentInstance != null)
                Instance = parentInstance;
            Redecorate();
        }

        public void Redecorate() {
            if (Instance.node == null)
            {
                Text = "TOP LEVEL";
                ImageIndex = Icon.IconList.Instance.GetIconIndexUnselected(Universe.Instance.Settings.DefaultIcon);
                SelectedImageIndex = Icon.IconList.Instance.GetIconIndexSelected(Universe.Instance.Settings.DefaultIcon);
                BackColor = Kbase.SnippetTreeView.SnippetPane.ConvertToColor(null);
            }
            else
            {
                Text = Instance.node.FullPath;
                ImageIndex = Icon.IconList.Instance.GetIconIndexUnselected(Instance.Snippet.Icon);
                SelectedImageIndex = Icon.IconList.Instance.GetIconIndexSelected(Instance.Snippet.Icon);
                BackColor = Kbase.SnippetTreeView.SnippetPane.ConvertToColor(Instance.Snippet.Color);
            }
        }

        public void ChopOffAllButParentPath()
        {
            int last = Text.LastIndexOf("\\");
            if (last != -1) {
                Text = Text.Substring(last + 1);
            }
        }

    }
}
