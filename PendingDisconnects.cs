using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Landoria.ModSentry
{
    // Coordinates graceful and forced disconnection of rejected clients.
    internal static class PendingDisconnects
    {
        private const float FallbackSeconds = 2f;
        private static readonly Dictionary<ZRpc, float> Deadlines =
            new Dictionary<ZRpc, float>();
        private static readonly HashSet<ZRpc> DisconnectRequested =
            new HashSet<ZRpc>();

        // Schedules a rejected connection for disconnection.
        internal static void Schedule(ZRpc rpc)
        {
            Deadlines[rpc] = Time.unscaledTime + FallbackSeconds;
        }

        // Advances disconnection after the client acknowledges rejection.
        internal static void Acknowledge(ZRpc rpc)
        {
            if (Deadlines.ContainsKey(rpc))
            {
                RequestDisconnect(rpc);
            }
        }

        // Removes all pending disconnection state for a connection.
        internal static void Remove(ZRpc rpc)
        {
            Deadlines.Remove(rpc);
            DisconnectRequested.Remove(rpc);
        }

        // Advances connections whose disconnection deadline expired.
        internal static void Tick()
        {
            ZRpc[] expired = Deadlines
                .Where(entry => Time.unscaledTime >= entry.Value)
                .Select(entry => entry.Key)
                .ToArray();
            foreach (ZRpc rpc in expired)
            {
                AdvanceDisconnect(rpc);
            }
        }

        // Clears every pending disconnection.
        internal static void Clear()
        {
            Deadlines.Clear();
            DisconnectRequested.Clear();
        }

        // Requests or forces the next disconnection stage.
        private static void AdvanceDisconnect(ZRpc rpc)
        {
            if (DisconnectRequested.Contains(rpc))
            {
                Remove(rpc);
                ModSentryHandshake.ForceDisconnect(rpc);
                return;
            }

            RequestDisconnect(rpc);
        }

        // Sends the initial disconnection request to a client.
        private static void RequestDisconnect(ZRpc rpc)
        {
            DisconnectRequested.Add(rpc);
            Deadlines[rpc] = Time.unscaledTime + FallbackSeconds;
            ModSentryHandshake.RequestDisconnect(rpc);
        }
    }
}
