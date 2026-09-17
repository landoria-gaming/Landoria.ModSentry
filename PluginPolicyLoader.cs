using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using Mono.Cecil;

namespace Landoria.ModSentry
{
    // Loads every plugin in approved DLLs, including plugins merged with ILRepack.
    internal static class PluginPolicyLoader
    {
        private const string RequiredDirectoryName = "ModSentry_Required";
        private const string OptionalDirectoryName = "ModSentry_Optional";

        // Creates the required and optional policy directories when absent.
        internal static void EnsureDirectories()
        {
            Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, RequiredDirectoryName));
            Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, OptionalDirectoryName));
        }

        // Loads the complete required and optional plugin policy.
        internal static PluginPolicy Load()
        {
            return new PluginPolicy(
                LoadDirectory(Path.Combine(Paths.ConfigPath, RequiredDirectoryName)),
                LoadDirectory(Path.Combine(Paths.ConfigPath, OptionalDirectoryName)));
        }

        // Loads every top-level DLL descriptor from a policy directory.
        private static IReadOnlyList<PluginDescriptor> LoadDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException(
                    $"ModSentry policy directory is missing: {directory}");
            }

            return Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly)
                .SelectMany(ReadDescriptors)
                .OrderBy(plugin => plugin.Guid, StringComparer.Ordinal)
                .ToList();
        }

        // Reads plugin descriptors while preserving identities within merged DLLs.
        internal static IReadOnlyList<PluginDescriptor> ReadDescriptors(string path)
        {
            try
            {
                using (AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(path))
                {
                    CustomAttribute[] attributes = assembly.MainModule.GetTypes()
                        .SelectMany(type => type.CustomAttributes)
                        .Where(item =>
                            item.AttributeType.FullName == typeof(BepInPlugin).FullName)
                        .ToArray();

                    if (attributes.Length == 0)
                    {
                        return new[] { CreateFallbackDescriptor(assembly, path) };
                    }

                    string hash = PluginInventory.Sha256(path);
                    return attributes.Select(attribute => CreateDescriptor(hash, attribute))
                        .ToArray();
                }
            }
            catch (BadImageFormatException exception)
            {
                ModSentryPlugin.Log.LogDebug(
                    $"Using a fallback descriptor for {path}: {exception}");
                return new[] { CreateFallbackDescriptor(null, path) };
            }
        }

        // Creates a descriptor for a DLL without BepInPlugin metadata.
        private static PluginDescriptor CreateFallbackDescriptor(AssemblyDefinition assembly, string path)
        {
            string name = assembly?.Name?.Name ?? Path.GetFileNameWithoutExtension(path);
            string guid = $"Landoria.NonBepInPlugin.{name}";
            string version = assembly?.Name?.Version?.ToString() ?? "0.0.0";

            return new PluginDescriptor(guid, name, version, PluginInventory.Sha256(path), false);
        }

        // Creates a descriptor from BepInPlugin metadata.
        private static PluginDescriptor CreateDescriptor(
            string hash, CustomAttribute attribute)
        {
            if (attribute.ConstructorArguments.Count < 3)
            {
                throw new InvalidDataException("Invalid BepInPlugin metadata.");
            }

            string guid = (string)attribute.ConstructorArguments[0].Value;
            string name = (string)attribute.ConstructorArguments[1].Value;
            string version = (string)attribute.ConstructorArguments[2].Value;
            return new PluginDescriptor(guid, name, version, hash);
        }
    }
}
