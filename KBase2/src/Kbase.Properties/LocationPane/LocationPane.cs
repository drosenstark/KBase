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
using System.Windows.Forms;
using Kbase.Model;
using Kbase.MainFrm;


namespace Kbase.Properties.LocationPane2
{
    class LocationPane : TreeView
    {
        public LocationPane() {
            ImageList = Icon.IconList.Instance.GetImageList(true);
        }

        bool settingUp;

        public void Edit(SnippetInstance selectedInstance)
        {
            // we never use selectedInstance here, but it might come in handy someday
            settingUp = true;
            Clear();

            List<SnippetInstance> instances = new List<SnippetInstance>(selectedInstance.Snippet.UI.SnippetInstances);
            SnippetInstanceSorter sorter = new SnippetInstanceSorter();
            instances.Sort(sorter);
            int lastId = -1;
            TreeNode parentNode = null;
            foreach (SnippetInstance instance in instances) {
                int id = instance.parent.Snippet.Id;
                if (id != lastId) {
                    parentNode = new LocationPaneNode(instance);
                    Nodes.Add(parentNode);
                    lastId = id;
                }
                LocationPaneNode node = new LocationPaneNode(instance);
                parentNode.Nodes.Add(node);
            }

            SetupNodes2();
            settingUp = false;

        }

        void SetupNodes2() {
            foreach (LocationPaneNode node in Nodes)
            {
                KillOnlyChildrenAndAdjustPaths(node);

                // check if the node has no children if it is selected
                if (node.Nodes.Count == 0)
                    IfSelectedSetAsSelected(node);
                // else if it has children, if any of its children are selected
                foreach (LocationPaneNode child in node.Nodes)
                {
                    IfSelectedSetAsSelected(child);
                }
            }
        }

        void IfSelectedSetAsSelected(LocationPaneNode node) {
            if (node.SelectMe)
                SelectedNode = node;
        }

        // verify that the last parent has at least one child.  If it doesn't, then it shows NO children at all.
        void KillOnlyChildrenAndAdjustPaths(LocationPaneNode parent)
        {
            if (parent != null)
            {
                // if there is just one child, kill it
                if (parent.Nodes.Count == 1)
                {
                    LocationPaneNode child = (LocationPaneNode)parent.Nodes[0];
                    // if that node was selected, now the parent has to be selected.
                    if (SelectedNode == child)
                        SelectedNode = parent;
                    parent.Text = child.Text;
                    parent.Nodes.Clear();
                }
                // otherwise we're showing a parent, so we can remove the last path
                else {
                    foreach (LocationPaneNode child in parent.Nodes) {
                        child.ChopOffLastPath();
                    }
                    // and the parent gets the short name, because it's showing
                    // the specific paths
                    parent.ChopOffAllButParentPath();
                }
            }
        }

        public void Clear() {
            bool wasSettingUp = settingUp;
            settingUp = true;
            Nodes.Clear();
            settingUp = wasSettingUp;
        }

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            if (settingUp || SelectedNode == null)
                return;
            
            // anyway, cancel
            e.Cancel = true;

            LocationPaneNode node = (LocationPaneNode)e.Node;

            // if the node has children, it can't be selected
            if (node.Nodes.Count > 0)
                return;

            DialogResult result;
            if (Universe.Instance.Settings.DoNotAskUserToConfirmSelectionChanceInLocationPane)
                result = DialogResult.OK;
            else
                result = MessageBox.Show("Change selection to " + node.OriginalInstance.node.FullPath + "?", MainForm.DialogCaption, MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                node.OriginalInstance.SelectExclusive();
            }
            
        }
    }
    class SnippetInstanceSorter : IComparer<SnippetInstance> {

        public int Compare(SnippetInstance one, SnippetInstance two) {
            if (one.parent == null || two.parent == null)
                return 0;
            int retVal = one.parent.Snippet.Title.CompareTo(two.parent.Snippet.Title);
            // if it's a tie, let's make sure we're talking about the same snippet
            if (retVal == 0)
            {
                retVal = one.parent.Snippet.Id.CompareTo(two.parent.Snippet.Id);
                // now to break a tie we compare the whole line
                if (retVal == 0 && one.node != null && two.node != null)
                {
                    retVal = one.node.FullPath.CompareTo(two.node.FullPath);
                }
            }
            return retVal;
        }
    
    }
}
