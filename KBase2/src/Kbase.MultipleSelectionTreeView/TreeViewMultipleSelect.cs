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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using Kbase.LibraryWrap;

namespace Kbase.MultipleSelectionTreeView
{
	/// <summary>
	/// Code originally based on 
    /// http://www.arstdesign.com/articles/treeviewms.html
	/// dr (2005-01-23): Changed to make it a switchable property.
    /// Later I changed the whole thing to be OO...
    /// OOing cool stuff VB developers have done is what .NET is all about!
	/// </summary>
	public abstract class TreeViewMultipleSelect : TreeView
	{
		// dr (2005-01-23): changed to private
		TreeNodeMultipleSelectArrayList selectedNodes;
		TreeNode lastNode, firstNode;
		internal NavigationHistory history = null;

		public TreeViewMultipleSelect()
		{
            Init();
		}



        public void Init() {
            selectedNodes = new TreeNodeMultipleSelectArrayList();
            LabelEdit = true;
            history = new NavigationHistory(this);
        }

		public TreeNodeMultipleSelectArrayList SelectedNodes
		{
			get
			{
				return selectedNodes;
			}
			set
			{
				removePaintFromNodes();
				selectedNodes.Clear();
				selectedNodes = value;
				paintSelectedNodes();
			}
		}

		public virtual void RemoveAllReferences(TreeNode node) {
			SelectedNodes.Remove(node);
		}

		public TreeNode SingleSelectedNode 
		{
			get 
			{
				TreeNode retVal = null;
				if (SelectedNodes.Count == 1)
					retVal = (TreeNode)SelectedNodes[0];
				return retVal;
			}
		}

		public virtual void RemoveAllCachedNodes() 
		{
			removePaintFromNodes();
			SelectedNodes.Clear();
		}



		/// <summary>
        /// here we kill the default selection behavior... in fact
        /// we never use any of the normal selection mechanisms
		/// </summary>
		/// <param name="e"></param>
		protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
		{
				base.OnBeforeSelect(e);
				e.Cancel = true;

		}

		protected override void OnAfterSelect(TreeViewEventArgs e)
		{
			base.OnAfterSelect(e);
			throw new Exception("This method should never fire.  Use ReplaceSelection***");
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
            if (e == null)
                return;
            try
            {
                base.OnMouseMove(e);
            }
            catch (NullReferenceException)
            { 
                // this freaky error is happening in Winforms on Mono, not sure what's up yet, but we can't debug Mono, you know?
            }
            catch (Exception ex) {
                MainFrm.MainForm.ShowError(ex);
            }
		}

		protected override void OnBeforeCollapse(TreeViewCancelEventArgs e)
		{
			base.OnBeforeCollapse (e);
			collapsingOrExpanding = true;
		}

		protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
		{
			base.OnBeforeExpand (e);
			collapsingOrExpanding = true;
		}


		bool collapsingOrExpanding = false;

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown (e);
			lastButton = e.Button;
		}
		
		MouseButtons lastButton; // .NET 1.1 shows the wrong button in mouseup
		TreeNode lastMouseUp = null;

		/// <summary>
		/// we have to use mouseup to trap the expansion events first
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp (e);
			object thing = GetNodeAt(e.X, e.Y);

			// handle left clicking OUTSIDE of the selection, which changes the
			// selection and then call the onrightclick event.
			if (lastButton != MouseButtons.Left) 
			{
				if (thing != null && !SelectedNodes.Contains(thing)) 
					ReplaceSelectionWith((TreeNode)thing);
				OnRightClick(e);
				return;
			}

			bool control = (ModifierKeys==Keys.Control);
			bool shift = (ModifierKeys==Keys.Shift);

