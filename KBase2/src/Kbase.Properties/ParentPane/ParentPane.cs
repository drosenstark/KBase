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
using Kbase.MainFrm;

namespace Kbase.Properties.ParentPane
{
    /// <summary>
    /// parentPane shows the tree backwards (parents under
    /// children) for a snippet...
    /// since snippets have multiple parents.
    /// The brilliant idea I never got to was
    /// a map where you could go left to parents, right for
    /// children...
    /// </summary>
    class ParentPane : TreeView
    {
        public ParentPane() {
            ImageList = Icon.IconList.Instance.GetImageList(true);
        }

        bool settingUp;

        public void Edit(SnippetInstance selectedInstance)
        {
            settingUp = true;
            Clear();
            if (selectedInstance.parent != null)
                selectMe = selectedInstance.parent.Snippet;


            ParentPaneNode top = new ParentPaneNode(selectedInstance.Snippet);
            Recurse(top);

            Nodes.Add(top);
            SelectedNode = selectMeWhenReady;

            settingUp = false;
        }

        // selectMe is the selectedInstances parent that is currently
        // being viewed in the SnippetPane
        Snippet selectMe = null;
        // this is the ParentPaneNode that corresponds to selectMe,
        // when we find it we store it in this variable
        // so we can select it when setup has been completed
        ParentPaneNode selectMeWhenReady = null;

        void Recurse(ParentPaneNode child) {
            foreach (Snippet parent in child.snippet.Parents)
            {
                if (parent != Universe.Instance.ModelGateway.TopLevelSnippet)
                {
                    ParentPaneNode node = new ParentPaneNode(parent);
                    if (parent == selectMe)
                        selectMeWhenReady = node;
                    child.Nodes.Add(node);
                    Recurse(node);
                }
            }
        }

        public void Clear() {
            bool wasSettingUp = settingUp;
            settingUp = true;
            Nodes.Clear();
            settingUp = wasSettingUp;
        }

        bool expanding = false;
        protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
        {
            expanding = true;
        }

        protected override void OnAfterExpand(TreeViewEventArgs e)
        {
            expanding = false;
        }

        protected override void OnBeforeCollapse(TreeViewCancelEventArgs e)
        {
            expanding = true;
        }

        protected override void OnAfterCollapse(TreeViewEventArgs e)
        {
            expanding = false;
        }

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            // first time selection is OK!
            if (SelectedNode == null || settingUp)
                return;

            e.Cancel = true;
            if (expanding) 
            {
                return;
            }

            ParentPaneNode node = (ParentPaneNode)e.Node;

            // user cannot click on top level node
            //if (Nodes.Contains(node))
            //    return;
            SnippetInstance instance = node.Instance;
            if (instance != null)
            {
                DialogResult result;
                if (Universe.Instance.Settings.DoNotAskUserToConfirmSelectionChanceInLocationPane)
                    result = DialogResult.OK;
                else
                    result = MessageBox.Show("Change selection to any instance of " + instance.node.Text + "?", MainForm.DialogCaption, MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                    instance.SelectExclusive();
            }
        }


    }
}
