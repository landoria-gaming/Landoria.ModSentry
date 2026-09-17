using BepInEx.Logging;

namespace Landoria.ModSentry
{
    // Manages connection rejection messages displayed to the client.
    internal static class ClientMessage
    {
        // Displays a server rejection and acknowledges its receipt.
        internal static void Receive(ZRpc rpc, string message)
        {
            ConnectionFailureMessages.Push("Landoria.ModSentry", message);
            ModSentryPlugin.Log.LogWarning($"Server rejected the connection: {message}");
            rpc.Invoke(ModSentryPlugin.RejectionAckRpc);
            ModSentryPlugin.Log.LogDebug(
                "Acknowledged the rejection; waiting for the server disconnect.");
        }

        // Removes any pending ModSentry connection message.
        internal static void Clear()
        {
            ConnectionFailureMessages.Clear("Landoria.ModSentry");
        }
    }
}
