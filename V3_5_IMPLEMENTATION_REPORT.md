# Rapport d'implémentation V3.5

## Résultat

Les phases 1, 2, 3 et 5 du cahier des charges ont été réalisées. La phase 4 de création d'appareils génériques a été abandonnée à la demande de l'utilisateur. La suppression existante d'une machine a été renforcée par un test complet de sauvegarde et de relecture. La duplication n'est pas proposée sans preuve d'import dans Dante Controller.

## Phase 1 - performances et en-têtes fixes

Cause identifiée :

- chaque clic appelait `RefreshTargetRows` ;
- cette méthode reconstruisait toute la matrice ;
- 4 225 contrôles visuels étaient recréés en 64 x 64 ;
- 16 641 contrôles visuels étaient recréés en 128 x 128.

Mesures headless sur la même machine, hors temps de construction initiale :

| Matrice | V3.4.2 avant | V3.5 après |
|---|---:|---:|
| 64 x 64, un clic | 4 231,58 ms | 70,29 ms |
| 128 x 128, un clic | 21 784,06 ms | 2,87 ms |
| 128 x 128, 100 clics | non mesuré | 490,80 ms, soit 4,91 ms/clic |

Ces mesures sont une exécution locale reproductible et non une garantie contractuelle. La V3.5 vérifie aussi qu'aucune reconstruction n'a lieu pendant les clics et que le projet principal reste inchangé avant application.

Les en-têtes TX et RX restent fixes, les ascenseurs sont synchronisés et les labels longs disposent d'une infobulle.

## Phase 2 - opérations de patch

- Patch 1:1 par premier TX, premier RX et nombre de canaux.
- Patch 1:1 également disponible directement dans la grille depuis le premier point sélectionné.
- Bouton `FLIP TX ⇄ RX` agrandi et coloré pour être immédiatement repérable.
- Contrôle des limites et collisions.
- Prévisualisation et changements en attente.
- Application atomique en un lot.
- Échange TX/RX sans créer de patch inverse.
- Dialogue appliquer, abandonner ou annuler si des changements sont en attente.
- Zoom 50, 67, 75, 100, 125, 150 et 200 %, réinitialisation et ajustement.
- Tab et Maj+Tab pour valider puis passer au canal suivant ou précédent, même quand l'application reconstruit le composant Easy patch après une modification XML.

## Phase 3 - traductions et mise en page

- Audit français/anglais étendu aux placeholders et textes dynamiques.
- Messages numériques bilingues.
- Avertissements DMT traduits.
- Fenêtres macOS d'import/export rendues adaptatives.
- Bouton de rafraîchissement rattaché à la prévisualisation.
- Panneaux Configuration défilables localement lorsque la hauteur manque.

## Phase 4 - décision de sécurité

- Création d'appareils génériques : non intégrée, conformément à la demande finale.
- Suppression d'une machine : conservée, avec nettoyage des subscriptions reconnues.
- Test ajouté : suppression, sauvegarde atomique, relecture et absence de référence pendante.
- Duplication : non intégrée. Copier ou inventer des identifiants techniques ne permet pas de garantir un XML importable.

## Phase 5 - imports stables et DMT

- Registre d'adaptateurs séparés pour JSON, CSV, XLSX DMT, ODS DMT et ZIP console.
- JSON versionné avec refus des propriétés inconnues.
- Refus des versions CSV mélangées.
- Refus des numéros de canaux dupliqués et des Dante ID invalides.
- Erreur claire lorsque les colonnes XLSX/ODS requises sont absentes.
- Rapport affiché avant application : format, source, version, listes, machines, canaux, lignes ignorées, labels vides et doublons.
- Fixtures générées selon les exporteurs DMT 2.14.0-RC1 du commit `3c34052`.
- Comparaison des sorties DMT réelles et des fixtures : JSON et CSV identiques.

## Validation automatisée

- 199 tests cœur et contrats Windows : réussis.
- 16 tests Avalonia/macOS sans écran : réussis.
- Build Windows Release : réussi, 0 warning, 0 erreur.
- Build macOS Release sur Windows : réussi, 0 warning, 0 erreur.

## Limites restantes

- Aucun import V3.5 n'a encore été effectué dans Dante Controller.
- Aucun Mac physique Intel ou Apple Silicon n'a été utilisé pendant cette session.
- Les tests macOS sont des tests Avalonia sans écran exécutés sous Windows.
- DMT 2.14.0-RC1 est validé au niveau de ses exporteurs source JSON/CSV, pas d'une Release binaire finale.
- Les identifiants techniques Dante restent protégés et ne sont jamais inventés.

## Checklist manuelle

Windows :

1. Ouvrir un XML de copie.
2. Vérifier une matrice 64 x 64 puis 128 x 128.
3. Tester clic, glissement, Tab, Maj+Tab, zoom et en-têtes fixes.
4. Préparer plusieurs changements, fermer sans appliquer, puis recommencer et appliquer.
5. Tester Patch 1:1 avec et sans collision.
6. Importer les fixtures DMT JSON et CSV.
7. Supprimer une machine sur une copie, sauvegarder sous un autre nom et importer dans Dante Controller.

macOS Intel et Apple Silicon :

1. Vérifier les mêmes opérations de matrice.
2. Réduire les fenêtres d'import/export et contrôler qu'aucun bouton n'est masqué.
3. Basculer français/anglais.
4. Tester la molette de zoom et les en-têtes fixes.
5. Contrôler l'ouverture et la sauvegarde d'une copie XML.

Dante Controller :

1. Importer le XML sans modification.
2. Importer un XML avec renommage et patch.
3. Contrôler les rôles de devices, les Dante ID, les media types et les interfaces.
4. Contrôler les subscriptions locales et distantes.
5. Importer le scénario avec suppression d'une machine.
6. Ne valider aucun résultat tant que Dante Controller n'a pas confirmé l'import.

## Proposition de réponse GitHub en anglais

Thank you again for the detailed feedback. V3.5 now updates patch cells incrementally instead of rebuilding the complete matrix after every click. In the local headless benchmark, a 64 x 64 click went from about 4.23 seconds to 70 ms, and a 128 x 128 click from about 21.78 seconds to 2.87 ms. One hundred staged clicks in 128 x 128 complete in about 491 ms without modifying the project before Apply.

The phase also adds fixed TX/RX headers, Patch 1:1, TX/RX selection swap, matrix zoom, Tab and Shift+Tab channel editing, layout and translation fixes, and a stricter adapter-based label import architecture. DMT 2.14.0-RC1 JSON and CSV outputs from commit `3c34052` are now covered by reproducible fixtures and automated tests.

The macOS UI tests pass headlessly, but physical Intel and Apple Silicon testing is still required. Dante Controller import validation also remains a manual gate, and no compatibility claim is made without that test.
