# NgMusic

**Contrôleur musical Windows orienté terminal et inspiré de PowerShell.**

[English](README.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [Français](README.fr.md)

## Configuration OAuth simplifiée

Il suffit maintenant d'exécuter :

```text
PS Music:\> login
```

Si aucun Client ID Google OAuth n'est configuré, NgMusic lance automatiquement l'assistant. Collez une fois le Client ID d'une application **Desktop app**.

Le Client ID non secret est enregistré dans :

`%LOCALAPPDATA%\NgMusic\config.json`

Les tokens OAuth restent dans Windows Credential Manager.

Commandes :

`setup`, `config show`, `config path`, `config reset`

Les variables d'environnement restent prises en charge et ont priorité sur la configuration locale.

[Dernière Release](https://github.com/ManuelPerilla/ngmusic/releases/latest)

La documentation anglaise reste canonique.
