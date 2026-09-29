# Troubleshooting

[Documentation home](../README.md) · [Installation](installation.md) · [Configuration](configuration.md)

## `ngmusic` is not recognized

If you installed the MSI, close the current terminal and open a new one so Windows reloads `PATH`.

If you use the portable ZIP, run `ngmusic.exe` from the extracted directory or add that directory to your own user `PATH`.

## OAuth is not configured

Set `NGMUSIC_GOOGLE_CLIENT_ID` to a Google OAuth client of type Desktop app.

See [Configuration](configuration.md).

## Google login opens but does not finish

- Confirm that the browser can reach `127.0.0.1`.
- Make sure security software is not blocking loopback networking.
- Retry `login`; the callback port changes each attempt.
- Confirm the OAuth client is a Desktop application client.

## Search says authentication or API key is required

Run `login` first, or configure `NGMUSIC_YOUTUBE_API_KEY`.

## Player opens but playback does not start

- Keep the player page visible.
- Ensure JavaScript and YouTube are not blocked by the browser or a network filter.
- Some videos may have embedding restrictions; try another search result.
- Browser autoplay restrictions may require one user interaction.

## MSI installation is blocked by SmartScreen or Defender

Early unsigned releases may produce reputation warnings even when built from public source.

Verify the release SHA-256 checksum, inspect the GitHub Actions build provenance, and use only artifacts attached to the official GitHub Release.

When official code signing is active, verify the Authenticode signature before running the installer.

## Portable ZIP is quarantined

Do not disable antivirus globally. Verify that the file came from the official release page and compare its hash with `SHA256SUMS.txt`.

If a clean official build is falsely detected, report the exact artifact and antivirus detection so the maintainer can investigate or submit it as a false positive.
