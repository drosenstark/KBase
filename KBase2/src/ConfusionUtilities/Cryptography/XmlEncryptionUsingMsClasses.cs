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
using System.Security.Cryptography.Xml;
using System.Xml;

namespace ConfusionUtilities.Cryptography
{
    public class XmlEncryptionUsingMsClasses
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="password"></param>
        /// <param name="content">true to replace content, false to replace entire element</param>
        public static void EncryptElement(XmlNode node, string password, bool content)
        {
            if (node is XmlElement)
            {
                EncryptElement((XmlElement)node, password, content);
            }
            else
                throw new Exception("Can only encrypt XmlElements");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="password"></param>
        /// <param name="content">true to replace content, false to replace entire element</param>
        public static void EncryptElement(XmlElement element, string password, bool content) {
            XmlDocument doc = element.OwnerDocument;
            EncryptedXml eXml = new EncryptedXml(doc);
            
            RijndaelWrapper wrapper = new RijndaelWrapper(password);
            byte[] cipherText = eXml.EncryptData((XmlElement)doc.FirstChild.FirstChild, wrapper.SymmetricAlgorithm, content);
            EncryptedData data = new EncryptedData();
            data.EncryptionMethod = new EncryptionMethod(wrapper.Url);
            data.CipherData = new CipherData(cipherText);
            data.KeyInfo = new KeyInfo();
            EncryptedXml.ReplaceElement(element, data, content);
        }


        public static void DecryptElement(XmlNode node, string password)
        {
            if (node is XmlElement)
            {
                DecryptElement((XmlElement)node, password);
            }
            else
                throw new Exception("Can only deencrypt XmlElements");
        }

        public static void DecryptElement(XmlElement encryptedElement, string password)
        {
            RijndaelWrapper wrapper = new RijndaelWrapper(password);
            EncryptedData data = new EncryptedData();
            data.LoadXml(encryptedElement);
            EncryptedXml result = new EncryptedXml();
            byte[] decrypted = result.DecryptData(data, wrapper.SymmetricAlgorithm);
            result.ReplaceData(encryptedElement, decrypted);
        }


    }
}
