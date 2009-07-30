/*
This file is part of TheKBase Desktop
A Multi-Hierarchical  Information Manager
Copyright (C) 2004-2007 Daniel Rosenstark

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
using System.Windows.Forms;
using System.Drawing;
using Kbase.MultipleSelectionTreeView;
using System.Reflection;
using System.IO;

namespace Kbase.Icon
{
    public class NamedIconSet : IComparable
    {
        public int IndexUnselected;
        public int IndexSelected;

        NamedImage imageSelected = null;

        public NamedImage ImageSelected {
            set {
                imageSelected = value;
            }
            get {
                return imageSelected;
            }
        }

        public void FillInImageSelected() { 
            if (imageSelected == null && ImageUnselected != null) {
                imageSelected = new NamedImageSelected((Image)ImageUnselected.image.Clone(), ImageUnselected.OriginalName);
            }
        }

        public NamedImage ImageUnselected;


        public string FancyName {
            get {
                return ImageUnselected.FancyName;
            }
        }

        public int CompareTo(object obj)
        {
            if (obj is NamedIconSet)
            {
                NamedIconSet other = (NamedIconSet)obj;
                return this.FancyName.CompareTo(other.FancyName);
            }
            else
            {
                return ToString().CompareTo(obj.ToString());
            }
        }
    }

}
