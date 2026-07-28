# Espace de travail Windows 2026.1

## Objectif

Le shell 2026.1 organise l'application Windows par intention sans modifier le
moteur XML ni reconstruire le document Dante.

## Organisation

La fenêtre principale comporte quatre zones :

1. une barre supérieure pour les commandes fréquentes, la recherche, la langue,
   le thème et le résumé de validation ;
2. une navigation latérale repliable ;
3. un espace de travail central ;
4. un inspecteur contextuel repliable.

La barre d'état conserve le message d'opération, le mode d'édition et les
compteurs TX/RX. Les commandes moins fréquentes restent dans leur espace de
travail afin de ne pas surcharger la barre supérieure.

## Sections

| Navigation 2026.1 | Vue actuellement utilisée |
|---|---|
| Projet | accueil, fichiers récents et récupération |
| Vue d'ensemble | indicateurs du projet et dernières modifications |
| Machines | listes rapides, actions globales, machine, canaux et tableau |
| Patch | espace unifié Matrice, Easy patch et Liste RX vers TX |
| Import / Export | Labels, Rapports et patchbook, Synoptique |
| Centre de validation | erreurs, avertissements, informations et chemins XML |
| Historique | modifications, comparaison XML, notices et journaux |
| Outils avancés | Atomic Bomb |

La banque de machines est accessible directement en tête de la page Machines :
la fenêtre regroupe par défaut la banque personnelle et les banques fournies
dans une liste dédupliquée. Un sélecteur permet d'isoler une banque et la
colonne Banque indique l'origine de chaque modèle. La version macOS suit le
même parcours.

L'inspecteur de droite suit une sélection unique partagée. Choisir une machine
dans Machines, un RX ou un TX dans Patch, ou un appareil dans Easy Patch met à
jour l'inspecteur. Cette dernière machine reste sélectionnée au retour dans la
page Machines, y compris après un renommage.

## Comportement adaptatif

- Navigation, réglages Machines et inspecteur sont ouverts à chaque lancement.
- Chaque zone dispose d'une flèche toujours visible pour la masquer ou la
  rouvrir. Aucun repli n'est déclenché automatiquement par la largeur.
- Les trois colonnes Machines répartissent davantage d'espace aux actions
  globales afin d'éviter les textes coupés.

Ces seuils répondent à la largeur logique disponible dans WPF. Ils ne
constituent pas, à eux seuls, une validation des échelles Windows 125 %, 150 %
et 200 %.

## Raccourcis disponibles

- `Ctrl+S` : enregistrer selon la politique de sauvegarde sûre ;
- `Ctrl+Maj+S` : enregistrer sous ;
- `Ctrl+F` : ouvrir la navigation si nécessaire et placer le focus dans la
  recherche ;
- `Ctrl+Z` : annuler lorsque le contexte actif l'autorise.
- `Ctrl+Y` ou `Ctrl+Maj+Z` : rétablir une commande 2026.1 annulée.

## Vérifications réalisées

Le 28 juillet 2026 :

- compilation Windows Release sans avertissement ;
- `397/397` tests Core/Windows réussis ;
- contrôle visuel réel de l'exécutable à environ `1266 x 813` et en fenêtre
  maximisée ;
- ouverture de la fixture anonymisée `representative-preset.xml` ;
- vérification des vues Projet, Vue d'ensemble et Machines ;
- vérification en thèmes sombre et clair ;
- vérification en français et en anglais ;
- vérification des flèches persistantes, panneaux ouverts et repli manuel ;
- vérification des trois modes de l'espace Patch ;
- vérification d'un clic direct, puis Annuler et Rétablir ;
- vérification d'une ligne RX visible à faible hauteur avec la bannière de
  soutien affichée ;
- `22/22` tests macOS sans écran réussis et compilation macOS sans
  avertissement.

Non vérifié dans ce lot :

- échelles système Windows exactes 125 %, 150 % et 200 % ;
- contraste élevé ;
- Narrator, NVDA et VoiceOver ;
- rendu 2026.1 sur un Mac physique.
