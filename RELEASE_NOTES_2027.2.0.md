# Dante Config Editor v2027

[English release notes](RELEASE_NOTES_EN.md)

## Windows et macOS 2027.2.0

- La matrice montre tout le projet, avec les machines repliées au départ et
  des en-têtes distincts pour les machines et leurs canaux.
- Un glissement propose une diagonale ou une colonne automatiquement. L'aperçu
  orange reste visible même si le pointeur ne suit pas une diagonale parfaite.
  Relâchez pour appliquer, Échap pour annuler ; une action annule tout le geste.
- Les clics conservent la matrice en place et ne redessinent plus le synoptique
  masqué. Les contrôles XML et l'historique transactionnel restent actifs.
- Double-cliquez un label pour le renommer directement ; tirez sa poignée pour
  prolonger une série. Le renommage en série, Flip, la recherche des sources et
  destinations, et le double-clic sur une machine restent disponibles.
- Multicast récapitule les flux de tout le projet : choisissez une machine,
  cochez les TX audio, puis créez, modifiez ou supprimez un flux.
- Sous Windows, le synoptique reste dans Outils. Une roue dentée à côté d'Espace de travail
  permet de choisir les raccourcis de navigation.

Le multicast concerne les flux Dante audio simples des presets 3.0.0. Les
formes inconnues restent intactes et non modifiables. Supprimer tous les flux
d'un preset ne garantit pas leur suppression sur le réseau : contrôlez ce point
dans Dante Controller. DCE reste un éditeur hors ligne.

La même matrice est portée dans l'interface native Avalonia sur Mac, avec ses
gestes automatiques, l'aperçu, le renommage direct, la poignée de série, la
recherche des liaisons, l'ouverture des machines et le gestionnaire multicast.
Les paquets Apple Silicon et Intel sont fabriqués depuis les mêmes sources.
