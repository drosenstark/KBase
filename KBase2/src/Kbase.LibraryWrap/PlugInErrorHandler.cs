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
