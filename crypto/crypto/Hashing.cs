using System.Security.Cryptography;
using System.Text;

namespace crypto
{
    public class Hashing
    {
        public static string GetHMAC(string text, string key)
        {
            key = key ?? "";

            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                return Convert.ToBase64String(hash);
            }

        }


    }
}