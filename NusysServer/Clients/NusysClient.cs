using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NusysServer
{
    public class NusysClient
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
        private static string _filepath = Constants.FILE_FOLDER + "Users";
        public static ConcurrentDictionary<string, NusysClient> Users = new ConcurrentDictionary<string, NusysClient>();
        public static ConcurrentDictionary<string, NusysClient> IDtoUsers = new ConcurrentDictionary<string, NusysClient>();
        private string _hashedUsername;
        private Dictionary<string, object> _dict;
        private byte[] _salt;
        private byte[] _password;
        public bool Active { set; get; }
        public string ID { get; private set; }

        public bool IsRosemary { get; private set; }
        public NusysClient(string hashedUsername, byte[] hashedpassword, byte[] salt, Dictionary<string, object> dict = null, bool save = true, string presetid = null)
        {
            _hashedUsername = hashedUsername;
            _password = hashedpassword;
            _salt = salt;
            _dict = dict ?? new Dictionary<string, object>();
            Active = false;
            Users[_hashedUsername] = this;
            //ID = presetid ?? Guid.NewGuid().ToString("N");
            ID = _hashedUsername;
            IDtoUsers[ID] = this;
            _dict["id"] = ID;

            if (hashedUsername == "rosemary")
            {
                IsRosemary = true;
            }
        }

        public void Update(Dictionary<string, object> dict)
        {
            if (dict != null)
            {
                foreach (var kvp in dict)
                {
                    _dict[kvp.Key] = kvp.Value;
                }
            }
        }

        public string GetUsername()
        {
            return _hashedUsername;
        }
        public byte[] GetSalt()
        {
            return _salt;
        }

        public byte[] GetPassword()
        {
            return _password;
        }

        public Dictionary<string, object> GetDict()
        {
            return _dict;
        }

        public static void ReadUsers()
        {
            try
            {
                return;
                if (!Directory.Exists(Constants.FILE_FOLDER) || !File.Exists(_filepath))
                {
                    return;
                }
                string line;
                using (StreamReader sr = new StreamReader(_filepath))
                {
                    line = sr.ReadToEnd();
                }

                var dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<String, object>>>(line, settings);
                foreach (var kvp in dict)
                {
                    var id = kvp.Key;
                    var userDict = kvp.Value;
                    var hashedUser = (string)userDict["hashed_username"];
                    var hashedPass = NusysLogins.GetBytes((string)userDict["hashed_password"]);
                    var salt = NusysLogins.GetBytes((string)userDict["salt"]);
                    userDict.Remove("hashed_username");
                    userDict.Remove("hashed_password");
                    userDict.Remove("salt");
                    var user = new NusysClient(hashedUser, hashedPass, salt, userDict, false, id);
                }
            }
            catch (Exception e)
            {
                ErrorLog.AddError(e);
            }
        }
    }
}
