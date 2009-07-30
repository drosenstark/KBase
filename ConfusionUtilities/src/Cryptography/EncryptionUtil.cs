/*
This file is part of Confusion Utilities
Copyright (C) 2004-2007 Daniel Rosenstark
license@confusionists.com
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace ConfusionUtilities.Cryptography
{
    public class EncryptionUtil
    {
         static Encoding encoding = new System.Text.UTF8Encoding();

        public static byte[] GetBytes(string text) {
            return encoding.GetBytes(text);
        }

        public static string GetString(byte[] bytes) {
            return encoding.GetString(bytes);
        }

        public static string GetStringBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static byte[] GetBytesFromBase64String(string base64Text)
        {
            return Convert.FromBase64String(base64Text);
        }

        /// <summary>
        /// Gets md5 hash of the key
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] GetSha256Hash(string password)
        {
            byte[] retVal = null;
            SHA256 hasher = SHA256.Create();
            retVal = hasher.ComputeHash(GetBytes(password));
            return retVal;
        }

        public static byte[] GetMD5Hash(string password)
        {
            return GetMD5Hash(GetBytes(password));
        }

        public static byte[] GetMD5Hash(byte[] password)
        {
            byte[] retVal = null;
            MD5CryptoServiceProvider hasher = new MD5CryptoServiceProvider();
            retVal = hasher.ComputeHash(password);
            return retVal;
        }

        public static string getSalt(int length)
        {
            byte[] salt = new byte[length];
            new Random().NextBytes(salt);
            return GetStringBase64(salt);
        }

        /// <summary>
        /// used for when we get the pass from the user and RETRIEVE 
        /// the salt from the DB
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string getHash(string pass, string salt)
        {
            string retVal = null;
            // the pass is UTF8
            byte[] passBytes = GetBytes(pass);
            // the salt was encoded from bytes originally
            // though we could do it wrong from UTF8
            // with no problem, as long as we're consistent
            byte[] saltBytes = GetBytesFromBase64String(salt);

            // mash 'em together
            byte[] saltPass = new byte[saltBytes.Length + passBytes.Length];
            Array.Copy(saltBytes, saltPass, saltBytes.Length);
            Array.Copy(passBytes, 0, saltPass, saltBytes.Length, passBytes.Length);
            
            // now hash it
            byte[] hash = GetMD5Hash(saltPass);

            retVal = GetStringBase64(hash);

            return retVal;
        }
    }
}
