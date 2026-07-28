# Baseline Dante Config Editor 2026.1

Date de mesure : 27 juillet 2026  
Branche de travail : `2026.1`  
Commit de départ : `25a1e7cc0568b86a56bdf039ecce060c8eeea1ec`  
Branche de départ : `main`, promue depuis la V3.6 stabilisée  
Tag stable présent dans l'historique : `v3.6.1`  
Version applicative détectée : `3.6.1`

## Portée

Cette baseline précède toute modification propre à la version 2026.1. Elle
sert de référence fonctionnelle, structurelle et mesurée pour les extractions
progressives demandées. La branche `2026.1` et `main` pointaient toutes deux
sur le commit de départ lors du relevé. Aucun changement n'était présent dans
le répertoire de travail.

Toute ancienne mention de « V4 » dans les documents de conception doit être
comprise comme « 2026.1 ». La version de travail prévue est
`2026.1.0-beta.1`; elle ne doit ni remplacer la V3.6 installée ni être fusionnée
dans `main` sans validation explicite.

## Environnement

- Windows `10.0.26200`, architecture `win-x64`
- .NET SDK `8.0.423`
- MSBuild `17.11.48`
- runtime .NET `8.0.29`
- Inno Setup `6.7.3`
- WPF sur Windows
- Avalonia `11.3.18` pour l'interface macOS
- Avalonia DataGrid `11.3.13`
- xUnit `2.9.3`

L'audit NuGet effectué après restauration ne signale aucun package vulnérable
pour l'application Windows ni pour le projet macOS avec les sources actuelles.

## Commandes et résultats

| Étape | Commande | Résultat | Temps mural |
|---|---|---:|---:|
| Restauration Windows | `dotnet restore .\DanteConfigEditorV3.csproj` | réussite | 1 012 ms |
| Tests Core/XML/contrats | `dotnet test .\tests\DanteConfigEditorV3.Tests\DanteConfigEditorV3.Tests.csproj -c Release` | 283/283 | 7 986 ms |
| Tests macOS headless | `dotnet test .\tests\DanteConfigEditor.Mac.Tests\DanteConfigEditor.Mac.Tests.csproj -c Release` | 20/20 | 39 261 ms |
| Build Windows | `dotnet build .\DanteConfigEditorV3.csproj -c Release --no-restore` | réussite, 0 warning | 4 252 ms |
| Build Core/macOS | `dotnet build .\src\DanteConfigEditor.Mac\DanteConfigEditor.Mac.csproj -c Release --no-restore` | réussite, 0 warning | 938 ms |
| Publication Windows autonome | `dotnet publish ... -r win-x64 --self-contained true -p:PublishSingleFile=true` | réussite | 15 650 ms |
| Installateur Windows | `.\installer\build_installer.ps1` | réussite | 35 637 ms |
| Benchmarks synthétiques | `dotnet run --project .\benchmarks\DanteConfigEditorV3.Benchmarks ...` | réussite | 8 067 ms |

La publication autonome produit un exécutable de `72 295 061` octets.
L'exécutable a été lancé pendant trois secondes : le processus est resté actif,
répondant, avec `ProductVersion=3.6.1` et `FileVersion=3.6.1.0`.

L'installateur de baseline mesure `69 387 586` octets et possède l'empreinte
SHA-256 suivante :

```text
fd59cf3fba251108e23417240218ef52b7ceb58e7a8da90dcb03a6b49a66a69e
```

Cette empreinte décrit uniquement la reconstruction locale du 27 juillet 2026.
Elle ne remplace pas l'empreinte d'une Release GitHub existante.

## État de la CI au départ

Les trois workflows exécutés sur le commit de départ dans `main` sont réussis :

- Windows CI :
  <https://github.com/Mamat79/Dante-Config-Editor/actions/runs/30278413211>
- macOS CI :
  <https://github.com/Mamat79/Dante-Config-Editor/actions/runs/30278414511>
- Machine-bank audit :
  <https://github.com/Mamat79/Dante-Config-Editor/actions/runs/30278413340>

La branche `2026.1` n'avait encore déclenché aucun workflow propre au moment du
relevé, car elle ne contenait aucun commit différent de `main`.

## Architecture initiale

| Élément | Responsabilité actuelle |
|---|---|
| `DanteConfigEditorV3.csproj` | application Windows WPF, compile directement `Models` et `Services` |
| `src/DanteConfigEditor.Core` | compile les mêmes `Models` et `Services` pour les partager avec macOS |
| `src/DanteConfigEditor.Mac` | interface Avalonia, hors refonte UI 2026.1 |
| `tests/DanteConfigEditorV3.Tests` | moteur XML, persistance, banque, patch, contrats UI et installateur |
| `tests/DanteConfigEditor.Mac.Tests` | contrats Avalonia headless et performances de matrice |
| `benchmarks` | presets synthétiques 10, 50 et 200 machines |
| `tools/DanteConfigEditor.ValidationPack` | scénarios non destructifs de validation manuelle |

Les principaux fichiers encore concentrés sont :

