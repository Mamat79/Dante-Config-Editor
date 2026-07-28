# Principes d’interface DCE 2026.1

## Objectif

L’interface 2026.1 doit permettre de comprendre l’état d’un projet avant de
modifier son XML. Elle privilégie la densité lisible, la navigation prévisible
et la proximité entre une action et son contexte.

## Structure Windows

Le shell est organisé en quatre zones :

1. navigation latérale ;
2. barre de projet et actions persistantes ;
3. espace de travail central ;
4. inspecteur contextuel.

Les réglages Machines sont affichés au premier lancement. Un utilisateur peut
réduire les panneaux secondaires, mais l’application ne doit jamais démarrer
avec ses réglages principaux masqués par défaut.

Les trois poignées de repli restent visibles dans les deux états : navigation
à gauche, réglages Machines et inspecteur à droite. Un redimensionnement de la
fenêtre ne doit jamais replier une zone sans action explicite de l'utilisateur.

## Navigation

- une section correspond à une intention métier ;
- la sélection d’une machine est conservée entre les vues compatibles ;
- Patch, Easy Patch et synoptique reposent sur la même session ;
- un résultat de validation peut naviguer vers la machine ou le canal concerné ;
- une action indisponible est désactivée avec une explication, pas avec une
  erreur tardive.

## Édition

- Entrée valide ;
- Tab valide et passe à l’élément suivant ;
- Maj+Tab valide et revient à l’élément précédent ;
- Échap annule l’édition locale ;
- une action groupée produit une seule entrée Annuler/Rétablir ;
- les actions destructives demandent confirmation ;
- une mutation ne doit jamais être cachée dans un événement visuel.

## Patch

Les représentations Patch partagent la même session et les mêmes identités
stables. Une action immédiate doit modifier le projet une seule fois et mettre
à jour les autres vues sans reconstruire toute la matrice. Les avertissements
de remplacement d’un RX déjà patché sont actifs par défaut.

## Couleurs et thèmes

- aucune information importante ne dépend uniquement de la couleur ;
- texte et fond doivent rester contrastés en thèmes clair et sombre ;
- les contrôles natifs ou propriétaires doivent recevoir les mêmes ressources ;
- l’état sélectionné conserve un texte lisible ;
- les alertes utilisent également un libellé et une icône.

## Dimensionnement

- les listes et matrices importantes sont virtualisées ;
- les boutons peuvent revenir à la ligne sans couper leur texte ;
- les panneaux secondaires cèdent la place au contenu principal avant de le
  masquer ;
- le minimum Windows reste `1120 x 720` ;
- les écrans `1366 x 768` et `1920 x 1080` font partie des contrats ;
- les contrôles doivent rester utilisables à 125 %, 150 % et 200 %, à vérifier
  manuellement sur le système cible.

## Accessibilité

Les actions principales doivent fournir :

- un nom accessible ;
- un texte d’aide ou une info-bulle ;
- un focus visible ;
- un ordre de tabulation cohérent ;
- un état textuel pour les erreurs, avertissements et sélections.

Les tests de contrat vérifient une partie de ces règles. Ils ne remplacent pas
un essai avec Narrateur, VoiceOver, contraste élevé et grossissement système.

## Captures et documentation

Une capture doit provenir d’une version réelle et d’un preset synthétique ou
anonymisé. Une ancienne capture utilisée pour expliquer un concept doit être
identifiée comme telle. Aucun écran recomposé ne doit être présenté comme une
capture de l’application.
