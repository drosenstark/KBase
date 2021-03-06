using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Kbase.MainFrm;
using Kbase.Model.Search;

namespace Kbase.Search
{
    public class SearchForm : System.Windows.Forms.Panel
	{
		private System.Windows.Forms.Button buttonSearch;
		private List<SearchFormPart> searchFormParts = new List<SearchFormPart>();
		private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Button buttonMore;
        private System.Windows.Forms.Button buttonLess;
        /// <summary>
		/// Required designer variable.
		/// </summary>

        private System.ComponentModel.Container components = null;

		public SearchForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
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
			this.buttonSearch = new System.Windows.Forms.Button();
			this.buttonReset = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// buttonSearch
			// 
			this.buttonSearch.Location = new System.Drawing.Point(488, 88);
			this.buttonSearch.Name = "buttonSearch";
			this.buttonSearch.TabIndex = 4;
			this.buttonSearch.Text = "Search";
			this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);


            // 
			// buttonReset
			// 
			this.buttonReset.Location = new System.Drawing.Point(8, 88);
			this.buttonReset.Name = "buttonReset";
			this.buttonReset.TabIndex = 5;
			this.buttonReset.Text = "Reset";
			this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);

            // 
            // buttonMore
            // 
            buttonMore = new Button();
            buttonMore.Width = 20;
            this.Controls.Add(buttonMore);
            this.buttonMore.Location = new System.Drawing.Point(625, 88);
            this.buttonMore.Name = "More";
            this.buttonMore.TabIndex = 5;
            this.buttonMore.Text = "+";
            this.buttonMore.Click += new System.EventHandler(this.buttonMore_Click);
            ToolTip tip = new ToolTip();
            tip.SetToolTip(buttonMore, "Add another search term");

            // 
            // buttonLess
            // 
            buttonLess = new Button();
            buttonLess.Width = 20;
            this.Controls.Add(buttonLess);
            this.buttonLess.Location = new System.Drawing.Point(570, 115);
            this.buttonLess.Name = "Less";
            this.buttonLess.TabIndex = 5;
            this.buttonLess.Text = "-";
            this.buttonLess.Click += new System.EventHandler(this.buttonLess_Click);
            tip.SetToolTip(buttonLess, "Remove the last search term");
            
            // 
			// SearchForm
			// 
			this.Controls.Add(this.buttonReset);
			// this.Controls.Add(this.searchFormPart);
			this.Controls.Add(this.buttonSearch);
			this.Name = "SearchForm";
			this.Text = "Search For Snippets";
			this.ResumeLayout(false);

            AddNewFormPart();
            AddNewFormPart();
            //AddNewFormPart();

        }

        void AddNewFormPart() {
            SearchFormPart part = new SearchFormPart();
            if (searchFormParts.Count == 0)
                part.FirstLine = true;
            this.searchFormParts.Add(part);
            this.Controls.Add(part);
            LayoutEverything();
        }

        void LayoutEverything() {
            int y = 8;
            foreach (SearchFormPart part in searchFormParts) {
                part.Location = new Point(8, y);
                y = y + part.Height;
            }
            buttonReset.Location = new Point(buttonReset.Location.X, y);
            buttonSearch.Location = new Point(buttonSearch.Location.X, y);
            buttonMore.Location = new Point(buttonMore.Location.X, y);
            buttonLess.Location = new Point(buttonLess.Location.X, y);
//            ClientSize = new Size(ClientSize.Width, y + buttonCancel.Height + 8);
        }


        private void buttonMore_Click(object sender, System.EventArgs e)
        {
            AddNewFormPart();
        }

        private void buttonLess_Click(object sender, System.EventArgs e)
        {
            if (this.searchFormParts.Count > 1) {
                SearchFormPart last = searchFormParts[searchFormParts.Count-1];
                searchFormParts.Remove(last);
                this.Controls.Remove(last);
                LayoutEverything();                
            }

        }

        public void Search()
        {
            buttonSearch_Click(null, null);
        }
		private void buttonSearch_Click(object sender, System.EventArgs e)
		{
			try 
			{
                List<SearchCriterion> criteria = new List<SearchCriterion>();
                foreach (SearchFormPart part in searchFormParts) {
                    criteria.Add(part.GetSearchCriterion());
                }
                Universe.Instance.ModelGateway.Search(criteria);
            } 
			catch (Exception e2) 
			{
				MainForm.ShowError(e2);
			}

		}


		private void buttonReset_Click(object sender, System.EventArgs e)
		{
			try 
			{
                foreach (SearchFormPart part in searchFormParts)
                {
                    part.Reset();
                }
            } 
			catch (Exception e2) 
			{
				MainForm.ShowError(e2);
			}
		}

	}
}
