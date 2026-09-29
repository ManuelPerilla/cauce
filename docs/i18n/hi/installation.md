# Installation

NgMusic modern Windows 10 और Windows 11 के लिए है।

| System | Package |
| --- | --- |
| Intel/AMD 64-bit | `NgMusic-<version>-win-x64.msi` |
| Windows on ARM | `NgMusic-<version>-win-arm64.msi` |
| 32-bit Windows | `NgMusic-<version>-win-x86.msi` |

MSI चलाएँ, जरूरत हो तो administrator approval दें, फिर नई terminal खोलकर `ngmusic` चलाएँ।

MSI Program Files में install करता है, upgrade/uninstall metadata register करता है और system `PATH` में NgMusic जोड़ता है। वर्तमान installer Start-menu shortcut नहीं बनाता।

Portable ZIP extract करके `ngmusic.exe` चलाएँ। Portable mode Program Files या `PATH` नहीं बदलता।

Official releases self-contained हैं; runtime के लिए .NET install करने की जरूरत नहीं।