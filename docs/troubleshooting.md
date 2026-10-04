# Troubleshooting Cauce

[Documentation](README.md) · [Installation](installation.md) · [Privacy](security.md)

## The app does not start

Extract the entire portable ZIP and keep its runtime files together. Confirm the package matches your Windows architecture: x64 or ARM64. Source development requires the .NET 10 SDK; a self-contained ZIP does not.

If Windows blocks an unsigned binary, verify its source and checksum. Report the exact Windows message/error code, app version and architecture. Do not disable Windows security controls. A policy-blocked process is not evidence of a completed test run.

## A local file is missing or will not play

Cauce stores paths rather than copying audio. If you moved, renamed or disconnected a file, restore its location or remove the stale reference and import its current path. The preview accepts MP3, WAV and M4A; decoding also depends on Windows media support and the individual codec/file.

Check that Windows can read the file and that it is not empty or damaged. A saved service link opens its service and cannot play inside Cauce. Report a failure with the format and reproduction steps; avoid attaching audio or private paths unnecessarily.

## The session cannot continue

Read the explanation in **Escuchar**:

- No available local files: add music or restore missing paths.
- No files in the chosen genre: set the genre in **Biblioteca** or choose another genre.
- All eligible files already played: allow repeats or change genre to start a new session.
- Artist spacing excludes all candidates: add other artists or reduce that rule.

Cauce keeps the rules you chose. Missing artist metadata cannot establish a reliable gap; edit artist and genre with **Guardar datos**. Edits affect the library only.

## Account buttons are disabled or login fails

Local playback works without an account. Disabled buttons mean no valid broker configuration is loaded. The distributor must supply public `auth.json` beside `Cauce.exe`, register the native client and configure social-provider connections. Restart after changing that file.

An enabled button confirms valid configuration, not live provider availability. Check broker reachability, callback registration, client ID and provider selector. An occupied loopback port, denied consent, timeout or interrupted connection can stop sign-in. Consult [accounts](cauce-accounts.md); never paste client secrets into the desktop or bypass certificate/token validation.

## The library warns about corruption or a newer version

Unreadable metadata is preserved in up to three `library.corrupt-*.json` files under `%LOCALAPPDATA%\Cauce`, with a visible warning. If all three slots are used, the original is preserved rather than overwriting a recovery copy. Keep backups before investigating or resetting.

A newer-schema library is not overwritten. Preserve it and use a compatible build. If saving fails, use **Cuenta → Exportar mis datos** before closing when possible. Reset only when you intend to remove references and preferences; audio stays intact.

## The window disappeared or motion feels uncomfortable

Minimizing moves Cauce to the notification area. Double-click its icon, choose **Abrir Cauce** or press **Alt+Shift+C** if available. Another app may own that shortcut.

Enable reduced motion or opaque surfaces in **Apariencia**, or choose **Alto contraste**. Repeat the introduction from **Guía** to revisit controls.

## Report a reproducible problem

Use **Soporte → Preparar reporte de un problema** or [GitHub Issues](https://github.com/ManuelPerilla/cauce/issues). Review the draft and include version, Windows architecture, steps, expected result and observed result. Omit tokens, account secrets and unreviewed exports. Sensitive reports follow [SECURITY.md](../SECURITY.md).
