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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Kbase.MainFrm
{
    public enum PasswordReason
    {
        LoadingEncryptedFile,
        UserWantsEncryption,
        UiIsBlockedDueToEncryption
    }

    public partial class SolicitPasswordForm : Form
    {
        public static string GetPassword(PasswordReason reason, string oldPass)
        {
            SolicitPasswordForm form = new SolicitPasswordForm();
            if (reason == PasswordReason.LoadingEncryptedFile)
            {
                form.Text = "The File You Are Loading Is Encrypted";
                form.textBoxPassword2.Visible = false;
            } else if (reason == PasswordReason.UiIsBlockedDueToEncryption) {
                form.Text = "Reenter Your Password To Continue";
                form.textBoxPassword2.Visible = false;
            }
            if (oldPass != null)
            {
                form.textBoxPassword.Text = oldPass;
                if (reason == PasswordReason.LoadingEncryptedFile)
                    form.Text += " - Incorrect Password";
                else
                    form.Text += " - Click Cancel To Remove Password";
            }
            form.ShowDialog();
            return form.password;
        }

        SolicitPasswordForm()
        {
            InitializeComponent();
        }

        string password = null;

        private void buttonGo_Click(object sender, EventArgs e)
        {
            password = textBoxPassword.Text;
            string password2 = null;
            if (textBoxPassword2.Visible)
                password2 = textBoxPassword2.Text;
            else
                password2 = password;
            if (password == null || password2 == null)
            {
                MessageBox.Show("Password cannot be null.");
            }
            else if (!password.Equals(password2))
            {
                MessageBox.Show("Passwords do not match.");
                password = null;
                textBoxPassword2.Text = "";
            }
            else
            {
                this.Close(); // with good password
            }
        }



        private void SolicitPasswordForm_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolTip tip = new System.Windows.Forms.ToolTip();
            tip.SetToolTip(textBoxPassword, "Password");
            tip.SetToolTip(textBoxPassword2, "Confirm Password");
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.password = null;
            this.Close();
        }
    }
}