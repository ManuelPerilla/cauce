# Preferencias y cuentas

En **Apariencia**, elige Sistema, Claro, Oscuro, Bosque o Alto contraste. Puedes reducir movimiento y transparencia. En **Escuchar**, selecciona género, separación entre artistas y si permites repeticiones. Cambiar de género inicia una nueva sesión.

Cauce guarda referencias, metadatos y preferencias en `%LOCALAPPDATA%\Cauce`: como máximo 10.000 referencias y 16 MiB. Nunca copia ni modifica el audio. El historial de escucha vive únicamente durante la sesión. **Exportar datos** incluye rutas y URLs; revísalo antes de compartirlo.

Las cuentas son opcionales. El distribuidor puede instalar un `auth.json` público junto a `Cauce.exe` para conectar un broker OIDC confiable por HTTPS con Google, Apple, Facebook y Microsoft. Sin configuración real, el acceso está deshabilitado. El cliente usa el navegador del sistema y Authorization Code con PKCE; no almacena tokens ni secretos. La cuenta no sincroniza biblioteca ni autoriza servicios musicales.

La [guía de cuentas](../../cauce-accounts.md) detalla el registro del broker, los callbacks y las pruebas reales necesarias. El usuario puede escuchar sus archivos sin configurar ningún proveedor.

[Inicio](../../../README.es.md) · [Guía en inglés](../../configuration.md)
