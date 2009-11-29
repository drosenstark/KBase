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
    public class ExternalEditButton : Button
    {
        public ExternalEditButton()
        {
            TextAlign = ContentAlignment.MiddleCenter;
            this.Text = "Edit Outside";
            ToolTip tip = new ToolTip();
            tip.ToolTipTitle = "Launch External Editor";
            tip.SetToolTip(this, "Press to launch an external editor with this snippet.");
            externalEditStopButton = new ExternalEditStopButton(this);

        }

        private Snippet snippet;
        public ExternalEditStopButton externalEditStopButton;

        public void Edit(Snippet snippet)
        {
            this.snippet = snippet;
            setVisibleState();
        }

        private void setVisibleState()
        {
            this.Visible = (snippet != null);
            this.externalEditStopButton.Visible = (snippet != null);
            if (snippet != null)
            {
                if (snippet.WatchingExternalFile)
                    this.Text = "Open Again";
                else
                    this.Text = "Edit Outside";
                this.Enabled = true;
                this.externalEditStopButton.Enabled = snippet.WatchingExternalFile;
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (snippet.WatchSnippet == null)
            {
                Universe.Instance.detailPane.Save();
                ExternalSnippet eSnippet = new ExternalSnippet(snippet);
                snippet.WatchSnippet = eSnippet;
                eSnippet.StartWatching();
                Universe.Instance.detailPane.Edit(snippet);
            }
            else
            {
                snippet.WatchSnippet.popOpen();
            }
            setVisibleState();

        }

        internal void StopWatching()
        {
            if (snippet == null)
                return;
            snippet.WatchSnippet.ShutDown();
            snippet.WatchSnippet = null;
            Universe.Instance.detailPane.Edit(snippet);
            setVisibleState();
        }
    }
}
