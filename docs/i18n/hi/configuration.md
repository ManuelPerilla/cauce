# Configuration और Google OAuth

NgMusic Google OAuth 2.0 Desktop flow और YouTube Data API v3 इस्तेमाल करता है।

| Variable | Purpose |
| --- | --- |
| `NGMUSIC_GOOGLE_CLIENT_ID` | Login के लिए जरूरी |
| `NGMUSIC_GOOGLE_CLIENT_SECRET` | Google दे तो |
| `NGMUSIC_YOUTUBE_API_KEY` | Optional search key |

Login पर NgMusic PKCE बनाता है, `127.0.0.1` पर temporary listener खोलता है, browser में Google authorization खोलता है और callback validate करता है।

Tokens Windows Credential Manager में `NgMusic.GoogleOAuth` नाम से store होते हैं।

NgMusic Google password मांगता या store नहीं करता।

Secrets, API keys, signing certificates या tokens repository में commit न करें।