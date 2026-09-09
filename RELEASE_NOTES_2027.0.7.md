# Dante Config Editor 2027.0.7

[English release notes](RELEASE_NOTES_2027.0.7_EN.md)

Cette version corrective conserve intégralement les licences existantes,
les projets, les banques personnelles et la fidélité du XML Dante.

## Export depuis StageFlow

- **Exporter le XML Dante** fonctionne maintenant depuis un projet `.stageflow`.
- Le chemin interne du paquet DCE n'est plus transmis à la fenêtre Windows
  **Enregistrer sous**.
- Le nom et le dossier proposés sont toujours des chemins physiques valides,
  avec un repli sûr si Windows refuse le dossier initial.

## Easy Patch

- Sélectionnez un ou plusieurs TX avec Ctrl ou Maj, puis déposez-les sur le
  premier RX : les TX sont affectés aux RX successifs.
- L'opération inverse est également disponible : sélectionnez un ou plusieurs
  RX, puis déposez-les sur le premier TX. Les RX reçoivent ce TX puis les TX
  successifs.
- Chaque RX conserve exactement une source. La série s'arrête proprement à la
  fin de la machine et DCE indique les canaux restés sans affectation.

## Validation

- Une référence vers une machine ou un canal TX absent d'un preset partiel est
  maintenant une **information**, pas un avertissement à acquitter.
- Cette référence est conservée dans le XML afin de préserver les abonnements
  prévus pour des appareils absents du preset courant.

## Vérifications

- 660 tests Windows et moteur XML réussis, dont 3 nouveaux cas de noms de fichiers portables Windows/macOS.
- 34 tests macOS/Avalonia réussis.
- 13 tests de version et de packaging macOS réussis.
- 5 tests du Worker de licence réussis.
- Build Windows et macOS Release : 0 erreur, 0 avertissement.
- Installation et démarrage réels vérifiés sous Windows.
- Cycle XML sans modification validé octet par octet et scénarios de renommage,
  patch, réseau et horloge validés automatiquement.

DCE reste un éditeur hors ligne indépendant d'Audinate. Le moteur conserve les
nœuds et références inconnus ; cette corrective n'invente aucune capacité
technique absente du preset.
