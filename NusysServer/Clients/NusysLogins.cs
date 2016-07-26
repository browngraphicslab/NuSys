using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NusysServer
{
    public class NusysLogins
    {
        private static RandomNumberGenerator _saltCrypto = RNGCryptoServiceProvider.Create();
        public static Tuple<bool, NusysClient> Validate(string hashedUser, string hashedPass, bool createNew)
        {
            if ((!NusysClient.Users.ContainsKey(hashedUser) && !createNew) || (NusysClient.Users.ContainsKey(hashedUser) && createNew))
            {
                return new Tuple<bool, NusysClient>(false, null);
            }

            var createdSalt = CreateSalt();
            var user = NusysClient.Users.ContainsKey(hashedUser)
                ? NusysClient.Users[hashedUser]
                : new NusysClient(hashedUser, Encrypt(Combine(GetBytes(hashedPass), createdSalt)), createdSalt);

            var salt = user.GetSalt();
            var sentPassString = GetString(Encrypt(Combine(GetBytes(hashedPass), salt)));
            var userPassString = GetString(user.GetPassword());
            var valid = sentPassString == userPassString;
            if (!valid)
            {
                user = null;
            }
            return new Tuple<bool, NusysClient>(valid, user);
        }

        public static byte[] CreateSalt()
        {
            var bytes = new byte[256];
            _saltCrypto.GetBytes(bytes);
            return bytes;
        }

        public static byte[] Encrypt(string plain)
        {
            return Encrypt(GetBytes(plain));
        }

        public static byte[] Encrypt(byte[] bytes)
        {
            var sha = System.Security.Cryptography.SHA256.Create();
            return sha.ComputeHash(bytes);
        }
        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public static bool IsLoginRequest(Dictionary<string, object> dict)
        {
            return dict.ContainsKey(Constants.LOGIN_REQUEST_STRING);
        }
    }
}
