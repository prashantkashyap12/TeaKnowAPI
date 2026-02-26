using System.Net.WebSockets;
using System.Text;

namespace userPanelOMR.WebSoket
{
    public class wsHandler
    {

        // Method to handle incoming WebSocket connections
        private readonly wsConnetionManager _connectionManager;
        public wsHandler(wsConnetionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        //  Method to add a new WebSocket connection
        public async Task BroadcastMessageAsync(string message)
        {
            var sockets = _connectionManager.GetAllSockets();
            var bytes = Encoding.UTF8.GetBytes(message);
            foreach (var socket in sockets)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        // Method to handle user-specific messages
        public async Task UserMessageAsync(string userId, string message)
        {
            var socket = _connectionManager.GetSocketById(userId);

            if (socket != null && socket.State == WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                try
                {
                    // Send the message to the specific user
                    await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch
                {
                    _connectionManager.RemoveSocket(userId);
                }
            }
        }




    }
}
