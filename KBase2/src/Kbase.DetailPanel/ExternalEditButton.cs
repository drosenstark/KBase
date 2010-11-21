/*
This file is part of TheKBase Desktop
A Multi-Hierarchical  Information Manager
Copyright (C) 2004-2010 Daniel Rosenstark

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

        static bool botheredForAutosave = false;

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (!botheredForAutosave && !Universe.Instance.mainForm.autoSaveMenu.Checked)
            {
                string message = "Autosave is off. It should be on so that when you save the external file, TheKBase is saved as well. Turn it on?";
				bool result = Universe.Instance.mainForm.ShowDialogYesNo(message);
                if (result)
                {
                    Universe.Instance.mainForm.ClickAutoSaveToggle(null, null);
                }
                botheredForAutosave = true;
            }
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
