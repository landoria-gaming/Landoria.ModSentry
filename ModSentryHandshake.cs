using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;

namespace Landoria.ModSentry
{
    // Coordinates inventory verification and connection admission.
    internal static class ModSentryHandshake
    {
        // Registers the client or server RPC handlers for a peer.
        internal static void Register(ZNet network, ZNetPeer peer)
        {
            NonceHandshake.Register(network, peer.m_rpc);
            if (network.IsServer())
            {
                peer.m_rpc.Register<ZPackage>(ModSentryPlugin.InventoryRpc, ReceiveInventory);
                peer.m_rpc.Register(ModSentryPlugin.RejectionAckRpc, ReceiveRejectionAck);
            }
            else
            {
                ClientMessage.Clear();
                peer.m_rpc.Register<string>(ModSentryPlugin.RejectionRpc, ClientMessage.Receive);
            }
        }

        // Validates an inventory received from a challenged client.
        internal static void ReceiveInventory(ZRpc rpc, ZPackage package)
        {
            if (NonceHandshake.IsFinal(rpc))
            {
                return;
            }

            try
            {
                if (!NonceHandshake.Consume(rpc, package))
                {
                    return;
                }

                IReadOnlyList<PluginDescriptor> inventory = PluginInventory.Deserialize(package);
                ValidationResult result = PolicyValidator.Validate(
                    ModSentryPlugin.EnsurePolicy(), inventory);
                Record(rpc, result);
            }
            catch (Exception exception)
            {
                ValidationResult result = ValidationResult.Reject(
                    "The installed mods could not be verified.",
                    $"Client inventory parsing failed: {exception}");
                Record(rpc, result);
            }
        }

        // Allows only accepted clients to continue joining the server.
        internal static bool Admit(ZRpc rpc)
        {
            if (HandshakeState.IsAccepted(rpc))
            {
                return true;
            }

            ValidationResult rejection = HandshakeState.RejectionFor(rpc);
            rejection = rejection ?? ValidationResult.Reject(
                "Mod verification did not complete. Please try again.",
                "PeerInfo arrived before an accepted ModSentry inventory.");
            rpc.Invoke(ModSentryPlugin.RejectionRpc, rejection.PlayerMessage);
            ModSentryPlugin.Log.LogWarning(rejection.TechnicalMessage);
            PendingDisconnects.Schedule(rpc);
            return false;
        }

        // Asks a rejected client to close its connection.
        internal static void RequestDisconnect(ZRpc rpc)
        {
            ModSentryPlugin.Log.LogDebug(
                "Requesting rejected pre-spawn client disconnection.");
            rpc?.Invoke("Disconnect");
        }

        // Closes a rejected connection that remained open.
        internal static void ForceDisconnect(ZRpc rpc)
        {
            ZNetPeer peer = ZNet.instance?.GetPeers()
                .FirstOrDefault(candidate => ReferenceEquals(candidate.m_rpc, rpc));
            if (peer != null)
            {
                ModSentryPlugin.Log.LogWarning(
                    "Rejected client did not disconnect; closing the server connection.");
                ZNet.instance.Disconnect(peer);
            }
        }

        // Returns a readable description of a peer.
        internal static string Describe(ZNetPeer peer)
        {
            return string.IsNullOrWhiteSpace(peer?.m_playerName)
                ? "with an unavailable player name" : $"'{peer.m_playerName}'";
        }

        // Records that the client received its rejection message.
        private static void ReceiveRejectionAck(ZRpc rpc)
        {
            PendingDisconnects.Acknowledge(rpc);
        }

        // Applies and communicates a completed validation result.
        internal static void Record(ZRpc rpc, ValidationResult result)
        {
            if (result.Accepted)
            {
                HandshakeState.Accept(rpc);
                VerifiedModpackMarker.Mark(rpc);
                ModSentryPlugin.Log.LogInfo(result.TechnicalMessage);
                return;
            }

            HandshakeState.Reject(rpc, result);
            rpc.Invoke(ModSentryPlugin.RejectionRpc, result.PlayerMessage);
            PendingDisconnects.Schedule(rpc);
            ModSentryPlugin.Log.LogWarning(result.TechnicalMessage);
        }
    }
}
