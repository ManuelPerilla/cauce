# Seguridad y privacidad

NgMusic es un cliente de escritorio y no opera un backend propio de telemetría o cuentas.

## Datos

Puede procesar búsquedas, tokens OAuth, datos básicos de perfil de Google y metadatos de videos de YouTube.

No recopila intencionalmente analítica propia, identificadores publicitarios ni telemetría del proyecto.

## Red

Las conexiones externas ocurren por funciones visibles: login con Google, refresh de tokens, búsquedas de YouTube y reproducción.

Los servidores locales de OAuth y player escuchan solo en `127.0.0.1` y puertos dinámicos.

## Tokens

Los tokens OAuth se guardan en Windows Credential Manager bajo `NgMusic.GoogleOAuth`.

## Privacidad

NgMusic no transfiere información a sistemas operados por el proyecto porque no existe backend propio. Los datos se transfieren a Google/YouTube cuando el usuario ejecuta funciones que requieren esos servicios.

No publiques secretos o detalles explotables en issues públicos. Usa un canal privado con el mantenedor para reportes sensibles.