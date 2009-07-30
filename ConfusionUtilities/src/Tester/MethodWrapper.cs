/*
This file is part of Confusion Utilities
Copyright (C) 2004-2007 Daniel Rosenstark
license@confusionists.com
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace ConfusionUtilities.Tester
{
    public class MethodWrapper : IComparable
    {
        MethodInfo info = null;
        public MethodWrapper(MethodInfo info) {
            this.info = info;        
        }

        public bool IsTest() {
            bool attributeTest = false;

            foreach (Attribute bute in info.GetCustomAttributes(true))
            {
                if (bute.TypeId == new Test().TypeId)
                    attributeTest = true;
            }
            return (attributeTest || info.Name.Length > 4 && info.Name.ToLower().Substring(0, 4).Equals("test"));
        }

        public MethodInfo MethodInfo {
            get {
                return info;
            }
        }

        public override string ToString()
        {
            string retVal = null;
            if (info.Name.Length < 5)
                retVal = info.Name;
            else {
                retVal = info.Name;
                if (retVal.ToLower().StartsWith("test")) 
                    retVal = info.Name.Substring(4, info.Name.Length - 4);
                retVal = PutInSpaces(retVal);
            }
            return retVal;
        }

        private string PutInSpaces(string text)
        {
            StringBuilder retVal = new StringBuilder();
            char[] retValAsChars = text.ToCharArray();
            for (int i = 0; i < retValAsChars.Length; i++) { 
                if (i>0 && Char.IsUpper(retValAsChars[i])) {
                    retVal.Append(" ");
                }
                retVal.Append(retValAsChars[i]);
            }
            
            return retVal.ToString();
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return ToString().CompareTo(obj.ToString());
        }

        #endregion
    }
}
