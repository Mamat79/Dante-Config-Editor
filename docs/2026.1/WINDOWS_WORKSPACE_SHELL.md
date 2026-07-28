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
| Banque de machines | accès et migration sûre de la banque |
| Import / Export | Labels, Rapports et patchbook, Synoptique |
| Centre de validation | erreurs, avertissements, informations et chemins XML |
| Historique | modifications, comparaison XML, notices et journaux |
| Outils avancés | Atomic Bomb |

La version macOS suit les mêmes sections et les mêmes parcours principaux.

## Comportement adaptatif

- Les réglages de la vue Machines restent affichés au premier lancement.
- Sous `1400` pixels logiques, l'inspecteur se replie pour préserver l'espace
  central.
- Sous `1160` pixels logiques, la navigation peut également se replier.
- Les boutons `Navigation` et `Afficher inspecteur` permettent de rouvrir les
  deux panneaux.
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
- `386/386` tests Core/Windows réussis ;
- contrôle visuel réel de l'exécutable à environ `1266 x 813` et en fenêtre
  maximisée ;
- ouverture de la fixture anonymisée `representative-preset.xml` ;
- vérification des vues Projet, Vue d'ensemble et Machines ;
- vérification en thèmes sombre et clair ;
- vérification en français et en anglais ;
- vérification du repli automatique de l'inspecteur ;
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
