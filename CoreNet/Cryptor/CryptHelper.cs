using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Cryptor
{
    public class CryptHelper
    {
        public static string RsaEncryptWithBase64(string _val, RSAParameters _publicParam)
        {
            return RsaCryptor.RsaEncryptWithBase64(_val, _publicParam);
        }
        public static string RsaDecryptWithBase64(string _val, RSAParameters _privateParam)
        {
            return RsaCryptor.RsaDecryptWithBase64(_val, _privateParam);
        }

        public static byte[] DhEncrypt(string _base64Str, byte[] _keyBytes, byte[] _IV)
        {
            return DHCryptor.DhEncrypt(_base64Str, _keyBytes, _IV);
        }

        public static byte[] DhDecrypt(byte[] _cryptBytes, byte[] _keyBytes, byte[] _IV)
        {
            var retBase64 = DHCryptor.DhDecrypt(_cryptBytes, _keyBytes, _IV);
            return Convert.FromBase64String(retBase64);
        }


        public static string PlainStrToBase64WithSha256(string _plainStr, string _saltKey = "")
        {
            return HashSha256.PlainStrToBase64WithSha256(_plainStr, _saltKey);
        }

        public static byte[] XorEncrypt(byte[] _data, byte[] _keyBytes)
        {
            return XORCryptor.ApplyXor(_data, _keyBytes);
        }
    }
}
