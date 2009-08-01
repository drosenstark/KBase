using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kbase.Model;
using System.Drawing;
using Kbase.LibraryWrap;
using Kbase.MainFrm;

namespace Kbase.DetailPanel
{
    public partial class ExternalEditButton : CheckBox
    {
        public ExternalEditButton()
        {
            Appearance = Appearance.Button;
            FlatStyle = FlatStyle.System;
            TextAlign = ContentAlignment.MiddleCenter;
        }

        private Snippet snippet;
        private bool initing = false;

        public void Edit(Snippet snippet)
        {
            initing = true;
            this.snippet = snippet;
            if (snippet == null)
                return;

            if (snippet.WatchSnippet != null)
            {
                this.Text = "Stop Outside Editing";
                ToolTip tip = new ToolTip();
                tip.ToolTipTitle = "Snippet In External Editor";
                tip.SetToolTip(this, "The snippet is being edited in an external editor. Click to stop 'watching' that file.");
                this.Checked = true;
            }
            else
            {
                this.Text = "Edit Outside";
                ToolTip tip = new ToolTip();
                tip.ToolTipTitle = "Launch External Editor";
                tip.SetToolTip(this, "Press to launch an external editor with this snippet.");
                this.Checked = false;
            }
            initing = false;
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);
            if (initing)
                return;
            if (Checked && snippet.WatchSnippet == null)
            {
                ExternalSnippet eSnippet = new ExternalSnippet(snippet);
                snippet.WatchSnippet = eSnippet;
                eSnippet.StartWatching();
            }
            else if (!Checked && snippet.WatchSnippet != null)
            {
                snippet.WatchSnippet.ShutDown();
                snippet.WatchSnippet = null;
            }
            else
            {
                MainForm.ShowError("houston we have a problem 2235");
            }

        }
    }
}
