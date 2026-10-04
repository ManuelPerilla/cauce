# Política de firma de código

La aprobación de SignPath Foundation está pendiente.

Cuando la firma esté activa:

**Free code signing provided by SignPath.io, certificate by SignPath Foundation.**

## Alcance

Solo pueden firmarse artefactos oficiales producidos desde `ManuelPerilla/cauce` mediante el pipeline aprobado.

No se usará la identidad de firma para proyectos ajenos, binarios modificados localmente sin procedencia verificable o software propietario externo.

## Roles actuales

- Committer: [ManuelPerilla](https://github.com/ManuelPerilla)
- Reviewer: [ManuelPerilla](https://github.com/ManuelPerilla)
- Signing approver: [ManuelPerilla](https://github.com/ManuelPerilla)

Los PR externos requieren revisión. Cada solicitud de firma requiere aprobación humana explícita.

Las cuentas con acceso a repositorio o firma deben usar MFA.

Las claves privadas de firma nunca deben aparecer en Git, logs o artefactos.

Las releases firmadas deben provenir del pipeline automatizado, completar el build normal y pasar aprobación manual antes de firmarse.
