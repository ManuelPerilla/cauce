# Préférences et comptes

**Apariencia** propose Sistema, Claro, Oscuro, Bosque et Alto contraste, avec réduction des animations et de la transparence. **Escuchar** permet de choisir le genre, l’espacement des artistes et les répétitions. Changer de genre démarre une nouvelle session.

Cauce conserve références, métadonnées et préférences dans `%LOCALAPPDATA%\Cauce`, avec une limite de 10 000 références et 16 MiB. Les fichiers audio ne sont ni copiés ni modifiés. L’historique n’existe que pendant la session. Une exportation contient des chemins et URL : vérifiez-la avant de la partager.

Les comptes sont facultatifs. Le distributeur peut placer un `auth.json` public à côté de `Cauce.exe` pour un broker OIDC HTTPS de confiance reliant Google, Apple, Facebook et Microsoft. Sans configuration réelle, la connexion est désactivée. Le navigateur système gère Authorization Code avec PKCE ; aucun jeton ni secret n’est conservé. Se connecter ne synchronise pas la bibliothèque et n’autorise aucun service musical.

Le [guide des comptes](../../cauce-accounts.md) décrit les inscriptions, callbacks et vérifications avec les fournisseurs réels. Écouter des fichiers locaux ne demande aucune configuration de compte.

[Accueil](../../../README.fr.md) · [Guide en anglais](../../configuration.md)
