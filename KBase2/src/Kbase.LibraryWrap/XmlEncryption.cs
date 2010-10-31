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

namespace Kbase.LibraryWrap
{
    class XmlEncryption
    {

        static XmlEncryption() {
            ENCRYPTED_NODE_NAME = ConfusionUtilities.Cryptography.XmlEncryption.ENCRYPTED_NODE_NAME;
        }

        internal static void EncryptElement(System.Xml.XmlNode xmlNode, string password, bool content)
        {
            ConfusionUtilities.Cryptography.XmlEncryption.EncryptElement(xmlNode, password, content);
        }

        internal static string ENCRYPTED_NODE_NAME = null;

        internal static void DecryptElement(System.Xml.XmlNode encryptedNode, string password)
        {
            ConfusionUtilities.Cryptography.XmlEncryption.DecryptElement(encryptedNode, password);
        }
    }
}
