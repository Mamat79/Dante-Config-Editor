# Dante Config Editor 2027.1.1

[English release notes](RELEASE_NOTES_2027.1.1_EN.md)

Cette version adapte automatiquement Dante Config Editor aux écrans larges,
compacts ou étroits, sans réduire la taille du texte.

## Interface adaptative

- Le bandeau, les marges et les panneaux se réorganisent selon l'espace
  disponible. Les mots ne sont plus coupés au milieu dans les boutons.
- Machines et Easy Patch empilent leurs zones sur les petits écrans ; le
  défilement reste disponible lorsque la hauteur est limitée.
- La navigation principale est recentrée sur Projet, Vue d'ensemble, Machines,
  Patch DCE, Synoptique et Outils. Les fonctions avancées restent accessibles
  dans Outils.

## Parcours simplifiés

- Nouveau projet DCE devient un assistant en deux étapes : informations du
  projet, puis première machine et configuration.
- Easy Patch distingue clairement Source / destination du mode Hybride
  recommandé. Les sélections et glisser-déposer multiples conservent les règles
  existantes.
- Windows et macOS utilisent les mêmes seuils de disposition et les mêmes
  libellés français/anglais.

## Compatibilité préservée

- Aucun changement des formats XML, DCE ou StageFlow.
- Les domaines StageFlow étrangers, nœuds XML inconnus, banques personnelles,
  préférences et emplacements de stockage sont préservés.
- Les licences DCEP1, DCEF1 et V2 déjà émises restent compatibles.
- DCE reste un éditeur hors ligne indépendant d'Audinate ; il ne pilote pas un
  réseau Dante en temps réel.

Les contrôles Windows, macOS automatisés, installation locale et artefacts sont
consignés dans `VALIDATION_2027.1.1.md`.
