# Installation

NgMusic cible Windows 10 et Windows 11 modernes.

| Système | Package |
| --- | --- |
| Intel/AMD 64 bits | `NgMusic-<version>-win-x64.msi` |
| Windows on ARM | `NgMusic-<version>-win-arm64.msi` |
| Windows 32 bits | `NgMusic-<version>-win-x86.msi` |

Téléchargez le MSI correspondant, vérifiez éventuellement le SHA-256, exécutez l'installateur et acceptez l'élévation administrateur si nécessaire. Ouvrez ensuite un nouveau terminal et lancez `ngmusic`.

Le MSI installe sous Program Files, enregistre les informations d'upgrade/désinstallation et ajoute NgMusic au `PATH` système. La version actuelle ne crée pas de raccourci dans le menu Démarrer.

Pour la version portable, extrayez le ZIP et lancez `ngmusic.exe`.

Les releases sont self-contained : .NET n'a pas besoin d'être installé séparément.