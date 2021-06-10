using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Cryptor
{
    internal class XORCryptor
    {
        public static byte[] ApplyXor(byte[] _origin, byte[] _keyBytes)
        {
            var ret = new byte[_origin.Length];
            Array.Copy(_origin, ret, _origin.Length);

            int idx = 0;

            while (idx < _origin.Length)
            {
                foreach (var keyEle in _keyBytes)
                {
                    ret[idx] ^= keyEle;
                    idx++;
                    if (idx == _origin.Length)
                        break;
                }
            }
            return ret;
        }
    }
}
