# Accessibilité et affichage

## État du shell Windows 2026.1

- Le shell expose une navigation latérale, un espace de travail et un
  inspecteur contextuel repliables.
- Les réglages Machines restent affichés au premier lancement.
- Sous `1400` pixels logiques, seul l'inspecteur secondaire est replié
  automatiquement ; sous `1160`, la navigation peut aussi être repliée.
- Les commandes principales ont été contrôlées visuellement à environ
  `1266 x 813`, en thèmes sombre et clair, en français et en anglais.
- Les vues Projet, Vue d'ensemble et Machines ont été vérifiées avec une
  fixture XML anonymisée.
- Les listes nouvelles utilisent des couleurs explicites pour éviter un fond
  blanc avec texte clair en thème sombre.
- `338/338` tests Windows et `20/20` tests macOS sans écran passent.

Ce contrôle ne remplace pas un essai matériel aux échelles Windows 125 %,
150 % et 200 %, en contraste élevé ou avec un lecteur d'écran. Aucun Mac
physique n'a été utilisé pour ce lot.

## État spécifique V3.6

- Les 16 tests Avalonia/macOS sans écran passent, y compris les commandes de
  banque, duplication, ajout de modèle, nouveau projet et l'ordre initial de la
  barre d'outils.
- Les contrats Windows vérifient la présence des libellés, infobulles,
  dimensions minimales et panneaux défilables des nouveaux dialogues.
- Le lancement de l'exécutable autonome et installé a été vérifié avec un
  processus répondant et le titre V3.6.
- La prise de contrôle visuelle automatisée Windows n'a pas été autorisée dans
  le délai du contrôle final. Les nouveaux dialogues V3.6 ne sont donc pas
  déclarés validés visuellement à 125 %, 150 % ou 200 %.
- Aucun contrôle réel n'a été effectué sur un Mac, avec VoiceOver, Narrator ou
  NVDA.

## Contrôles réellement effectués

### Windows

- ouverture et navigation de l'application WPF avec les contrôles d'automatisation Windows ;
- fenêtre compacte observée à environ `1266 x 813` : repli automatique des réglages et tableau machines accessible ;
- fenêtre maximisée observée à environ `1920 x 1032` : panneaux et tableau accessibles sans recouvrement incohérent ;
- onglet `Easy patch` ouvert et inspecté visuellement en thème sombre ; RX à gauche, TX à droite et onglet actif lisible ;
- composant `Easy patch` rendu avec prévisualisation à `1600 x 820` en thèmes sombre et clair, puis à `1200 x 650` en thème sombre ;
- menus RX/TX maintenus hors de tout ascenseur de page ; à faible hauteur, seul le panneau central utilise un ascenseur interne ;
- tableau de prévisualisation redimensionné avec quatre colonnes lisibles et sans défilement horizontal de page ;
- sélections et commandes actionnées via l'arbre d'accessibilité ;
- libellés d'automatisation et info-bulles présents sur les menus et flèches précédent/suivant RX/TX ;
- noms d'automatisation ajoutés aux commandes d'application directe, d'ajout au lot et d'application après prévisualisation ;
- sélecteur de machine de `Détail machine` utilisé au clavier, avec alerte de protection des changements en attente ;
- exécutable final installé contrôlé : démarrage, bouton `Ouvrir XML`, ouverture d'une fixture anonymisée et résumé du projet lus par l'arbre d'accessibilité.

Les captures WPF de ce cycle antérieur ont permis un contrôle visuel à environ
`1920 x 1032`. Elles ne couvrent pas les nouveaux dialogues de banque V3.6 et
ne remplacent pas les essais encore manuels de contraste élevé, d'échelle
système et de lecteur d'écran.

### macOS / Avalonia headless

- test à `1366 x 768` : barre de patch accessible et contenu principal conservé ;
- test à `1920 x 1080` : disposition large sans perte des commandes testées ;
- atelier de patch testé à sa taille minimale `960 x 640` ;
- ordre de focus vérifié de `Ouvrir XML` vers `Ajouter XML au projet` avec Tab ;
- alertes placées dans le rail latéral et visibles sur un preset aux formats mélangés ;
- matrice de patch et changements en attente testés sans écran.

Ces tests Avalonia headless vérifient la structure et le comportement. Ils ne remplacent pas un contrôle VoiceOver sur un Mac réel.

## Contrôles restant manuels

Les points suivants **n'ont pas été validés matériellement dans ce cycle** :

- lecteur d'écran Windows Narrator ou NVDA ;
- VoiceOver sur macOS réel ;
- mode contraste élevé Windows ;
- échelle système exacte à 125 %, 150 % et 200 % ;
- écran physique `1366 x 768` et `1920 x 1080` ;
- grossissement système supérieur à 200 % ;
- navigation clavier exhaustive de toutes les boîtes de dialogue.

## Checklist manuelle

### Clavier et focus

- [ ] Toutes les commandes principales sont atteignables avec Tab et Maj+Tab.
- [ ] Le focus visible suit un ordre logique.
- [ ] Entrée et Espace activent les boutons, cases et cellules attendus.
- [ ] Échap ferme les dialogues sans appliquer les changements en attente.
- [ ] La sélection et la prévisualisation Easy patch restent utilisables entièrement au clavier.
- [ ] La sélection multiple TX et RX fonctionne au clavier avec Ctrl/Maj.

### Thèmes et contraste

- [ ] Le thème sombre garde un texte lisible dans les listes, tableaux et onglets.
- [ ] Le thème clair garde un contraste suffisant pour les textes secondaires.
- [ ] Le contraste élevé Windows conserve les contours, sélections et focus.
- [ ] Les états modifié, avertissement et erreur ne reposent pas uniquement sur la couleur.

### Taille et mise à l'échelle

- [ ] `1366 x 768` à 100 % : aucune commande critique inaccessible.
- [ ] `1920 x 1080` à 100 % : tableau et panneaux utilisent correctement l'espace.
- [ ] 125 %, 150 % et 200 % : aucun texte ou bouton tronqué.
- [ ] Les noms longs sont ellipsés ou défilables sans recouvrir une autre commande.
- [ ] La matrice et les tableaux conservent leurs ascenseurs internes.

### Lecteurs d'écran

- [ ] Le titre et le rôle de chaque fenêtre sont annoncés.
- [ ] Les boutons icônes et commandes de patch ont un nom accessible.
- [ ] Les cellules actives de la matrice indiquent clairement l'affectation.
- [ ] Les alertes sont annoncées sans déplacement brutal du focus.
- [ ] Les messages d'erreur identifient le champ ou l'action concernée.

Tout défaut observé doit préciser plateforme, résolution, échelle, thème, langue et étapes de reproduction.
