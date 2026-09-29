# Configuration और Google OAuth

`login` चलाएँ। Client ID missing होने पर NgMusic setup wizard automatically खोलता है।

Google OAuth **Desktop app** Client ID paste करने के बाद यह यहाँ save होता है:

`%LOCALAPPDATA%\NgMusic\config.json`

OAuth tokens JSON में नहीं, Windows Credential Manager में store होते हैं।

- `setup`: Client ID configure/replace
- `config show`: status
- `config path`: config path
- `config reset`: local config delete
- `logout`: OAuth token delete

Environment variables supported हैं और local config से higher priority रखते हैं।
