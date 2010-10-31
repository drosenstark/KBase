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
using System.Windows.Forms;
using Kbase.MainFrm;
using System.Diagnostics;

namespace Kbase
{
    /// <summary>
    /// Facade, wraps the password and acts as a service facade for the password form
    /// </summary>
    public class Encryption
    {
        string password = null;

        public string Password {
            get {
                return password;
            }
            set {
                password = value;
            }
        }

        public bool On {
            get {
                return (password != null);
            }
        }

        public void Reset() {
            password = null;
        }

        /// <summary>
        /// this method does NOT set the password in this class... clients must do that
        /// </summary>
        /// <param name="lastTry"></param>
        /// <returns></returns>
        public string SolicitPasswordOnLoad(string lastTry) {
            return SolicitPasswordForm.GetPassword(PasswordReason.LoadingEncryptedFile,lastTry);
        }


        public bool SolicitPasswordOnUIBlock()
        {
            string check = SolicitPasswordForm.GetPassword(PasswordReason.UiIsBlockedDueToEncryption, null);
            return (password.Equals(check));
        }


        /// <summary>
        /// this method DOES set the password, and will also clear out the encryption
        /// </summary>
        /// <param name="lastTry"></param>
        /// <returns></returns>
        public string SolicitPassword()
        {
            string retVal = SolicitPasswordForm.GetPassword(PasswordReason.UserWantsEncryption,Password);
            Password = retVal;
            return retVal;
        }

    }
}
