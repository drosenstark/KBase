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
	public class SearchForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button buttonSearch;
		private System.Windows.Forms.Button buttonCancel;
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
			this.buttonCancel = new System.Windows.Forms.Button();
			// this.searchFormPart = new SearchFormPart();
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
			this.AcceptButton = buttonSearch;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Location = new System.Drawing.Point(408, 88);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.TabIndex = 4;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// searchFormPart
			// 
			// this.searchFormPart.Location = new System.Drawing.Point(8, 8);
			// this.searchFormPart.Size = new System.Drawing.Size(652, 24);

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
			this.AutoScaleDimensions = new SizeF(5, 13);
			this.ClientSize = new System.Drawing.Size(670, 120);
			this.ControlBox = false;
			this.Controls.Add(this.buttonReset);
			// this.Controls.Add(this.searchFormPart);
			this.Controls.Add(this.buttonSearch);
			this.Controls.Add(this.buttonCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "SearchForm";
			ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Search For Snippets";
			this.Load += new System.EventHandler(this.SearchForm_Load);
			this.ResumeLayout(false);

            this.LostFocus += new EventHandler(SearchForm_LostFocus);
            AddNewFormPart();
            AddNewFormPart();
            AddNewFormPart();
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
            buttonCancel.Location = new Point(buttonCancel.Location.X, y);
            buttonReset.Location = new Point(buttonReset.Location.X, y);
            buttonSearch.Location = new Point(buttonSearch.Location.X, y);
            buttonMore.Location = new Point(buttonMore.Location.X, y);
            buttonLess.Location = new Point(buttonLess.Location.X, y);
            ClientSize = new Size(ClientSize.Width, y + buttonCancel.Height + 8);
        }

        void SearchForm_LostFocus(object sender, EventArgs e)
        {
        }

		private void comboBoxConcat_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
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
        private void buttonCancel_Click(object sender, System.EventArgs e)
		{
			try 
			{
				this.Visible = false;
			} 
			catch (Exception e2) 
			{
				MainForm.ShowError(e2);
			}

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
                this.Visible = false;
            } 
			catch (Exception e2) 
			{
				MainForm.ShowError(e2);
			}

		}

		private void SearchForm_Load(object sender, System.EventArgs e)
		{
		
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
