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
using System.Text;

namespace Kbase.LibraryWrap
{
    internal class PlugInErrorHandler : Kbase2Api.ErrorHandler
    {
        #region ErrorHandler Members

        public void ShowError(Exception e, string text)
        {
            MainFrm.MainForm.ShowError(e, text);
        }

        public void ShowError(string text)
        {
            MainFrm.MainForm.ShowError(new Exception(text));
        }

        public void ShowError(Exception e)
        {
            MainFrm.MainForm.ShowError(e);
        }

        public void ShowErrorSilent(Exception e, string text)
        {
            MainFrm.MainForm.ShowErrorSilent(e);
        }

        public void ShowErrorSilent(string text)
        {
            MainFrm.MainForm.ShowErrorSilent(new Exception(text));
        }

        public void ShowErrorSilent(Exception e)
        {
            MainFrm.MainForm.ShowErrorSilent(e);
        }

        #endregion
    }
}
