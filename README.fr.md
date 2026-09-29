# NgMusic

**Lecteur musical Windows orienté terminal, inspiré de PowerShell.**

Trois formats sont pris en charge :

- Setup.exe : assistant graphique recommandé.
- MSI : Windows Installer natif avec configurateur après une installation interactive.
- ZIP portable : extraire et exécuter.

Le MSI interactif aboutit au même état fonctionnel que Setup.exe : Program Files, PATH, Client ID OAuth et raccourcis optionnels.

Le MSI silencieux reste silencieux pour les déploiements administrés.

Flux normal : `ngmusic → login → search → play`
