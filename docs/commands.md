# Cauce interface actions

[Documentation](README.md) · [Detailed guide](cauce.md)

Cauce is controlled through its desktop interface. The current preview has no terminal command interface. Labels below match the Spanish UI.

| Section or control | Action |
| --- | --- |
| **Escuchar** | Choose a genre, allow or prevent repeats and set artist spacing from zero to five songs |
| **Reproducir / pausar** | Start the eligible local song, pause it or resume it |
| **Siguiente** | Select the next eligible local file using the current session rules |
| **Vol.** | Adjust playback volume for the running process |
| **Biblioteca → Añadir archivos** | Add MP3, WAV and M4A references without copying audio |
| **Biblioteca → Guardar datos** | Save edited artist and genre in Cauce metadata; the audio file is untouched |
| **Biblioteca → Abrir enlace seleccionado** | Open a saved HTTPS reference in its own service |
| **Biblioteca → Quitar** | Remove the selected reference without deleting its music file |
| **Compacto** | Switch between the full interface and compact transport view |
| **Apariencia** | Choose a theme, reduce motion or transparency, and toggle optional tips |
| **Guía** | Read common questions and repeat the introduction |
| **Cuenta** | Use configured sign-in, sign out, export or reset local data |
| **Soporte** | Prepare a GitHub report draft for review before submitting |

Changing the genre starts a new in-memory session. A queue with no eligible candidates explains the blocking rule; Cauce does not silently relax it or stream a service link. The progress bar displays position; seeking and a previous-track transport control are not implemented in this preview.

## Notification area and shortcut

Minimizing hides the window in the Windows notification area while playback can continue. Double-click the Cauce icon or choose **Abrir Cauce** to restore it. Its menu also offers **Reproducir / pausar**, **Siguiente** and **Salir**.

**Alt+Shift+C** restores the window when Windows permits Cauce to register it. If another application owns the shortcut, use the tray icon instead. Closing the main window or choosing **Salir** exits and attempts to save pending preferences.

## Development entry points

```powershell
./build/dev.ps1
./build/cauce.ps1 -Runtime win-x64
./build/release.ps1 -Version 0.6.0-rc.1 -Architectures x64,arm64
```

These are repository scripts, not music-player commands. Outputs and verification requirements are documented in [building and releasing](releasing.md).

