# Landoria ModSentry

Snapshot builds use LandoriaModActions@v4.5 and the shared private references.
Requires MOD_REFERENCES_TOKEN. All five versions must match; only manifest and informational version use -snapshot.

ModSentry verifies that the mods installed on each client match the mods expected
by the server. This lets every player use the same mods and play under the same
conditions.

## Features

- Checks that players have the required mods and only use approved files and versions.
- Rejects connections with extra, missing, or outdated mods and explains what needs fixing.
- Prevents players from reusing an old mod check response when connecting again. (Nonce-based server challenge.)

> **ModSentry verifies client mod files.** It does not detect cheats or injected code.
> A modified client can falsify its mod inventory.

## Installation

Most of Landoria mods need to be installed also on the dedicated server, we show you in this video how to do it on windows:

[Setup a Valheim Modded dedicated server on Windows](https://youtu.be/X5J1wSRr7Uo)

| Crossplay support | Steam network support |
|---|---
| Yes | No

| Client required | Dedicated server required | Player-hosted server |
|---|---|---|
| Yes | Yes | Not supported |

## Server configuration

Configure the client mod policy with these folders on the server:

| Folder | Purpose |
|---|---|
| `BepInEx/config/ModSentry_Required` | Contains every mod and library DLL required on clients. Include `Landoria.ModSentry.dll` here. |
| `BepInEx/config/ModSentry_Optional` | Contains mods and library DLLs that clients may use but do not need. |

Copy the approved client DLLs into the appropriate folder. ModSentry compares
their plugin identifier, version, and SHA-256 hash with each connecting client.
Any client DLL listed in neither folder is rejected.

If a DLL contains several plugins, ModSentry checks each plugin's identifier and
version against the same approved DLL hash. Copy that DLL only once.

Server-only mods stay in `BepInEx/plugins` and are not copied into either policy
folder.

## Contact

Report bugs through [GitHub Issues](https://github.com/landoria-gaming/Landoria.ModSentry/issues).
For questions and feedback, use [GitHub Discussions](https://github.com/landoria-gaming/Landoria.ModSentry/discussions).
