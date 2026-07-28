# Rapport de nettoyage

## Mise à jour 2026.1 - 2026-07-28

Le nettoyage 2026.1 a été effectué après vérification des références dans le
code, l'installateur, les notices, le catalogue de banques et la Release
GitHub active.

- état avant nettoyage : 576 fichiers suivis, 37 794 578 octets ;
- état après nettoyage : 547 fichiers suivis, 29 145 683 octets ;
- réduction : 29 fichiers et 8 648 895 octets ;
- suppression des vidéos, sous-titres, sommes et captures V3.3/V3.5 qui
  n'étaient plus référencés ;
- suppression des deux archives GitHub V3.6 absentes du catalogue 2026.1 ;
- suppression de l'ancienne description de Release V3.4 ;
- conservation des vidéos, captures, notices et archives 2026.1 ;
- conservation des petites banques V3.6 sous `Resources` uniquement parce
  qu'elles servent encore de fixtures aux tests de lecture et de migration ;
- aucune Release, aucun tag, aucun XML de production et aucune banque
  personnelle n'ont été supprimés ou modifiés.

Les règles `.gitignore` empêchent désormais la réintroduction accidentelle de
captures `docs/media/v*` et de vidéos historiques nommées
`dce-v*-presentation-*`.

## Historique du nettoyage V3.07 Beta

Date : 2026-07-11  
Branche : `main`  
Point de départ : `46dbfcc6a1048733fb50eb3f3b9da6e49852a2a7`  
Tag local de sécurité : `safety-v3.07-before-validation-20260711`

## Périmètre et règles

L'audit porte uniquement sur le dépôt `Mamat79/Dante-Config-Editor`. Aucun fichier situé dans un corpus local externe n'a été déplacé ou supprimé. Aucune Release GitHub et aucun tag existant n'ont été supprimés.

Le nettoyage distingue :

- les sources, tests, fixtures anonymisées et documents utiles à conserver ;
- les sorties régénérables à supprimer localement et à ignorer ;
- les anciens fichiers confirmés sans appel ni référence active ;
- les données réelles ou sensibles, qui ne doivent jamais entrer dans Git.

## État initial observé

- 98 fichiers suivis avant nettoyage.
- `dist` : 2 fichiers, 64,06 Mio, dont l'installateur local et une archive source régénérables.
- `tmp` : 997 fichiers, 460,63 Mio.
- sorties `bin`/`obj` imbriquées : environ 502 Mio.
- total local régénérable supprimé : environ 1,0 Gio.
- aucun dossier `bin`, `obj`, `dist`, `publish`, `tmp`, `artifacts` ou `TestResults` ne subsiste après nettoyage.

## Fichiers suivis et binaires

Aucun `.exe`, `.dll`, `.pdb`, `.zip` ou `.dmg` n'est suivi dans Git.

Les seuls binaires suivis sont intentionnels :

- `DanteEdit.ico` et `packaging/macos/DanteEdit.png`, nécessaires aux paquets Windows/macOS ;
- quatre PDF distincts : quick start et notice complète en français et en anglais.

Les deux JSON de résultats de benchmark étaient des sorties générées et dépendantes de la machine. Ils ont été retirés du dépôt. Les mesures historiques utiles restent synthétisées dans `AUDIT_V3_06.md`.

## XML, secrets et données réelles

Les seuls XML suivis sont :

- `tests/DanteConfigEditorV3.Tests/Fixtures/representative-preset.xml` ;
- `tests/DanteConfigEditorV3.Tests/Fixtures/merge-preset.xml`.

Ils utilisent des noms `DEVICE-*`, des fabricants de test et des identifiants synthétiques. Aucun XML de production, nom de projet réel ou chemin Radio France n'est suivi.

La recherche de motifs de secrets n'a trouvé ni clé API, ni token GitHub, ni mot de passe, ni clé privée. Les seules occurrences de `token` correspondent à des `CancellationToken` .NET.

## Documentation

