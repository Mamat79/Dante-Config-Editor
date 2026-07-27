# Dante Config Editor 2026.1 Beta

[English release notes](RELEASE_NOTES_EN.md)

## Statut

`2026.1.0-beta.1` est une refondation progressive de DCE à partir de la V3.6
stabilisée. Elle ne remplace pas la V3.6 : les applications, raccourcis,
identifiants d’installation et profils locaux sont distincts.

DCE reste un outil tiers non officiel, sans affiliation avec Audinate. Il
travaille uniquement sur des fichiers hors ligne.

## Principales évolutions

- séparation Domain, DanteXml, Application et Infrastructure ;
- session centrale, commandes transactionnelles, Annuler/Rétablir et historique ;
- projet `.dceproj` versionné pour la disposition, les notes et les ressources
  propres à DCE, sans pollution du XML Dante ;
- shell Windows 2026.1 avec navigation latérale et inspecteur ;
- espace Patch unique avec tableau, matrice, Easy Patch, sélection et 1:1 ;
- synoptique interactif synchronisé avec la sélection et le patch ;
- Centre de validation filtrable et exportable ;
- profils XML capables de limiter une structure inconnue ou de l’ouvrir en
  lecture seule ;
- banque format 2 et migration V3.6 par copie vérifiée ;
- profil local 2026.1 isolé.

## Fidélité XML

Le document original reste la source. DCE effectue des mutations ciblées et
conserve les nœuds, attributs, namespaces, ordre et valeurs inconnues. Une
sauvegarde utilise un temporaire, une relecture, une validation, une copie de
sécurité et un remplacement atomique.

Le corpus automatisé couvre notamment les presets partiels, TX ou RX seuls,
subscriptions locales `.`, sources absentes, canaux absents, namespace par
défaut, Unicode, extensions inconnues, interfaces multiples et modes audio
mixtes.

La V3.6 a été importée avec succès dans Dante Controller par le mainteneur.
Pour cette bêta 2026.1, **validation manuelle Dante Controller requise** avec
la checklist `docs/2026.1/DANTE_CONTROLLER_MANUAL_VALIDATION.md`.

## Performance

Sur le preset synthétique de 200 machines avec 64 TX et 64 RX chacune :

- édition groupée : `317,410 ms` vers `38,092 ms` ;
- allocations d’édition : `390,759 Mio` vers `29,358 Mio` ;
- validation : `86,948 ms` vers `36,062 ms` ;
- sauvegarde XML : `501,695 ms` vers `363,457 ms`.

## Validation automatisée

- 364 tests Core/Windows réussis ;
- 20 tests Avalonia/macOS sans écran réussis ;
- build Windows Release sans warning ;
- corpus synthétique sauvegardé et comparé sémantiquement sans perte.

Les nombres finaux de l’artefact livré sont consignés dans le rapport bêta.

## Installation

### Windows

`DanteConfigEditor2026_1_Beta_Installer.exe` inclut le runtime .NET 8 et les
notices bilingues. Le dossier proposé est
`C:\Program Files\Dante Config Editor 2026.1 Beta\`.

### macOS

Deux DMG autonomes sont prévus pour Apple Silicon et Intel. Ils sont signés ad
hoc mais non notariés.

## Limites

- création complète de projet toujours expérimentale ;
- aucune identité matérielle réelle créée lors d’une duplication ou insertion
  depuis une banque ;
- interface 2026.1 complète actuellement centrée sur Windows ;
- installateur Windows non signé Authenticode ;
- DMG non notariés ;
- aucune Release GitHub publiée automatiquement.
