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
    public class ExternalEditStopButton : Button
    {
        public ExternalEditStopButton(ExternalEditButton editButton)
        {
            TextAlign = ContentAlignment.MiddleCenter;
            this.editButton = editButton;
            this.Text = "Stop Editing";
            this.Enabled = false;
        }

        private ExternalEditButton editButton;

        protected override void OnClick(EventArgs e)
        {
            editButton.StopWatching();
        }
    }
}
