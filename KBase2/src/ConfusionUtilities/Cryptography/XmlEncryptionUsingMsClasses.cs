/*
This file is part of Confusion Utilities
Copyright (C) 2004-2007 Daniel Rosenstark
license@confusionists.com
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
