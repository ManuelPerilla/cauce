# Configuration et Google OAuth

NgMusic utilise OAuth 2.0 pour application Desktop et YouTube Data API v3.

| Variable | Usage |
| --- | --- |
| `NGMUSIC_GOOGLE_CLIENT_ID` | Requise pour login |
| `NGMUSIC_GOOGLE_CLIENT_SECRET` | Si Google en fournit un |
| `NGMUSIC_YOUTUBE_API_KEY` | Facultative pour recherche sans OAuth |

Lors du login, NgMusic génère PKCE, ouvre un listener temporaire sur `127.0.0.1`, lance le navigateur système, valide le callback et échange le code OAuth.

Les tokens sont stockés dans Windows Credential Manager sous `NgMusic.GoogleOAuth`.

NgMusic ne demande ni ne stocke le mot de passe Google.

Ne commitez jamais secrets OAuth, API keys, certificats de signature ou tokens.