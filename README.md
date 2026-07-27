# Dante Config Editor 2026.1 Beta

[English documentation](README_EN.md)

Éditeur local et hors ligne de presets XML pour Dante Controller, développé
par Mamat avec l’aide d’agents de développement.

> **Statut : bêta sur la branche `2026.1`.**
> DCE est un outil tiers non officiel, sans affiliation avec Audinate. Il ne
> pilote aucun réseau Dante et n’utilise ni SDK ni API Audinate. Travaillez sur
> une copie et contrôlez le XML final dans Dante Controller avant exploitation.

La V3.6 reste la référence stable. Ses XML modifiés par DCE ont été importés
avec succès dans Dante Controller par le mainteneur. La bêta 2026.1 bénéficie
de tests structurels et sémantiques renforcés, mais sa validation manuelle doit
être consignée séparément pour le commit et le type de preset concernés.

## Documentation

- [Vidéo de présentation 2026.1 - français](docs/media/dce-2026-1-presentation-fr.mp4)
- [Sous-titres français séparés](docs/media/dce-2026-1-presentation-fr.srt)
- [2026.1 presentation video - English](docs/media/dce-2026-1-presentation-en.mp4)
- [Separate English subtitles](docs/media/dce-2026-1-presentation-en.srt)
- [Démarrage rapide FR](docs/QuickStart_DanteConfigEditorV3_FR.pdf)
- [Notice complète FR](docs/Notice_DanteConfigEditorV3_FR.pdf)
- [English quick start](docs/QuickStart_DanteConfigEditorV3_EN.pdf)
- [Full English guide](docs/Notice_DanteConfigEditorV3_EN.pdf)
- [Architecture 2026.1](docs/2026.1/ARCHITECTURE_2026_1.md)
- [Format de projet `.dceproj`](docs/2026.1/DCEPROJECT_FORMAT.md)
- [Format des banques](docs/2026.1/DEVICE_LIBRARY_FORMAT.md)
- [Migration depuis la V3.6](docs/2026.1/MIGRATION_V3_6_TO_2026_1.md)
- [Performance](docs/2026.1/PERFORMANCE_REPORT.md)
- [Checklist Dante Controller](docs/2026.1/DANTE_CONTROLLER_MANUAL_VALIDATION.md)
- [Limites connues](KNOWN_LIMITATIONS.md)

## Pourquoi DCE existe

DCE est né d’un besoin de terrain : vérifier rapidement une configuration
Dante sans ouvrir successivement toutes les pages de Dante Controller. Il
permet de survoler dans une même application les machines, latences, fréquences
d’échantillonnage, encodages, modes réseau, Preferred Masters, IP et
subscriptions, puis de corriger les valeurs reconnues.

Le second besoin était le renommage sur un réseau déjà patché. Lorsqu’une
machine ou un TX est renommé, DCE met à jour les références XML reconnues afin
de préserver le patch. Enfin, DCE permet de préparer, fusionner, documenter et
contrôler un projet sans connexion au réseau Dante.

Le logiciel d’origine a été écrit manuellement comme un simple éditeur XML.
Les agents actuels ont ensuite permis d’accélérer la sécurisation, les tests,
les installateurs, la documentation et la séparation progressive du moteur.
Les besoins métier et les décisions fonctionnelles restent dirigés par Mamat.

## Ce que 2026.1 apporte

- architecture progressive en Domain, DanteXml, Application et Infrastructure ;
- session de projet centrale et commandes métier transactionnelles ;
- Annuler/Rétablir et historique borné ;
- projet DCE versionné `.dceproj`, distinct du XML Dante ;
- profil XML avec capacités explicites et mode lecture seule ;
- shell Windows avec navigation latérale et inspecteur contextuel ;
- espace Patch unique avec vues tableau, matrice, Easy Patch, sélection et 1:1 ;
- synoptique interactif synchronisé avec la sélection et le patch ;
- Centre de validation filtrable, navigable et exportable ;
- banque de machines format 2 et migration non destructive de la V3.6 ;
- profil local 2026.1 isolé de la V3.6 ;
- index et caches invalidés à chaque mutation XML ;
- corpus XML synthétique élargi et benchmarks 10, 50 et 200 machines.

La version macOS conserve pour le moment l’organisation visuelle de la V3.6,
mais utilise le moteur partagé, le profil 2026.1 et une identité de paquet
distincte.

## Trois formats à ne pas confondre

### XML Dante

Le XML reste le fichier destiné à Dante Controller. DCE modifie le document
d’origine de manière ciblée afin de conserver les nœuds, attributs,
namespaces, valeurs et extensions inconnues. La sauvegarde utilise un fichier
temporaire, une relecture, une validation, une copie de sécurité puis un
remplacement atomique.

### Projet `.dceproj`

Le `.dceproj` est un conteneur de travail propre à DCE. Il peut conserver le
XML, le nom du projet, la disposition, les annotations, le journal, les
références de banque et les ressources DCE. Il ne doit jamais être importé
directement dans Dante Controller : exportez d’abord son XML Dante.

### Banque de machines

Une banque contient des modèles réutilisables et partageables. Une insertion
crée une instance indépendante ; elle ne lie pas le projet au modèle source.
Les identités matérielles, IP, flows et subscriptions ne sont pas recopiés par
défaut. Les banques incluses gardent leur nom historique `3.6` et sont
installées sans remplacer un dossier existant.

## Fonctions principales

