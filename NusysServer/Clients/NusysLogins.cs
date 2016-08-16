using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysServer
{
    public class NusysLogins
    {
        private static RandomNumberGenerator _saltCrypto = RNGCryptoServiceProvider.Create();

        /// <summary>
        /// method used to validate a login attempt.  
        /// Will return a bool for success and a nusysClient for the logged in client if it was valid
        /// </summary>
        /// <param name="hashedUsername"></param>
        /// <param name="hashedPassword"></param>
        /// <returns></returns>
        public static Tuple<bool, NusysClient> Validate(string hashedUsername, string hashedPassword)
        {
            var doubleHashedUsername = GetString(Encrypt(hashedUsername));

            //create query to get user info based off of username
            var getInfoQuery = new SQLSelectQuery(new SingleTable(Constants.SQLTableType.Users),new SqlQueryEquals(Constants.SQLTableType.Users, NusysConstants.USERS_TABLE_HASHED_USER_ID_KEY, doubleHashedUsername));

            var infoQueryResults = getInfoQuery.ExecuteCommand();

            if (!infoQueryResults.Any())
            {
                throw new Exception("That username wasn't found, it is case-sensitive.");
            }

            var userMessage = infoQueryResults.First();

            var user = NusysClient.CreateFromDatabaseMessage(userMessage);

            //if the passed in username and password is valid
            if (GetString(Encrypt(Combine(GetBytes(hashedPassword), GetBytes(user.Salt)))) == user.Password)
            {
                return new Tuple<bool, NusysClient>(true, user);
            }

            //if it wasn't valid, return false
            throw new Exception("That username and password combination was invalid.");
        }

        /// <summary>
        /// method used to create a new user when the username isn't already taken.
        /// Returns a new client if the new user was made
        /// </summary>
        /// <param name="hashedUsername"></param>
        /// <param name="hashedPassword"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public static NusysClient CreateNewUser(string hashedUsername, string hashedPassword, string displayName)
        {
            var doubleHashedUsername = GetString(Encrypt(hashedUsername));
            var checkQuery = new SQLSelectQuery(new SingleTable(Constants.SQLTableType.Users),
                new SqlQueryEquals(Constants.SQLTableType.Users, NusysConstants.USERS_TABLE_HASHED_USER_ID_KEY,
                    doubleHashedUsername));
            if (checkQuery.ExecuteCommand().Any())
            {
                throw new Exception("That username is already taken!");
            }

            //if the name isn't taken, create salt adn add it to final password
            var salt = CreateSalt();
            var finalPass = GetString(Encrypt(Combine(GetBytes(hashedPassword), salt)));
            var saltString = GetString(salt);

            //create message for table insertion
            var insertMessage = new Message()
            {
                { NusysConstants.USERS_TABLE_HASHED_USER_ID_KEY, doubleHashedUsername},
                { NusysConstants.USERS_TABLE_HASHED_PASSWORD_KEY, finalPass },
                { NusysConstants.USERS_TABLE_LAST_TEN_COLLECTIONS_USED_KEY, new List<string>() },
                { NusysConstants.USERS_TABLE_SALT_KEY, saltString },
                { NusysConstants.USERS_TABLE_USER_DISPLAY_NAME_KEY, displayName }
            };

            //create the query for inserting the new person
            var insertQuery = new SQLInsertQuery(Constants.SQLTableType.Users, insertMessage);
            if (insertQuery.ExecuteCommand())
            {
                //return the client from that message
                return NusysClient.CreateFromDatabaseMessage(insertMessage);
            }
            return null;
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
    }
}
