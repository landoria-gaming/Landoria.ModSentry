using System;
using System.Collections.Generic;
using System.Linq;

namespace Landoria.ModSentry
{
    // Validates a client inventory against the server plugin policy.
    internal static class PolicyValidator
    {
        // Returns the first policy mismatch or an accepted result.
        internal static ValidationResult Validate(PluginPolicy policy,
            IReadOnlyList<PluginDescriptor> actual)
        {
            Dictionary<string, PluginDescriptor> installed = ToDictionary(actual, "client");
            Dictionary<string, PluginDescriptor> required = ToDictionary(policy.Required, "required");
            Dictionary<string, PluginDescriptor> optional = ToDictionary(policy.Optional, "optional");

            foreach (PluginDescriptor expected in policy.Required)
            {
                if (!installed.TryGetValue(expected.Guid, out PluginDescriptor found))
                {
                    return Missing(expected);
                }

                ValidationResult mismatch = Compare(expected, found, false);
                if (mismatch != null)
                {
                    return mismatch;
                }
            }

            foreach (PluginDescriptor found in actual.OrderBy(item => item.Guid, StringComparer.Ordinal))
            {
                if (required.ContainsKey(found.Guid))
                {
                    continue;
                }

                if (!optional.TryGetValue(found.Guid, out PluginDescriptor expected))
                {
                    return Unexpected(found);
                }

                ValidationResult mismatch = Compare(expected, found, true);
                if (mismatch != null)
                {
                    return mismatch;
                }
            }

            return ValidationResult.Accept();
        }

        // Indexes descriptors by GUID and rejects duplicate identities.
        private static Dictionary<string, PluginDescriptor> ToDictionary(
            IEnumerable<PluginDescriptor> plugins, string source)
        {
            try
            {
                return plugins.ToDictionary(item => item.Guid, StringComparer.Ordinal);
            }
            catch (ArgumentException exception)
            {
                ModSentryPlugin.Log.LogError(
                    $"Duplicate plugin GUID in {source} inventory: {exception}");
                throw new InvalidOperationException($"Duplicate plugin GUID in {source} inventory.", exception);
            }
        }

        // Compares the version and hash of an expected plugin.
        private static ValidationResult Compare(PluginDescriptor expected,
            PluginDescriptor actual, bool optional)
        {
            string kind = $"{(optional ? "optional" : "required")} " +
                (expected.IsBepInPlugin ? "plugin" : "library");
            if (!string.Equals(expected.Version, actual.Version, StringComparison.Ordinal))
            {
                return ValidationResult.Reject(
                    optional
                        ? UpdateMessage("Optional mod mismatch", expected)
                        : UpdateMessage("Mod update required", expected),
                    $"{kind} {expected.Guid} version mismatch: expected {expected.Version}, received {actual.Version}.");
            }

            if (!string.Equals(expected.Hash, actual.Hash, StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.Reject(
                    UpdateMessage("Mod mismatch", expected),
                    $"{kind} {expected.Guid} SHA-256 mismatch: expected {expected.Hash}, received {actual.Hash}.");
            }

            return null;
        }

        // Formats a player-facing update instruction.
        private static string UpdateMessage(string reason, PluginDescriptor expected)
        {
            return $"{reason}: {expected.Name} {expected.Version}.";
        }

        // Creates a rejection for a missing required descriptor.
        private static ValidationResult Missing(PluginDescriptor expected)
        {
            string kind = expected.IsBepInPlugin ? "mod" : "library";
            return ValidationResult.Reject(
                UpdateMessage($"Required {kind} missing", expected),
                $"Required {kind} {expected.Guid} {expected.Version} is missing.");
        }

        // Creates a rejection for an unsupported descriptor.
        private static ValidationResult Unexpected(PluginDescriptor actual)
        {
            string kind = actual.IsBepInPlugin ? "mod" : "library";
            return ValidationResult.Reject(
                $"Unsupported {kind} detected: {actual.Name}. Remove it before reconnecting.",
                $"Unexpected {kind} {actual.Guid} {actual.Version} with SHA-256 {actual.Hash}.");
        }
    }
}
