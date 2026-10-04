# Cauce

**Votre musique garde le fil.**

Cauce est un lecteur natif Windows en C#, WPF et .NET 10. La préversion **0.6.0-rc.1 est non signée**, avec des ZIP portables pour x64 et ARM64.

- Lecture locale MP3, WAV et M4A via Windows, selon la validité des fichiers et la prise en charge des codecs.
- Sessions conservant le genre choisi, règles déterministes de répétition et d’espacement des artistes, avec explication lorsque la file s’arrête.
- Bibliothèque de références et métadonnées sans copie ni modification du son, limitée à 10 000 références et 16 MiB.
- Distinction entre fichiers lisibles, absents et liens HTTPS vers des services ; les liens s’ouvrent à l’extérieur du lecteur.
- Mode compact, zone de notification, cinq thèmes, animations et transparence réductibles, tutoriel et rapports d’erreur à relire.

Aucun compte n’est requis pour les fichiers locaux. Le client OIDC facultatif reste désactivé jusqu’à la configuration d’un broker et d’inscriptions réelles Google, Apple, Facebook et Microsoft. Aucun catalogue de streaming ni synchronisation cloud n’est intégré. L’interface actuelle est en espagnol.

Avec Windows et le SDK .NET 10 :

```powershell
dotnet run --project src/Cauce.Desktop
```

[Installation](docs/i18n/fr/installation.md) · [Préférences](docs/i18n/fr/configuration.md) · [Commandes](docs/i18n/fr/commands.md) · [Dépannage](docs/i18n/fr/troubleshooting.md)

[Architecture](docs/i18n/fr/architecture.md) · [Sécurité](docs/i18n/fr/security.md) · [Publications](docs/i18n/fr/releasing.md) · [Signature](docs/i18n/fr/code-signing-policy.md)

[Guide complet](docs/cauce.md) · [Comptes](docs/cauce-accounts.md) · [Code](https://github.com/ManuelPerilla/cauce)

[English](README.md) · [Español](README.es.md) · Français · [हिन्दी](README.hi.md) · [简体中文](README.zh-CN.md)
