# Cuentas de Cauce

Cauce funciona con la biblioteca local sin cuenta. El cliente incluye una conexión OIDC configurable para Google, Apple, Facebook y Microsoft **a través de un broker de identidad**. No hay un broker desplegado ni credenciales de producción incluidas. Un botón no acredita una integración: hasta instalar una configuración válida, las cuentas deben aparecer como no disponibles.

La cuenta identifica al usuario durante esta ejecución. **No sincroniza música, preferencias ni biblioteca**, no enlaza una suscripción musical y no concede acceso a Spotify, YouTube ni otros catálogos.

## Arquitectura y límites

1. El cliente público de Windows abre el navegador predeterminado. No muestra contraseñas en WebView.
2. Duende IdentityModel OidcClient construye y verifica el flujo Authorization Code + PKCE S256 y `state`. Un nonce nuevo, generado por Microsoft IdentityModel, se incluye en cada solicitud.
3. El broker se encarga del proveedor social y devuelve el código al callback local. Los secretos de Google/Apple/Facebook/Microsoft, si corresponden, se quedan en el broker.
4. Duende canjea el código por HTTPS. Su extensión de validación usa Microsoft IdentityModel para comprobar firma con JWKS, algoritmo asimétrico permitido, issuer, audience, expiración, claims OIDC y nonce. Se exige `azp` si existen varias audiencias. No se acepta un ID token ausente, sin firma o sin `sub`.
5. Sólo se conserva el nombre visible en memoria. Los tokens no se almacenan, renuevan, registran ni se usan para sincronizar; `offline_access` no se solicita. El proceso de inicio de sesión usa temporalmente los tokens necesarios para validarlos. El cierre de sesión elimina las referencias locales y cancela un intento pendiente; no borra cookies del navegador ni cierra la sesión global del proveedor.

El alcance está fijado a `openid profile`. No se solicita `email`, contactos, biblioteca ni permisos de música. `LoadProfile` está deshabilitado: no se hace otra llamada a UserInfo; si el ID token no incluye nombre, la interfaz muestra “Cuenta conectada”. Los datos adicionales que un broker incluya en sus tokens siguen sujetos a su configuración, aunque Cauce no los conserve.

## Configuración del distribuidor

Crear un cliente **nativo/público**, sin secreto, en un broker OIDC compatible con discovery, PKCE S256 y tokens firmados. Habilitar las cuatro conexiones sociales y registrar su retorno HTTPS en cada proveedor. El callback social pertenece al broker; el callback de Cauce pertenece al cliente nativo.

Instalar `auth.json` junto a `Cauce.exe`, bajo la misma política de integridad que el ejecutable. Contiene sólo configuración pública. No pedir a cada usuario que registre aplicaciones ni copie secretos. La aplicación lee el archivo al arrancar; reiniciarla tras cambiarlo. No distribuir el siguiente ejemplo como configuración funcional:

```json
{
  "authority": "https://YOUR-BROKER.example",
  "clientId": "YOUR-PUBLIC-NATIVE-CLIENT-ID",
  "loopbackPort": 43821,
  "timeoutSeconds": 180,
  "providers": {
    "google": { "connection": "YOUR-GOOGLE-CONNECTION" },
    "apple": { "connection": "YOUR-APPLE-CONNECTION" },
    "facebook": { "connection": "YOUR-FACEBOOK-CONNECTION" },
    "microsoft": { "connection": "YOUR-MICROSOFT-CONNECTION" }
  }
}
```

`connection` es un selector de Auth0; sus valores reales los determina el administrador. Para otros brokers se admiten `acr_values` (por ejemplo, un selector de IdentityServer) o `kc_idp_hint` de Keycloak. Estos parámetros **no son intercambiables entre proveedores**: configurar sólo los que entienda el broker. La configuración debe contener las cuatro claves en minúsculas. Otros campos/parámetros se rechazan, incluidos secretos o intentos de modificar `state`, `nonce`, `scope`, PKCE o redirect URI. `IsConfigured` sólo acredita que el archivo es válido; no confirma conectividad, registros externos ni disponibilidad del servicio.

La autoridad debe ser HTTPS y se conserva la validación normal de certificados y discovery. No desactivar validación TLS, de issuer, de endpoints, de firma ni de audiencia para resolver errores de configuración.

El callback es `http://127.0.0.1:43821/callback/` para el puerto del ejemplo. Registrar exactamente ese valor en el cliente nativo del broker. El listener se liga sólo a IPv4 loopback y acepta GET sin cuerpo, con Host esperado y cabeceras limitadas a 16 KiB. Se abre únicamente durante el intento, se cierra al completarlo/cancelarlo y no requiere ASP.NET, URLACL ni ejecutar Cauce como administrador.

Si el broker permite puertos dinámicos para clientes nativos según RFC 8252, usar `loopbackPort: 0`: el sistema elige y reserva el puerto al abrir el socket. Si exige callback exacto, fijar un puerto no reservado; un puerto ocupado produce un error y no debe “solucionarse” escuchando en todas las interfaces. La ventana de autenticación dura entre 30 y 600 segundos (180 por defecto); el cierre de la ventana del navegador no es detectable de forma fiable, por lo que el usuario puede cancelar desde la aplicación o esperar el límite. El listener no hace de servidor general ni recibe callbacks `form_post`: el broker debe usar `response_mode=query` en su flujo code nativo.

