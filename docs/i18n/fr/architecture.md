# Architecture

`Cauce.Core` contient les pistes, l’importation, les règles déterministes de la file et le stockage JSON borné. Il ne dépend ni de WPF, ni du réseau, ni de l’authentification. La file filtre disponibilité et genre, contrôle les répétitions et espace les artistes ; elle explique son arrêt sans assouplir les règles automatiquement.

`Cauce.Desktop` contient l’interface WPF, le modèle de vue, les thèmes, les adaptateurs audio Windows et le client d’identité facultatif. Fichiers locaux et liens externes sont des sources distinctes. L’importation conserve chemins et petites métadonnées, pas le son.

Les surfaces translucides sont statiques. Les animations courtes répondent aux interactions ; la liste est virtualisée et la progression est actualisée chaque seconde pendant la lecture. Il n’y a pas d’animation décorative continue ni de navigateur intégré.

`Cauce.Core.Tests`, `Cauce.Auth.Tests` et `Cauce.Desktop.Smoke` vérifient règles, frontières d’authentification et états de l’interface. Les captures de CI ne remplacent pas des mesures sur des machines et bibliothèques représentatives.

[Accueil](../../../README.fr.md) · [Guide en anglais](../../architecture.md)
