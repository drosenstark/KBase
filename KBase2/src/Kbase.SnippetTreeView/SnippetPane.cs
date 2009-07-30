using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using Kbase.MultipleSelectionTreeView;
using Kbase.Model;
using System.Collections.Generic;
using Kbase.Serialization;
using Kbase.Icon;
using Kbase.MainFrm;
using Kbase.Model.Search;
using Kbase.LibraryWrap;


namespace Kbase.SnippetTreeView
{
    /// <summary>
    /// Summary description for SnippetPane.
    /// </summary>

    public class SnippetPane : ScrollingTreeView
    {

        static ImageList singletonImageList;

        /// <summary>
        /// static constructor, always gets called first.
        /// </summary>
        static SnippetPane()
        {
            try
            {
                singletonImageList = IconList.Instance.GetImageList(true);
            }
            catch (Exception ex)
            {
                MainForm.ShowErrorSilent(ex);
                singletonImageList = null;
            }
        }

        public SnippetPane()
        {
            // Assign the ImageList to the TreeView.
            ImageList = singletonImageList;
            ImageIndex = IconList.Instance.defaultIconIndex;

            AllowDrop = true;
            LabelEdit = true; // in client server, this is impossible
        }


        // removes all nodes and resets the toplevel snippet
        public void Reset()
        {
            Snippet top = Universe.Instance.ModelGateway.TopLevelSnippet;
            Reset(top);
        }


        public void Reset(Snippet topLevelSnippet)
        {
            BeginUpdate();
            SelectSnippetLinkStop();
            RemoveAllCachedNodes();
            Nodes.Clear();
            topLevelSnippet.UI.ShowChildren();
            topLevelSnippet.UI.ShowGrandChildren();
            OnAfterZeroSelect();
            EndUpdate();
        }

        public void OnAfterRestore(Snippet topLevelSnippet) {
            BeginUpdate();
            topLevelSnippet.UI.ShowChildren();
            topLevelSnippet.UI.ShowGrandChildren();
            OnAfterZeroSelect();
            EndUpdate();
        }

        public override void RemoveAllReferences(TreeNode node)
        {
            base.RemoveAllReferences(node);
            highlightedNodes.Remove(node);
            if (localClipboard != null)
            {
                localClipboard.Remove(node);
            }
        }

