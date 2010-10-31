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
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Kbase.SnippetTreeView
{
    public partial class SelectSnippetForm : Form
    {


        public SelectSnippetForm()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Universe.Instance.snippetPane.SelectSnippetLinkStopNoSelection();
            Universe.Instance.snippetPane.Focus();
        }

        private void SelectSnippetForm_Load(object sender, EventArgs e)
        {
        }

        private void SelectSnippetForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Universe.Instance.snippetPane.SelectSnippetLinkStopNoSelection();
            e.Cancel = true;
        }

    }
}