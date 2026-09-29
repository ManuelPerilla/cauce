# Installation

Setup.exe, MSI et ZIP portable sont tous officiellement pris en charge.

Après une installation MSI interactive, le configurateur NgMusic s'ouvre afin d'enregistrer le Client ID Google OAuth Desktop et les raccourcis. L'état final est donc aligné avec Setup.exe.

Un MSI silencieux (/qn) n'ouvre aucune interface, ce qui convient aux déploiements administrés. L'utilisateur peut ensuite exécuter `ngmusic setup`.

Le mode portable ne modifie ni Program Files ni PATH.
