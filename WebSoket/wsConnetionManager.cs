using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace userPanelOMR.WebSoket
{
    public class wsConnetionManager
    {
        // har connected client ka WebSocket object is list mein connetion store hoga. abhi List Declear.
        private readonly ConcurrentDictionary<string, WebSocket> _userSockets = new();


        // 1. Naya client connect hota hai, uska WebSocket object is method ke through list mein add kiya. 
        public void AddSocket(string userId, WebSocket socket)
        {
            // Add or update socket for user. <>
            _userSockets.AddOrUpdate(userId, socket, (key, oldSocket) =>
            {
                // Agar purana socket open hai, to usay close kar do
                if (oldSocket.State == WebSocketState.Open)
                {
                    // Purana socket ko close karte hain, normal closure ke saath(koi error dikkat ni hui hai), msg "Replaced by new connection" ke saath, token k sath koi bhi conncection ni kiya gya async way me.
                    oldSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Replaced by new connection", CancellationToken.None).Wait();
                }
                // Naya socket ko add karte hain or return karte hain
                _userSockets[userId] = socket;
                return socket;
            });
        }

        // 2. Kisi user ka WebSocket object ko get karne ke liye, userId pass karte hain.
        // 2. User ka WebSocket object is method ke through get karte hain, jisme userId pass karte hain.
        public WebSocket GetSocketById(string userId)
        {
            // Agar userId ke liye socket exist karta hai, to usay return karte hain
            if (_userSockets.TryGetValue(userId, out var socket))
            {
                return socket;
            }
            // Agar userId ke liye socket exist nahi karta hai, to null return karte hain
            return null;
        }


        // 3. Client disconnect hota hai, to uska WebSocket object is method ke through list se remove kiya.
        public void RemoveSocket(string userId)
        {
            // Agar userId ke liye socket exist karta hai, to usay remove karte hain
            if (_userSockets.TryRemove(userId, out var socket))
            {
                // Agar socket open hai, to usay close karte hain
                if (socket.State == WebSocketState.Open)
                {
                    socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed by server", CancellationToken.None).Wait();
                }
            }
        }

        // 4. WebSocket connection ki total count return karte hain
        public int Count => _userSockets.Count;

        // 5. GetAllSockets method se sabhi connected WebSocket objects ko return karte hain
        public IEnumerable<WebSocket> GetAllSockets() => _userSockets.Values;

    }
}
