using System.Collections.Generic;

namespace Landoria.ModSentry
{
    // Holds the required and optional plugin policies.
    internal sealed class PluginPolicy
    {
        // Creates a policy from required and optional descriptors.
        internal PluginPolicy(IReadOnlyList<PluginDescriptor> required,
            IReadOnlyList<PluginDescriptor> optional)
        {
            Required = required;
            Optional = optional;
        }

        internal IReadOnlyList<PluginDescriptor> Required { get; }
        internal IReadOnlyList<PluginDescriptor> Optional { get; }
    }
}
