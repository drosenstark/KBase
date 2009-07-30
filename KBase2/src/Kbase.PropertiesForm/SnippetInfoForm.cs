using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Kbase.PropertiesForm
{
    public partial class SnippetInfoForm : Form
    {
        public SnippetInfoForm()
        {
            InitializeComponent();
        }

        public void addProperty(string title, object obj) {
            this.Text = title;
            propertyGrid1.SelectedObject = obj;
        }

        /// <summary>
        /// factory method
        /// </summary>
        /// <param name="title"></param>
        /// <param name="obj"></param>
        public static void showProps(string title, object obj) {
            SnippetInfoForm form = new SnippetInfoForm();
            form.addProperty(title, obj);
            form.Show();
        }
    }
}