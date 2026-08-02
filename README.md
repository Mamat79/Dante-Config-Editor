# Dante Config Editor 2026.1

[English documentation](README_EN.md)

Éditeur local et hors ligne de presets XML pour Dante Controller, développé
par Mamat avec l’aide d’agents de développement.

> **Statut : version officielle publiée depuis la branche principale `main`.**
> DCE est un outil tiers non officiel, sans affiliation avec Audinate. Il ne
> pilote aucun réseau Dante et n’utilise ni SDK ni API Audinate. Travaillez sur
> une copie et contrôlez le XML final dans Dante Controller avant exploitation.

La version 2026.1 et ses XML modifiés ont été importés avec succès dans Dante
Controller par le mainteneur. Les tests structurels et sémantiques complètent
ces essais réels. Un contrôle du fichier final reste recommandé pour chaque
nouvelle structure de preset avant exploitation.

## Documentation

- [Notice visuelle complète 2026.1.1 - français (MKV, 11 min 12 s)](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.1.1/dce-2026-1-guide-visuel-fr.mkv)
  La piste française est sélectionnable et peut être désactivée dans le lecteur.
- [Vidéo de présentation 2026.1 - français](docs/media/dce-2026-1-presentation-fr.mp4)
- [2026.1 presentation video - English](docs/media/dce-2026-1-presentation-en.mp4)
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
- navigation latérale et inspecteur contextuel cohérents sur Windows et macOS ;
- espace Patch unique avec Matrice, Easy patch et Liste RX vers TX ;
- synoptique interactif synchronisé avec la sélection et le patch ;
- Centre de validation filtrable, navigable et exportable ;
- banque de machines format 2 et migration non destructive de la V3.6 ;
- profil local 2026.1 isolé de la V3.6 ;
- index et caches invalidés à chaque mutation XML ;
- corpus XML synthétique élargi et benchmarks 10, 50 et 200 machines.

La version macOS suit désormais la même organisation fonctionnelle que
Windows, avec les mêmes parcours principaux, le moteur partagé, le profil
2026.1 et une identité de paquet distincte. Quelques contrôles gardent le rendu
natif de chaque plateforme.

## Trois formats à ne pas confondre

### XML Dante

Le XML reste le fichier destiné à Dante Controller. DCE modifie le document
d’origine de manière ciblée afin de conserver les nœuds, attributs,
namespaces, valeurs et extensions inconnues. La sauvegarde utilise un fichier
temporaire, une relecture, une validation, une copie de sécurité puis un
remplacement sécurisé de la destination.

### Projet `.dceproj`

Le `.dceproj` est un conteneur de travail propre à DCE. Il peut conserver le
XML, le nom du projet, la disposition, les annotations, le journal, les
références de banque et les ressources DCE. Il ne doit jamais être importé
directement dans Dante Controller : exportez d’abord son XML Dante.

### Banque de machines

Une banque contient des modèles réutilisables et partageables. Une insertion
crée une instance indépendante ; elle ne lie pas le projet au modèle source.
Les identités matérielles, IP, flows et subscriptions ne sont pas recopiés par
défaut. Les banques `DCE Generic Roles 2026.1` et
`DCE Community Devices 2026.1` sont intégrées à l'application. Elles
fournissent 45 modèles, dont 43 modèles communautaires illustrés et assainis.
La fenêtre affiche désormais en une seule liste dédupliquée la banque
personnelle et les banques fournies. Le sélecteur permet aussi d'isoler une
banque ; la colonne `Banque` indique l'origine de chaque modèle. Les modèles
fournis restent en lecture seule et peuvent être dupliqués dans la banque
personnelle. Les banques officielles sont gérées dans
`Documents/Dante Config Editor/Included Machine Banks`. Le bouton
`Mettre à jour` consulte GitHub, vérifie le SHA-256, sauvegarde l'ancienne copie
et installe la nouvelle sans jamais remplacer la banque personnelle.
Lors de l'ajout, la quantité vaut `1` par défaut et peut atteindre `100`. DCE
affiche les noms qui seront créés (`Nom`, `Nom-2`, `Nom-3`, etc.), valide tout le
lot avant de modifier le XML, puis laisse la banque ouverte pour poursuivre les
ajouts.

Au démarrage, DCE vérifie aussi silencieusement si une nouvelle Release existe.
Si une version plus récente est disponible, il propose de télécharger puis de
lancer l'installateur vérifié. La vérification reste accessible manuellement
depuis `Aide > Rechercher les mises à jour`.

### Fusion de deux XML et identité des rôles

`Ajouter un XML au projet` conserve le premier XML comme base et importe les
rôles compatibles du second. DCE compare à la fois les noms et la paire
technique `device_id` / `process_id`.

- **Importer uniques seulement** réutilise le rôle déjà présent lorsque la même
  identité technique est rencontrée. Les subscriptions importées sont
  redirigées vers son nom courant.
