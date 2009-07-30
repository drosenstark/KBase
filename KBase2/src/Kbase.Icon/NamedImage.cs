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
    public abstract class NamedImage
    {
        public Image image;
        public string OriginalName;
        public NamedImage(Image image, string name)
        {
            this.image = image;
            this.OriginalName = name;
        }

        protected abstract string Prefix
        {
            get;
        }

        private string fancyName = null;

        /// <summary>
        /// Fancy name is the same for closed and open, it is the name without the prefix
        /// and without the file extension.
        /// </summary>
        public string FancyName
        {
            get
            {
                if (fancyName == null)
                {
                    int start = OriginalName.IndexOf(Prefix);
                    if (start != -1)
                    {
                        start += Prefix.Length;
                        int end = OriginalName.LastIndexOf(".ico") - start;
                        fancyName = OriginalName.Substring(start, end);
                    }
                    else
                        fancyName = OriginalName;
                }
                return fancyName;
            }
        }


    }

    public class NamedImageUnselected : NamedImage {

        public NamedImageUnselected(Image image, string name) : base(image, name) {
        }

        protected override string Prefix
        {
            get {
                return "closed";
            }
        }    
    }


    public class NamedImageSelected : NamedImage
    {

        public NamedImageSelected(Image image, string name) : base(image, name) {
        }

        protected override string Prefix
        {
            get
            {
                return "open";
            }
        }    


    }


}
