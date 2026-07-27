# Journal des versions DCE

Toutes les dates utilisent le calendrier ISO. Les versions antérieures à
2026.1 sont conservées dans [CHANGELOG_V3.md](CHANGELOG_V3.md).

## [2026.1.0-beta.1] - 2026-07-27

### Ajouté

- architecture progressive en cinq couches autour du moteur XML V3.6 ;
- session de projet et dispatcher de commandes transactionnelles ;
- historique borné, Annuler et Rétablir ;
- conteneur de projet `.dceproj` versionné et sécurisé ;
- migration non destructive du profil local V3.6 ;
- banque de machines format 2, migrable par copie ;
- shell Windows 2026.1, navigation latérale et inspecteur contextuel ;
- espace Patch unifié et cinq représentations synchronisées ;
- synoptique interactif relié à la sélection et aux subscriptions ;
- Centre de validation dépendant des capacités du profil XML ;
- corpus XML synthétique et anonymisé élargi ;
- checklist de validation manuelle Dante Controller.

### Modifié

- renommages groupés et patchs utilisent des index dérivés du document ;
- validation et garde-fou réutilisent un cache invalidé à chaque mutation XML ;
- installation Windows et paquet macOS possèdent une identité 2026.1 séparée ;
- profil local déplacé vers `%LOCALAPPDATA%\DanteConfigEditor2026.1` ;
- catalogue de banques GitHub lu depuis `main`.

### Performance

- édition groupée de 200 machines : `317,410 ms` vers `38,092 ms` ;
- allocations du même scénario : `390,759 Mio` vers `29,358 Mio` ;
- validation de 200 machines : `86,948 ms` vers `36,062 ms`.

Les mesures complètes et leur méthode figurent dans
`docs/2026.1/PERFORMANCE_REPORT.md`.

### Sécurité et compatibilité

- aucune réécriture globale du document XML d’origine ;
- balises, attributs, namespaces et valeurs inconnues préservés ;
- structures fondamentales inconnues limitées ou en lecture seule ;
- sauvegarde XML et `.dceproj` temporaire, validée, sauvegardée puis remplacée
  atomiquement ;
- corpus versionné exclusivement synthétique et anonymisé.

La V3.6 a été testée avec succès dans Dante Controller par le mainteneur.
**Validation manuelle Dante Controller requise pour la sortie 2026.1.**

### Distribution

- Windows : `DanteConfigEditor2026_1_Beta_Installer.exe` ;
- macOS Apple Silicon et Intel : DMG autonomes dédiés ;
- aucune Release GitHub créée automatiquement ;
- aucune fusion automatique dans `main`.
