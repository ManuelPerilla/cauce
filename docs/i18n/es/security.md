# Seguridad y privacidad

Cauce no incluye telemetría, envío automático de errores, caché de portadas ni duplicación de audio. La biblioteca guarda referencias y preferencias bajo `%LOCALAPPDATA%\Cauce`, limitada a 10.000 elementos y 16 MiB. El historial de sesión y la identidad de cuenta permanecen en memoria.

Una biblioteca ilegible se conserva en hasta tres archivos de recuperación. Una versión de esquema más nueva no se sobrescribe. **Borrar datos locales** elimina la biblioteca y sus recuperaciones, no tu música.

El acceso opcional OIDC usa navegador del sistema, PKCE, state y nonce; exige HTTPS y validación de firma, emisor, audiencia y caducidad. Los secretos de proveedor pertenecen al broker. Cauce no guarda tokens ni proporciona sincronización en la nube.

Los enlaces externos abren un servicio que tiene sus propias condiciones. El borrador de errores no adjunta registros ni archivos automáticamente. Las exportaciones contienen rutas y URLs; elimina datos privados antes de compartirlos.

Reporta vulnerabilidades según [SECURITY.md](../../../SECURITY.md). La vista previa actual está sin firma; consulta la [política de firma](code-signing-policy.md).

[Inicio](../../../README.es.md) · [Guía en inglés](../../security.md)