        public override void RemoveAllCachedNodes()
        {
            base.RemoveAllCachedNodes();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            try
            {
                base.OnKeyDown(e);

                // first make sure to do the key clicks that the mainform wants
                Universe.Instance.mainForm.OnKeyDownPublic(e);
                if (e.Control)
                {
                    if (e.KeyCode == Keys.C)
                        KeypressCopy();
                    else if (e.KeyCode == Keys.V)
                        KeypressPaste();
                    else if (e.KeyCode == Keys.X)
                        KeypressCut();
                }
                // alt shift up and down
                else if (e.Shift)
                {
                    if (e.Alt)
                    {
                        if (e.KeyCode == Keys.Up)
                            KeyPressMoveUp();
                        else if (e.KeyCode == Keys.Down)
                            KeyPressMoveDown();
                    }
                }
                else
                {
                    if (e.KeyCode == Keys.Delete)
                        ClickDelete(null, null);
                    else if (e.KeyCode == Keys.Insert)
                        KeypressInsert();
                    else if (e.KeyCode == Keys.F2)
                        KeypressRename();
                    else if (e.KeyCode == Keys.Down)
                        KeypressDown();
                    else if (e.KeyCode == Keys.Up)
                        KeypressUp();
                    else if (e.KeyCode == Keys.Add)
                        KeypressExpand();
                    else if (e.KeyCode == Keys.Subtract)
                        KeypressCollapse();
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            try
            {
                base.OnKeyPress(e);
                // suppress the beeps
                e.Handled = true;

            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }



        protected override void OnDragEnter(DragEventArgs e)
        {
            try
            {
                base.OnDragEnter(e);
                e.Effect = DragDropEffects.All;
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }


        protected override void OnAfterExpand(TreeViewEventArgs e)
        {
            base.OnAfterExpand(e);
            SnippetTNode node = (SnippetTNode)e.Node;
            // this.BeginUpdate();
            node.Snippet.OnBeforeExpand();
            node.Snippet.UI.ShowGrandChildren();
            // this.EndUpdate();
        }

        protected override void OnAfterCollapse(TreeViewEventArgs e)
        {
            base.OnAfterCollapse(e);
            SnippetTNode node = (SnippetTNode)e.Node;
            if (node.Snippet.UI.CascadeOnCollapse)
            {
                foreach (TreeNode child in node.Nodes)
                {
                    child.Collapse();
                }
            }
            //node.Snippet.UI.HideChildren();
            node.Snippet.UI.HideGrandChildren();
        }


        #region SnippetSelection swap out of default behaviors, also see OnAfterSingleSelect
        SelectSnippetForm selectSnippetForm = new SelectSnippetForm();
        bool SelectingSnippetLink = false;

        bool SuppressNoneSelect = false;
        SnippetTNode selectionBeforeSnipetLinkSelection = null;

        /// <summary>
        /// see OnAfterLinkSelection
        /// </summary>
        SomeSnippetEventHandler OnLinkSelection = null;
        Control controlReceivesFocus = null;

        /// <summary>
        /// the user clicks out and does NOT select
        /// </summary>
        public void SelectSnippetLinkStopNoSelection()
        {
            SelectSnippetLinkStop();
            ReplaceSelectionWith(selectionBeforeSnipetLinkSelection);
        }

        public void GiveFocusToOtherControlAsync()
        {
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(GiveFocusToOtherControl));
            t.Start();
        }

        public void GiveFocusToOtherControl()
        {
            System.Threading.Thread.Sleep(100);
            if (controlReceivesFocus != null)
            {
                Invoke(new ZeroArgumentEventHandler(GiveFocusToOtherControlInner));
                controlReceivesFocus = null;
            }
        }

        public void GiveFocusToOtherControlInner()
        {
            controlReceivesFocus.Focus();
        }

        public void SelectSnippetLinkStop()
        {
            SuppressNoneSelect = false;
            SelectingSnippetLink = false;
            selectSnippetForm.Visible = false;
            suppressShiftAndControlSelections = false;
            GiveFocusToOtherControlAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventOnLinkSelection"></param>
        /// <param name="eventAfterLinkSelection">can be null</param>
        public void SelectSnippetLinkStart(SomeSnippetEventHandler eventOnLinkSelection,
            Control controlReceivesFocus)
        {
            // what to do when it's selected
            OnLinkSelection = eventOnLinkSelection;
            this.controlReceivesFocus = controlReceivesFocus;
            selectionBeforeSnipetLinkSelection = SingleSelectedNode as SnippetTNode;
            SelectingSnippetLink = true;
            selectSnippetForm.Visible = true;
            SuppressNoneSelect = true;
            suppressShiftAndControlSelections = true;
        }

        public override bool OnBeforeSingleSelect(TreeNode node)
        {
            if (SelectingSnippetLink)
            {
                SnippetTNode theNode = (SnippetTNode)node;
                SelectSnippetLinkStop();
                if (theNode != null)
                    OnLinkSelection(theNode.Snippet);

                return false;
            }
            else
                return true;

        }
        #endregion

        /// <summary>
        /// </summary>
        /// <param name="node"></param>
        protected override void OnAfterSingleSelect(TreeNodeMultipleSelect node)
        {

            base.OnAfterSingleSelect(node);
            SnippetTNode theNode = (SnippetTNode)node;
            RemoveNodeHighlights();
            theNode.SnippetInstance.OnAfterSelectExclusive();
            Universe.Instance.OnAfterSelect(theNode.SnippetInstance);
        }

        protected override void OnAfterMultipleSelect()
        {
            base.OnAfterMultipleSelect();
            RemoveNodeHighlights();
            Universe.Instance.OnAfterSelect(GetSelectedNodesAsSnippets());
        }

        List<SnippetInstance> GetSelectedNodesAsSnippetInstances()
        {
            List<SnippetInstance> retVal =
                    new List<SnippetInstance>(SelectedNodes.Count);
            foreach (SnippetTNode node in SelectedNodes)
            {
                if (node != null && node.Snippet != null && !node.Dead)
                    retVal.Add(node.SnippetInstance);
            }
            return retVal;
        }


        List<Snippet> GetSelectedNodesAsSnippets()
        {
            List<Snippet> retVal =
                    new List<Snippet>(SelectedNodes.Count);
            foreach (SnippetTNode node in SelectedNodes)
            {
                if (node != null && node.Snippet != null && !node.Dead)
                    retVal.Add(node.Snippet);
            }
            return retVal;
        }

        protected override void OnAfterZeroSelect()
        {
            base.OnAfterZeroSelect();
            RemoveNodeHighlights();
            if (!SuppressNoneSelect)
            {
                Universe.Instance.OnAfterSelectNone();
            }
        }

        public ArrayList highlightedNodes = new ArrayList();
        public void RemoveNodeHighlights()
        {
            ArrayList removed = new ArrayList(highlightedNodes.Count);
            foreach (SnippetTNode node in highlightedNodes)
            {
                if (SingleSelectedNode == null || node.Snippet != ((SnippetTNode)SingleSelectedNode).Snippet)
                {
                    node.RemoveHighlight();
                    removed.Add(node);
                }
            }
            foreach (Object obj in removed)
            {
                highlightedNodes.Remove(obj);
            }

        }


        public override object OnSelectedNodesDrag()
        {
            try
            {
                // if we're client server, this is actually just a shell...
                // see OnDragDrop which will, if we're not pasting to another
                // running TheKbase.exe, use a locally cached version that
                // has not been serialized at all
                SerializableUniverse serializable = new SerializableUniverse(GetSelectedNodesAsSnippets());
                // cache our serializable so we can figure out if the drag is local
                hashCodeOfNodesBeingDragged = serializable.GetHashCode();
                return serializable;
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
                return null;
            }
        }

        internal int hashCodeOfNodesBeingDragged;

        protected override void OnAfterLabelEdit(NodeLabelEditEventArgs e)
        {
            SnippetTNode node = (SnippetTNode)e.Node;
            BeginUpdate();
            try
            {
                base.OnAfterLabelEdit(e);
                if (e.CancelEdit != true && e.Label != null)
                {
                    node.Snippet.Title = e.Label;
                    // refresh the announcement in the various subpanes because 
                    // we've suffered a renaming

                    if (Universe.Instance.detailPane.IsEditingSnippet(node.Snippet))
                        Universe.Instance.mainForm.AnnounceEditing(node.Snippet);
                    OnAfterSingleSelect(node);
                    e.CancelEdit = true;
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
                e.CancelEdit = true;
            }
            finally
            {
                EndUpdate();
            }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            try
            {
                base.OnDragDrop(e);
                Point localPoint = PointToClient(new Point(e.X, e.Y));
                SnippetTNode to = (SnippetTNode)GetNodeAt(localPoint.X, localPoint.Y);
                Snippet toSnippet = null;
                if (to == null)
                    toSnippet = Universe.Instance.ModelGateway.TopLevelSnippet;
                else
                    toSnippet = to.Snippet;
                SerializableUniverse draggedData = (SerializableUniverse)e.Data.GetData(typeof(SerializableUniverse));
                // if the drag has come from another KBase, do this
                if (draggedData != null && draggedData.GetHashCode() != hashCodeOfNodesBeingDragged)
                {
                    Paste(draggedData, toSnippet);
                    return;
                }

                if (!SelectedNodes.Contains(to))
                {
                    ArrayList selectedNodesCopy = new ArrayList(SelectedNodes);
                    foreach (SnippetTNode node in selectedNodesCopy)
                    {
                        RemoveFromSelection(node);
                    }
                    if (e.KeyState == 8) // copy, CONTROL CTRL KEY PRESSED
                    {
                        CopyNodes(selectedNodesCopy, to);
                    }
                    else //if (e.KeyState == 4)  // MOVE, SHIFT KEY PRESSED
                    {
                        MoveNodes(selectedNodesCopy, to);
                    }
                }

            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }

        private void MoveNodes(ICollection nodes, SnippetTNode to)
        {
            if (nodes == null)
                return;
            RemoveAllCachedNodes();

            ArrayList newSelection = new ArrayList(nodes.Count);

            foreach (SnippetTNode node in nodes)
            {
                TreeNode selectMe = MoveNode(node, to, false);
                newSelection.Add(selectMe);
            }
            ReplaceSelectionWith(newSelection);
        }

        /// <summary>
        /// </summary>
        /// <param name="who">This is the node that we're copying, the original.</param>
        /// <param name="to">The new parent.</param>
        /// <param name="changeSelection"></param>
        public TreeNode MoveNode(SnippetTNode who, SnippetTNode to, bool changeSelection)
        {
            Snippet destination = null;
            if (to == null)
                destination = Universe.Instance.ModelGateway.TopLevelSnippet;
            else
                destination = to.Snippet;
            try
            {
                // yes, this is tricky, but what are the alternatives?  We have to pass information 
                // on our position in the tree
                Snippet from = who.SnippetInstance.parent.Snippet;
                who.Snippet.MoveSnippet(from, destination);

                SnippetInstance reselect = null;
                if (to == null)
                {
                    reselect = who.Snippet.UI.GetInstanceUnderTopLevel();
                }
                else
                {
                    reselect = who.Snippet.UI.GetInstanceUnder(to.SnippetInstance);
                }

                TreeNode node = reselect.node;
                // select the new node in its location
                if (changeSelection)
                    ReplaceSelectionWith(node);
                return node;
            }
            catch (NewChildIsAncestorException)
            {
                string msg;
                if (to == null)
                {
                    msg = String.Format("Illegal move (top level {0} already exists)", who.Text);
                }
                else
                {
                    msg = String.Format("Illegal move ({0} is an ancestor of {1} or {1} already has a {0})", who.Text, to.Text);
                }
                MessageBox.Show(msg, MainForm.DialogCaption, MessageBoxButtons.OK);
            }
            return null;
        }

        private void CopyNodes(ICollection nodes, SnippetTNode to)
        {
            if (nodes == null)
                return;

            ArrayList newSelection = new ArrayList(nodes.Count);
            foreach (SnippetTNode node in nodes)
            {
                TreeNode newNode = CopyNode(node, to, false);
                if (newNode != null)
                    newSelection.Add(newNode);
            }

            // replace the selection
            ReplaceSelectionWith(newSelection);
        }


        /// <summary>
        /// </summary>
        /// <param name="who">This is the node that we're copying, the original.</param>
        /// <param name="to">The new parent.</param>
        /// <param name="changeSelection"></param>
        /// <returns></returns>
        public SnippetTNode CopyNode(SnippetTNode who, SnippetTNode to, bool changeSelection)
        {
            Snippet destination = null;
            if (to == null)
                destination = Universe.Instance.ModelGateway.TopLevelSnippet;
            else
                destination = to.Snippet;
            try
            {
                who.Snippet.CopySnippet(destination);

                SnippetInstance reselect = null;
                if (to == null)
                {
                    reselect = who.Snippet.UI.GetInstanceUnderTopLevel();
                }
                else
                {
                    reselect = who.Snippet.UI.GetInstanceUnder(to.SnippetInstance);
                }

                TreeNode node = reselect.node;
                // select the new node in its location
                if (changeSelection)
                    ReplaceSelectionWith(node);
                return reselect.node;
            }
            catch (NewChildIsAncestorException)
            {
                string msg;
                if (to == null)
                {
                    msg = String.Format("Illegal copy (top level {0} already exists)", who.Text);
                }
                else
                {
                    msg = String.Format("Illegal copy ({0} is an ancestor of {1} or {1} already has a {0})", who.Text, to.Text);
                }
                MessageBox.Show(msg, MainForm.DialogCaption, MessageBoxButtons.OK);
            }
            return null;
        }

        protected override void SetupRightClickMenu()
        {
            base.SetupRightClickMenu();
            RightClickMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("&New Snippet");
            menuItem.Click += new EventHandler(ClickNew);
            RightClickMenu.MenuItems.Add(menuItem);

            RightClickMenu.MenuItems.Add(new MenuItem("-"));

            menuItem = new MenuItem("&Rename");
            menuItem.Click += new EventHandler(ClickRename);
            RightClickMenu.MenuItems.Add(menuItem);
            topLevelMenuDontShow.Add(menuItem);

            menuItem = new MenuItem("&Delete");
            menuItem.Click += new EventHandler(ClickDelete);
            RightClickMenu.MenuItems.Add(menuItem);
            topLevelMenuDontShow.Add(menuItem);

            menuItem = new MenuItem("Delete &All Instances");
            menuItem.Click += new EventHandler(ClickDeleteAll);
            RightClickMenu.MenuItems.Add(menuItem);
            topLevelMenuDontShow.Add(menuItem);

            RightClickMenu.MenuItems.Add(new MenuItem("-"));

            menuItem = new MenuItem("Cu&t");
            menuItem.Click += new EventHandler(ClickCut);
            RightClickMenu.MenuItems.Add(menuItem);
            topLevelMenuDontShow.Add(menuItem);

            menuItem = new MenuItem("&Copy");
            menuItem.Click += new EventHandler(ClickCopy);
            RightClickMenu.MenuItems.Add(menuItem);
            topLevelMenuDontShow.Add(menuItem);

            menuItem = new MenuItem("&Paste");
            menuItem.Click += new EventHandler(ClickPaste);
            RightClickMenu.MenuItems.Add(menuItem);

            RightClickMenu.MenuItems.Add(new MenuItem("-"));

            menuItem = new MenuItem("&Repeat Search");
            menuItem.Click += new EventHandler(ClickRepeatSearch);
            RightClickMenu.MenuItems.Add(menuItem);
            repeatSearch = menuItem;

            menuItem = new MenuItem("&Sort");
            RightClickMenu.MenuItems.Add(menuItem);

            MenuItem subMenuItem = new MenuItem("by &Title");
            subMenuItem.Click += new EventHandler(ClickSortByTitle);
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("by &Icon");
            subMenuItem.Click += new EventHandler(ClickSortByIcon);
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("by &Create Date");
            subMenuItem.Click += new EventHandler(ClickSortByCreateDate);
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("by &Modified Date");
            subMenuItem.Click += new EventHandler(ClickSortByModifiedDate);
            menuItem.MenuItems.Add(subMenuItem);

            
            menuItem = new MenuItem("Move &Up");
            menuItem.Click += new EventHandler(ClickMoveUp);
            RightClickMenu.MenuItems.Add(menuItem);
            topLevelMenuDontShow.Add(menuItem);

            menuItem = new MenuItem("Move &Down");
            menuItem.Click += new EventHandler(ClickMoveDown);
            RightClickMenu.MenuItems.Add(menuItem);
            topLevelMenuDontShow.Add(menuItem);

            RightClickMenu.MenuItems.Add(new MenuItem("-"));

            menuItem = new MenuItem("Zoom &In");
            menuItem.Click += new EventHandler(ClickZoomIn);
            RightClickMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("Zoom &Out");
            menuItem.Click += new EventHandler(ClickZoomOut);
            RightClickMenu.MenuItems.Add(menuItem);

            RightClickMenu.MenuItems.Add(new MenuItem("-"));

            menuItem = new MenuItem("Choose &Colors");
            menuItem.Click += new EventHandler(ClickChooseColors);
            RightClickMenu.MenuItems.Add(menuItem);
            topLevelMenuDontShow.Add(menuItem);

            menuItem = new MenuItem("Choose &Icon");
            RightClickMenu.MenuItems.Add(menuItem);
            menuItem.Click += new EventHandler(ClickChooseIcon);
            topLevelMenuDontShow.Add(menuItem);

            RightClickMenu.MenuItems.Add(new MenuItem("-"));

            menuItem = new MenuItem("Snippet &Info");
            menuItem.Click += new EventHandler(ShowInfo);
            RightClickMenu.MenuItems.Add(menuItem);
        
        }

        MenuItem repeatSearch;
        List<MenuItem> topLevelMenuDontShow = new List<MenuItem>();

        protected override void OnRightClick(MouseEventArgs e)
        {
            if (RightClickMenu == null)
                SetupRightClickMenu();

            SnippetTNode poppedUp = (SnippetTNode)this.GetNodeAt(e.X, e.Y);
            repeatSearch.Enabled = (poppedUp != null && poppedUp.Snippet.HasSearchSaved);
            foreach (MenuItem item in topLevelMenuDontShow)
            {
                item.Enabled = (poppedUp != null);
            }
            base.OnRightClick(e);
        }

        public void ClickRename(object sender, System.EventArgs e)
        {
            try
            {
                BeginNameEdit(MenuPoppedUpNode as SnippetTNode);
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }


        public void ClickChooseColors(object sender, System.EventArgs e)
        {
            try
            {
                if (SelectedNodes.Count == 0 || !SelectedNodes.Contains(MenuPoppedUpNode))
                    return;
                ColorDialog colorDialog = new ColorDialog();
                colorDialog.Color = ConvertToColor(((SnippetTNode)SelectedNodes[0]).Snippet.Color);
                DialogResult result = colorDialog.ShowDialog();
                if (!result.Equals(DialogResult.Cancel))
                {
                    foreach (SnippetTNode node in SelectedNodes)
                    {
                        node.Snippet.Color = ConvertColorToString(colorDialog.Color);
                    }
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }

        #region Color to String operations
        static System.Drawing.ColorConverter colorConverter = new System.Drawing.ColorConverter();
        public static System.Drawing.Color ConvertToColor(string text)
        {
            Color retVal = Universe.Instance.snippetPane.BackColor; 
            if (text != null)
            {
                try
                {
                    retVal = (System.Drawing.Color)colorConverter.ConvertFromInvariantString(text);
                }
                catch (Exception ex) {
                    Logger.Log("Problem converting color " + text);
                    Logger.Log(ex);
                }
            }
            return retVal;
        }

        public static string ConvertColorToString(System.Drawing.Color color)
        {
            return colorConverter.ConvertToInvariantString(color);
        }
        #endregion



        public void ClickChooseIcon(object sender, System.EventArgs e)
        {
            try
            {
                if (SelectedNodes.Count == 0 || !SelectedNodes.Contains(MenuPoppedUpNode))
                    return;

                // check if they all have the same icon, in which case we preselect it
                bool sameIcon = true;
                string icon = null;
                foreach (SnippetTNode node in SelectedNodes)
                {
                    if (icon == null)
                        icon = node.Snippet.Icon;
                    else
                    {
                        if (node.Snippet.Icon != icon)
                        {
                            sameIcon = false;
                            break;
                        }
                    }
                }

                // prep the selector
                IconSelector selector = new IconSelector();
                selector.SelectedIcon = icon;
                selector.ShowDialog(Universe.Instance.mainForm);

                // if all icons are not the same and the user chose the one that was
                // showing, maybe he just jumped out without really meaning it.
                if (!sameIcon && selector.SelectedIcon == icon)
                {
                    DialogResult okay = MessageBox.Show(this, "These snippets had different icons. Set them all to " + icon + "?", MainForm.DialogCaption, MessageBoxButtons.YesNo);
                    if (okay == DialogResult.No)
                        return;
                }
                foreach (SnippetTNode node in SelectedNodes)
                {
                    node.Snippet.Icon = selector.SelectedIcon;
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }

        public void ClickDeleteAll(object sender, System.EventArgs e)
        {
            try
            {
                if (SelectedNodes.Count == 0 || !SelectedNodes.Contains(MenuPoppedUpNode))
                    return;

                DeleteAllInstances();
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }



        public void ClickDelete(object sender, System.EventArgs e)
        {
            try
            {
                if (SelectedNodes.Count == 0)
                    return;

                if (SelectedNodes.Count > 1)
                {
                    DeleteNodes();
                }
                else
                {
                    DeleteNode((SnippetTNode)SelectedNodes[0], true);
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }

        public void DeleteAllInstances()
        {
            DialogResult okay = MessageBox.Show(
                "You are about to delete all instances of the " + SelectedNodes.Count + " selected Snippets. " +
                "Are you sure?", MainForm.DialogCaption, MessageBoxButtons.YesNo);
            if (okay == DialogResult.No)
                return;

            // copy the collection
            ArrayList selectedNodesClone = (ArrayList)SelectedNodes.Clone();
            foreach (SnippetTNode node in selectedNodesClone)
            {
                node.Snippet.DeleteSnippet();
            }

        }


        public void DeleteNodes()
        {
            foreach (SnippetTNode node in SelectedNodes)
            {
                if (node.Snippet.UI.SnippetInstances.Count == 1)
                {
                    DialogResult okay = MessageBox.Show(
                        "Deleting will permanently remove some of the " + SelectedNodes.Count + " selected Snippets. " +
                        "Delete anyway?", MainForm.DialogCaption, MessageBoxButtons.YesNo);
                    if (okay == DialogResult.No)
                        return;
                    else
                        break;
                }
            }
            // gotta copy the collection to avoid concurrency problems in the foreach
            ArrayList selectedNodesCopy = new ArrayList(SelectedNodes);
            RemoveAllCachedNodes();
            TreeNode nextFocusPoint = null;
            foreach (SnippetTNode node in selectedNodesCopy)
            {
                try
                {
                    nextFocusPoint = node.NextVisibleNode;
                    Delete(node, false);
                }
                catch (NullReferenceException)
                {
                    // this is no problem, happens if a parent is deleted which has already killed its child.					
                }
            }
            if (nextFocusPoint != null)
                ReplaceSelectionWith(nextFocusPoint);
        }

        public void DeleteNode(SnippetTNode node, bool changeSelection)
        {
            if (node.Snippet.WillDeleteRemoveSnippet())
            {
                DialogResult okay = MessageBox.Show(
                    "Deleting will permanently remove one or more Snippets (including \"" +
                    node.Text + "\"). Delete anyway?", MainForm.DialogCaption, MessageBoxButtons.YesNo);
                if (okay == DialogResult.Yes)
                    Delete(node, changeSelection);
            }
            else
                Delete(node, changeSelection);
        }


        public void Delete(SnippetTNode node, bool changeSelection)
        {
            SnippetTNode nextNode = null;
            // this stuff to find the next node is a little sloppy because it might get wiped out
            // in the node.Remove() if it's a twin or even a child of a twin (of course).
            // Problems of multiple hierarchies :)
            if (changeSelection)
            {
                nextNode = (SnippetTNode)node.NextVisibleNode;
                if (nextNode == null)
                    nextNode = (SnippetTNode)node.PrevVisibleNode;
            }

            node.Snippet.DeleteSnippet(node.SnippetInstance.parent.Snippet);

            if (changeSelection)
            {
                if (nextNode != null && !nextNode.Dead)
                    ReplaceSelectionWith(nextNode);
                else if (Nodes.Count > 0)
                    ReplaceSelectionWith(Nodes[0]);
            }

        }

        LocalClipboard localClipboard = null;

        public void ClickCopy(object sender, System.EventArgs e)
        {
            try
            {
                CopyOrMove(false);
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }

        public void ClickCopyToWindowsClipboard(object sender, System.EventArgs e)
        {
            try
            {
                DialogResult okay = MessageBox.Show(
                    "This will retrieve the texts from the server for all " + SelectedNodes.Count + " selected snippets and their children. " +
                    "\nThis may take while. Are you sure?", MainForm.DialogCaption, MessageBoxButtons.YesNo);
                if (okay == DialogResult.No)
                    return;
                CopyOrMove(false);
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            } finally { 
            
            }
        }


        public void ClickCut(object sender, System.EventArgs e)
        {
            try
            {
                CopyOrMove(true);
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }

        private void CopyOrMove(bool move)
        {
            if (MenuPoppedUpNode == null || !SelectedNodes.Contains(MenuPoppedUpNode) || SelectedNodes.Count == 0)
            {
                string verb;
                if (move)
                    verb = "cut";
                else
                    verb = "copied";
                MessageBox.Show(this, "Nothing was " + verb + " to the clipboard. Right click one of the selected snippets.", MainForm.DialogCaption, MessageBoxButtons.OK);
            }
            else
            {
                localClipboard = new LocalClipboard(SelectedNodes, move);
                localClipboard.CopyToWindowsClipboard();
            }
        }

        public void ClickPaste(object sender, System.EventArgs e)
        {
            try
            {
                BeginUpdate();
                Cursor = Cursors.WaitCursor; //this.UseWaitCursor = true;
                SnippetTNode to = (SnippetTNode)MenuPoppedUpNode;

                Snippet toSnippet = null;
                if (to == null)
                    toSnippet = Universe.Instance.ModelGateway.TopLevelSnippet;
                else
                    toSnippet = to.Snippet;

                IDataObject dataBlob = Clipboard.GetDataObject();

                SerializableUniverse serializableUniverse = LocalClipboard.GetFromWindowsClipboard(dataBlob);

                // if there's nothing in the clipboard, EVEN IF THE LOCAL CLIPBOARD HAS DATA
                // ignore the paste
                if (serializableUniverse == null && dataBlob.GetDataPresent(typeof(String)))
                {
                    Paste((String)dataBlob.GetData(typeof(String)), to);
                    return;
                }


                // if the local clipboard is empty, or
                // if there are different usable data in the Windows Clipboard
                // paste from the Windows clipboard
                if (localClipboard == null || (serializableUniverse != null &&
                    localClipboard.Version.CompareTo(serializableUniverse.Version) != 0))
                    Paste(serializableUniverse, toSnippet);
                else
                    PasteFromLocalClipboard(to);
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
            finally
            {
                EndUpdate();
                Cursor = Cursors.Arrow; // UseWaitCursor = false;
            }

        }

        /// <summary>
        /// Paste text as nodes! Massive snippet insertion, also
        /// </summary>
        /// <param name="wholeText"></param>
        /// <param name="to"></param>
        void Paste(string wholeText, SnippetTNode to)
        {
            char[] split = { (char)13, (char)10 };
            string[] snippetTexts = wholeText.Split(split);
            foreach (string text in snippetTexts)
            {
                if (text.Length > 0)
                {
                    Snippet snippet = null;
                    if (to != null)
                        snippet = to.Snippet.AddChildSnippet();
                    else
                        snippet = Universe.Instance.ModelGateway.TopLevelSnippet.AddChildSnippet();
                    snippet.Title = text;
                }
            }
        }

        void Paste(SerializableUniverse serializable, Snippet to)
        {
            List<Snippet> newSnippets = serializable.Restore(to, true);
            foreach (Snippet snippet in newSnippets)
            {
                snippet.UI.GetInstanceUnder(to).Select();
            }
        }

        public void PasteFromLocalClipboard(SnippetTNode to)
        {
            // note that the PoppedUpNode does not have to be a value.  It might be a "root"
            // node
            if (localClipboard == null || localClipboard.Contains(to))
                return;

            // on move, clear the clipboard
            if (localClipboard.IsMove)
            {
                MoveNodes(localClipboard, to);
                Clipboard.SetDataObject(new SerializableUniverse()); // clear the clipboard, can't use null
                localClipboard = null;
            }
            else
                CopyNodes(localClipboard, to);
        }


        public void KeyPressMoveUp()
        {
            if (SelectedNodes.Count > 1)
            {
                Universe.Instance.mainForm.ShowMessage("Sorry, you cannot move multiple nodes.");
                return;
            }
            foreach (SnippetTNode node in SelectedNodes)
            {
                node.SnippetInstance.parent.Snippet.MoveUpChild(node.Snippet);
                node.EnsureVisible();
            }
        }

        public void ClickMoveUp(object sender, System.EventArgs e)
        {
            try
            {
                if (MenuPoppedUpNode == null || !SelectedNodes.Contains(MenuPoppedUpNode))
                    return;
                KeyPressMoveUp();
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }


        public void ClickMoveDown(object sender, System.EventArgs e)
        {
            try
            {
                if (MenuPoppedUpNode == null || !SelectedNodes.Contains(MenuPoppedUpNode))
                    return;
                KeyPressMoveDown();

            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }

        public void KeyPressMoveDown()
        {

            if (SelectedNodes.Count > 1)
            {
                Universe.Instance.mainForm.ShowMessage("Sorry, you cannot move multiple nodes.");
                return;
            }

            foreach (SnippetTNode node in SelectedNodes)
            {
                node.SnippetInstance.parent.Snippet.MoveDownChild(node.Snippet);
                node.EnsureVisible();
            }
        }


        /// <summary>
        /// keypress is the preparation for the KeyPressXXXX methods that use a Single node
        /// to work from a keypress and expect something in the MenuPoppedUpNode
        /// which is a member variable
        /// </summary>
        public void Keypress()
        {
            if (SingleSelectedNode != null)
                MenuPoppedUpNode = (SnippetTNode)SingleSelectedNode;
        }

        public void KeypressInsert()
        {
            Keypress();
            ClickNew(null, null);
        }

        public void KeypressRename()
        {
            Keypress();
            ClickRename(null, null);
        }


        void KeypressCopy()
        {
            // fake the popped up menu, otherwise
            // just paste into the top level nodes
            if (SelectedNodes.Count > 0)
            {
                MenuPoppedUpNode = (SnippetTNode)SelectedNodes[0];
                ClickCopy(null, null);
            }
        }

        void KeypressCut()
        {
            // fake the popped up menu, otherwise
            // just paste into the top level nodes
            if (SelectedNodes.Count > 0)
            {
                MenuPoppedUpNode = (SnippetTNode)SelectedNodes[0];
                ClickCut(null, null);
            }
        }


        void KeypressPaste()
        {
            // fake the popped up menu, otherwise
            // just paste into the top level nodes
            if (SelectedNodes.Count == 1)
                MenuPoppedUpNode = (SnippetTNode)SelectedNodes[0];
            else if (SelectedNodes.Count == 0)
                MenuPoppedUpNode = null;
            ClickPaste(null, null);
        }


        public void KeypressExpand()
        {
            if (SingleSelectedNode != null)
                SingleSelectedNode.Expand();
        }

        public void KeypressCollapse()
        {
            if (SingleSelectedNode != null)
                SingleSelectedNode.Collapse();
        }

        /// <summary>
        /// wow, I cannot believe that I actually had to implement this crap
        /// </summary>
        void KeypressDown()
        {
            if (SelectedNodes.Count > 0 && ((TreeNode)SelectedNodes[0]).NextVisibleNode != null)
                ReplaceSelectionWith(((TreeNode)SelectedNodes[0]).NextVisibleNode);
        }

        /// <summary>
        /// wow, I cannot believe that I actually had to implement this crap
        /// </summary>
        void KeypressUp()
        {
            if (SelectedNodes.Count > 0 && ((TreeNode)SelectedNodes[0]).PrevVisibleNode != null)
                ReplaceSelectionWith(((TreeNode)SelectedNodes[0]).PrevVisibleNode);
        }


        public void ShowInfo(object sender, System.EventArgs e)
        {
            try
            {
                if (SelectedNodes.Count > 0)
                {
                    foreach (SnippetTNode node in SelectedNodes)
                    {
                        Kbase.PropertiesForm.SnippetInfoForm.showProps("Show Snippet Info", node.Snippet);
                    }
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }


        public void ClickNew(object sender, System.EventArgs e)
        {
            try
            {
                Snippet destination = null;
                if (MenuPoppedUpNode != null)
                    destination = ((SnippetTNode)MenuPoppedUpNode).Snippet;
                else
                    destination = Universe.Instance.ModelGateway.TopLevelSnippet;
                Snippet snippet = destination.AddChildSnippet();

                //  now find the new node
                SnippetInstance selectAfterInstance;
                if (destination == Universe.Instance.ModelGateway.TopLevelSnippet)
                    selectAfterInstance = snippet.UI.GetInstanceUnderTopLevel();
                else
                    selectAfterInstance = snippet.UI.GetInstanceUnder(((SnippetTNode)MenuPoppedUpNode).SnippetInstance);

                SnippetTNode selectAfter = selectAfterInstance.node;
                ReplaceSelectionWith(selectAfter);
                BeginNameEdit(selectAfter);
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }

        void BeginNameEdit(SnippetTNode node)
        {
            if (node == null)
                return;
            if (LabelEdit)
            {
                node.BeginEdit();
            }
            else // if client server, really
            {
                Universe.Instance.snippetDetailPane.BeginEdit();// Focus();
            }
        }

        public void ClickRepeatSearch(object sender, System.EventArgs e)
        {
            try
            {
                if (SelectedNodes.Count != 1)
                    return;
                SnippetTNode selectedNode = (SnippetTNode)SelectedNodes[0];
                if (selectedNode.Snippet.HasSearchSaved)
                {
                    bool expanded = selectedNode.IsExpanded;
                    IList<SearchCriterion> criteria = selectedNode.Snippet.Criteria;
                    Universe.Instance.ModelGateway.SearchRepeat(selectedNode.Snippet, criteria);
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2, "Error repeating your search. Check the parents of the search snippet for possible circularities.");
            }
        }



        public void ClickSortByTitle(object sender, System.EventArgs e)
        {
            try
            {
                if (SelectedNodes.Count == 0)
                    Universe.Instance.ModelGateway.TopLevelSnippet.SortChildrenByTitle();
                else
                {
                    foreach (SnippetTNode node in SelectedNodes)
                    {
                        node.Snippet.SortChildrenByTitle();
                    }
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }

        public void ClickSortByIcon(object sender, System.EventArgs e)
        {
            try
            {
                if (SelectedNodes.Count == 0)
                    Universe.Instance.ModelGateway.TopLevelSnippet.SortChildrenByIcon();
                else
                {
                    foreach (SnippetTNode node in SelectedNodes)
                    {
                        node.Snippet.SortChildrenByIcon();
                    }
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }



        public void ClickSortByCreateDate(object sender, System.EventArgs e)
        {
            try
            {
                if (SelectedNodes.Count == 0)
                    Universe.Instance.ModelGateway.TopLevelSnippet.SortChildrenByCreateDate();
                else
                {
                    foreach (SnippetTNode node in SelectedNodes)
                    {
                        node.Snippet.SortChildrenByCreateDate();
                    }
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }

        public void ClickSortByModifiedDate(object sender, System.EventArgs e)
        {
            try
            {
                if (SelectedNodes.Count == 0)
                    Universe.Instance.ModelGateway.TopLevelSnippet.SortChildrenByModifiedDate();
                else
                {
                    foreach (SnippetTNode node in SelectedNodes)
                    {
                        node.Snippet.SortChildrenByModifiedDate();
                    }
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }


        public void ClickZoomIn(object sender, System.EventArgs e)
        {
            try
            {
                ZoomIn();
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }

        public void ClickZoomOut(object sender, System.EventArgs e)
        {
            try
            {
                ZoomOut();
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }
        public void ZoomIn()
        {
            Font = new Font(Font.FontFamily, Font.Size + 1);
        }

        public void ZoomOut()
        {
            Font = new Font(Font.FontFamily, Font.Size - 1);
        }

        /// <summary>
        /// override the TreeViewMultipleSelect 
        /// </summary>
        /// <param name="e"></param>
        public override void OnError(Exception e)
        {
            MainForm.ShowError(e);
        }

        public override void OnErrorSilent(Exception e)
        {
            MainForm.ShowErrorSilent(e);
        }

    }
}