- **Renommer automatiquement ou manuellement** conserve un second rôle
  indépendant. DCE le rend générique en retirant l’identité matérielle,
  l’interface réseau, les flows multicast et le Preferred Master provenant de
  l’autre projet.
- DCE ne fabrique jamais de faux `device_id`. Dante Controller peut ensuite
  affecter ce rôle à l’appareil d’origine ou à un autre appareil compatible.

Cette distinction permet de fusionner deux installations sans produire deux
rôles portant la même identité matérielle et sans perdre les références de
patch reconnues.

## Fonctions principales

- ouverture, analyse, comparaison et fusion de XML ;
- renommage direct ou en série des machines, RX et TX ;
- mise à jour des subscriptions reconnues après renommage d’un TX ;
- patch par tableau, sélection, grille, glissement et série 1:1 ;
- navigation croisée dans la matrice : retrouver la source d’un RX ou choisir
  l’une des destinations d’un TX ;
- matrice détachable dans une grande fenêtre conservant RX, TX, FLIP, 1:1 et
  zoom ;
- FLIP des rôles RX/TX affichés dans Easy Patch ;
- modification ciblée des latences, formats audio, réseau et Preferred Master ;
- profils et actions globales sur une sélection non verrouillée ;
- suppression et duplication prudente d’un rôle de machine ;
- ajout transactionnel depuis une banque ;
- création hors ligne d’un projet minimal au format XML 3.0.0 ;
- import/export de labels JSON, CSV, DMT XLSX/ODS, A&H dLive/Avantis et
  Yamaha CL/QL ;
- rapports TXT/PDF, patchbooks et comparaison avant/après ;
- synoptique SVG/PDF avec emplacements et câbles regroupés ;
- récupération automatique et sauvegardes sécurisées ;
- interface française/anglaise et thèmes clair/sombre ;
- premier lancement en thème clair, puis restauration du dernier thème et de
  la dernière langue choisis ;
- menu supérieur standard pour retrouver rapidement les commandes Fichier,
  Édition, Machines, Affichage, Outils et Aide ;
- outil de formation Atomic Bomb entièrement hors ligne, avec clé de sécurité,
  ouverture automatique du capot, puis ARM, LOCK et FIRE.

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

Pour les réglages dépendant du matériel, la présence de la balise dans le rôle
chargé constitue la preuve minimale de capacité. DCE ne crée donc pas
`redundancy`, `preferred_master`, `samplerate`, `encoding`,
`unicast_latency` ou `ipv4_address` lorsqu'ils sont absents. La commande est
désactivée ou refusée avec une explication précise ; une action globale ignore
les machines non compatibles et indique combien ont été écartées.

Les tests automatisés couvrent notamment :

- cycle ouverture, sauvegarde sans modification et réouverture ;
- comparaison XML sémantique ;
- namespaces par défaut et balises inconnues ;
- Unicode et ordre de balises ;
- subscriptions locales `.`, sources ou canaux absents ;
- interfaces IPv4 multiples et conservation de l’interface secondaire ;
- refus de créer des réglages techniques absents et contrôle d'un corpus local
  de 11 XML en lecture seule ;
- renommages, patch, fusion, récupération, duplication et banque ;
- presets synthétiques de 10, 50 et 200 machines avec 64 TX et 64 RX.

Ces tests ne remplacent pas l’import final dans Dante Controller.

## Installer DCE 2026.1.1

La [Release DCE 2026.1.1](https://github.com/Mamat79/Dante-Config-Editor/releases/tag/v2026.1.1)
regroupe les installateurs Windows et macOS, les notices, les vidéos et les
banques fournies. Les workflows GitHub Actions conservent également leurs
artefacts après un run réussi.

### Windows 11 x64

Artefact : `DCE-2026.1.1-Windows-Installer`

Fichier : `DanteConfigEditor2026_1_1_Installer.exe`

L’installateur autonome inclut .NET 8 et les notices FR/EN. Le dossier proposé
est `C:\Program Files\Dante Config Editor 2026.1\`. L’AppId, les
raccourcis et le profil `%LOCALAPPDATA%\DanteConfigEditor2026.1` sont distincts
de la V3.6. Désinstaller DCE 2026.1 ne supprime ni XML, ni projet, ni banque, ni
profil V3.6.

### macOS

- `DanteConfigEditor2026_1_1_macOS_AppleSilicon.dmg`
- `DanteConfigEditor2026_1_1_macOS_Intel.dmg`

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
- création de projet limitée au profil XML 3.0.0 actuellement pris en charge ;
- rôle dupliqué ou issu d’une banque sans identité matérielle réelle ;
- profil XML inconnu limité ou en lecture seule ;
- légères différences de rendu natif entre Windows et macOS ;
- installateur Windows non signé Authenticode ;
- DMG non notariés ;
- contrôle dans Dante Controller recommandé pour chaque nouvelle structure de
  preset importante.

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
