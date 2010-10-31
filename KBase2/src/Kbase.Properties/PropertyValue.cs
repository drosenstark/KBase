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
using Kbase.Model;
using System.Diagnostics;

namespace Kbase.Properties
{

    internal class PropertyValue
    {
        internal static PropertyValue UNDEFINED_PROPERTY = new PropertyValue("*** NONE ***");
        internal static PropertyValue MULTIPLE_VALUES = new PropertyValue("*** MULTIPLE VALUES ***");

        Snippet snippet = null;
        string title = null;

        internal PropertyValue(Snippet snippet) {
            this.snippet = snippet;        
        }

        internal PropertyValue(string title)
        {
            this.title = title;
        }

        public Snippet Snippet {
            get {
                return snippet;
            }
        }

        public override string ToString()
        {
            if (snippet != null)
                return snippet.Title;
            else {
                Debug.Assert(title != null);
                return title;
            }
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PropertyValue))
                return false;
            else {
                bool retVal;
                PropertyValue objCasted = (PropertyValue)obj;
                if (snippet == null) {
                    retVal = title.Equals(objCasted.title);
                } else {
                    retVal = (snippet == objCasted.snippet);
                }
                return retVal;
            }

        }
    }


}
