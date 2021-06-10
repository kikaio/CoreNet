using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Cryptor
{
    internal class DHCryptor
    {
        public static byte[] DhEncrypt(string _msgBase64, byte[] _key, byte[] _iv)
        {
            var ret = default(byte[]);
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;
                ICryptoTransform enc = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, enc, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(_msgBase64);
                        }
                    }
                    ret = ms.ToArray();
                }
            }
            return ret;
        }

        public static string DhDecrypt(byte[] _bytes, byte[] _key, byte[] _iv)
        {
            var ret = default(string);
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;
                ICryptoTransform dec = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream(_bytes))
                {
                    using (var cs = new CryptoStream(ms, dec, CryptoStreamMode.Read))
                    {
                        using (var sr = new StreamReader(cs))
                        {
                            ret = sr.ReadToEnd();
                        }
                    }
                }
            }
            return ret;
        }
    }
}
