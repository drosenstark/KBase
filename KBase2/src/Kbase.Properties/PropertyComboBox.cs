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
using System.Windows.Forms;
using Kbase.SnippetTreeView;
using Kbase.Model;
using System.Collections.Generic;
using Kbase.MainFrm;

namespace Kbase.Properties
{
	/// <summary>
	/// Summary description for PropertyComboBox.
	/// </summary>
	public class PropertyComboBox : UserControl
	{
		private System.Windows.Forms.ComboBox comboBox1;
		internal System.Windows.Forms.Label label1;

		// propertySnippet is the snippet of the property whose subcategories we're displaying
		Snippet propertySnippet;
        private Splitter splitter;

        List<Snippet> editingTheseSnippets = null;

		
		public PropertyComboBox(Snippet snippet)
		{
			InitializeComponent();
			this.propertySnippet = snippet;
			UpdatePropertyList();
		}

		private void InitializeComponent()
		{
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitter = new System.Windows.Forms.Splitter();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(91, 0);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(237, 21);
            this.comboBox1.TabIndex = 0;
            this.comboBox1.GotFocus += new System.EventHandler(this.PropertyComboBox_GotFocus);
            this.comboBox1.SelectedValueChanged += new System.EventHandler(this.comboBox1_SelectedValueChanged);
            this.comboBox1.KeyUp += new KeyEventHandler(comboBox1_KeyUp);
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 24);
            this.label1.TabIndex = 1;
            // 
            // splitter
            // 
            this.splitter.Location = new System.Drawing.Point(88, 0);
            this.splitter.Name = "splitter";
            this.splitter.Size = new System.Drawing.Size(3, 24);
            this.splitter.TabIndex = 1;
            this.splitter.TabStop = false;
            // 
            // PropertyComboBox
            // 
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.splitter);
            this.Controls.Add(this.label1);
            this.Name = "PropertyComboBox";
            this.Size = new System.Drawing.Size(328, 24);
            this.ResumeLayout(false);

		}

        void comboBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter) {
                comboBox1_OnKeyUpEnter();            
            }
        }

        void comboBox1_OnKeyUpEnter()
        {
            string newText = comboBox1.Text;
            if (newText.Equals(PropertyValue.MULTIPLE_VALUES.ToString()) || newText.Equals(PropertyValue.UNDEFINED_PROPERTY.ToString()))
                return;
            bool isNew = true;
            foreach (PropertyValue value in comboBox1.Items) {
                if (value.ToString().Equals(newText)) {
                    isNew = false;
                    comboBox1.SelectedItem = value;
                    break;
                }            
            }
            if (isNew)
            {
                DialogResult result = MessageBox.Show("Property value " + newText + " does not exist. Create snippet?", MainForm.DialogCaption, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    Snippet newValue = propertySnippet.AddChildSnippet();
                    newValue.Title = newText;
                    PropertyValue value = new PropertyValue(newValue);
                    comboBox1.Items.Add(value);
                    comboBox1.SelectedItem = value;
                }
            }
            else { 
            
            }
        }


		public void Edit(List<Snippet> editingTheseSnippets) 
		{
            if (comboBox1.Items.Contains(PropertyValue.MULTIPLE_VALUES))
                comboBox1.Items.Remove(PropertyValue.MULTIPLE_VALUES);
            // if there's no change, don't redo this processing
			if (this.propertySnippet == null || editingTheseSnippets.Equals(this.editingTheseSnippets))
				return;
			this.editingTheseSnippets = editingTheseSnippets;

            UpdatePropertyList();
            
            // cycle through the currentSnippets 
			// to make sure that each one shares the propertyValue
			PropertyValue hopingFor = null;   // the first propertyValue we find.  The other
			// ones should match hopingFor
			bool areAllTheSame = true;
			
			foreach (Snippet snippet in editingTheseSnippets) 
			{
                Snippet propertyRaw = null;
                PropertyValue property = null;
                try
                {
                    propertyRaw = snippet.GetPropertyValue(propertySnippet);
                }
                catch (MultiplePropertiesSelectedException) {
                    // this is only ONE type of multiple properties, when the 
                    // snippet sits under multiple children of the property
                    property = PropertyValue.MULTIPLE_VALUES;
                    areAllTheSame = false;
                    break;
                }
				// translate the null to an undefined prop
                if (propertyRaw == null)
                    property = PropertyValue.UNDEFINED_PROPERTY;
                else
                    property = new PropertyValue(propertyRaw);

				// if it's the first time around
				if (hopingFor == null) 
					hopingFor = property;
				else 
				{
					if (!hopingFor.Equals(property)) 
					{
                        // this is the OTHER type of multiple properties,
                        // when several snippets in the selection group have 
                        // different values of the property.
						areAllTheSame = false;
						break;
					} 
				}
			}

            try
            {
                // we're selecting, so we have to shut off the events that
                // are for user selection
                suspendAfterSelected = true;
                if (!areAllTheSame)
                {
                    comboBox1.Items.Add(PropertyValue.MULTIPLE_VALUES);
                    comboBox1.SelectedItem = PropertyValue.MULTIPLE_VALUES;
                }
                else
                {
                    if (comboBox1.Items.Contains(PropertyValue.MULTIPLE_VALUES))
                        comboBox1.Items.Remove(PropertyValue.MULTIPLE_VALUES);
                    foreach (PropertyValue val in comboBox1.Items)
                    {
                        if (val.Equals(hopingFor))
                        {
                            comboBox1.SelectedItem = val;
                            break;
                        }
                    }
                }
            }
            finally {
                // turn events back on
                suspendAfterSelected = false;
            }
		}

        // when values are being changed as a result of a selection change
        // we have to make sure the after selection behavior doesn't fire
        // We suspend it by setting this variable to true.
        bool suspendAfterSelected = false;        

		public void UpdatePropertyList() 
		{
			comboBox1.Items.Clear();
			comboBox1.Items.Add(PropertyValue.UNDEFINED_PROPERTY);

            //if (editingTheseSnippets != null)
            //    comboBox1.Items.Add(PropertyValue.MULTIPLE_VALUES);

            foreach (Snippet child in propertySnippet.Children)
            {
                PropertyValue value = new PropertyValue(child);
                comboBox1.Items.Add(value);	
            }
            label1.Text = propertySnippet.Title;
		}


		private void PropertyComboBox_Resize(object sender, System.EventArgs e)
		{
			try 
			{
				comboBox1.Width = Width - comboBox1.Left;

			} 
			catch (Exception e2) 
			{
				MainForm.ShowError(e2);
			}
		}

		private void PropertyComboBox_GotFocus(object sender, EventArgs e)
		{
			try 
			{
				UpdatePropertyList();
			} 
			catch (Exception e2) 
			{
				MainForm.ShowError(e2);
			}

		}


		private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
		{
            if (suspendAfterSelected)
                return;
            try 
			{
				if (editingTheseSnippets == null)
					return;

                PropertyValue selectedVal = comboBox1.SelectedItem as PropertyValue;

                // if the user has somehow selected multiple values, we don't do anything
                if (selectedVal == PropertyValue.MULTIPLE_VALUES || selectedVal == null) {
                    return;
                }

                // we handle all cases
                // 1. user selected undefined from not undefined
                // 2. user selected and had undefined
                // 3. user had a value and wants a new one
                // 4. user had multiple values and wants just one (new 2007-01-18)

                foreach (Snippet snippet in editingTheseSnippets) {
                    bool isMultipleValues = false;
                    Snippet currentValue = null;
                    try
                    {
                        currentValue = snippet.GetPropertyValue(this.propertySnippet);
                    }
                    catch (MultiplePropertiesSelectedException) {
                        isMultipleValues = true;
                    }
                    // handle the multiple case
                    if (isMultipleValues) {
                        List<Snippet> currentValues = snippet.GetMultiplePropertyValues(this.propertySnippet);
                        SelectProperty(selectedVal, snippet, currentValues);
                    }
                    else if (currentValue == null)
                    {
                        // if it's not already null, of course
                        if (selectedVal != PropertyValue.UNDEFINED_PROPERTY)
                            selectedVal.Snippet.AddChildSnippet(snippet);
                    }
                    else {
                        SelectProperty(selectedVal, snippet, currentValue);
                    }
                }			
			} 
			catch (Exception e2) 
			{
				MainForm.ShowError(e2);
			}
		}

        void SelectProperty(PropertyValue selectedVal, Snippet snippet, List<Snippet> currentValues)
        {
            DialogResult okay = DialogResult.Yes;
            if (selectedVal == PropertyValue.UNDEFINED_PROPERTY) {
                if (currentValues.Count == snippet.ParentCount) { 
                    okay = MessageBox.Show(
                        "You are about to permanently delete " + snippet.Title + ". " +
                        "Are you sure?", MainForm.DialogCaption, MessageBoxButtons.YesNo);
                }
                if (okay == DialogResult.Yes)
                {
                    foreach (Snippet currentValue in currentValues)
                    {
                        currentValue.RemoveChildSnippet(snippet);
                    }
                }
            }
            // if it's already got this value, then just remove all the other ones
            else if (currentValues.Contains(selectedVal.Snippet))
            {
                foreach (Snippet currentValue in currentValues)
                {
                    if (currentValue != selectedVal.Snippet) {
                        currentValue.RemoveChildSnippet(snippet);
                    }
                }
            }
            else { 
                // first add it to the new value
                selectedVal.Snippet.AddChildSnippet(snippet);
                // and now delete the old ones
                foreach (Snippet currentValue in currentValues)
                {
                    currentValue.RemoveChildSnippet(snippet);
                }
            }
        }

        void SelectProperty(PropertyValue selectedVal, Snippet snippet, Snippet currentValue) {
            if (selectedVal == PropertyValue.UNDEFINED_PROPERTY)
            {
                DialogResult okay = DialogResult.Yes;
                if (snippet.ParentCount == 1)
                {
                    okay = MessageBox.Show(
                        "You are about to permanently delete " + snippet.Title + ". " +
                        "Are you sure?", MainForm.DialogCaption, MessageBoxButtons.YesNo);
                }
                if (okay == DialogResult.Yes)
                    currentValue.RemoveChildSnippet(snippet);
            }
            else
            {
                if (currentValue != selectedVal.Snippet)
                    snippet.MoveSnippet(currentValue, selectedVal.Snippet);
            }
        }

        public Snippet getPropertySnippet() {
            return propertySnippet;
        }
		public override string ToString()
		{
			return propertySnippet.Title;
		}


	}
}
