namespace Landoria.ModSentry
{
    // Exposes the connection handshake registry for Valheim RPC peers.
    internal static class HandshakeState
    {
        private static readonly HandshakeRegistry<ZRpc> Registry =
            new HandshakeRegistry<ZRpc>();

        // Records an accepted RPC connection.
        internal static void Accept(ZRpc rpc)
        {
            Registry.Accept(rpc);
        }

        // Records a rejected RPC connection.
        internal static void Reject(ZRpc rpc, ValidationResult result)
        {
            Registry.Reject(rpc, result);
        }

        // Reports whether an RPC connection was accepted.
        internal static bool IsAccepted(ZRpc rpc)
        {
            return Registry.IsAccepted(rpc);
        }

        // Returns the rejection recorded for an RPC connection.
        internal static ValidationResult RejectionFor(ZRpc rpc)
        {
            return Registry.RejectionFor(rpc);
        }

        // Removes all state for an RPC connection.
        internal static void Remove(ZRpc rpc)
        {
            Registry.Remove(rpc);
        }

        // Clears every recorded RPC handshake result.
        internal static void Clear()
        {
            Registry.Clear();
        }
    }
}
