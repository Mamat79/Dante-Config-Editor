# Rapport de performance Dante Config Editor 2026.1

Date : 27 juillet 2026
Référence avant optimisation : `79d02b3`
Référence après optimisation : `4971c0d`

## Méthode

Le banc `DanteConfigEditorV3.Benchmarks` produit des presets synthétiques de
10, 50 et 200 machines, avec 64 TX et 64 RX par machine. Chaque résultat
ci-dessous est la médiane de neuf passages : trois processus indépendants,
chacun exécutant trois passages par taille.

Le préchauffage exécute seize cycles de mutation et de validation afin que le
JIT tiered soit actif avant la première mesure. Les fichiers JSON bruts restent
des sorties locales ignorées par Git.

Scénarios mesurés :

- chargement XML ;
- renommage groupé d'une machine et de ses 64 TX ;
- patch groupé de 64 RX ;
- validation structurée 2026.1 ;
- nouvel appel du garde-fou après validation ;
- création de la session de matrice ;
- génération du modèle de synoptique ;
- sauvegarde XML ;
- sauvegarde et ouverture d'un `.dceproj`.

Environnement identique à la baseline : Windows `10.0.26200`, .NET SDK
`8.0.423`, runtime .NET `8.0.29`, configuration Release.

## Temps du moteur

Valeurs en millisecondes. Une valeur plus basse est meilleure.

| Machines | Chargement avant | Après | Édition avant | Après | Patch avant | Après | Validation avant | Après | Sauvegarde XML avant | Après |
|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| 10 | 34,071 | 29,969 | 44,347 | 3,049 | 5,642 | 1,677 | 10,959 | 5,140 | 89,506 | 59,898 |
| 50 | 103,645 | 87,810 | 198,820 | 20,755 | 20,167 | 11,424 | 39,294 | 29,838 | 220,508 | 195,369 |
| 200 | 252,502 | 267,278 | 317,410 | 38,092 | 46,545 | 28,536 | 86,948 | 36,062 | 501,695 | 363,457 |

Gains principaux à 200 machines :

- édition groupée : `-88,0 %` ;
- patch groupé : `-38,7 %` ;
- validation initiale : `-58,5 %` ;
- sauvegarde XML : `-27,6 %`.

Le chargement 200 machines varie de `+5,9 %`, sous la limite de régression de
10 %. Les deux autres tailles sont plus rapides.

## Matrice, synoptique et projet DCE

| Machines | Session matrice avant | Après | Synoptique avant | Après | Sauvegarde `.dceproj` avant | Après | Ouverture `.dceproj` avant | Après |
|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| 10 | 0,211 | 0,121 | 1,164 | 0,638 | 60,642 | 37,208 | 13,563 | 8,819 |
| 50 | 0,944 | 0,786 | 4,955 | 4,347 | 125,142 | 104,038 | 40,829 | 52,992 |
| 200 | 1,435 | 2,550 | 9,394 | 21,850 | 283,989 | 173,780 | 140,960 | 121,234 |

Les mesures matrice et synoptique à 200 machines sont très sensibles à la
charge de la machine. À allocations identiques, le synoptique a varié entre
`10,935 ms` et `23,897 ms` selon le processus. Aucun code du synoptique n'a été
modifié dans ce lot et la valeur maximale reste inférieure à 24 ms. Ce point
doit être remesuré sur un runner dédié avant d'en déduire une régression.

L'ouverture `.dceproj` à 50 machines présente également une variation isolée
de 12 ms alors que les tailles 10 et 200 progressent. Il n'existe pas de
tendance croissante avec la taille.

## Allocations mémoire

Valeurs en Mio.

| Machines | Édition avant | Après | Patch avant | Après | Validation avant | Après | Working set final avant | Après |
|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| 10 | 20,335 | 1,643 | 1,605 | 1,174 | 7,206 | 2,888 | 61,211 | 56,531 |
| 50 | 98,322 | 7,478 | 7,666 | 5,638 | 34,040 | 7,828 | 84,996 | 90,289 |
| 200 | 390,759 | 29,358 | 30,406 | 22,382 | 134,666 | 26,325 | 152,492 | 158,254 |

À 200 machines, les allocations de l'édition baissent de `92,5 %` et celles
de la validation de `80,5 %`. Le working set final augmente de `3,8 %`, soit
environ 5,8 Mio, principalement pour les index en mémoire.

## Corrections appliquées

- index des machines par nom et identité stable ;
- index RX par identité et Dante Id ;
- index TX par label et numéro, avec conservation du cas ambigu ;
- regroupement des renommages TX d'un lot, puis un seul parcours des RX ;
- cache défensif du garde-fou et de la validation ;
- invalidation du cache par l'événement `XDocument.Changed`, y compris pour une
  mutation XML directe hors des commandes métier ;
- copie du résultat en sortie afin qu'un appelant ne puisse pas altérer le
  cache ;
- arrêt immédiat de la comparaison sur tout sous-arbre XML strictement
  identique ;
- préchauffage explicite des chemins de validation dans le banc.

Le second appel du garde-fou, après la validation initiale, passe de
`7,715 / 28,008 / 56,407 ms` à environ `0,007 / 0,009 / 0,009 ms` pour
10 / 50 / 200 machines. La validation initiale continue bien d'exécuter le
contrôle complet.

## Virtualisation et bornes

- la grille WPF active la virtualisation des lignes et des colonnes en mode
  recyclage ;
- les tableaux de prévisualisation et la liste principale virtualisent les
  lignes ;
- les contrats sont couverts par `PatchWorkspaceUiContractTests` ;
- la pile de commandes 2026.1 est bornée, tout comme l'historique XML
  historique ;
- les résultats de benchmarks et les presets volumineux restent régénérables
  et ne sont pas commités.

## Limites restantes

- la validation initiale est encore appelée synchroniquement par
  `ProjectSession`; son exécution hors thread UI reste à traiter si des corpus
  sensiblement plus gros dépassent les mesures actuelles ;
- le banc mesure le modèle de matrice, pas le coût de rendu WPF de milliers de
  cellules ;
- le banc mesure la construction du diagramme, pas l'export PDF ni le rendu
  visuel final ;
- aucune preuve d'absence de fuite sur plusieurs heures n'a été produite ;
- les valeurs dépendent du poste, de l'antivirus, du JIT et de la charge
  thermique ; elles servent à comparer deux commits sur la même machine, pas à
  définir une promesse absolue.

## Commande

```powershell
dotnet run --project .\benchmarks\DanteConfigEditorV3.Benchmarks\DanteConfigEditorV3.Benchmarks.csproj -c Release -- --phase 2026.1 --commit 4971c0d --output .\benchmarks\results\2026.1.json
```
