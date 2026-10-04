# Arquitectura

`Cauce.Core` contiene las pistas, el importador, las reglas deterministas de cola y el almacenamiento JSON acotado. No depende de WPF, redes ni autenticación. La cola filtra disponibilidad y género, controla repeticiones y separa artistas; informa por qué se detiene en lugar de relajar reglas automáticamente.

`Cauce.Desktop` contiene la interfaz WPF, el modelo de vista, los temas, los adaptadores de audio de Windows y el cliente opcional de identidad. Los archivos locales y enlaces externos son fuentes distintas. Importar música guarda rutas y pequeños metadatos, no archivos de audio.

Las superficies translúcidas son estáticas; las animaciones breves responden a la interacción. La lista está virtualizada y el progreso se actualiza cada segundo durante la reproducción. No hay visualizaciones decorativas continuas ni un navegador embebido.

`Cauce.Core.Tests`, `Cauce.Auth.Tests` y `Cauce.Desktop.Smoke` comprueban reglas, límites de autenticación y estados de interfaz. Las capturas de CI no sustituyen mediciones de consumo con bibliotecas y equipos reales.

[Inicio](../../../README.es.md) · [Guía en inglés](../../architecture.md)
