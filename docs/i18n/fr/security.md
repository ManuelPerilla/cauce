# Sécurité et confidentialité

NgMusic est un client desktop sans backend de télémétrie ou de comptes exploité par le projet.

Il peut traiter des requêtes de recherche, tokens OAuth, informations de profil Google de base et métadonnées YouTube.

Le projet ne collecte pas intentionnellement d'analytics propriétaires, d'identifiants publicitaires ou de télémétrie.

Les requêtes réseau correspondent à des fonctions visibles : login Google, refresh OAuth, recherche YouTube et lecture.

Les services locaux écoutent uniquement sur `127.0.0.1`.

Les tokens OAuth sont stockés dans Windows Credential Manager sous `NgMusic.GoogleOAuth`.

Ne publiez pas de secrets ou détails exploitables dans une issue publique.