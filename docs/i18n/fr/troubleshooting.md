# Dépannage

## `ngmusic` introuvable

Après installation MSI, ouvrez un nouveau terminal. En portable, exécutez `ngmusic.exe` depuis le dossier extrait.

## OAuth non configuré

Définissez `NGMUSIC_GOOGLE_CLIENT_ID`.

## Login incomplet

Vérifiez l'accès à `127.0.0.1`, le firewall et le type Desktop du client OAuth.

## Recherche exige auth/API key

Exécutez `login` ou configurez `NGMUSIC_YOUTUBE_API_KEY`.

## SmartScreen / Defender

Ne désactivez pas globalement l'antivirus. Téléchargez uniquement depuis les Releases officielles, vérifiez SHA-256 et la signature Authenticode lorsqu'elle est disponible.