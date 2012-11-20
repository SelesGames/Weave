using System;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace Weave.Data.Library
{
    internal class CryptoHelper
    {
        // approach using MD5 and GUIDs
        public static Guid ComputeHash(string val)
        {
            IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(val, BinaryStringEncoding.Utf8);
            var md5 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var hashBuffer = md5.HashData(buffer);
            
            byte[] hashBytes;
            CryptographicBuffer.CopyToByteArray(hashBuffer, out hashBytes);

            Array.Resize(ref hashBytes, 16);
            var guid = new Guid(hashBytes);
            return guid;
        }
    }
}