## Qué hace falta por proveedor

| Proveedor | Registro que gestiona el distribuidor/broker | Datos que nunca van en Cauce |
| --- | --- | --- |
| Google | Proyecto, consentimiento y cliente web para el callback HTTPS del broker; configurar la conexión. Un cliente desktop directo sería otro diseño y no habilita Apple/Facebook automáticamente. | Secreto del cliente web, si se requiere. |
| Apple | App ID con Sign in with Apple, Services ID asociado, dominio/retorno HTTPS y clave privada para la conexión del broker. Confirmar elegibilidad y acceso al programa de desarrolladores. | Clave privada y JWT usado como client secret; su renovación corresponde al servidor. |
| Facebook | Aplicación Meta con Facebook Login, dominios/retornos autorizados y configuración de la conexión; revisar los requisitos vigentes para usuarios externos/modo público. | App secret. |
| Microsoft | Registro de aplicación, audiencia de cuentas admitidas y retorno del broker; configurar credencial de servidor si la integración elegida es confidencial. | Client secret o certificado privado del broker. |

La marca del botón debe coincidir con la conexión configurada. Tener `auth.json` no crea estos registros ni garantiza aprobación de proveedores. Para Apple y Facebook se usa el broker precisamente para que el cliente de escritorio no pretenda custodiar un secreto confidencial.

## Validación antes de distribuir

Las comprobaciones automatizadas están en `tests/Cauce.Auth.Tests`: ejecutar `dotnet run --project tests/Cauce.Auth.Tests/Cauce.Auth.Tests.csproj -c Release`. Los workflows de Cauce las ejecutan en Windows. Usan claves y tokens sintéticos, un transporte que prohíbe conexiones remotas y sockets loopback; no abren navegador ni requieren credenciales. Cubren configuración, validación de identidad, generación de PKCE/state, rechazo previo al canje y límites/cierre del callback. Si una política de Windows bloquea su ejecución, conservar esa protección y registrar la limitación; compilar no equivale a superar las pruebas.

Pendiente con un broker y registros reales: inicio/cancelación de cada proveedor, usuarios nuevos y recurrentes, cuentas Microsoft personales/organizacionales según alcance, retorno de Apple, expiración de credenciales del broker, rotación JWKS, negativa de consentimiento, conexión interrumpida, puertos ocupados, timeout y cancelación al cerrar la app. Probar que los intentos con `state`, nonce, issuer, audience o firma incorrectos se rechazan. La compilación y las pruebas locales no sustituyen esta validación de extremo a extremo.

No activar telemetría de solicitudes/respuestas de autenticación ni volcar excepciones de proveedor al registro de soporte: pueden contener códigos o datos personales. Si se implementa sesión persistente en el futuro, diseñar primero el almacenamiento protegido de Windows, revocación, caducidad, cierre de sesión y política de datos; no guardar tokens en JSON.

## Dependencias y fuentes oficiales

- [Duende.IdentityModel.OidcClient 7.1.0](https://www.nuget.org/packages/Duende.IdentityModel.OidcClient/7.1.0), Apache-2.0; [documentación y extensión del navegador](https://docs.duendesoftware.com/identitymodel-oidcclient/automatic/). Su política por defecto no exige firma: Cauce sí la exige y proporciona validador.
- [Microsoft.IdentityModel.Protocols.OpenIdConnect 8.23.0](https://www.nuget.org/packages/Microsoft.IdentityModel.Protocols.OpenIdConnect/8.23.0), MIT; [validador oficial OIDC](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/dev/src/Microsoft.IdentityModel.Protocols.OpenIdConnect/OpenIdConnectProtocolValidator.cs).
- [RFC 8252: OAuth para aplicaciones nativas](https://www.rfc-editor.org/rfc/rfc8252), navegador externo, PKCE y loopback.
- [Google: aplicaciones nativas](https://developers.google.com/identity/protocols/oauth2/native-app) y [políticas OAuth](https://developers.google.com/identity/protocols/oauth2/policies).
- [Apple: configurar el entorno](https://developer.apple.com/documentation/signinwithapple/configuring-your-environment-for-sign-in-with-apple) y [otras plataformas](https://developer.apple.com/documentation/signinwithapple/incorporating-sign-in-with-apple-into-other-platforms).
- [Meta: Facebook Login manual](https://developers.facebook.com/docs/facebook-login/guides/advanced/manual-flow/) y [guía Microsoft de registro Facebook](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/facebook-logins).
- [Microsoft: navegador del sistema y clientes de escritorio](https://learn.microsoft.com/en-us/entra/msal/dotnet/acquiring-tokens/using-web-browsers).
- [Auth0: Authorization Code con PKCE](https://auth0.com/docs/get-started/authentication-and-authorization-flow/authorization-code-flow-with-pkce).
