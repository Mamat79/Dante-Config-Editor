# Dante Config Editor 2027.0.5

[English release notes](RELEASE_NOTES_2027.0.5_EN.md)

Cette version commune Windows et macOS clarifie les projets DCE et StageFlow,
fiabilise le synoptique et recentre la validation sur les problèmes réellement
utiles avant export.

## Nouveautés

- Un projet autonome DCE reste un fichier XML Dante : créez-le de zéro, ouvrez-le,
  modifiez-le puis exportez-le pour Dante Controller sans dépendre de StageFlow.
- Dans un projet `.stageflow`, **Enregistrer** conserve désormais les emplacements,
  l'ordre, la visibilité et les positions manuelles du synoptique dans le domaine
  Dante. Ces données de travail ne sont jamais ajoutées au XML Dante exporté.
- La page **Import / Export** donne un accès direct à l'ouverture d'un XML Dante
  Controller et à l'export vers Dante Controller, en plus du menu Fichier.
- Le centre de validation ne signale plus les RX libres ni les machines légitimes
  sans TX ou sans RX. Les contrôles bloquants de structure, d'identifiants et de
  références restent actifs.
- Le stockage du synoptique utilise une identité portable pour retrouver chaque
  machine après fermeture et réouverture d'un projet StageFlow.
- Les libellés, notices et numéros de version sont alignés entre Windows,
  macOS Apple Silicon et macOS Intel.

## Compatibilité préservée

- Les formats de licence `DCEP1`, `DCEF1` et V2 restent compatibles.
- Les banques personnelles, préférences, projets XML et anciens projets DCE restent
  conservés lors de la mise à niveau.
- Le moteur de modification XML ciblée, la fusion, le patch, le renommage en série,
  la banque de machines et les exports PDF/SVG ne sont pas remplacés.
- StageFlow reste gratuit et facultatif. DCE prépare les configurations hors ligne
  et ne pilote pas directement le réseau ou le matériel Dante.

Les fichiers XML générés par DCE ont été importés avec succès dans Dante Controller
par le mainteneur. Aucun nouveau test sur matériel Dante physique n'est revendiqué
pour cette mise à jour centrée sur le workflow et la persistance du synoptique.
