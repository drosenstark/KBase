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
using Kbase.Icon;
using Kbase.LibraryWrap;

namespace Kbase.SnippetTreeView
{

    class IconSelectorTreeView : TreeView
    {
        public IconSelectorTreeView()
        {

            ImageList = new ImageList();
            // we need all the icons (to show them as open, too)
            // but we only iterate through the closed ones

            TreeNode firstWordNode = new TreeNode("just avoiding a null pointer exception");

            // we need a sorted set
            List<NamedIconSet> sortedList = new List<NamedIconSet>(IconList.Instance.icons.Values);
            sortedList.Sort();

            int i = 0;
            foreach (NamedIconSet set in sortedList)
            {
                string fancyName;
                NamedImage image = set.ImageUnselected;
                fancyName = image.FancyName;
                string lastPartOfFancyName = fancyName.Substring(fancyName.IndexOf(" "));
                TreeNode node = new TreeNodeIconSelector(lastPartOfFancyName, set);


                node.ImageIndex = i++;
                ImageList.Images.Add(set.ImageUnselected.image);

                node.SelectedImageIndex = i++;
                ImageList.Images.Add(set.ImageSelected.image);


                // now create the node where we'll put it if necessary
                string firstWord = fancyName.Substring(0, fancyName.IndexOf(" "));
                if (!firstWord.Equals(firstWordNode.Text))
                {
                    firstWordNode = new TreeNode(firstWord);
                    firstWordNode.ImageIndex = i - 2;
                    Nodes.Add(firstWordNode);
                }

                firstWordNode.Nodes.Add(node);
            }
            initializing = true;
        }

        // you cannot select top level nodes, they're for classifying
        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            base.OnBeforeSelect(e);
            if (Nodes.Contains(e.Node))
                e.Cancel = true;
        }

        public TreeNodeIconSelector SelectedNodeCached = null; // we cache this because on close something else gets selected in Mono

        // so we don't close the form on the first select
        bool initializing = true;
        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            base.OnAfterSelect(e);
            if (initializing)
                initializing = false;
            else
            {
				SelectedNodeCached = SelectedNode as TreeNodeIconSelector;
                this.FindForm().Close();
            }
        }


        public void ResetSelectedNode(TreeNode node)
        {
            initializing = true;
            SelectedNode = node;
        }


    }

}
