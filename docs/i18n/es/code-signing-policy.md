# Política de firma de código

Cauce 0.6.0-alpha.1 está **sin firma**. La aprobación de SignPath Foundation está pendiente; el repositorio tampoco contiene aún una licencia. El titular debe elegir una antes de solicitar la vía de SignPath Foundation. No se atribuirá firma a un paquete que no la tenga.

Cuando exista una aprobación y firma activas, corresponde la atribución:

**Free code signing provided by SignPath.io, certificate by SignPath Foundation.**

Sólo son elegibles artefactos de [ManuelPerilla/cauce](https://github.com/ManuelPerilla/cauce) producidos por sus scripts mantenidos. La identidad de firma no se utiliza para proyectos ajenos ni binarios de origen no verificable.

ManuelPerilla desempeña los roles de committer, reviewer y signing approver. Las contribuciones externas se revisan antes de integrarlas. **Cada solicitud de firma requiere aprobación humana explícita** del responsable. El acceso al repositorio y al servicio de firma exige MFA; claves y credenciales nunca van en código, registros ni artefactos.

El proceso debe verificar origen, metadatos, compilación y firma con sello de tiempo antes de una publicación oficial. El paquete portable no modifica PATH ni requiere privilegios elevados; sus datos locales y eliminación se describen en [instalación](installation.md) y [seguridad](security.md).

Ante un incidente: suspender firmas y publicaciones, identificar versiones, conservar evidencia, investigar y coordinar revocación y aviso de reparación.

[Inicio](../../../README.es.md) · [Guía en inglés](../../code-signing-policy.md)
