# Validation de livraison V3.07 officielle

## Statut

Ce document suit la promotion de la **V3.07 officielle**. Son nom historique est conservé pour le pack documentaire demandé, mais il ne signifie pas que l'application est une Release Candidate ni un produit officiel Audinate.

Un résultat automatisé réussi ne prouve pas la compatibilité avec Dante Controller. La version ne peut être qualifiée pour un usage terrain qu'après les imports manuels décrits dans `MANUAL_DANTE_CONTROLLER_TESTS.md`.

## Référence de travail

- dépôt : `Mamat79/Dante-Config-Editor` ;
- branche : `main` ;
- version : `3.07` ;
- tag de sécurité avant promotion officielle : `safety-v3.07-beta-before-official-20260711` ;
- tag de sécurité avant travaux : `safety-v3.07-before-validation-20260711` ;
- système de validation locale : Windows x64, .NET 8 ;
- commit fonctionnel validé : `bdb1de9ae7b6b80342e18d1f2172d83e74b7bc43` ;
- date de validation locale : 2026-07-11.

## Contrôles automatisés

| Contrôle | État | Preuve attendue |
|---|---|---|
| Restore application et projets de tests | Réussi localement | trois restores, code retour 0 |
| Tests Core en Release | Réussi localement | 67 réussis, 0 échec, 0 ignoré |
| Tests UI Mac headless en Release | Réussi localement | 7 réussis, 0 échec, 0 ignoré |
| Build Windows en Release | Réussi localement | 0 warning, 0 erreur |
| Build interface Mac en Release | Réussi localement | 0 warning, 0 erreur |
| Publish Windows `win-x64` et autonome installateur | Réussi localement | publication framework-dependent et mono-fichier autonome produites |
| Publish Mac autonome `osx-arm64` et `osx-x64` | Réussi localement | deux publications produites ; DMG réservé au runner macOS |
| Construction installateur Inno Setup | Réussi localement | 66 840 678 octets, SHA-256 dans `TESTING.md` |
| Installation de remplacement sur ce PC | Réussi localement | deux passes, une entrée, Program Files, notices, raccourci et démarrage vérifiés |
| GitHub Actions Windows et macOS | Réussi sur le commit officiel | runs `29168999560` et `29168999565` |
| Scan NuGet vulnérable | Réussi localement | aucun package vulnérable signalé dans les sept projets |
| Publication GitHub | Réussie | `main`, tag de sécurité, tag et Release stable `v3.07` publiés |
| État Git final | À contrôler après le dernier push documentaire | branche `main` propre attendue |

## Contrôles fonctionnels locaux déjà couverts

- garde-fou XML lié à une identité technique stable ;
- blocage des chemins XML inconnus ;
- sauvegarde atomique et protection d'une destination existante ;
- récupération liée à la nouvelle destination après `Enregistrer sous` ;
- namespace XML par défaut ;
- aliases de subscription reconnus et patch local `.` ;
- interface IPv4 principale sans modification implicite de l'interface secondaire ;
- fusion, suppression de machine, formats audio et actions groupées ;
- presets synthétiques de 10, 50 et 200 machines en 64 TX / 64 RX ;
- atelier de patch visuel : sélection multiple, affectation séquentielle, matrice et changements en attente ;
- comportement compact des interfaces Windows et Mac.

Les nombres exacts de tests et les temps mesurés sont consignés dans `TESTING.md`.

## Validation Dante Controller

**État : non testée dans ce cycle.**

Aucune case d'import ne doit être marquée comme réussie sans preuve obtenue dans une version identifiée de Dante Controller. Utiliser `tools/New-ValidationPack.ps1`, puis suivre `MANUAL_DANTE_CONTROLLER_TESTS.md` sur une copie de travail.

## Décision de livraison

- [x] tous les tests locaux sont verts ;
- [x] les builds et publications sont sans warning ;
- [x] l'installateur de remplacement est construit et vérifié ;
- [x] l'application installée démarre depuis Program Files ;
- [x] les notices et raccourcis installés sont présents ;
- [x] le push `main` et le tag de sécurité sont publiés ;
- [x] les workflows GitHub Actions du commit de livraison sont verts ;
- [x] les limites restantes sont cohérentes avec `KNOWN_LIMITATIONS.md` ;
- [x] aucune donnée XML de production n'est suivie dans Git.

La V3.07 officielle reste un outil tiers hors ligne. L'import Dante Controller demeure une validation manuelle séparée et obligatoire avant toute utilisation terrain.
