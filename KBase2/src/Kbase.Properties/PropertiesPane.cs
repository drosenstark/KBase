using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using Kbase.Model;
using System.Drawing;
using Kbase.MainFrm;

namespace Kbase.Properties
{
    class PropertiesPane : TabPage
    {

        ArrayList propertyCombos = new ArrayList();
        TabControl parent;
        internal Label instructions;
        internal List<Snippet> lastSelectedSnippets = null;


        public PropertiesPane() {
            instructions = new Label();
            instructions.Text = "Drag properties here";
            instructions.ForeColor = Color.BlueViolet;
            instructions.Width = this.ClientSize.Width;
            Controls.Add(instructions);
            this.Text = "Properties";
            this.AutoScroll = true;
            this.HorizontalScroll.Visible = false;
        }

        public TabControl ParentContainer {
            set {
                this.parent = value;
                value.Controls.Add(this);
            }
        }

        public void RegisterProperty(Snippet property, List<Snippet> selectedSnippets)
        {
            PropertyComboBox box = new PropertyComboBox(property);
            if (selectedSnippets != null)
                box.Edit(selectedSnippets);
            Controls.Add(box);
            this.propertyCombos.Add(box);
            box.MouseDown += new MouseEventHandler(box_MouseDown);
            box.label1.MouseDown += new MouseEventHandler(box_MouseDown);
        }


        public virtual void box_MouseDown(object sender, MouseEventArgs e) { 
            // this is used to 
        }

        public void DeregisterProperty(PropertyComboBox box)
        {
            Controls.Remove(box);
            this.propertyCombos.Remove(box);
            LayoutProperties();
        }

        public void OnAfterRestoreProperties()
        {
            LayoutProperties();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            LayoutProperties();

        }

        /// <summary>
        /// </summary>
        /// <returns>height of the properties</returns>
        internal int LayoutProperties()
        {

            int y = 0;
            int tabIndex = 0;

            foreach (PropertyComboBox box in propertyCombos)
            {
                box.Location = new System.Drawing.Point(0, y);
                y += box.Height;
                box.Width = ClientSize.Width;
                box.TabIndex = tabIndex++;
            }

            // if there are no properties show the instructions
            if (propertyCombos.Count == 0)
            {
                instructions.Visible = true;
                instructions.Top = y;
                y += instructions.Height;
                instructions.Width = ClientSize.Width;
            }
            else
                instructions.Visible = false;

            return y;

        }


        internal virtual List<Snippet> getProperties()
        {
            List<Snippet> retVal = new List<Snippet>();
            foreach (PropertyComboBox combo in propertyCombos)
            {
                retVal.Add(combo.getPropertySnippet());
            }
            return retVal;
        }

        internal virtual void Edit(List<Snippet> selectedSnippets)
        {
            foreach (PropertyComboBox box in propertyCombos)
            {
                box.Edit(selectedSnippets);
            }
            lastSelectedSnippets = selectedSnippets;
        }

        internal void Reset()
        {
            ArrayList copy = new ArrayList(propertyCombos);
            foreach (PropertyComboBox box in copy)
            {
                DeregisterProperty(box);
            }
        }


    }
}
