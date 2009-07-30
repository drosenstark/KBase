using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Kbase.Model;

namespace Kbase.Properties
{
    internal class PropertySet : PropertiesPane
    {

        Snippet propertySetParent = null;

        internal Snippet PropertySetParent {
            get {
                if (propertySetParent == null)
                    throw new Exception("PropertySet queried for its parent, but it's null.");
                return propertySetParent;
            }
        }
        internal PropertySet(Snippet propertySetParent) : base() {
            this.propertySetParent = propertySetParent;
            refreshProperties();
        }

        internal override void Edit(List<Snippet> selectedSnippets)
        {
            refreshProperties();
            base.Edit(selectedSnippets);
        }

        internal void refreshProperties() {
            // are the properties the same... if they are, don't even touch them
            if (!ArePropsUpToDate())
            {
                base.Reset();
                if (propertySetParent.ChildrenCount > 0)
                {
                    foreach (Snippet snippet in propertySetParent.Children)
                    {
                        RegisterProperty(snippet, lastSelectedSnippets);
                    }
                }
            }
            this.Text = propertySetParent.Title;
            // this has to get the title updates
            instructions.Text = "The Snippet " + propertySetParent.Title + " does not yet have children to use as properties.";
            this.ImageIndex = Kbase.Icon.IconList.Instance.GetIconIndexUnselected(propertySetParent.Icon);

        }

        bool ArePropsUpToDate() {
            List<Snippet> properties = getProperties();
            if (propertySetParent.Children.Count != properties.Count)
                return false;
            for (int i=0;i<propertySetParent.ChildrenCount;i++) {
                if (!properties[i].Equals(propertySetParent.Children[i])) {
                    return false;
                }
            }
            return true;
        }
    }
}
