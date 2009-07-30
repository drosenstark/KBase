using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Kbase.MainFrm;
using System.Collections;
using Kbase.SnippetTreeView;
using Kbase.Model;

namespace Kbase.Properties
{
    internal class DefaultPropertiesPane : PropertiesPane
    {

        public DefaultPropertiesPane() {
            this.AllowDrop = true;
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
        protected override void OnDragDrop(DragEventArgs e)
        {
            try
            {
                base.OnDragDrop(e);
                Kbase.Serialization.SerializableUniverse draggedData = (Kbase.Serialization.SerializableUniverse)e.Data.GetData("Kbase.SerializableUniverse");
                // if the drag has come from another KBase, jump out
                if (draggedData != null && draggedData.GetHashCode() != Universe.Instance.snippetPane.hashCodeOfNodesBeingDragged)
                {
                    return;
                }

                ArrayList selectedNodesCopy = new ArrayList(Universe.Instance.snippetPane.SelectedNodes);
                foreach (SnippetTNode node in selectedNodesCopy)
                {
                    Snippet snippet = node.Snippet;
                    RegisterProperty(snippet, lastSelectedSnippets);
                }
                Universe.Instance.ModelGateway.Dirty = true;
                LayoutProperties();

            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }


        public override void box_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Clicks == 2)
                {
                    PropertyComboBox box = null;
                    if (sender is PropertyComboBox)
                        box = (PropertyComboBox)sender;
                    else if (sender is Label)
                        box = (PropertyComboBox)((Label)sender).Parent;

                    if (box != null)
                    {
                        if (MessageBox.Show(this, "Are you sure you want to remove the property " + box + "?", MainForm.DialogCaption, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            DeregisterProperty(box);
                            Universe.Instance.ModelGateway.Dirty = true;
                        }
                    }
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }


    }
}
