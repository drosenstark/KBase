using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using Kbase.Model.Search;

namespace Kbase.Search
{
    /// <summary>
    /// Note to self: in the future, don't use this UserControl
    /// crap. You're tying to things that can't be ported to 
    /// .NET Mobile. Bastards!
    /// </summary>
	public class SearchFormPart : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.ComboBox boxParentOrAncestor;
		private System.Windows.Forms.ComboBox boxContainsIs;
		private System.Windows.Forms.ComboBox boxConcat;
        private System.Windows.Forms.ComboBox boxTextOrTitle;
        private System.Windows.Forms.TextBox textBoxSearchText;
        private System.Windows.Forms.LinkLabel selectLink;
        private DateTimePicker dateTimePicker1;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SearchFormPart()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			LoadComboBoxes();

		}

        bool doneLoading = false;
		private void LoadComboBoxes() {
			LoadBox(boxParentOrAncestor,SearchTypeWhere.Ancestor);
			LoadBox(boxTextOrTitle,SearchTypeTextTitle.Text);
			LoadBox(boxContainsIs,SearchTypeIsContains.Contains);
			LoadBox(boxConcat,SearchTypeConcat.And);
            dateTimePicker1.Text = DateTime.Today.ToString();
            doneLoading = true;
		}

		bool firstLine = false;

		public bool FirstLine {
			set {
				firstLine = value;
				SetupConcat();
			}
		}

		private void SetupConcat() {
			if (firstLine) 
			{
				boxConcat.SelectedIndex = boxConcat.Items.IndexOf(SearchTypeConcat.None);
				boxConcat.Visible = false;
			} 
			else {
				boxConcat.Visible = true;
				boxConcat.Items.Remove(SearchTypeConcat.None);
				boxConcat.SelectedItem = SearchTypeConcat.And;
			}
		}

		public SearchCriterion GetSearchCriterion() {
			SearchCriterion retVal = new SearchCriterion();
            retVal.ConcatWithLast =  SearchTypeConcat.None;
            if (boxConcat.SelectedItem != null)
                retVal.ConcatWithLast = (SearchTypeConcat)SearchCriterionConverter.GetTextAsEnum(boxConcat.SelectedItem.ToString(), typeof(SearchTypeConcat));
            retVal.IsContains = (SearchTypeIsContains)SearchCriterionConverter.GetTextAsEnum(boxContainsIs.SelectedItem.ToString(), typeof(SearchTypeIsContains));
            retVal.Where = (SearchTypeWhere)SearchCriterionConverter.GetTextAsEnum(boxParentOrAncestor.SelectedItem.ToString(), typeof(SearchTypeWhere));
            retVal.TextTitle = (SearchTypeTextTitle)SearchCriterionConverter.GetTextAsEnum(boxTextOrTitle.SelectedItem.ToString(), typeof(SearchTypeTextTitle));
			retVal.Word = textBoxSearchText.Text;
			retVal.IgnoreCase = true;
			return retVal;
		}

        /// <summary>
        /// The obj is of the Type that we want to get all of the values for
        /// </summary>
        /// <param name="box"></param>
        /// <param name="obj"></param>
		private void LoadBox(ComboBox box, object obj) {
			Array items = Enum.GetValues(obj.GetType());
			foreach (Object where in items) 
			{
				box.Items.Add(SearchCriterionConverter.GetEnumAsText(where));
			}
			box.SelectedIndex = 0;
		}		

		public void Reset() {
			if (!firstLine) 
				boxConcat.SelectedItem = SearchTypeConcat.And;
			boxParentOrAncestor.SelectedIndex = 0;
			boxTextOrTitle.SelectedIndex = 0;
			boxContainsIs.SelectedIndex = 0;
			textBoxSearchText.Clear();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.boxParentOrAncestor = new System.Windows.Forms.ComboBox();
            this.boxContainsIs = new System.Windows.Forms.ComboBox();
            this.boxConcat = new System.Windows.Forms.ComboBox();
            this.textBoxSearchText = new System.Windows.Forms.TextBox();
            this.boxTextOrTitle = new System.Windows.Forms.ComboBox();
            this.selectLink = new System.Windows.Forms.LinkLabel();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.SuspendLayout();
            // 
            // boxParentOrAncestor
            // 
            this.boxParentOrAncestor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.boxParentOrAncestor.Location = new System.Drawing.Point(48, 0);
            this.boxParentOrAncestor.Name = "boxParentOrAncestor";
            this.boxParentOrAncestor.Size = new System.Drawing.Size(104, 21);
            this.boxParentOrAncestor.TabIndex = 2;
            // 
            // boxContainsIs
            // 
            this.boxContainsIs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.boxContainsIs.Location = new System.Drawing.Point(248, 0);
            this.boxContainsIs.Name = "boxContainsIs";
            this.boxContainsIs.Size = new System.Drawing.Size(72, 21);
            this.boxContainsIs.TabIndex = 4;
            this.boxContainsIs.SelectedIndexChanged += new System.EventHandler(this.boxContainsIs_SelectedIndexChanged);
            // 
            // boxConcat
            // 
            this.boxConcat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.boxConcat.Location = new System.Drawing.Point(0, 0);
            this.boxConcat.Name = "boxConcat";
            this.boxConcat.Size = new System.Drawing.Size(48, 21);
            this.boxConcat.TabIndex = 1;
            // 
            // textBoxSearchText
            // 
            this.textBoxSearchText.Location = new System.Drawing.Point(320, 0);
            this.textBoxSearchText.Name = "textBoxSearchText";
            this.textBoxSearchText.Size = new System.Drawing.Size(232, 20);
            this.textBoxSearchText.TabIndex = 5;
            // 
            // boxTextOrTitle
            // 
            this.boxTextOrTitle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.boxTextOrTitle.Location = new System.Drawing.Point(152, 0);
            this.boxTextOrTitle.Name = "boxTextOrTitle";
            this.boxTextOrTitle.Size = new System.Drawing.Size(96, 21);
            this.boxTextOrTitle.TabIndex = 3;
            this.boxTextOrTitle.SelectedIndexChanged += new System.EventHandler(this.boxTextOrTitle_SelectedIndexChanged);
            // 
            // selectLink
            // 
            this.selectLink.Location = new System.Drawing.Point(562, 0);
            this.selectLink.Name = "selectLink";
            this.selectLink.Size = new System.Drawing.Size(100, 20);
            this.selectLink.TabIndex = 6;
            this.selectLink.TabStop = true;
            this.selectLink.Text = "Select Snippet";
            this.selectLink.Click += new System.EventHandler(this.selectLink_Click);
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(320, 0);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(232, 20);
            this.dateTimePicker1.TabIndex = 7;
            this.dateTimePicker1.Value = new System.DateTime(2007, 11, 29, 0, 0, 0, 0);
            this.dateTimePicker1.Visible = false;
            this.dateTimePicker1.VisibleChanged += new System.EventHandler(this.dateTimePicker1_VisibleChanged);
            this.dateTimePicker1.ValueChanged += new System.EventHandler(this.dateTimePicker1_ValueChanged);
            // 
            // SearchFormPart
            // 
            this.Controls.Add(this.dateTimePicker1);
            this.Controls.Add(this.boxParentOrAncestor);
            this.Controls.Add(this.boxContainsIs);
            this.Controls.Add(this.boxConcat);
            this.Controls.Add(this.textBoxSearchText);
            this.Controls.Add(this.boxTextOrTitle);
            this.Controls.Add(this.selectLink);
            this.Name = "SearchFormPart";
            this.Size = new System.Drawing.Size(652, 24);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        void selectLink_Click(object sender, EventArgs e)
        {
            Universe.Instance.mainForm.Show();
            Universe.Instance.mainForm.Focus();
            Universe.Instance.snippetPane.SelectSnippetLinkStart(new SomeSnippetEventHandler(this.PlugId), this.Parent);
        }

        public void PlugId(Kbase.Model.Snippet snippet) {
            this.textBoxSearchText.Text = snippet.Id.ToString();
            this.boxTextOrTitle.SelectedIndex = this.boxTextOrTitle.Items.IndexOf(SearchCriterionConverter.GetEnumAsText(SearchTypeTextTitle.Id));
        }

        /// <summary>
        /// correct the input
        /// if it's a date, it must be before, is or after
        /// if it's not a date, it must be is or contains
        /// if it's an id, it must be is
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void boxTextOrTitle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!doneLoading) return;
            SearchTypeTextTitle textTitle = (SearchTypeTextTitle)SearchCriterionConverter.GetTextAsEnum(boxTextOrTitle.SelectedItem.ToString(), typeof(SearchTypeTextTitle));
            SearchTypeIsContains isContains = (SearchTypeIsContains)SearchCriterionConverter.GetTextAsEnum(boxContainsIs.SelectedItem.ToString(), typeof(SearchTypeIsContains));
            bool isDate = (textTitle == SearchTypeTextTitle.Created || textTitle == SearchTypeTextTitle.Modified);
            dateTimePicker1.Visible = isDate;
            if (isDate)
            {
                if (isContains != SearchTypeIsContains.Is && isContains != SearchTypeIsContains.Before && isContains != SearchTypeIsContains.After)
                {
                    boxContainsIs.SelectedIndex = boxContainsIs.Items.IndexOf(SearchCriterionConverter.GetEnumAsText(SearchTypeIsContains.Before));
                }
            }
            else if (textTitle == SearchTypeTextTitle.Id) {
                boxContainsIs.SelectedIndex = boxContainsIs.Items.IndexOf(SearchCriterionConverter.GetEnumAsText(SearchTypeIsContains.Is));
            }
            else
            {
                if (isContains == SearchTypeIsContains.Before || isContains == SearchTypeIsContains.After)
                {
                    boxContainsIs.SelectedIndex = boxContainsIs.Items.IndexOf(SearchCriterionConverter.GetEnumAsText(SearchTypeIsContains.Is));
                }
            }
        }

        /// <summary>
        /// Correct the input
        /// if it's before or after, it must be a date
        /// if it's contains, it must not be a date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void boxContainsIs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!doneLoading) return;
            SearchTypeTextTitle textTitle = (SearchTypeTextTitle)SearchCriterionConverter.GetTextAsEnum(boxTextOrTitle.SelectedItem.ToString(), typeof(SearchTypeTextTitle));
            SearchTypeIsContains isContains = (SearchTypeIsContains)SearchCriterionConverter.GetTextAsEnum(boxContainsIs.SelectedItem.ToString(), typeof(SearchTypeIsContains));
            bool isDate = (textTitle == SearchTypeTextTitle.Created || textTitle == SearchTypeTextTitle.Modified);
            if (isContains == SearchTypeIsContains.Is)
            {
                // IS is okay for everybody :)
            }
            else if (isContains == SearchTypeIsContains.Before || isContains == SearchTypeIsContains.After)
            {
                if (!isDate) {
                    boxTextOrTitle.SelectedIndex = boxTextOrTitle.Items.IndexOf(SearchCriterionConverter.GetEnumAsText(SearchTypeTextTitle.Created));
                }
            }
            else if (isContains == SearchTypeIsContains.Contains)
            {
                if (isDate || textTitle == SearchTypeTextTitle.Id)
                {
                    boxTextOrTitle.SelectedIndex = boxTextOrTitle.Items.IndexOf(SearchCriterionConverter.GetEnumAsText(SearchTypeTextTitle.Title));
                }
            }
        }

        /// <summary>
        /// plug the date into the search box when the user makes a change to it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (doneLoading)
                textBoxSearchText.Text = dateTimePicker1.Text;
        }

        /// <summary>
        /// plug the date into the search box if the user choses to see it
        /// ValueChanged may never fire
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dateTimePicker1_VisibleChanged(object sender, EventArgs e)
        {
            if (doneLoading && dateTimePicker1.Visible)
                textBoxSearchText.Text = dateTimePicker1.Text;
        }

	}
}
