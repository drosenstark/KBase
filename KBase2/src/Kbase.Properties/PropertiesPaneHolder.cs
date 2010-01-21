using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Kbase.Model;
using Kbase.MainFrm;
using System.Collections;
using System.Diagnostics;

namespace Kbase.Properties
{
    internal class PropertiesPaneHolder : TabControl
    {

        List<PropertiesPane> propertiesPanes = new List<PropertiesPane>();
        
        
        public PropertiesPaneHolder() {
            Search.SearchForm searchForm = Universe.Instance.mainForm.searchForm;
            searchForm.Dock = DockStyle.Fill;
            TabPage searchTab = new TabPage("Search");
            searchTab.Controls.Add(searchForm);
            this.TabPages.Add(searchTab);


            // Properties Panes
            DefaultPropertiesPane defaultPropertiesPane = new DefaultPropertiesPane();
            defaultPropertiesPane.ParentContainer = this;
            this.propertiesPanes.Add(defaultPropertiesPane);
            Dock = DockStyle.Fill;
            AutoSize = true;
            AllowDrop = true;
            DragEnter += new DragEventHandler(propertiesPaneHolder_DragEnter);
            DragDrop += new DragEventHandler(propertiesPaneHolder_DragDrop);
            ToolTip tip = new ToolTip();
            tip.SetToolTip(this,
                "Drag individual properties to the Properties tab\n" +
                "or drag new Property Sets to the tab to create a new Property Set in a new tab.");
            tip.ToolTipTitle = "Add Properties Or Property Sets";
            Universe.Instance.propertiesPaneHolder = this;
            this.ImageList = Kbase.Icon.IconList.Instance.GetImageList(false);


        
        }

        void PropertiesPaneHolder_DoubleClick(object sender, EventArgs e)
        {
            if (sender is PropertySet) {
                PropertySet removeMe = sender as PropertySet;

                string message = "Are you sure you want to remove PropertySet " + removeMe.Text + "?";
                DialogResult result = MessageBox.Show(message, MainForm.DialogCaption, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    RemovePropertySet(removeMe);
                }
            }            
        }

        void propertiesPaneHolder_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                Kbase.Serialization.SerializableUniverse draggedData = (Kbase.Serialization.SerializableUniverse)e.Data.GetData("Kbase.SerializableUniverse");
                // if the drag has come from another KBase, jump out
                if (draggedData != null && draggedData.GetHashCode() != Universe.Instance.snippetPane.hashCodeOfNodesBeingDragged)
                {
                    return;
                }

                ArrayList selectedNodesCopy = new ArrayList(Universe.Instance.snippetPane.SelectedNodes);
                foreach (Kbase.SnippetTreeView.SnippetTNode node in selectedNodesCopy)
                {
                    Snippet snippet = node.Snippet;
                    AddPropertySet(snippet);
                }
                Universe.Instance.snippetDetailPane.LayoutProperties();

            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }

        void propertiesPaneHolder_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }


        internal void Reset()
        {
            List<PropertySet> killThese = new List<PropertySet>();
            foreach (PropertiesPane setPane in propertiesPanes)
            {
                if (setPane is DefaultPropertiesPane)
                    setPane.Reset();
                else
                    killThese.Add(setPane as PropertySet);
            }
            foreach (PropertySet set in killThese) {
                RemovePropertySet(set);
            }
            
        }

        public void RegisterProperty(Snippet property)
        {
            foreach (PropertiesPane setPane in propertiesPanes)
            {
                if (setPane is DefaultPropertiesPane)
                {
                    setPane.RegisterProperty(property, Universe.Instance.snippetDetailPane.selectedSnippets);

                }
            }

        }


        internal List<Snippet> getProperties()
        {
            List<Snippet> retVal = new List<Snippet>();
            foreach (PropertiesPane setPane in propertiesPanes)
            {
                if (setPane is DefaultPropertiesPane)
                    retVal.AddRange(setPane.getProperties());
            }
            return retVal;
        }

        internal List<Snippet> getPropertySets()
        {
            List<Snippet> retVal = new List<Snippet>();
            foreach (PropertiesPane setPane in propertiesPanes)
            {
                if (setPane is PropertySet)
                {
                    PropertySet set = setPane as PropertySet;
                    retVal.Add(set.PropertySetParent);
                }
            }
            return retVal;
        }


        public void AddPropertySet(Snippet parent)
        {
            PropertySet pane = new PropertySet(parent);
            pane.ParentContainer = this;
            propertiesPanes.Add(pane);
            pane.DoubleClick += new EventHandler(PropertiesPaneHolder_DoubleClick);
            ToolTip tip = new ToolTip();
            tip.SetToolTip(pane, "Double click here to remove this PropertySet.");
        }

        public void RemovePropertySet(PropertySet set)
        {
            this.Controls.Remove(set);
            propertiesPanes.Remove(set);
        }

        public void Edit(List<Snippet> selectedSnippets) {
            foreach (PropertiesPane set in propertiesPanes) {
                set.Edit(selectedSnippets);
            }
        }

        public void OnAfterRestoreProperties() {
            foreach (PropertiesPane set in propertiesPanes)
            {
                set.OnAfterRestoreProperties();
            }
        }

        public void LayoutProperties() {
            foreach (PropertiesPane set in propertiesPanes)
            {
                set.LayoutProperties();
            }
        }


    }
}