			if (thing != null) 
			{
				TreeNode node = (TreeNode)thing;
                if (!collapsingOrExpanding && node.Equals(lastMouseUp)
                    && node.Equals(SingleSelectedNode) && !control && !shift)
                {
                    //// this should work but it doesn't 
                    if (LabelEdit)
                        node.BeginEdit();
                }
                else
                {
                    if (!collapsingOrExpanding)
                        DoMultipleSelectionStuff(node);
                    lastMouseUp = SingleSelectedNode; // will be null if there's not just one selected
                }
			} 
			else // clearing out everything, user selected null
			{
				removePaintFromNodes();
				selectedNodes.Clear();
                CallEvents();
			}

			collapsingOrExpanding = false;
		}

		protected TreeNodeMultipleSelect MenuPoppedUpNode = null;
		protected ContextMenu RightClickMenu = null;

		protected virtual void OnRightClick(MouseEventArgs e) 
		{
			if (RightClickMenu == null) 
			{
				SetupRightClickMenu();
			}

			MenuPoppedUpNode = (TreeNodeMultipleSelect)this.GetNodeAt(e.X, e.Y);

            // there may in fact be no node
            if (RightClickMenu != null) 
			{
				RightClickMenu.Show(this,new Point(e.X, e.Y)); 
			}
		}

		protected virtual void SetupRightClickMenu() {}


		protected virtual void OnAfterSingleSelect(TreeNodeMultipleSelect node) 
		{
			node.PaintAsSelectedUnique();
			history.AddSelection(node);
		}

		protected virtual void OnAfterMultipleSelect() 
		{
			history.AddSelection(SelectedNodes);
		}

		protected virtual void OnAfterZeroSelect() 
		{
		}


		public void RemoveFromSelection(TreeNode node) 
		{
			if (SelectedNodes.Contains(node)) 
			{
				removePaintFromNodes();
				selectedNodes.Remove( node );
				paintSelectedNodes();
				CallEvents();
			}
		}

        public void ReplaceSelectionWithNull()
        {
            ReplaceSelectionWith((ICollection)null);
        }

		public void ReplaceSelectionWith(TreeNode node) 
		{
			try 
			{
                if (!OnBeforeSingleSelect(node))
                    return;
				if (node == null || node.TreeView == null)
					return;
				removePaintFromNodes();
				SelectedNodes.Clear();
				if (node != null) 
				{
					SelectedNodes.Add(node);
				}
				paintSelectedNodes();
				CallEvents();
				node.EnsureVisible();
			} 
			catch (Exception e) {
				OnError(e);
			}
		}

		public void ReplaceSelectionWith(ICollection nodes) 
		{
			try 
			{
				if (nodes == null) 
				{
					removePaintFromNodes();
					SelectedNodes.Clear();
					return;
				}
				// if it's just one, call the single node method
				if (nodes.Count == 1) 
				{
					IEnumerator enumerator = nodes.GetEnumerator();
					enumerator.MoveNext();
					TreeNode oneNode = (TreeNode)enumerator.Current;
					ReplaceSelectionWith(oneNode);
					return;
				}

				removePaintFromNodes();
				SelectedNodes.Clear();
				foreach (TreeNode node in nodes) 
				{
					if (!SelectedNodes.Contains(node)) 
					{
						SelectedNodes.Add(node);
						node.EnsureVisible();
					}
				}
				paintSelectedNodes();
				CallEvents();
			} 
			catch (Exception ex) {
				OnErrorSilent(ex);
			}
		}


		public void AddToSelection(TreeNode node) 
		{
            if (!OnBeforeSingleSelect(node))
                return;
            removePaintFromNodes();
			if (node != null && !SelectedNodes.Contains(node)) 
			{
				SelectedNodes.Add(node);
			}
			paintSelectedNodes();
			CallEvents();
		}



		protected void CallEvents() 
		{
			if (SelectedNodes.Count == 1) 
				OnAfterSingleSelect((TreeNodeMultipleSelect)SelectedNodes[0]);
			else if (SelectedNodes.Count == 0) 
				OnAfterZeroSelect();
			else  
				OnAfterMultipleSelect();
		}

        protected bool suppressShiftAndControlSelections = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns>false if cancel</returns>
        public virtual bool OnBeforeSingleSelect(TreeNode node) {
            return true;
        }

		protected void DoMultipleSelectionStuff(TreeNode node)
		{
            if (!OnBeforeSingleSelect(node))
                return;
			removePaintFromNodes();
			try 
			{
				bool bControl = (ModifierKeys==Keys.Control);
				bool bShift = (ModifierKeys==Keys.Shift);

			
				lastNode = node;
				if (!bShift) firstNode = node; // store begin of shift sequence
				
				
				if (bControl) 
					ControlClick(node);
				else if (bShift)
					ShiftClick(node);
				else 
					PlainClick(node);

				paintSelectedNodes();
				CallEvents();
			} 
			catch (Exception e) 
			{
				OnError(e);
			}
		}

		void ControlClick(TreeNode node) 
		{
            if (suppressShiftAndControlSelections)
                return; 
			if ( !selectedNodes.Contains( node ) ) // new node ?
			{
				selectedNodes.Add( node );
			}
			else  // not new, remove it from the collection
			{
				selectedNodes.Remove( node );
			}
		}

		void ShiftClick(TreeNode node) 
		{
            if (suppressShiftAndControlSelections)
                return;
            Queue myQueue = new Queue();
					
			TreeNode uppernode = firstNode;
			TreeNode bottomnode = node;
			// case 1 : begin and end nodes are parent
			bool bParent = isParent(firstNode, node); // is firstNode parent (direct or not) of node
			if (!bParent)
			{
				bParent = isParent(bottomnode, uppernode);
				if (bParent) // swap nodes
				{
					TreeNode t = uppernode;
					uppernode = bottomnode;
					bottomnode = t;
				}
			}
			if (bParent)
			{
				TreeNode n = bottomnode;
				while ( n != uppernode.Parent)
				{
					if ( !selectedNodes.Contains( n ) ) // new node ?
						myQueue.Enqueue( n );

					n = n.Parent;
				}
			}
				// case 2 : nor the begin nor the end node are descendant one another
			else
			{
				if ( (uppernode.Parent==null && bottomnode.Parent==null) || (uppernode.Parent!=null && uppernode.Parent.Nodes.Contains( bottomnode )) ) // are they siblings ?
				{
					int nIndexUpper = uppernode.Index;
					int nIndexBottom = bottomnode.Index;
					if (nIndexBottom < nIndexUpper) // reversed?
					{
						TreeNode t = uppernode;
						uppernode = bottomnode;
						bottomnode = t;
						nIndexUpper = uppernode.Index;
						nIndexBottom = bottomnode.Index;
					}

					TreeNode n = uppernode;
					while (nIndexUpper <= nIndexBottom)
					{
						if ( !selectedNodes.Contains( n ) ) // new node ?
							myQueue.Enqueue( n );
								
						n = n.NextNode;

						nIndexUpper++;
					} // end while
							
				}
				else
				{
					if ( !selectedNodes.Contains( uppernode ) ) myQueue.Enqueue( uppernode );
					if ( !selectedNodes.Contains( bottomnode ) ) myQueue.Enqueue( bottomnode );
				}
			}

			selectedNodes.AddRange( myQueue );

			firstNode = node; // let us chain several SHIFTs if we like it
		}

		void PlainClick(TreeNode node) 
		{
			// it's just a simple select, so remove all the nodes, this one is the new dog
			selectedNodes.Clear();
			selectedNodes.Add( node );
		}

		protected bool isParent(TreeNode parentNode, TreeNode childNode)
		{
			if (parentNode==childNode)
				return true;

			TreeNode n = childNode;
			bool bFound = false;
			while (!bFound && n!=null)
			{
				n = n.Parent;
				bFound = (n == parentNode);
			}
			return bFound;
		}

		protected void paintSelectedNodes()
		{
			foreach ( TreeNodeMultipleSelect n in selectedNodes )
			{
				try 
				{
					n.Selected = true;
				}
				catch(Exception ex) 
				{
					OnError(ex);
				}
			}
			if (SelectedNodes.Count == 0)
				SelectedNode = null;
		}

		protected void removePaintFromNodes()
		{
			if (selectedNodes.Count==0) 
				return;

			foreach ( TreeNodeMultipleSelect n in selectedNodes )
			{
				try 
				{
					n.Selected = false;
				} 
				catch(Exception ex) 
				{
					OnErrorSilent(ex);
				}
			}
    	}

        public void NavigateBackward(object o, EventArgs e)
        {
            NavigateBackward();
        }

        public void NavigateBackward() 
		{
			history.Backward();
		
		}

        public void NavigateForward(object o, EventArgs e) {
            NavigateForward();
        } 

		public void NavigateForward() 
		{
			history.Forward();		
		}


		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Shift && !e.Alt & !e.Control) 
			{
				if (e.KeyCode == Keys.Up)
					KeypressSelectUp();
				else if (e.Shift && e.KeyCode == Keys.Down)
					KeypressSelectDown();
			}
		}

		/// <summary>
		/// expand the selection downwards
		/// </summary>
		void KeypressSelectDown() 
		{
			if (SelectedNodes.Count == 0) 
				return;
			TreeNode keyNode = ((TreeNode)SelectedNodes[0]); 

			if (SelectedNodes.Count != 1 && !SelectedNodes.Contains(keyNode.NextVisibleNode)) 
				ReplaceSelectionWith(keyNode);
			else
			{
				TreeNode addMe = keyNode; 
				while (SelectedNodes.Contains(addMe)) 
				{
					addMe = addMe.NextVisibleNode;
					if (addMe == null) 
						break;
				}
				if (addMe != null)
					AddToSelection(addMe);
			}
		}

		/// <summary>
		/// expand the selection upwards
		/// </summary>
		void KeypressSelectUp() 
		{
			if (SelectedNodes.Count == 0) 
				return;
			TreeNode keyNode = ((TreeNode)SelectedNodes[0]); 

			if (SelectedNodes.Count != 1 && !SelectedNodes.Contains(keyNode.PrevVisibleNode)) 
				ReplaceSelectionWith(keyNode);
			else 
			{
				TreeNode addMe = keyNode; 
				while (SelectedNodes.Contains(addMe)) 
				{
					addMe = addMe.PrevVisibleNode;
					if (addMe == null) 
						break;
				}
				if (addMe != null)
					AddToSelection(addMe);
			}
		}

		/// <summary>
		/// DO NOT OVERRIDE THIS METHOD, BUT RATHER ONSELECTEDNODESDRAG
		/// </summary>
		/// <param name="e"></param>
		protected override void OnItemDrag(ItemDragEventArgs e)
		{
			try 
			{
				base.OnItemDrag (e);
				if (e.Button != MouseButtons.Left || !(e.Item is TreeNodeMultipleSelect))
					return;
				if (!SelectedNodes.Contains(e.Item))
					ReplaceSelectionWith((TreeNodeMultipleSelect)e.Item);
				object toDrag = OnSelectedNodesDrag();
                if (toDrag != null)
                {
                    DoDragDrop(toDrag, DragDropEffects.Move);
                }
			} 
			catch (Exception e2) 
			{
				OnError(e2);
			}
		}

		/// <summary>
		/// DO NOT OVERRIDE OnItemDrag.  Instead, override this method.
		/// </summary>
		/// <returns>The item to drag</returns>
		public virtual object OnSelectedNodesDrag() {
			return null;
		}

		/// <summary>
		/// This will be overwritten by subclasses that know what to do with their errors
		/// </summary>
		/// <param name="e"></param>
		public abstract void OnError(Exception e);
		public abstract void OnErrorSilent(Exception e);

	}

}
