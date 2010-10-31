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
using System.Xml;

namespace ConfusionUtilities.Cryptography
{
    /// 
    /// <summary>
    /// will change an element into an encryptedData element, like this
    /// <hey><EncryptedData xmlns="http://www.w3.org/2001/04/xmlenc#"><EncryptionMethod Algorithm="http://www.w3.org/2001/04/xmlenc#aes128-cbc" /><CipherData><CipherValue>2rC1kHhNQtf8pDQIgEeZTccLVmQXdCpy47TWPNBB1bIEp8Gvb/A/pYNNEgtF+dFN4SmL9IczJayKc8/A+xDDeMRaIhjigIf1yzQcQX3TnXyI40cbrNwfN92O8FzRniHB</CipherValue></CipherData></EncryptedData></hey>
    /// </summary>
    public class XmlEncryption
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="password"></param>
        /// <param name="content">true to replace content, false to replace entire element</param>
        public static void EncryptElement(XmlNode element, string password, bool content)
        {
            RijndaelWrapper wrapper = new RijndaelWrapper(password);
            byte[] cipherXml;
            if (!content)
                cipherXml = wrapper.EncryptMessage(EncryptionUtil.GetBytes(element.OuterXml));
            else
                cipherXml = wrapper.EncryptMessage(EncryptionUtil.GetBytes(element.InnerXml));
            byte[] iv = wrapper.SymmetricAlgorithm.IV;
            byte[] wholeShebang = new byte[cipherXml.Length + iv.Length];
            iv.CopyTo(wholeShebang, 0);
            cipherXml.CopyTo(wholeShebang, iv.Length);
            string encryptedMessage = EncryptionUtil.GetStringBase64(wholeShebang);

            string encryptedElementAsString = string.Format(ENCRYPTED_DATA_TEMPLATE,wrapper.Url);
            encryptedElementAsString = encryptedElementAsString.Replace(REPLACE_THIS, encryptedMessage);
            if (!content)
                element.ParentNode.InnerXml = encryptedElementAsString;
            else
            {
                element.InnerXml = encryptedElementAsString;
            }
        }

        public static void DecryptElement(XmlNode encryptedNode, string password)
        {
            RijndaelWrapper wrapper = new RijndaelWrapper(password);
            string cipherXml = encryptedNode.InnerXml;
            int begin = cipherXml.IndexOf(TAG_BEGIN) + TAG_BEGIN.Length;
            int end = cipherXml.IndexOf(TAG_END);
            string wholeSheBangString = cipherXml.Substring(begin, end - begin);
            byte[] wholeShebang = EncryptionUtil.GetBytesFromBase64String(wholeSheBangString);
            byte[] iv = new byte[wrapper.SymmetricAlgorithm.IV.Length];
            byte[] cipherText = new byte[wholeShebang.Length - iv.Length];
            Array.Copy(wholeShebang, iv, iv.Length);
            Array.Copy(wholeShebang,iv.Length, cipherText, 0,wholeShebang.Length - iv.Length);
            string plainXml = EncryptionUtil.GetString(wrapper.DecryptMessage(cipherText, iv));
            IsLegalXmlText(plainXml);
            encryptedNode.ParentNode.InnerXml = plainXml;
        }

        /// <summary>
        /// depending on the results of the decryption, sometimes this bombs out
        /// though the decryption seems to go fine. This will throw an Exception
        /// with any other problems.
        /// </summary>
        /// <param name="plainXml"></param>
        /// <returns></returns>
        static void IsLegalXmlText(string plainXml) {
            XmlDocument doc = new XmlDocument();
            XmlElement element = doc.CreateElement("test");
            element.InnerXml = plainXml;
            doc.AppendChild(element);
        }

        const string ENCRYPTED_DATA_TEMPLATE = "<EncryptedData xmlns=\"http://www.w3.org/2001/04/xmlenc#\"><EncryptionMethod Algorithm=\"{0}\"/><CipherData><CipherValue>REPLACE_THIS</CipherValue></CipherData></EncryptedData>";
        public const string ENCRYPTED_NODE_NAME = "EncryptedData";
        const string REPLACE_THIS = "REPLACE_THIS";
        const string TAG_BEGIN = "<CipherValue>";
        const string TAG_END = "</CipherValue>";
    }
}
