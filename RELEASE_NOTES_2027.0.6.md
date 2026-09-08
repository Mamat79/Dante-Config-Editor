# Dante Config Editor 2027.0.6

[English release notes](RELEASE_NOTES_2027.0.6_EN.md)

Cette version corrective conserve intégralement les licences, projets, banques
personnelles et formats XML existants.

## Réseau et XML

- Les commandes **Redondant** et **Daisychain** restent disponibles quand une
  machine contient une balise `redundancy`, même sans section IPv4.
- L'onglet IP explique clairement son indisponibilité lorsqu'aucune balise
  `ipv4_address` n'existe. DCE préserve le XML et n'invente pas cette balise.

## Easy Patch

- Sélectionnez un ou plusieurs TX avec Ctrl ou Maj, puis déposez-les sur le
  premier RX à patcher.
- Le premier TX est affecté au RX visé et les suivants aux RX successifs.
- L'application est immédiate, respecte l'avertissement de remplacement choisi
  et conserve strictement une seule source par RX.

## Vérifications

- 651 tests Windows et moteur XML réussis.
- 34 tests macOS/Avalonia réussis.
- 13 tests de version et de packaging macOS réussis.
- 7 tests de notices réussis, avec rendu visuel FR/EN contrôlé.
- 5 tests du Worker de licence réussis.
- Build Windows Release : 0 erreur, 0 avertissement.
- Installation et démarrage réels vérifiés sous Windows.

DCE reste un éditeur hors ligne indépendant d'Audinate. Les XML produits par
cette génération ont déjà été importés avec succès dans Dante Controller par le
mainteneur ; cette corrective n'ajoute aucune balise technique absente.
