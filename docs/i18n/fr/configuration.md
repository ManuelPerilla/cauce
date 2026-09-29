# Configuration et Google OAuth

Exécutez `login`. Si le Client ID manque, NgMusic lance automatiquement l'assistant interactif.

Collez le Client ID Google OAuth de type **Desktop app**. Il est enregistré dans :

`%LOCALAPPDATA%\NgMusic\config.json`

Les tokens OAuth ne sont pas stockés dans ce JSON mais dans Windows Credential Manager.

- `setup` : configurer/remplacer le Client ID
- `config show` : voir l'état
- `config path` : chemin du fichier
- `config reset` : supprimer la configuration locale
- `logout` : supprimer le token OAuth

Les variables d'environnement sont toujours prises en charge et prioritaires.
