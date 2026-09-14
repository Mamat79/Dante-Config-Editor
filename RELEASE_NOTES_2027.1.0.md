# Dante Config Editor 2027.1.0

[English release notes](RELEASE_NOTES_2027.1.0_EN.md)

Cette version améliore l'utilisation sur les écrans compacts tout en conservant
les projets, les banques personnelles, les XML et toutes les licences existantes.

## Écrans compacts

- Machines, Patch, Easy Patch et Synoptique se réorganisent selon la largeur
  disponible. Le défilement reste un recours et les mots ne sont plus coupés.
- Les commandes de bas de page reviennent à la ligne lorsque nécessaire et les
  libellés de navigation restent entiers.
- Les boutons, champs, dialogues et états désactivés restent lisibles en thème
  clair comme en thème sombre.

## Nouveau projet et banques

- **Nouveau projet DCE** ouvre directement une configuration vide en mémoire,
  sans imposer un nom, un chemin, un XML ou une première machine.
- Avant de remplacer un travail modifié, DCE propose Enregistrer, Ne pas
  enregistrer ou Annuler.
- Les consoles et boîtiers d'entrées/sorties Yamaha reçoivent des propositions
  `Y001-`, `Y002-`, etc., modifiables et sans collision. Aucun appareil importé
  ni aucune autre marque n'est renommé automatiquement.

## Licence et fabrication macOS

- Le rappel de l'essai de 30 jours reste immédiatement refermable. Après
  expiration, toutes les fonctions sont rendues accessibles après 60 secondes ;
  une activation valide libère immédiatement l'interface.
- DCEP1, DCEF1, les licences V2, les activations existantes et le stockage
  stable restent intégralement compatibles.
- Le workflow Codemagic valide désormais une paire `DMG + SHA-256` déjà publiée
  avant de reconstruire ou republier. Une reprise cohérente réussit sans tenter
  d'écraser un DMG existant ; une paire incomplète est réparée explicitement.

## Vérifications

- 686 tests Windows et moteur partagé réussis.
- 35 tests macOS/Avalonia réussis.
- Builds Release Windows et macOS : 0 erreur, 0 avertissement.
- Les contrôles natifs Windows, l'installation locale et les artefacts macOS
  sont consignés dans le reçu de validation joint à cette release.

DCE reste un éditeur hors ligne indépendant d'Audinate. Cette livraison ne
revendique ni pilotage en temps réel du réseau Dante, ni signature commerciale
Windows, ni notarisation Apple.
