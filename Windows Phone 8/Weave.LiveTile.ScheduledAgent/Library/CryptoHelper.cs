using System;

namespace Common.Hashing
{
    internal class CryptoHelper
    {
        // approach using MD5 and GUIDs
        public static Guid ComputeHash(string val)
        {
            var md5 = new System.Security.Cryptography.SHA1Managed();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(val);
            byte[] hash = md5.ComputeHash(inputBytes);
            Array.Resize(ref hash, 16);
            var guid = new Guid(hash);
            return guid;
        }
    }
}
