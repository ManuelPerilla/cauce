# NgMusic

**Contrôleur musical Windows orienté terminal, inspiré de PowerShell, utilisant Google OAuth et le lecteur intégré pris en charge par YouTube.**

[English](README.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [Français](README.fr.md)

> **État du projet :** jeune mais utilisable. Les versions publiques sont compilées automatiquement avec GitHub Actions pour Windows x64, ARM64 et x86.

## Qu'est-ce que NgMusic ?

NgMusic est un shell musical personnel pour Windows destiné aux utilisateurs qui préfèrent le clavier et le terminal à une interface multimédia classique. Il propose des commandes inspirées de PowerShell pour la recherche, la lecture, le volume, la navigation temporelle, la file d'attente et l'authentification Google.

La lecture reste dans le lecteur IFrame officiel et visible de YouTube. NgMusic n'extrait pas les flux multimédia bruts, ne télécharge pas les pistes et ne crée pas de couche cachée de récupération audio.

## Fonctionnalités

- OAuth 2.0 Google Desktop avec PKCE et redirection loopback.
- Tokens OAuth stockés dans **Windows Credential Manager**.
- Recherche via YouTube Data API v3.
- Contrôle terminal d'un lecteur YouTube IFrame visible.
- File d'attente et historique.
- MSI et ZIP portable pour x64, ARM64 et x86.
- Releases self-contained, sans installation séparée de .NET.
- Pipeline GitHub Actions reproductible avec SHA-256.

## Télécharger

Utilisez la [dernière Release GitHub](https://github.com/ManuelPerilla/ngmusic/releases/latest).

| Architecture | Installateur | Portable |
| --- | --- | --- |
| x64 | `*-win-x64.msi` | `*-win-x64-portable.zip` |
| ARM64 | `*-win-arm64.msi` | `*-win-arm64-portable.zip` |
| x86 | `*-win-x86.msi` | `*-win-x86-portable.zip` |

Le MSI installe NgMusic dans Program Files et ajoute `ngmusic` au `PATH` système. Une élévation administrateur peut être demandée. La version portable ne modifie ni Program Files ni le `PATH`.

## Documentation

- [Installation](docs/i18n/fr/installation.md)
- [Configuration et OAuth](docs/i18n/fr/configuration.md)
- [Référence des commandes](docs/i18n/fr/commands.md)
- [Architecture](docs/i18n/fr/architecture.md)
- [Sécurité et confidentialité](docs/i18n/fr/security.md)
- [Dépannage](docs/i18n/fr/troubleshooting.md)
- [Builds et releases](docs/i18n/fr/releasing.md)
- [Politique de signature](docs/i18n/fr/code-signing-policy.md)

La documentation anglaise est la source canonique. En cas de divergence, la version anglaise prévaut.

## État de la signature

L'approbation de SignPath Foundation est en attente. Lorsque la signature sera activée :

**Free code signing provided by SignPath.io, certificate by SignPath Foundation.**
