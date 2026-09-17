using System.Collections.Generic;

namespace Landoria.ModSentry
{
    // Tracks accepted and rejected handshake results by peer.
    internal sealed class HandshakeRegistry<TPeer>
    {
        private readonly HashSet<TPeer> _accepted = new HashSet<TPeer>();
        private readonly Dictionary<TPeer, ValidationResult> _rejected =
            new Dictionary<TPeer, ValidationResult>();

        // Records a peer as accepted and clears any rejection.
        internal void Accept(TPeer peer)
        {
            _rejected.Remove(peer);
            _accepted.Add(peer);
        }

        // Records a peer as rejected and clears any acceptance.
        internal void Reject(TPeer peer, ValidationResult result)
        {
            _accepted.Remove(peer);
            _rejected[peer] = result;
        }

        // Reports whether a peer has been accepted.
        internal bool IsAccepted(TPeer peer)
        {
            return _accepted.Contains(peer);
        }

        // Returns the rejection recorded for a peer, if any.
        internal ValidationResult RejectionFor(TPeer peer)
        {
            return _rejected.TryGetValue(peer, out ValidationResult result) ? result : null;
        }

        // Removes all state associated with a peer.
        internal void Remove(TPeer peer)
        {
            _accepted.Remove(peer);
            _rejected.Remove(peer);
        }

        // Removes all recorded handshake results.
        internal void Clear()
        {
            _accepted.Clear();
            _rejected.Clear();
        }
    }
}
