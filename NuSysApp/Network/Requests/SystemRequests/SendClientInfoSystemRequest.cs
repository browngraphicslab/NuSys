using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace NuSysApp
{
    public class SendClientInfoSystemRequest : SystemRequest
    {
        public SendClientInfoSystemRequest() : base(SystemRequestType.SendClientInfo){}
        public SendClientInfoSystemRequest(Message m) : base(m) { }

        public override async Task CheckOutgoingRequest()
        {
            #region Try on another machine
            /*
            IReadOnlyList<User> users = await User.FindAllAsync();

            var current = users.Where(p => p.AuthenticationStatus == UserAuthenticationStatus.LocallyAuthenticated &&
                                        p.Type == UserType.LocalUser).FirstOrDefault();

            // user may have username
            var data = await current.GetPropertyAsync(KnownUserProperties.AccountName);
            string displayName = (string)data;

            //or may be authinticated using hotmail 
            if (String.IsNullOrEmpty(displayName))
            {

                string a = (string)await current.GetPropertyAsync(KnownUserProperties.FirstName);
                string b = (string)await current.GetPropertyAsync(KnownUserProperties.LastName);
                displayName = string.Format("{0} {1}", a, b);
            }
            var a = await Windows.System.User.FindAllAsync();
            var b = a.ToImmutableList();
            var c = b[0];
            var l = new List<string>()
            {
                KnownUserProperties.DisplayName,
                KnownUserProperties.AccountName,
                KnownUserProperties.DomainName,
                KnownUserProperties.FirstName,
                KnownUserProperties.LastName,
                KnownUserProperties.GuestHost,
                KnownUserProperties.PrincipalName,
                KnownUserProperties.ProviderName
            };
            var d = await c.GetPropertiesAsync(l);
            var proplist = new Dictionary<string,object>();
            foreach (var e in d)
            {
                proplist[e.Key] = e.Value;
            }
            /*
            System.Environment.
            var s = System.Security.Principal.GenericPrincipal.Current.Identity.Name;*/
#endregion Try on another machine
            var filepath = AppContext.BaseDirectory;
            var fronttrim = filepath.Remove(0, 9);
            int i = 0;
            while (fronttrim[i] != '/')
            {
                i++;
            }
            var trimmed = fronttrim.Remove(i, fronttrim.Length - i);
            _message["name"] = trimmed;
        }
        public override async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, NetworkSession session, string senderIP)
        {
            SessionController.Instance.NuSysNetworkSession.NetworkMembers[senderIP] = new NetworkUser(senderIP);
            SessionController.Instance.NuSysNetworkSession.NetworkMembers[senderIP].Name = _message.GetString("name");
        }
    }
}