- `README_V3.md` dupliquait le README principal et l'historique déjà présent dans `CHANGELOG_V3.md` : fichier supprimé.
- `README.md` reste l'entrée utilisateur et développeur.
- `CHANGELOG_V3.md` conserve les références V3.03, V3.04, V3.05 et V3.06 comme historique ; elles ne sont pas des fichiers distribués en parallèle.
- `AUDIT_V3_06.md` est conservé comme preuve de la baseline et du durcissement.
- les quatre notices PDF sont complémentaires, pas des doublons.
- `RELEASE_NOTES.md` reste le document de version distribuable.

## Installation et paquets

L'installateur actif est `installer/DanteConfigEditorV3.iss`, construit par `installer/build_installer.ps1`.

Les anciens scripts `install.cmd`, `install.ps1` et `uninstall.ps1` installaient une copie par utilisateur dans `%LOCALAPPDATA%`. Ils n'étaient plus appelés et contredisaient le workflow Inno Setup actuel dans Program Files : ils ont été supprimés.

Le dossier local `dist` a été nettoyé car il est entièrement régénérable. L'installateur publié dans GitHub Releases n'a pas été supprimé.

## Code mort confirmé

Une recherche de déclarations sans autre occurrence dans les sources, XAML ou tests a permis de retirer 15 méthodes sans appel :

- l'ancien gestionnaire de navigation `NavigationButton_Click` ;
- deux anciennes actions globales de preferred master ;
- neuf anciennes méthodes de prévisualisation globale et leur helper privé ;
- deux helpers de patch devenus sans appel.

Les 202 clés de `LocalizationService` ont toutes au moins un usage détecté. Aucun bloc de traduction n'a été supprimé.

Les classes volumineuses restent :

| Fichier | Lignes avant nettoyage | Observation |
|---|---:|---|
| `MainWindow.xaml.cs` | 3 284 | orchestration UI Windows encore trop large, atelier de patch inclus |
| `Models/DanteProject.cs` | 1 673 | chargement et mutations unitaires encore regroupés après extractions |
| `src/DanteConfigEditor.Mac/MainWindow.axaml.cs` | 1 680 | orchestration UI Mac à surveiller |
| `Services/LocalizationService.cs` | 695 | dictionnaire bilingue centralisé |

Leur réduction fonctionnelle sera progressive et testée ; aucune réécriture générale n'est prévue.

## Dépendances

Versions observées :

- .NET 8 ;
- Avalonia `11.3.18` et Avalonia Headless `11.3.18` ;
- Avalonia DataGrid `11.3.13` ;
- xUnit `2.9.3`, runner Visual Studio `3.1.5` ;
- Microsoft.NET.Test.Sdk `18.7.0` ;
- coverlet.collector `10.0.1`.

Le scan de vulnérabilités doit être rejoué pendant la validation finale. Avalonia 12 et la migration xUnit v3 ne sont pas mélangés à ce durcissement, car ils changent les contraintes d'interface et de tests ; ils devront faire l'objet d'un chantier séparé validé sur Windows et macOS.

## `.gitignore`

Les exclusions ajoutées couvrent maintenant :

- `artifacts`, `TestResults`, fichiers TRX et couverture ;
- paquets NuGet ;
- DMG, bundles `.app` et sommes SHA-256 ;
- résultats de benchmark ;
- sorties du pack de validation manuelle ;
- `.DS_Store` macOS.

## Éléments conservés

- toutes les sources Windows, Core et macOS ;
- fixtures anonymisées et tests ;
- scripts de build Windows/macOS et Inno Setup ;
- générateur et quatre notices PDF ;
- audit V3.06 et historique Git ;
- benchmark source ;
- icônes ;
- installation V3.07 déjà présente dans Program Files ;
- Releases et tags GitHub existants.

## Limites de ce nettoyage

- l'absence de code mort est établie par compilation, recherche statique et usages connus, pas par un analyseur commercial exhaustif ;
- aucune compatibilité Dante Controller n'est déclarée à partir de ce nettoyage ;
- les versions réelles de Dante Controller, fabricants et modèles devront être renseignées dans la matrice au fur et à mesure des imports prouvés ;
- les sorties `dist`, `bin`, `obj` et de benchmark seront recréées temporairement pendant la validation finale puis nettoyées à nouveau.
