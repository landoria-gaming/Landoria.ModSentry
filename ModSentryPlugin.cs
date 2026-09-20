using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Landoria.ModSentry
{
    // Initializes ModSentry and maintains its connection verification state.
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class ModSentryPlugin : BaseUnityPlugin
    {
        internal const string InventoryRpc = "Landoria_ModSentry_Inventory";
        internal const string RejectionRpc = "Landoria_ModSentry_Rejection";
        internal const string RejectionAckRpc = "Landoria_ModSentry_RejectionAck";
        internal const int ProtocolVersion = 2;
        private const string PluginGuid = "Landoria.ModSentry";
        private const string PluginName = "Landoria.ModSentry";
        private const string PluginVersion = "1.0.19";

        internal static ManualLogSource Log { get; private set; }
        internal static PluginPolicy Policy { get; private set; }

        // Initializes the plugin and its policy directories.

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            Logger.LogInfo($"AssemblyVersion: {GetType().Assembly.GetName().Version}.");
            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();
            PluginPolicyLoader.EnsureDirectories();
            Log.LogInfo($"{PluginName} {PluginVersion} is loaded.");
        }

        // Loads the server policy once and returns the cached result.
        internal static PluginPolicy EnsurePolicy()
        {
            if (Policy == null)
            {
                Policy = PluginPolicyLoader.Load();
                Log.LogInfo($"Loaded {Policy.Required.Count} required and " +
                            $"{Policy.Optional.Count} optional client mod policies.");
            }

            return Policy;
        }

        // Advances verification and disconnect timeouts each frame.
        private void Update()
        {
            NonceHandshake.Tick();
            PendingDisconnects.Tick();
        }

        // Clears all ModSentry state when the plugin unloads.
        private void OnDestroy()
        {
            Log?.LogInfo($"{PluginName} {PluginVersion} is unloaded.");
            NonceHandshake.Clear();
            HandshakeState.Clear();
            PendingDisconnects.Clear();
            ClientMessage.Clear();
            Policy = null;
            _harmony?.UnpatchSelf();
            _harmony = null;
            Log = null;
        }
    }
}
