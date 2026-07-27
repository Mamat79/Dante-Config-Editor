# ADR 0005 - Banque format 2 et migration par copie

## Statut

Accepté pour 2026.1.

## Contexte

La banque V3.6 format 1 est déjà utilisée localement et publiée avec des modèles
anonymisés. La modifier en place au premier démarrage risquerait de toucher une
banque personnelle ou partagée. Le format ne protégeait pas non plus l'image
par une empreinte.

## Décision

- Le format 1 reste lisible.
- Les nouvelles banques utilisent le format 2.
- Le format 2 ajoute l'empreinte des images et conserve les propriétés JSON
  inconnues.
- La migration 1 vers 2 écrit exclusivement dans un nouveau dossier.
- Une archive vérifiée est créée avant la conversion.
- La source est contrôlée par SHA-256 avant et après.
- L'ajout et la duplication passent par le dispatcher de commandes.

## Conséquences

- Une banque V3.6 continue de fonctionner sans migration urgente.
- La 2026.1 peut proposer une copie modernisée sans toucher à l'original.
- Deux banques peuvent cohabiter et leur emplacement reste un choix utilisateur.
- Les modèles intégrés V3.6 n'ont pas besoin d'être réécrits dans Git pour
  rester compatibles.
- Une future version 3 du format devra ajouter une migration explicite au même
  point central.

