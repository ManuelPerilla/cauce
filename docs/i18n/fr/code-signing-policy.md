# Politique de signature de code

Cauce 0.6.0-alpha.1 est **non signé**. L’approbation de SignPath Foundation est en attente ; aucune licence n’est encore présente dans le dépôt. Le propriétaire doit en choisir une avant de demander la voie SignPath Foundation. Un paquet sans signature ne doit jamais être présenté comme signé.

Lorsque l’approbation et la signature sont actives, l’attribution requise est :

**Free code signing provided by SignPath.io, certificate by SignPath Foundation.**

Seuls les artefacts issus de [ManuelPerilla/cauce](https://github.com/ManuelPerilla/cauce) et des scripts maintenus sont admissibles. L’identité de signature ne sert ni aux projets tiers ni aux binaires d’origine invérifiable.

ManuelPerilla assure les rôles de committer, reviewer et signing approver. Les contributions externes sont revues avant intégration. **Chaque demande de signature exige une approbation humaine explicite**. Les accès au dépôt et au service de signature exigent MFA. Clés et identifiants ne figurent jamais dans le code, les journaux ou les artefacts.

Le processus vérifie origine, métadonnées, compilation et signature horodatée avant publication officielle. Le ZIP portable ne modifie pas PATH et n’exige pas de privilèges élevés ; voir [installation](installation.md) et [sécurité](security.md).

En cas d’incident : suspendre signatures et publications, identifier les versions, préserver les preuves, enquêter, coordonner la révocation et publier un avis de correction.

[Accueil](../../../README.fr.md) · [Guide en anglais](../../code-signing-policy.md)
