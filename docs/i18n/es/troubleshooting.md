# Solución de problemas

## `ngmusic` no se reconoce

Tras instalar el MSI, cierra y abre una terminal nueva. En portable ejecuta `ngmusic.exe` desde la carpeta extraída.

## OAuth no configurado

Define `NGMUSIC_GOOGLE_CLIENT_ID` con un cliente OAuth Desktop.

## Login no termina

Comprueba que el navegador pueda acceder a `127.0.0.1`, que el firewall no bloquee loopback y que el cliente OAuth sea Desktop.

## Search pide autenticación o API key

Ejecuta `login` o configura `NGMUSIC_YOUTUBE_API_KEY`.

## El player abre pero no reproduce

Mantén la página visible, verifica que JavaScript/YouTube no estén bloqueados y prueba otro resultado si el video no permite embedding.

## SmartScreen o Defender avisan

No desactives el antivirus globalmente. Descarga solo desde Releases oficiales, compara SHA-256 y verifica firma Authenticode cuando esté disponible.