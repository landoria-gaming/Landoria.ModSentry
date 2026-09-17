namespace Landoria.ModSentry
{
    // Represents the accepted or rejected result of inventory validation.
    internal sealed class ValidationResult
    {
        // Creates a validation result with player and technical messages.
        private ValidationResult(bool accepted, string playerMessage, string technicalMessage)
        {
            Accepted = accepted;
            PlayerMessage = playerMessage;
            TechnicalMessage = technicalMessage;
        }

        internal bool Accepted { get; }
        internal string PlayerMessage { get; }
        internal string TechnicalMessage { get; }

        // Creates an accepted validation result.
        internal static ValidationResult Accept()
        {
            return new ValidationResult(true, string.Empty, "Client plugin inventory accepted.");
        }

        // Creates a rejected validation result.
        internal static ValidationResult Reject(string playerMessage, string technicalMessage)
        {
            return new ValidationResult(false, playerMessage, technicalMessage);
        }
    }
}
