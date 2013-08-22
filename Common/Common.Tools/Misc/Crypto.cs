using Platform.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Tools.Misc
{
    public class Crypto
    {
        public sealed class SHA1
        {
            public static string ComputeSHA1(string message, string key)
            {
                try
                {
                    using (HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key)))
                    {
                        byte[] sha1Bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));

                        StringBuilder sha1 = new StringBuilder();
                        for (int i = 0; i < sha1Bytes.Length; i++)
                        {
                            sha1.Append(sha1Bytes[i].ToString("x2"));
                        }

                        return sha1.ToString();
                    }
                }
                catch (Exception e)
                {
                    Logger.Current.Error("Crypto.ComputeSHA1", "Couldn't compute SHA1 for message", e, message);
                }

                return null;
            }

            public static string ComputeSHA1(string message)
            {
                try
                {
                    using (SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider())
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(message);

                        return BitConverter.ToString(
                            cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
                    }
                }
                catch (Exception e)
                {
                    Logger.Current.Error("Crypto.ComputeSHA1", "Couldn't compute SHA1 for message", e, message);
                }

                return null;
            }
        }

        public sealed class SHA256
        {
            public static byte[] ComputeHMACSHA256(byte[] message, byte[] key)
            {
                try
                {
                    using (HMACSHA256 hmacSha256 = new HMACSHA256(key))
                    {
                        return hmacSha256.ComputeHash(message);
                    }
                }
                catch (Exception e)
                {
                    Logger.Current.Error("Crypto.ComputeHMACSHA256", "Couldn't compute SHA256 for message", e);
                }

                return null;
            }
        }

        public sealed class MD5
        {
            public static string GetMD5(string input)
            {
                using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                {
                    byte[] bs = Encoding.UTF8.GetBytes(input);
                    bs = md5.ComputeHash(bs);

                    StringBuilder s = new StringBuilder();
                    foreach (byte b in bs)
                    {
                        s.Append(b.ToString("x2").ToLower());
                    }

                    return s.ToString();
                }
            }
        }

        public sealed class Base64
        {
            public static byte[] UrlDecodeBase64(string input)
            {
                input = input.PadRight(input.Length + (4 - input.Length % 4) % 4, '=');
                input = input.Replace('-', '+').Replace('_', '/');

                return Convert.FromBase64String(input);
            }
        }
    }
}
