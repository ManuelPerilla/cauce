# Troubleshooting

## `ngmusic` command नहीं मिलता

MSI install के बाद नई terminal खोलें। Portable build में extracted folder से `ngmusic.exe` चलाएँ।

## OAuth configure नहीं है

`NGMUSIC_GOOGLE_CLIENT_ID` सेट करें।

## Login पूरा नहीं होता

देखें कि browser `127.0.0.1` access कर सकता है और firewall loopback block नहीं कर रहा।

## Search auth/API key मांगता है

`login` चलाएँ या `NGMUSIC_YOUTUBE_API_KEY` सेट करें।

## SmartScreen/Defender warning

Antivirus globally disable न करें। Official Release से download करें, SHA-256 verify करें और signing उपलब्ध होने पर Authenticode signature verify करें।