using System;
using HarmonyLib;
using BepInEx.Logging;

namespace Landoria.ModSentry
{
    // Registers the ModSentry handshake for each new connection.
    [HarmonyPatch(typeof(ZNet), "OnNewConnection")]
    internal static class RegisterHandshakePatch
    {
        // Registers handshake RPCs after Valheim creates a peer.
        private static void Postfix(ZNet __instance, ZNetPeer peer)
        {
            ModSentryHandshake.Register(__instance, peer);
        }
    }

    // Delays client peer information until the inventory challenge starts.
    [HarmonyPatch(typeof(ZNet), "SendPeerInfo")]
    [HarmonyBefore("Landoria.CharacterVault")]
    internal static class SendInventoryPatch
    {
        // Allows peer information only after the client begins verification.
        private static bool Prefix(ZNet __instance, ZRpc rpc, string __1)
        {
            return __instance.IsServer() || NonceHandshake.AllowPeerInfo(__instance, rpc, __1);
        }

    }

    // Prevents unverified clients from completing server admission.
    [HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
    [HarmonyBefore("Landoria.CharacterVault")]
    internal static class ValidatePeerPatch
    {
        // Allows peer admission only when ModSentry accepted the inventory.
        private static bool Prefix(ZRpc rpc)
        {
            return ZNet.instance == null || !ZNet.instance.IsServer() ||
                   ModSentryHandshake.Admit(rpc);
        }
    }

    // Restores the verified marker after synced player data is replaced.
    [HarmonyPatch(typeof(ZNet), "RPC_ServerSyncedPlayerData")]
    internal static class RestoreServerAdmissionMarkersPatch
    {
        // Synchronizes the marker with the current handshake state.
        private static void Postfix(ZRpc rpc)
        {
            SetVerifiedMarker(rpc, HandshakeState.IsAccepted(rpc));
        }

        // Adds or removes the verified marker for a connection.
        private static void SetVerifiedMarker(ZRpc rpc, bool marked)
        {
            if (marked)
            {
                VerifiedModpackMarker.Mark(rpc);
                return;
            }
            VerifiedModpackMarker.Unmark(rpc);
        }
    }

    // Clears all verification state when a peer disconnects.
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Disconnect))]
    internal static class ClearHandshakePatch
    {
        // Removes the disconnected peer from every ModSentry registry.
        private static void Prefix(ZNetPeer peer)
        {
            if (peer?.m_rpc != null)
            {
                NonceHandshake.Remove(peer.m_rpc);
                HandshakeState.Remove(peer.m_rpc);
                VerifiedModpackMarker.Unmark(peer.m_rpc);
                PendingDisconnects.Remove(peer.m_rpc);
            }
        }
    }


}
