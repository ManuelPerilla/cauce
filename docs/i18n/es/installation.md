# Instalación

## Recomendado: Setup.exe gráfico

Descarga el `*-setup.exe` correspondiente a x64, ARM64 o x86.

El asistente:

1. muestra bienvenida;
2. pide el Google OAuth Client ID tipo Desktop app;
3. permite elegir accesos directos;
4. instala NgMusic tras pedir UAC;
5. guarda el Client ID en tu perfil;
6. puede lanzar NgMusic.

No necesitas instalar .NET.

El Client ID queda en `%LOCALAPPDATA%\NgMusic\config.json`. Los tokens OAuth se guardan en Windows Credential Manager.

El MSI directo y el ZIP portable siguen disponibles.

Después del Setup.exe:

```text
ngmusic
login
search "Massive Attack Teardrop"
play 1
```
