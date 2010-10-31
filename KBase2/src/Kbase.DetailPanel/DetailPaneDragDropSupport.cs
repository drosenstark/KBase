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
