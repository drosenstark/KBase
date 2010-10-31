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
using System.Drawing;
using System.Windows.Forms;

namespace Kbase.MultipleSelectionTreeView
{
    public class NavigationToolBar : ToolStrip
    {

        TreeViewMultipleSelect tree = null;
        ToolStripButton forward = null;
        ToolStripButton backward = null;

        public NavigationToolBar(TreeViewMultipleSelect tree, System.Drawing.Icon backIcon, System.Drawing.Icon forwardIcon)
        {
            // register the tree with this
            this.tree = tree;
            // and register me with the history so it can fire events on us
            if (tree.history != null)
                tree.history.toolBar = this;
            EventHandler backwardEvent = new EventHandler(tree.NavigateBackward);
            backward = makeButton("Backward", backIcon, backwardEvent);
            EventHandler forwardEvent = new EventHandler(tree.NavigateForward);
            forward = makeButton("Forward", forwardIcon, forwardEvent);
            OnAfterNavigation();
        }

        public NavigationToolBar(TreeViewMultipleSelect tree, Image backIcon, Image forwardIcon)
        {
            // register the tree with this
            this.tree = tree;
            // and register me with the history so it can fire events on us
            if (tree.history != null)
                tree.history.toolBar = this;
            EventHandler backwardEvent = new EventHandler(tree.NavigateBackward);
            backward = makeButton("Backward", backIcon, backwardEvent);
            EventHandler forwardEvent = new EventHandler(tree.NavigateForward);
            forward = makeButton("Forward", forwardIcon, forwardEvent);
            OnAfterNavigation();
        }


        public ToolStripButton makeButton(string toolTipText, System.Drawing.Icon icon, EventHandler onClick)
        {
            Image image = Image.FromHbitmap(icon.ToBitmap().GetHbitmap());
            return makeButton(toolTipText, image, onClick);
        }

        public ToolStripButton makeButton(string toolTipText, Image image, EventHandler onClick)
        {
            ToolStripButton retVal = new ToolStripButton(toolTipText, image, onClick);
            retVal.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            retVal.ImageTransparentColor = this.BackColor;
            Items.Add(retVal);
            return retVal;
        }

        internal void OnAfterNavigation()
        {
            if (tree == null || tree.history == null)
                return;

            backward.Enabled = tree.history.CanMoveBackward();
            forward.Enabled = tree.history.CanMoveForward();
       }
    }

}
