# ADR 0002 - Profils XML basés sur les capacités

Statut : accepté  
Date : 27 juillet 2026

## Contexte

Les structures rencontrées ne correspondent pas toujours à une version
officielle identifiable. Déduire une compatibilité à partir d'un simple numéro
ou convertir une structure inconnue serait dangereux.

## Décision

Le détecteur décrit ce qu'il reconnaît réellement :

- structure complète ;
- structure partielle ;
- structure inconnue en lecture seule.

Chaque profil expose des capacités indépendantes pour les noms, canaux, patch,
réseau, audio, création et sauvegarde. L'interface doit suivre ces capacités.

## Conséquences

- aucune version Dante officielle n'est inventée ;
- une action indisponible peut expliquer précisément sa raison ;
- les balises inconnues restent conservées par le moteur et le garde-fou ;
- l'ajout de nouveaux profils exige des fixtures anonymisées et des tests.