| Fichier | Lignes | Risque |
|---|---:|---|
| `MainWindow.xaml.cs` | 4 408 | orchestration et état Windows fortement couplés |
| `PatchWorkspaceView.xaml.cs` | 2 689 | édition, patch, navigation et rendu réunis |
| `src/DanteConfigEditor.Mac/MainWindow.axaml.cs` | 2 288 | orchestration macOS regroupée |
| `MainWindow.xaml` | 1 918 | surface WPF monolithique |
| `Models/DanteProject.cs` | 1 750 | chargement et mutations XML encore concentrés |
| `Services/SynopticExportService.cs` | 1 022 | calcul de graphe et export réunis |
| `Services/LocalizationService.cs` | 885 | dictionnaire bilingue monolithique |
| `Services/MachineBankRepository.cs` | 734 | persistance, images et transactions réunies |
| `Services/DanteXmlChangeGuardService.cs` | 672 | garde-fou complexe et sensible |

La recherche statique ne trouve aucune utilisation directe de `XDocument`,
`XElement`, `XAttribute` ou `XNamespace` dans les fichiers code-behind WPF et
Avalonia. La séparation reste néanmoins incomplète : Windows compile encore le
moteur dans son propre assemblage et les fenêtres pilotent directement de
nombreux services et mutations de `DanteProject`.

## Inventaire fonctionnel de référence

La V3.6 de départ fournit notamment :

- ouverture et sauvegarde atomique de XML Dante hors ligne ;
- sauvegardes, récupération et comparaison avant/après ;
- garde-fou des chemins XML et conservation des zones inconnues ;
- renommage unitaire et en série des machines, TX et RX ;
- paramètres réseau et audio, Preferred Master et actions groupées ;
- suppression, duplication et création expérimentale de rôles génériques ;
- fusion de XML et résolution des doublons ;
- banque de machines versionnée, partageable et distribuable ;
- patch classique, matrice, Easy Patch, plages et Flip ;
- synoptique interactif avec exports SVG et PDF ;
- imports et exports de labels génériques, DMT, Allen & Heath et Yamaha ;
- rapports, patchbooks, validation, filtres et recherche ;
- français, anglais, thèmes clair et sombre ;
- Atomic Bomb et outils de formation ;
- applications Windows et macOS, installateurs et notices.

Cet inventaire constitue le filet de sécurité fonctionnel. Une extraction
2026.1 ne doit pas supprimer silencieusement l'un de ces parcours.

## Mesures de performance initiales

Presets synthétiques, 64 TX et 64 RX par machine, médiane de trois passages :

| Machines | Chargement | Modification groupée | Garde-fou | Sauvegarde | Allocation modification |
|---:|---:|---:|---:|---:|---:|
| 10 | 37,800 ms | 40,434 ms | 7,357 ms | 81,775 ms | 20,334 Mio |
| 50 | 77,437 ms | 175,538 ms | 12,343 ms | 232,787 ms | 98,321 Mio |
| 200 | 260,517 ms | 304,284 ms | 50,372 ms | 495,692 ms | 390,759 Mio |

Le coût mémoire des modifications groupées reste le principal point
d'amélioration. La V3.6 utilise encore des copies complètes du document pour
certaines opérations et pour l'annulation.

## Risques et limites connus

- La V3.6 de départ a été importée et testée avec succès dans Dante Controller
  par le mainteneur. Cette baseline automatisée n'a pas reproduit elle-même ce
  test et les futures sorties 2026.1 devront être validées à nouveau.
- La création d'un projet complet et l'ajout d'un rôle générique restent
  expérimentaux tant qu'un import réel n'a pas été consigné.
- Les extensions constructeur inconnues sont conservées, mais leur signification
  matérielle ne peut pas être déduite hors ligne.
- Les structures de preset incompatibles ne doivent jamais être converties par
  supposition.
- Les grandes fenêtres WPF et Avalonia augmentent le risque de régression lors
  d'une refonte visuelle.
- L'accessibilité réelle à 125 %, 150 %, 200 %, en contraste élevé et avec un
  lecteur d'écran nécessite encore des essais manuels.
- Les tests macOS exécutés sous Windows sont headless ; ils ne remplacent pas un
  essai sur un Mac réel.

## Validations manuelles encore nécessaires

Avant toute publication stable de 2026.1 :

1. importer dans une version identifiée de Dante Controller un XML sauvegardé
   sans modification ;
2. importer des variantes avec renommage, patch, suppression, duplication et
   ajout depuis la banque ;
3. vérifier les identités, références, TX, RX, sample rates, latences, réseau et
   Preferred Master ;
4. contrôler l'interface Windows aux échelles 100 %, 125 %, 150 % et 200 % ;
5. contrôler les thèmes, le clavier, le focus et les textes français/anglais ;
6. tester l'installateur 2026.1 Beta côte à côte avec la V3.6 ;
7. tester le paquet macOS sur Intel et Apple Silicon lorsque disponible.

**Validation manuelle Dante Controller requise.**
