# Sécurité et confidentialité

Cauce n’intègre ni télémétrie, ni envoi automatique d’erreurs, ni cache de pochettes, ni duplication audio. Références et préférences sont stockées dans `%LOCALAPPDATA%\Cauce`, limitées à 10 000 éléments et 16 MiB. Historique et identité du compte restent en mémoire pendant la session.

Une bibliothèque illisible est préservée dans au plus trois fichiers de récupération. Un schéma plus récent n’est jamais écrasé. **Borrar datos locales** supprime bibliothèque et récupérations, sans toucher à la musique.

L’OIDC facultatif utilise navigateur système, PKCE, state et nonce, avec HTTPS et vérification de signature, émetteur, audience et expiration. Les secrets des fournisseurs appartiennent au broker. Cauce ne conserve pas de jetons et ne synchronise pas les données dans le cloud.

Les services ouverts par lien ont leurs propres conditions. Le brouillon de rapport n’ajoute automatiquement ni journaux ni fichiers. Les exportations contiennent chemins et URL : retirez les informations privées avant partage.

Signalez les vulnérabilités selon [SECURITY.md](../../../SECURITY.md). La préversion est non signée ; voir la [politique de signature](code-signing-policy.md).

[Accueil](../../../README.fr.md) · [Guide en anglais](../../security.md)
