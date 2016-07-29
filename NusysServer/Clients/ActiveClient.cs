using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NusysServer
{
    public class ActiveClient
    {
        public static ConcurrentDictionary<NuWebSocketHandler, ActiveClient> ActiveClients = new ConcurrentDictionary<NuWebSocketHandler, ActiveClient>();
        private static BiDictionary<string, NusysClient> _waitingSessionIDs = new BiDictionary<string, NusysClient>();
        public string SessionID { get { return _sessionId; } }
        public NusysClient Client { get { return _client; } }
        public NuWebSocketHandler SocketHandler { get { return _socketHandler; } }

        private readonly NusysClient _client;
        private readonly NuWebSocketHandler _socketHandler;
        private readonly string _sessionId;
        private List<string> _subscribedCollections;

        public ActiveClient(NusysClient client, NuWebSocketHandler socketHandler, string sessionId)
        {
            if (client == null || socketHandler == null || sessionId == null)
            {
                throw new NullReferenceException("Cannot create an active client with a null client, sessionid, or sockethandler");
            }
            _client = client;
            _socketHandler = socketHandler;
            _sessionId = sessionId;
            _subscribedCollections = new List<string>();
            ActiveClients.TryAdd(socketHandler, this);
            client.Active = true;
        }

        public void Disconnect()
        {
            ActiveClient outClient;
            ActiveClients.TryRemove(_socketHandler, out outClient);
            Client.Active = false;
        }

        public static bool AddClient(string sessionID, NuWebSocketHandler handler)
        {
            if (_waitingSessionIDs.ContainsKey(sessionID))
            {
                var client = _waitingSessionIDs[sessionID];
                if (client.Active)
                {
                    return false;
                }
                var activeClient = new ActiveClient(client, handler, sessionID);
                _waitingSessionIDs.Remove(sessionID);
                return true;
            }
            return false;
        }

        public static void WaitForClient(string sessionID, NusysClient client)
        {
            if (!_waitingSessionIDs.ContainsKey(sessionID) && !_waitingSessionIDs.ContainsValue(client))
            {
                _waitingSessionIDs.Add(sessionID, client);
            }
            else if (!_waitingSessionIDs.ContainsKey(sessionID) && _waitingSessionIDs.ContainsValue(client))
            {
                var presentID = _waitingSessionIDs.GetKeyByValue(client);
                _waitingSessionIDs.Remove(presentID);
                _waitingSessionIDs.Add(sessionID, client);
            }
        }
        public static bool ClientExists(string sessionId)
        {
            return ActiveClients.Count(kvp => kvp.Value.SessionID == sessionId) > 0;
        }

        public static NuWebSocketHandler GetWebSocket(string sessionId)
        {
            return ActiveClients.FirstOrDefault(kvp => kvp.Value?.SessionID == sessionId).Key;
        }
    }
}
