using System.Linq;

namespace Landoria.ModSentry
{
    // Exposes server-owned markers for verified ModSentry connections.
    public static class VerifiedModpackMarker
    {
        private const string Key = "landoria.modsentry_verified_modpack";
        private const string Value = "1";

        // Marks the peer associated with an accepted connection.
        public static void Mark(ZRpc rpc)
        {
            ZNetPeer peer = FindPeer(rpc);
            if (peer != null)
            {
                peer.m_serverSyncedPlayerData[Key] = Value;
            }
        }

        // Reports whether the host belongs to a marked peer.
        public static bool IsMarked(string hostName)
        {
            return ZNet.instance?.GetPeers().Any(peer =>
                peer?.m_socket?.GetHostName() == hostName &&
                peer.m_serverSyncedPlayerData.TryGetValue(Key, out string value) &&
                value == Value) == true;
        }

        // Removes the verified marker from a connection peer.
        public static void Unmark(ZRpc rpc)
        {
            FindPeer(rpc)?.m_serverSyncedPlayerData.Remove(Key);
        }

        // Finds the peer associated with an RPC connection.
        private static ZNetPeer FindPeer(ZRpc rpc)
        {
            return ZNet.instance?.GetPeers()
                .FirstOrDefault(peer => ReferenceEquals(peer.m_rpc, rpc));
        }
    }
}
