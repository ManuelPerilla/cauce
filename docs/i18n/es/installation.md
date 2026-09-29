# Instalación

NgMusic ofrece Setup.exe, MSI y ZIP portable.

## Setup.exe

Es la opción recomendada. Configura OAuth antes de instalar y permite crear accesos directos.

## MSI interactivo

Al hacer doble clic:

1. instala NgMusic en Program Files;
2. añade NgMusic al PATH;
3. abre el configurador al terminar;
4. permite guardar el Client ID y crear accesos directos.

El resultado funcional queda alineado con Setup.exe.

## MSI silencioso

Con `/qn` no abre interfaz ni configurador. Esto es intencional para GPO, Intune, SCCM y scripts.

Después puede ejecutarse `ngmusic setup` o usarse configuración por variables de entorno.

## Portable

No instala nada. El primer `login` puede lanzar el asistente de configuración.
