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
