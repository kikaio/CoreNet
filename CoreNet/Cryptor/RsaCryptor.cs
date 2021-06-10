using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Cryptor
{
    internal class RsaCryptor
    {

        public static string RsaEncryptWithBase64(string _base64, RSAParameters _publicParam)
        {
            using (var _rsa = new RSACryptoServiceProvider())
            {
                byte[] _data = Convert.FromBase64String(_base64);
                _rsa.ImportParameters(_publicParam);
                byte[] encrypted = _rsa.Encrypt(_data, false);
                return Convert.ToBase64String(encrypted);
            }
        }

        public static string RsaDecryptWithBase64(string _base64, RSAParameters _privateParam)
        {
            using (var _rsa = new RSACryptoServiceProvider())
            {
                _rsa.ImportParameters(_privateParam);
                byte[] _data = Convert.FromBase64String(_base64);
                byte[] decBytes = _rsa.Decrypt(_data, false);
                return Convert.ToBase64String(decBytes);
            }
        }
    }
}