- ouverture, analyse, comparaison et fusion de XML ;
- renommage direct ou en série des machines, RX et TX ;
- mise à jour des subscriptions reconnues après renommage d’un TX ;
- patch par tableau, sélection, grille, glissement et série 1:1 ;
- FLIP des rôles RX/TX affichés dans Easy Patch ;
- modification ciblée des latences, formats audio, réseau et Preferred Master ;
- profils et actions globales sur une sélection non verrouillée ;
- suppression et duplication prudente d’un rôle de machine ;
- ajout transactionnel depuis une banque ;
- création expérimentale d’un projet minimal ;
- import/export de labels JSON, CSV, DMT XLSX/ODS, A&H dLive/Avantis et
  Yamaha CL/QL ;
- rapports TXT/PDF, patchbooks et comparaison avant/après ;
- synoptique SVG/PDF avec emplacements et câbles regroupés ;
- récupération automatique et sauvegardes atomiques ;
- interface française/anglaise et thèmes clair/sombre ;
- outil de formation Atomic Bomb, entièrement hors ligne.

## Import et export de labels

DCE échange les labels en JSON/CSV générique, dans les classeurs DMT
XLSX/ODS pour dLive et Avantis, dans les CSV natifs Allen & Heath dLive et
Avantis, ainsi que dans les packages Yamaha CL/QL. Les modèles natifs sont
fournis avec l’application et chaque export crée un nouveau fichier sans
modifier le modèle source.

L’intégration DMT a d’abord été pensée avec
[dLive MIDI Tools](https://github.com/togrupe/dlive-midi-tools). Elle reste un
échange de fichiers hors ligne : DCE ne communique directement ni avec DMT ni
avec une console.

## Sécurité XML

DCE bloque par défaut une sauvegarde lorsque le garde-fou détecte une mutation
technique non autorisée. Les identités et champs sensibles reconnus sont suivis
par identité stable, pas uniquement par nom. Les balises inconnues sont
conservées ; une structure fondamentale inconnue entraîne une édition limitée
ou la lecture seule.

Les tests automatisés couvrent notamment :

- cycle ouverture, sauvegarde sans modification et réouverture ;
- comparaison XML sémantique ;
- namespaces par défaut et balises inconnues ;
- Unicode et ordre de balises ;
- subscriptions locales `.`, sources ou canaux absents ;
- interfaces IPv4 multiples et conservation de l’interface secondaire ;
- renommages, patch, fusion, récupération, duplication et banque ;
- presets synthétiques de 10, 50 et 200 machines avec 64 TX et 64 RX.

Ces tests ne remplacent pas l’import final dans Dante Controller.

## Installer la bêta

Aucune Release GitHub 2026.1 n’est publiée automatiquement. Les paquets de la
branche sont disponibles dans les artefacts des workflows GitHub Actions après
un run réussi.

### Windows 11 x64

Artefact : `DCE-2026.1-Beta-Windows-Installer`

Fichier : `DanteConfigEditor2026_1_Beta_Installer.exe`

L’installateur autonome inclut .NET 8 et les notices FR/EN. Le dossier proposé
est `C:\Program Files\Dante Config Editor 2026.1 Beta\`. L’AppId, les
raccourcis et le profil `%LOCALAPPDATA%\DanteConfigEditor2026.1` sont distincts
de la V3.6. Désinstaller la bêta ne supprime ni XML, ni projet, ni banque, ni
profil V3.6.

### macOS

- `DanteConfigEditor2026_1_Beta_macOS_AppleSilicon.dmg`
- `DanteConfigEditor2026_1_Beta_macOS_Intel.dmg`

Le runtime .NET est inclus. Les bundles sont signés ad hoc mais ne sont pas
notariés par Apple. Au premier lancement, macOS peut donc demander une
ouverture explicite depuis le menu contextuel.

## Construire et tester

Prérequis : SDK .NET 8. Inno Setup 6 est aussi nécessaire pour l’installateur
Windows. Les DMG doivent être construits sur macOS.

```powershell
dotnet restore .\DanteConfigEditorV3.csproj
dotnet test .\tests\DanteConfigEditorV3.Tests\DanteConfigEditorV3.Tests.csproj -c Release
dotnet test .\tests\DanteConfigEditor.Mac.Tests\DanteConfigEditor.Mac.Tests.csproj -c Release
dotnet build .\DanteConfigEditorV3.csproj -c Release
dotnet publish .\DanteConfigEditorV3.csproj -c Release -r win-x64 --self-contained true
.\installer\build_installer.ps1
```

## Limites

- aucune commande en temps réel du réseau Dante ;
- aucune API ou SDK Audinate ;
- création complète de projet encore expérimentale ;
- rôle dupliqué ou issu d’une banque sans identité matérielle réelle ;
- profil XML inconnu limité ou en lecture seule ;
- interface 2026.1 complète actuellement centrée sur Windows ;
- installateur Windows non signé Authenticode ;
- DMG non notariés ;
- validation manuelle Dante Controller requise pour chaque nouvelle structure
  de preset importante.

## Soutenir DCE

Dante Config Editor reste entièrement gratuit et toutes ses fonctions sont
disponibles sans contribution. Les moyens facultatifs de soutenir le projet
sont décrits dans [docs/SUPPORT_DCE.md](docs/SUPPORT_DCE.md).

## Remerciements

Merci à [Tobi / @togrupe](https://github.com/togrupe), auteur de
[dLive MIDI Tools](https://github.com/togrupe/dlive-midi-tools), pour ses
retours, ses idées sur les workflows de patch et sa contribution aux échanges
de labels DMT.

Merci à Charles Bouticourt pour l’idée de la fonction de formation
`Atomic Bomb`.

---

**By Mamat**<br>
*et ses agents*<br>
`-------[]--`
