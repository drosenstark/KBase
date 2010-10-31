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
using System.Security.Cryptography;

namespace ConfusionUtilities.Cryptography
{
    /// <summary>
    /// Class originally taken from http://www.developerfusion.co.uk/show/4647/
    /// But redone to make it a bit more OO (or a bit less?)
    /// </summary>
    public class RijndaelWrapper 
    {
        RijndaelManaged cipher = null;

        public string Url {
            get
            {
                string retVal = null;

                if (cipher.KeySize == 128)
                    retVal = "http://www.w3.org/2001/04/xmlenc#aes128-cbc";
                    // retVal = EncryptedXml.XmlEncAES128Url;
                if (cipher.KeySize == 192)
                    retVal = "http://www.w3.org/2001/04/xmlenc#aes192-cbc";
                    /// retVal = EncryptedXml.XmlEncAES192Url;
                if (cipher.KeySize == 256)
                    retVal = "http://www.w3.org/2001/04/xmlenc#aes256-cbc";
                    // retVal = EncryptedXml.XmlEncAES256Url;
                return retVal;
            }
    
        }

        public RijndaelManaged SymmetricAlgorithm {
            get {

                if (cipher == null)
                    throw new NullReferenceException("Algorithm hasn't been initialized.");
                else
                    return cipher;
            }
        
        }
        public RijndaelWrapper(byte[] key)
        {
            CreateCipher();
            cipher.Key = key;
            cipher.GenerateIV();
        }



        /// <summary>
        /// Creates the cipher using an SHA256 of the pass with no
        /// salt
        /// </summary>
        /// <param name="password"></param>
        public RijndaelWrapper(string password) {
            byte[] key = EncryptionUtil.GetMD5Hash(password);
            CreateCipher();
            cipher.Key = key;
            cipher.GenerateIV();
        }

        public byte[] Key
        {
            get { return cipher.Key; }
            set { cipher.Key = value; }
        }

        public byte[] EncryptMessage(byte[] plainText)
        {
            byte[] result;
            ICryptoTransform transform = cipher.CreateEncryptor();
            result = transform.TransformFinalBlock(plainText, 0, plainText.Length);
            return result;
        }


        public byte[] DecryptMessage(byte[] input, byte[] initializationVector)
        {
            cipher.IV = initializationVector;
            ICryptoTransform transform = cipher.CreateDecryptor();
            byte[] plainText = transform.TransformFinalBlock(input, 0, input.Length);
            return plainText;
        }
        
        /// <summary>
        /// we have to use 128-128 because of the key size... in any case,
        /// block size would be 128 but we could use Sha256 to get a key size of
        /// 256 bytes, but Kbase Mobile does not support that.
        /// </summary>
        void CreateCipher()
        {
            cipher = new RijndaelManaged();
            cipher.KeySize = 128;
            cipher.BlockSize = 128;
            cipher.Mode = CipherMode.CBC;
            cipher.Padding = PaddingMode.ISO10126;
        }


    }
}
