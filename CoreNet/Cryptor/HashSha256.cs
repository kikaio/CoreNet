using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Cryptor
{
    internal class HashSha256
    {
        public static string PlainStrToBase64WithSha256(string _plainStr, string _saltkey)
        {
            var sb = new StringBuilder(_plainStr);
            sb.Append(_saltkey);
            var shaMgr = new SHA256Managed();
            var encBytes = shaMgr.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            return Convert.ToBase64String(encBytes);
        }
    }
}
