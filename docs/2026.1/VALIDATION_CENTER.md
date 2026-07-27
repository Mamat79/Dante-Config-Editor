# Centre de validation 2026.1

## Objectif

Le Centre de validation rassemble les contrôles hors ligne de DCE sans
modifier le XML. Il ne remplace ni Dante Controller ni un test sur le réseau
réel. Il rend explicites les contrôles effectués automatiquement, leur cible
et les éléments qui restent hors simulation.

## Sources des résultats

`ProjectValidationService` assemble :

1. la validation structurelle du moteur XML V3.6 ;
2. l'intégrité des identités, canaux, flows et subscriptions ;
3. la comparaison avec le document source et le garde-fou de sauvegarde ;
4. le profil XML détecté ;
5. les capacités autorisées par ce profil ;
6. un rappel du périmètre qui nécessite un contrôle externe.

Le service est en lecture seule. Il ne corrige pas le document et ne propose
aucune correction automatique lorsque celle-ci n'est pas prouvée sûre.

## Niveaux

- **Erreur bloquante** : l'état ne doit pas être sauvegardé, ou le profil ne
  permet pas une écriture fiable.
- **Avertissement** : l'état reste sauvegardable mais doit être examiné.
- **Information** : état valide ou particularité conservée, par exemple un RX
  libre ou une subscription locale.

Les compteurs restent calculés sur le résultat complet, indépendamment du
filtre et de la recherche visibles.

## Profil et capacités

Les profils initiaux sont :

- `recognized-complete` : structure reconnue et capacités complètes selon les
  éléments réellement présents ;
- `recognized-partial` : structure lisible, mais opérations restreintes ;
- `unknown-read-only` : structure fondamentale inconnue, sauvegarde et édition
  bloquées.

Les actions Windows consultent `DanteXmlCapabilities`. Un profil en lecture
seule ne peut pas réactiver une commande en contournant le centre de
validation. Les exports sans mutation restent disponibles.

## Cibles et navigation

Une anomalie peut cibler :

- une machine par son identité stable de session ;
- un canal TX ou RX par l'identité stable de sa machine et son `danteId` ;
- une subscription par la machine RX et le `danteId` RX ;
- le projet lorsque le problème concerne la racine ou le profil.

Le bouton **Ouvrir l'élément** et le double-clic :

- ouvrent la machine et sélectionnent le canal lorsqu'il est identifiable ;
- ouvrent la vue **RX vers TX** et sélectionnent le RX pour une subscription ;
- restent désactivés lorsqu'aucune cible fiable n'existe.

Le chemin XML affiché est un repère de diagnostic. Il n'est jamais exécuté
comme une requête de modification.

## Rapport exporté

Le rapport TXT contient :

- le fichier et la date de validation ;
- le profil, le niveau de reconnaissance et le mode d'accès ;
- les compteurs ;
- le message humain ;
- le détail technique conservé ;
- la cible et le chemin XML ;
- l'action suggérée ;
- une section distincte pour les éléments hors validation automatique.

Le fichier de référence V3.6 a été validé par l'utilisateur dans Dante
Controller. Pour un projet 2026.1 modifié, DCE vérifie automatiquement la
structure et la cohérence ; la disponibilité du matériel, le firmware et le
comportement du réseau actif ne peuvent pas être simulés hors ligne.

## Traduction

Les niveaux, catégories, profils, capacités, actions et messages du nouveau
centre sont disponibles en français et en anglais. Les validateurs historiques
produisent encore leur détail technique brut en français : en anglais, le
message humain est traduit et le détail original reste visible pour ne perdre
aucune information de diagnostic.

## Vérifications

La phase a été contrôlée avec :

- tests du service de validation et des profils ;
- tests de contrat XAML ;
- suite Windows complète ;
- ouverture réelle du preset anonymisé représentatif ;
- thèmes sombre et clair ;
- langues française et anglaise ;
- recherche `DEVICE-B` ;
- navigation vers `DEVICE-B` ;
- navigation vers le RX local `DEVICE-A / RX 1`.

Ces vérifications ne constituent pas un nouvel import Dante Controller du
build 2026.1.
