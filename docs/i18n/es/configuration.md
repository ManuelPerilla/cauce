# Configuración y OAuth de Google

La forma recomendada ya no requiere variables de entorno.

Ejecuta:

```text
PS Music:\> login
```

Si falta el Client ID, NgMusic abre el asistente automáticamente. Pega el OAuth Client ID de una aplicación Google de tipo **Desktop app**.

El Client ID se guarda en:

```text
%LOCALAPPDATA%\NgMusic\config.json
```

El Client ID no es un secreto. Los tokens OAuth se guardan por separado en Windows Credential Manager.

## Comandos

- `setup`: configurar o reemplazar el Client ID local.
- `config show`: mostrar estado y origen.
- `config path`: mostrar ubicación del JSON.
- `config reset`: eliminar configuración local.
- `logout`: eliminar el token OAuth guardado.

Las variables `NGMUSIC_GOOGLE_CLIENT_ID`, `NGMUSIC_GOOGLE_CLIENT_SECRET` y `NGMUSIC_YOUTUBE_API_KEY` siguen soportadas. El Client ID de entorno tiene prioridad sobre el guardado localmente.

NgMusic nunca solicita ni almacena tu contraseña de Google.
