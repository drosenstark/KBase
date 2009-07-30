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
using System.Text;
using Kbase.Model.Search;

namespace Kbase.Serialization
{
    [Serializable()]
    public class SerializableCriterion
    {
        string criterionText = null;
        public SerializableCriterion(SearchCriterion criterion) {
            makeText(criterion);   
        }

        public SerializableCriterion(string criterionText)
        {
            this.criterionText = criterionText;
        }
        
        /// <returns>null if the criterion is blank</returns>
        public string GetAsText()
        {
            return criterionText;
        }

        private void makeText(SearchCriterion criterion) {
            if (criterion.Word == null || criterion.Word.Length == 0)
                return;
            StringBuilder retVal = new StringBuilder();
            AppendNumber(retVal, "ConcatWithLast", (int)criterion.ConcatWithLast);
            int ignoreCase = 0;
            if (criterion.IgnoreCase)
                ignoreCase = 1;
            AppendNumber(retVal, "IgnoreCase", ignoreCase);
            AppendNumber(retVal, "IsContains", (int)criterion.IsContains);
            AppendNumber(retVal, "TextTitle", (int)criterion.TextTitle);
            AppendNumber(retVal, "Where", (int)criterion.Where);
            AppendText(retVal, "Word", criterion.Word);
            criterionText = retVal.ToString();
        }

        public SearchCriterion GetCriterion() {
            SearchCriterion retVal = new SearchCriterion();
            retVal.ConcatWithLast = (SearchTypeConcat)Int32.Parse(GetValue("ConcatWithLast", criterionText));
            int ignoreCase = Int32.Parse(GetValue("IgnoreCase", criterionText));
            retVal.IgnoreCase = (ignoreCase == 1);
            retVal.IsContains = (SearchTypeIsContains)Int32.Parse(GetValue("IsContains", criterionText));
            retVal.TextTitle = (SearchTypeTextTitle)Int32.Parse(GetValue("TextTitle", criterionText));
            retVal.Where = (SearchTypeWhere)Int32.Parse(GetValue("Where", criterionText));
            retVal.Word = GetLastValue("Word",criterionText);
            return retVal;
        }

        void AppendNumber(StringBuilder text, string name, int number)
        {
            text.AppendFormat("{0}={1};", name, number);
        }

        void AppendText(StringBuilder text, string name, String textParam)
        {
            text.AppendFormat("{0}={1};", name, textParam);
        }


        /// <summary>
        /// for parsing strings like this
        /// ConcatWithLast=0;IgnoreCase=whatever was here!;IsContains=0;TextTitle=1;Where=1;Word=adfads and hello there;feee;
        /// </summary>
        string GetValue(string name, String text)
        {
            string retVal = null;
            int where = text.IndexOf(name) + name.Length + 1; // where does the equals sign end
            int end = text.IndexOf(";", where);
            retVal = text.Substring(where, end - where);
            return retVal;
        }

        /// <summary>
        /// for parsing strings like this
        /// ConcatWithLast=0;IgnoreCase=whatever was here!;IsContains=0;TextTitle=1;Where=1;Word=adfads and hello there;feee;
        /// get last value allows the last value to contain semicolons, equals signs, whatever
        /// </summary>
        string GetLastValue(string name, String text)
        {
            string retVal = null;
            int where = text.IndexOf(name) + name.Length + 1; // where does the equals sign end
            int end = text.LastIndexOf(";");
            retVal = text.Substring(where, end - where);
            return retVal;
        }


    }
}
