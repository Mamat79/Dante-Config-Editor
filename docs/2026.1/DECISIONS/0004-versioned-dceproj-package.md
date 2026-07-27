# ADR 0004 - Paquet `.dceproj` versionné

## Statut

Accepté pour 2026.1.

## Contexte

Le XML Dante ne doit contenir ni mise en page, ni annotations, ni préférences
d'interface propres à DCE. Des fichiers compagnons dispersés seraient difficiles
à déplacer, sauvegarder et migrer ensemble.

## Décision

Le projet DCE utilise un conteneur ZIP de données avec :

- un manifeste JSON versionné ;
- le XML Dante comme entrée indépendante ;
- des JSON par responsabilité ;
- des images facultatives limitées à PNG, JPEG et WebP ;
- une empreinte SHA-256 pour chaque entrée de contenu ;
- aucune extraction arbitraire.

L'infrastructure valide le paquet avant ouverture et avant remplacement de la
destination. Elle applique temporaire, relecture, backup et remplacement
atomique.

Le XML direct reste un parcours de premier rang. DCE ne convertit jamais un XML
en `.dceproj` sans action explicite.

## Conséquences

- Les données DCE n'altèrent pas le XML destiné à Dante Controller.
- Un projet devient transportable dans un seul fichier.
- Le schéma et les migrations doivent rester documentés et testés.
- Les champs inconnus du manifeste sont conservés autant que possible.
- Les données volumineuses sont bornées et les exécutables sont refusés dans
  `assets/`.
- Le journal persistant ne rend pas les anciennes opérations annulables.

