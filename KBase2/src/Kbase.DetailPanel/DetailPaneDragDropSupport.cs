using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Kbase.DetailPanel
{
    public partial class DetailPane
    {

        protected override void OnDragEnter(System.Windows.Forms.DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);
            // allow file drops
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
            {
                drgevent.Effect = DragDropEffects.All;
            }
        }

        protected override void OnDragDrop(System.Windows.Forms.DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);

            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = drgevent.Data.GetData(DataFormats.FileDrop) as string[];
                bool multiple = false;
                foreach (string file in files)
                {
                    // make sure it's not a file
                    if (!new DirectoryInfo(file).Exists)
                    {
                        if (multiple)
                            Paste("\n\r");
                        insertFileLink(file);
                        multiple = true;
                    }
                }
            }
        }
    }
}
