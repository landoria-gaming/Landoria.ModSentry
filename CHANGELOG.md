# Changelog

## 1.0.19

- Automatically create the ModSentry_Required and ModSentry_Optional folders at startup.

## 1.0.18

- Migration to Valheim 1.0.x.

## 1.0.14

- Detect known cheat tools, with server-configurable kick or ban (kick by default).
- Prevent reuse of an old mod check response when connecting again.

## 1.0.13

- Updated documentation

## 1.0.12

- Updated README.

## 1.0.11

- Documentation

## 1.0.10

- Kick rejected pre-spawn clients and preserve the custom rejection message without forcing a local logout.

## 1.0.9

- Remember whether a connection sent an approved plugin list.

## 1.0.8

- Accept non-BepInEx DLLs when explicitly listed in optional or required policy entries.

## 1.0.7

- Emphasize identical mod setups and playing conditions for every player.

## 1.0.6

- Add unit tests.

## 1.0.5

- Disconnect rejected pre-admission connections directly so clients return to the main menu instead of remaining on a black screen.

## 1.0.4

- Return rejected clients to the main menu and display the server's rejection reason.

## 1.0.3

- Document Valheim version compatibility

## 1.0.2

- Update thunderstore readme

## 1.0.1

- Shorten ModSentry rejection messages.
- Deliver ModSentry rejection details reliably.

## 1.0.0

- Initial version.
- Read plugin metadata from the plugin class when validating server policy files.
- Confirm delivery of rejection details before disconnecting incompatible clients.
- Explain which mod failed validation and which version the server expects.
