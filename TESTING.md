# Tests et historique DCE

## Validation de sécurité XML et installation du 2026-07-29

Périmètre :

- version conservée : `2026.1.0-beta.1` ;
- branche : `2026.1` ;
- commit de code validé : `02765f4` ;
- capacités techniques déduites uniquement des balises déjà présentes dans
  chaque rôle Dante ;
- matrice Patch compactée et navigation « Voir dans Patch » corrigée ;
- documentation et captures bilingues régénérées.

Résultats locaux depuis une restauration propre :

- restauration application Windows : `1,402 s` ;
- restauration tests Core/Windows : `2,003 s` ;
- restauration tests Avalonia/macOS : `1,284 s` ;
- 409 tests Core/Windows réussis, 0 échec, 0 ignoré ;
- 22 tests Avalonia/macOS sans écran réussis, 0 échec, 0 ignoré ;
- 3 tests d'intégration locale réussis sur 11 XML réels ouverts en lecture
  seule, sans modifier ni versionner les originaux ;
- build Windows Release : 0 avertissement, 0 erreur ;
- build macOS Release : 0 avertissement, 0 erreur ;
- aucun package NuGet vulnérable signalé pour Windows ou macOS ;
- contrôles visuels Windows en français et en anglais, thèmes clair et sombre,
  à `1920 x 1024` et `1536 x 864` ;
- quatre PDF rendus et contrôlés : 56 pages, sans chevauchement, texte tronqué
  ni caractère d'encodage incorrect.

Installateur GitHub réellement téléchargé puis installé :

- fichier : `DanteConfigEditor2026_1_Beta_Installer.exe` ;
- taille : `74 220 239` octets ;
- SHA-256 :
  `5CC6FCC8EEBE77A37AF0EDC8D15BA63E6DA892A19DDBD564563405DD727C6AAC` ;
- installation silencieuse réussie avec le code `0` dans
  `C:\Program Files\Dante Config Editor 2026.1 Beta` ;
- raccourcis Bureau et menu Démarrer présents ;
- entrée de désinstallation unique présente ;
- exécutable installé lancé avec la fixture représentative, répondant et
  déclaré en `2026.1.0-beta.1` ;
- 41 modèles communautaires et 2 rôles génériques installés ;
- banque personnelle inchangée avant/après installation : 77 fichiers,
  1 587 873 octets, aucune entrée ajoutée ou retirée, empreinte d'inventaire
  SHA-256 identique
  `3b64b07d2cb8ee379e8beaec5df56d13b00707a4a2c89ad674423b7778db313e`.

GitHub Actions au commit `e54b363` :

- [Windows CI, exécution 30407812954](https://github.com/Mamat79/Dante-Config-Editor/actions/runs/30407812954) :
  succès, 409 tests et installateur produit ;
- [macOS CI, exécution 30407812941](https://github.com/Mamat79/Dante-Config-Editor/actions/runs/30407812941) :
  succès, 409 tests Core, 22 tests Avalonia et deux DMG produits ;
- DMG Apple Silicon : `63 445 544` octets, SHA-256
  `01bb2a05bd1e136ee1726fec2727c6342ca69b3b8246728df98a4038f3ef6a7a` ;
- DMG Intel : `65 251 577` octets, SHA-256
  `863a21eba92e6ce7d603c4c540a5d20a798abfda56cb624862ebc6a1a121ea76`.

Les huit fichiers remplacés dans la prérelease `v2026.1` ont été retéléchargés
depuis leur URL publique puis comparés à leurs sources : huit correspondances
SHA-256 sur huit. Les vidéos, sous-titres, Quick Start et banques déjà publiés
sont restés inchangés.

## Validation de maintenance 2026.1 Beta du 2026-07-28

Périmètre :

- version conservée : `2026.1.0-beta.1` ;
- branche : `2026.1` ;
- réorganisation ergonomique de Machines, de la banque et des panneaux ;
- simplification visuelle du pupitre Atomic Bomb ;
- documentation bilingue et nettoyage des médias historiques non référencés.

Résultats locaux :

- 397 tests Core/Windows réussis, 0 échec, 0 ignoré ;
- 22 tests Avalonia/macOS sans écran réussis, 0 échec, 0 ignoré ;
- build Windows Release : 0 avertissement, 0 erreur ;
- publication Windows autonome `win-x64` réussie et lancement réel répondant ;
- aucun package NuGet vulnérable signalé pour Windows ou macOS ;
- quatre PDF rendus et contrôlés visuellement : 56 pages au total, sans
  chevauchement ni texte tronqué ;
- extraction du texte des quatre PDF sans caractère de remplacement, `Ø`
  parasite ni séquence d'encodage UTF-8 incorrecte ;
- captures Windows contrôlées en français et en anglais, thèmes clair et
  sombre, y compris en largeur réduite ;
- parité macOS contrôlée par compilation et tests de contrat ; aucun Mac
  physique n'était disponible pour une validation visuelle native.

Installateur local réellement installé :

- fichier : `DanteConfigEditor2026_1_Beta_Installer.exe` ;
- taille : `74 240 406` octets ;
- SHA-256 :
  `FA743F5283CD9977A48E65EF45ABED56E0382DEA5FC5D2BBDC24849322FBE48A` ;
- installation silencieuse réussie dans
  `C:\Program Files\Dante Config Editor 2026.1 Beta` ;
- raccourcis Bureau et menu Démarrer présents ;
- exécutable installé lancé, répondant et déclaré en
  `2026.1.0-beta.1` ;
- 41 modèles communautaires et 2 rôles génériques installés ;
- banque personnelle inchangée avant/après installation : 77 fichiers,
  1 587 873 octets, empreinte d'inventaire SHA-256
  `36103497188849D75FA9D053FFA79DD27874C68C943EB424505DA402B0200C2F`.

## Validation finale des paquets 2026.1 Beta du 2026-07-27

Périmètre :

- version : `2026.1.0-beta.1` ;
- branche : `2026.1` ;
- base V3.6 : `25a1e7c` ;
- commit de code et de paquets validé : `7a50b3c` ;
- installation et profil distincts de la V3.6 ;
- aucune fusion dans `main` et aucune Release GitHub dans ce cycle.

Résultats locaux :

- restore Windows, tests Core et tests Mac réussi ;
- 364 tests Core/Windows réussis, 0 échec ;
- 20 tests Avalonia/macOS sans écran réussis, 0 échec ;
- builds Windows et macOS Release : 0 warning, 0 erreur ;
- publications autonomes `win-x64`, `osx-arm64` et `osx-x64` réussies ;
- aucun package NuGet vulnérable signalé ;
- installateur Windows construit avec Inno Setup 6.7.3 ;
- installation silencieuse, lancement et deux passages de mise à niveau
  réussis ;
- une seule inscription bêta après mise à niveau et inscription V3.6 stable
  conservée ;
- vues principales contrôlées en français/anglais et thèmes sombre/clair avec
  une fixture synthétique ;
- patch immédiat, avertissement de remplacement et annulation vérifiés sans
  ouvrir de XML de production.

Installateur local réellement installé :

- fichier : `DanteConfigEditor2026_1_Beta_Installer.exe` ;
- taille : `69 759 045` octets ;
- SHA-256 :
  `2ad5fa4ab53b1621738c1c30841e5f1a0af4f89eb928e9f41482f305d8404b90` ;
- signature Authenticode : absente.

GitHub Actions au commit `7a50b3c` :

- [Windows CI](https://github.com/Mamat79/Dante-Config-Editor/actions/runs/30298793379) :
  364 tests réussis et installateur produit ;
- [macOS CI](https://github.com/Mamat79/Dante-Config-Editor/actions/runs/30298792909) :
  364 tests Core et 20 tests Avalonia réussis, deux DMG produits ;
- installateur CI : `69 767 745` octets, SHA-256
  `72b84b0e6effb03d534d2a68b94ad5119231c7a0237077081e0fe56ef6fa044b` ;
- DMG Apple Silicon : `52 850 639` octets, SHA-256
  `b1774f3eb710853b289242b3a090544438ff1b5d2ba5cfdd51fb03f9223cd206` ;
- DMG Intel : `54 282 491` octets, SHA-256
  `0e4c9b52930ac8191b77270d5cab487cf70e970b326c0c5a927410806c9097a6`.

La différence d'empreinte entre l'installateur local et celui de CI provient
de deux constructions distinctes ; chaque artefact correspond à son propre
fichier `.sha256`. Le paquet local est celui qui a été installé et testé sur
ce PC.

La V3.6 a été importée avec succès dans Dante Controller par le mainteneur.
Le commit 2026.1 ci-dessus a été validé par tests XML structurels et
sémantiques, mais n'a pas fait l'objet d'un nouvel import manuel Dante
Controller consigné pendant ce cycle.

## Validation performance et corpus 2026.1 du 2026-07-27

Périmètre :

- branche : `2026.1` ;
- référence avant optimisation : `79d02b3` ;
- moteur optimisé : `4971c0d` ;
- corpus exclusivement synthétique et anonymisé ;
- aucun XML de production ajouté au dépôt ;
- aucun import Dante Controller exécuté avec cette révision précise.

Résultats :

- 364 tests Core/Windows réussis, 0 échec, 0 ignoré ;
- 20 tests Avalonia/macOS sans écran réussis, 0 échec, 0 ignoré ;
- build Windows Release réussi, 0 warning, 0 erreur ;
- sept fixtures passent ouverture, validation, sauvegarde sans modification,
  comparaison XML sémantique et réouverture ;
- presets synthétiques 10, 50 et 200 machines, avec 64 TX et 64 RX par
  machine, exécutés par le banc ;
- à 200 machines, l'édition groupée passe de `317,410 ms` à `38,092 ms` et
  ses allocations de `390,759 Mio` à `29,358 Mio` ;
- à 200 machines, la validation passe de `86,948 ms` à `36,062 ms`.

Rapports :

- `docs/2026.1/PERFORMANCE_REPORT.md` ;
- `docs/2026.1/COMPATIBILITY_CORPUS.md` ;
- `docs/2026.1/DANTE_CONTROLLER_MANUAL_VALIDATION.md` ;
- `COMPATIBILITY_MATRIX.md`.

Commandes :

```powershell
dotnet test .\tests\DanteConfigEditorV3.Tests\DanteConfigEditorV3.Tests.csproj -c Release
dotnet test .\tests\DanteConfigEditor.Mac.Tests\DanteConfigEditor.Mac.Tests.csproj -c Release
dotnet build .\DanteConfigEditorV3.csproj -c Release
dotnet run --project .\benchmarks\DanteConfigEditorV3.Benchmarks\DanteConfigEditorV3.Benchmarks.csproj -c Release -- --phase 2026.1 --commit 4971c0d --output .\benchmarks\results\2026.1.json
```

Le corpus réel facultatif est lancé uniquement depuis des copies locales :

```powershell
$env:DANTE_REAL_XML_ROOT = 'D:\chemin\vers\copies-anonymisees'
$env:DANTE_REAL_XML_REQUIRED = '1'
dotnet test .\tests\DanteConfigEditorV3.Tests\DanteConfigEditorV3.Tests.csproj -c Release --filter Category=LocalIntegration
```

La V3.6 a été validée par le mainteneur dans Dante Controller. Pour la sortie
2026.1, la validation structurelle automatisée est acquise mais un nouvel
import manuel reste à consigner avec la checklist dédiée.

## Validation du soutien facultatif V3.6 du 2026-07-26

Périmètre :

- branche : `v3.6` ;
- affichage local du QR PayPal et lien PayPal.Me validé, sans paiement intégré ;
- aucun SDK de paiement, formulaire intégré, WebView, télémétrie ou connexion réseau au démarrage ;
- aucun XML de production ni code de modification Dante changé.

Résultats :

- 283 tests Core/Windows réussis, 0 échec, 0 ignoré ;
- 20 tests Avalonia/macOS sans écran réussis, 0 échec, 0 ignoré ;
- builds Windows et macOS Release réussis, 0 warning, 0 erreur ;
- ressource QR identique sur Windows et macOS, avec ouverture explicite de PayPal.Me dans le navigateur système ;
- rappel local testé : absent au premier lancement, visible au vingtième, report de 20 lancements, désactivation définitive, récupération après fichier de préférence corrompu et absence pendant les tests ;
- GitHub Funding vérifié avec le seul lien PayPal.Me approuvé ;
- fenêtre `Soutenir DCE` vérifiée dans l'application Windows installée en français et en anglais par l'arbre d'accessibilité, avec noms et descriptions de boutons ;
- notices complètes FR/EN régénérées en 15 pages et dernière page rendue visuellement sans texte tronqué ni caractère accentué incorrect ;
- installateur final : `67 812 681` octets, SHA-256 `C16EC7AD735EB7C3A8E105505CAD8A9414847AFFD0CDC72F14631D822295E39D` ;
- installation finale réussie avec code 0 ; exécutable V3.6, aide au soutien, raccourci Menu Démarrer et raccourci Bureau présents.

Limites de preuve :

- aucun achat ni paiement n'a été effectué ;
- aucun Mac physique n'a été utilisé ; les DMG restent à construire et contrôler sur le runner GitHub macOS ;
- la capture graphique automatisée de la fenêtre WPF est restée blanche, mais le contenu, les langues et l'accessibilité ont été vérifiés par UI Automation ;
- aucun import Dante Controller n'était requis ni réalisé, cette évolution ne modifiant pas le XML.

## Validation locale de la branche V3.6 du 2026-07-25

Périmètre :

- branche : `v3.6`, issue du commit V3.5 `09985050aab242682bab82b792276c68ef3693fb` ;
- système : Windows `10.0.26200`, .NET SDK `8.0.423`, runtime `8.0.29` ;
- Inno Setup `6.7.3` ;
- aucun Mac physique, matériel Dante ni import Dante Controller utilisé.

Résultats :

- 258 tests Core/Windows réussis, 0 échec, 0 ignoré ;
- 16 tests Avalonia/macOS sans écran réussis, 0 échec, 0 ignoré ;
- builds Windows et macOS Release réussis, 0 warning, 0 erreur ;
- publications autonomes `win-x64`, `osx-arm64` et `osx-x64` réussies ;
- audit NuGet : aucun package vulnérable signalé avec les sources consultées ;
- dix XML locaux copiés vers un temporaire, soit 176 machines et 5 004 labels, chargés, validés, sauvegardés, comparés sémantiquement et relus ; originaux inchangés ;
- Quick Start FR/EN : 1 page chacun ; notices complètes FR/EN : 15 pages chacune ;
- extraction de texte et rendu visuel des 32 pages contrôlés sans `Ø` parasite, caractère de remplacement, chevauchement ni troncature visible ;
- installateur `DanteConfigEditorV3_6_Installer.exe` : 67 322 778 octets, version produit `3.6`, version fichier `3.6.0`, SHA-256 `321D40BAC7BF99BD66E988810AC81EB3AEE14B0C4B1B7C6D8F78B31A69AA4DAB`, non signé ;
- installation puis second passage de mise à niveau réussis avec code 0 : une entrée DCE V3.6, un groupe Démarrer, un raccourci Bureau, un désinstalleur et aucune trace de raccourci V3.5 ;
- lancement de l'exécutable autonome et installé : processus répondant, titre `Dante Config Editor V3.6`, version `3.6.0.0`.

Benchmark synthétique Release, médiane de trois exécutions, 64 TX et 64 RX par machine :

| Machines | Chargement | Modification groupée | Garde-fou | Sauvegarde | Allocation sauvegarde |
|---:|---:|---:|---:|---:|---:|
| 10 | 57,4 ms | 109,8 ms | 18,8 ms | 206,5 ms | 22,6 Mio |
| 50 | 188,8 ms | 393,8 ms | 31,9 ms | 386,1 ms | 110,6 Mio |
| 200 | 433,8 ms | 1 081,4 ms | 142,8 ms | 996,9 ms | 440,8 Mio |

Limites de preuve :

- aucun XML V3.6 n'a été importé réellement dans Dante Controller ;
- les publications macOS ont été produites sur Windows, mais les bundles `.app`, signatures ad hoc et DMG doivent être assemblés et vérifiés sur un runner ou un Mac réel ;
- le contrôle visuel automatisé Windows n'a pas reçu l'autorisation de prise de contrôle dans le délai imparti ;
- les échelles 125 %, 150 %, 200 %, le contraste élevé, les lecteurs d'écran et VoiceOver restent à tester manuellement.

## Validation locale de la branche V3.5 du 2026-07-23

Périmètre :

- branche : `v3.5` ;
- base avant optimisation : `174236ab2ce417dfe51c76fcf7237f980c6e2162` ;
- système de test : Windows, .NET 8 ;
- aucun Mac physique ni Dante Controller utilisé pendant cette validation.

Résultats :

- 199 tests cœur et contrats Windows réussis ;
- 16 tests Avalonia/macOS sans écran réussis ;
- builds Windows et macOS Release réussis sans warning ;
- clic 64 x 64 avant/après : `4 231,58 ms` / `70,29 ms` ;
- clic 128 x 128 avant/après : `21 784,06 ms` / `2,87 ms` ;
- 100 clics en attente dans une matrice 128 x 128 : `490,80 ms`, sans reconstruction ni modification du projet principal ;
- suppression d'une machine sauvegardée et relue sans subscription pendante ;
- JSON et CSV DMT 2.14.0-RC1 reproduits depuis les exporteurs du commit `3c34052`, puis importés avec succès ;
- propriétés JSON inconnues, versions CSV mélangées, canaux dupliqués et colonnes XLSX/ODS manquantes refusés.

Les temps sont issus d'une exécution headless locale et ne constituent pas des seuils contractuels. Le détail se trouve dans `V3_5_IMPLEMENTATION_REPORT.md`.

## Validation locale de la V3.2 officielle du 2026-07-21

Source validée avant promotion du tag immuable `v3.2` :

- système local : Windows `10.0.26200`, `win-x64` ;
- .NET SDK `8.0.423` ;
- Inno Setup `6.7.3`.

Résultats locaux :

- 129 tests Core et contrats Windows réussis, 0 échec, 0 ignoré ;
- 9 tests Avalonia sans écran réussis, 0 échec, 0 ignoré ;
- builds WPF et Avalonia Release réussis, 0 warning, 0 erreur ;
- publication autonome Windows `win-x64` réussie et exécutable maintenu actif pendant le contrôle de lancement ;
- aucun package vulnérable signalé pour les applications Windows et macOS avec les sources NuGet consultées le 2026-07-21 ;
- Quick Start FR/EN : 1 page chacun ; notices complètes FR/EN : 10 pages chacune ;
- extraction de texte et rendu Poppler des PDF contrôlés sans caractère `Ø` parasite, caractère Unicode de remplacement, texte coupé ni chevauchement visible.

Échange de labels avec les exemples fournis :

- quatre modèles XLSX DMT lus et exportés dans des copies : Avantis 48 canaux, Avantis Mat 64, dLive 48 et dLive Mat 128 ;
- CSV Allen & Heath lus et exportés dans des copies : Avantis 96 entrées et dLive 122 entrées nommées ;
- packages Yamaha lus et exportés dans des copies : CL5 V4.1 avec 72 entrées et QL5 V4.1 avec 64 entrées ;
- premiers labels relus avec les nouvelles valeurs dans chaque copie ;
- hashes SHA-256 des huit modèles et exports originaux inchangés après les essais ;
- tests automatisés de conservation des autres fichiers Yamaha, dont `DCAName.csv`.

Synoptique :

- tests de regroupement de câbles, routes orthogonales, troncs partagés, identité stable et légende dense réussis ;
- rendu réel contrôlé sur une copie en lecture seule d'un preset de 20 machines et 36 câbles ;
- SVG produit en `1988 x 2090` avec légende sur deux colonnes et sans ajout de donnée au XML Dante.

Installateur Windows local :

- fichier : `dist/DanteConfigEditorV3_2_Installer.exe` ;
- taille : `68 815 173` octets ;
- version produit : `3.2`, version fichier : `3.2.0` ;
- SHA-256 : `E1926527DBDF844C5E272313B70503E135BF58BA637A4C9BA8832049833598AD` ;
- signature Authenticode : absente (`NotSigned`) ;
- installation propre puis mise à niveau de contrôle réussies avec code retour 0 ;
- une seule entrée V3 présente dans `C:\Program Files\Dante Config Editor V3.2\` ;
- désinstalleur stable `unins000.exe`, sans `unins001.exe` après la mise à niveau ;
- ancienne installation et ancien dossier V3.1 absents ;
- raccourcis V3.2 Menu Démarrer et Bureau présents ;
- lancement réel de l'exécutable installé réussi, processus répondant et titre `Dante Config Editor V3.2`.

Limites de cette validation : les DMG doivent être assemblés et vérifiés sur le runner macOS du workflow de Release. Aucun lancement manuel n'a été effectué sur un Mac physique, aucune notarisation Apple et aucun import dans Dante Controller ou test sur matériel Dante réel ne sont revendiqués.

## Validation locale de la V3.1 officielle du 2026-07-21

Source validée avant création du tag `v3.1` :

- branche : `main` ;
- système local : Windows `10.0.26200`, `win-x64` ;
- .NET SDK `8.0.423` ;
- Inno Setup `6.7.3`.

Résultats locaux :

- 114 tests Core et contrats Windows réussis, 0 échec, 0 ignoré ;
- 9 tests Avalonia sans écran réussis, 0 échec, 0 ignoré ;
- builds WPF et Avalonia Release réussis, 0 warning, 0 erreur ;
- publications autonomes `win-x64`, `osx-arm64` et `osx-x64` réussies ;
- aucun package vulnérable signalé pour les applications Windows et macOS avec les sources NuGet consultées le 2026-07-21 ;
- Quick Start FR/EN : 1 page chacun ; notices complètes FR/EN : 10 pages chacune ;
- extraction de texte PDF contrôlée avec `V3.1`, `DMT`, `Atomic Bomb` et `By Mamat et ses agents`, sans caractère `Ø` parasite ni caractère Unicode de remplacement.

Échange de labels DMT :

- JSON et CSV testés avec plusieurs machines, directions TX/RX, Unicode et champs CSV protégés ;
- plages appliquées à plusieurs machines et renommage TX avec mise à jour des subscriptions testés ;
- lecture réussie des cinq modèles locaux DMT 2.13.0 : Avantis, Avantis Mat, dLive, dLive Mat et Mixing Station ;
- export réel dans une copie temporaire de `dLiveChannelList.xlsx` réussi avec adaptation explicite `TrèsLongLabel` vers `TresLong` et `Chœur` vers `Choeur` ;
- hash SHA-256 du modèle DMT source inchangé après l'export ; la copie temporaire a été supprimée après le contrôle.

Installateur Windows :

- fichier : `dist/DanteConfigEditorV3_1_Installer.exe` ;
- taille : `68 782 698` octets ;
- version produit : `3.1`, version fichier : `3.1.0` ;
- SHA-256 : `DFA2634F035C0522961AC7A00CD26476260E22FAF460CD46FFAAB820749D7E1B` ;
- deux passages silencieux successifs réussis avec code retour 0 ;
- une seule installation présente dans `C:\Program Files\Dante Config Editor V3.1\` ;
- ancienne V3.09 supprimée et aucun raccourci V3.07/V3.08/V3.09 restant dans les menus Démarrer ou sur le Bureau ;
- raccourcis V3.1 Menu Démarrer et Bureau présents ;
- lancement réel de l'exécutable installé réussi, titre `Dante Config Editor V3.1` ;
- preset synthétique `representative-preset.xml` chargé : 3 devices, 3 TX, 4 RX et 3 patchs actifs ;
- commandes `Importer des labels` et `Exporter des labels` actives ; fenêtre d'export contrôlée avec choix multi-machine, JSON, CSV, XLSX DMT, aperçu et adaptation DMT explicite ;
- onglet `Atomic Bomb` présent après `Sécurité et journal`, avec bouton dédié et trois confirmations annoncées.

Publication GitHub :

- workflows `Windows CI` `29814000720`, `macOS CI` `29814000620` et `Versioned release` `29814002843` réussis ;
- Release `v3.1` publiée comme `Latest`, sans modification des Releases `v3.08` et `v3.09` ;
- installateur Windows public : `68 791 161` octets, SHA-256 `EA91DCF8BB68A304C7A856E78580D3C6CF2E46C8A238A2FF3DCDF7778676BD90` ;
- l'installateur public a été retéléchargé, comparé à son fichier `.sha256`, puis installé avec succès par-dessus la construction locale ;
- DMG Apple Silicon : `52 218 759` octets, SHA-256 `DA6850D27DB8C3FF4CEAFA5A61225AB234BD4680D794D89C87216AEA2AABEF3C` ;
- DMG Intel : `52 974 183` octets, SHA-256 `89A00AA1D9A88CC1A1E798554971D83FAF72E6D07C6060A3989783237B1BED5A` ;
- le workflow a été rendu sensible au numéro de version des présentations ; les anciennes vidéos V3.09 jointes initialement à V3.1 ont été retirées uniquement de V3.1.

Limites de cette validation : les publications macOS ont été produites depuis Windows, mais les DMG signés ad hoc doivent encore être assemblés et vérifiés par le workflow macOS. Aucun lancement manuel n'a été effectué sur un Mac physique, aucune notarisation Apple et aucun import dans Dante Controller ou test sur matériel Dante réel ne sont revendiqués.

## Validation locale de la V3.09 officielle du 2026-07-20

Source validée avant ajout de ce relevé :

- branche : `main` ;
- commit fonctionnel et documentaire : `30e38cd` ;
- système local : Windows `10.0.26200`, `win-x64` ;
- .NET SDK `8.0.423`, PowerShell `7.6.3`, Git `2.51.0.windows.1` ;
- Inno Setup `6.7.3`.

Résultats locaux :

- 100 tests Core et contrats Windows réussis, 0 échec, 0 ignoré ;
- 9 tests Avalonia sans écran réussis, 0 échec, 0 ignoré ;
- build WPF Release réussi, 0 warning, 0 erreur ;
- rendu réel Windows contrôlé en thème sombre, bouton `Atomic Bomb` rond rouge lisible et sans fond jaune ;
- noms de machines Atomic Bomb uniques vérifiés sur un preset synthétique de 80 machines, sans préfixe `ATOM-` ;
- Quick Start FR/EN : 1 page chacun ; notices complètes FR/EN : 10 pages chacune ;
- extraction de texte PDF contrôlée sans ancienne appellation, caractère `Ø` parasite ni absence d'`Atomic Bomb` ;
- présentations FR/EN : H.264, 1920 x 1080, 30 images/s, 67 secondes ; visuel Atomic Bomb contrôlé dans les deux langues.

Installateur Windows :

- fichier : `dist/DanteConfigEditorV3_09_Installer.exe` ;
- taille : `68 751 519` octets ;
- version produit : `3.09`, version fichier : `3.9.0.0` ;
- SHA-256 : `EC49F279E02533640B11102D600C8377A47C067C5113B0FFF4CBB4178B7F50C8` ;
- remplacement local de V3.08 par V3.09 réussi, puis mise à niveau V3.09 avec le paquet final réussie ;
- une seule entrée V3 reste installée dans `C:\Program Files\Dante Config Editor V3.09\` ;
- ancien registre et ancien dossier V3.08 absents ;
- raccourcis menu Démarrer et Bureau présents ;
- huit documents installés identiques bit à bit aux sources du dépôt ;
- lancement réel de l'exécutable installé réussi, titre `Dante Config Editor V3.09`.

La CI GitHub n'avait pas encore été relancée au moment de ce relevé local. Les DMG V3.09 doivent être produits et vérifiés par le workflow macOS avant publication de la Release. Aucun lancement manuel n'a été effectué sur un Mac physique, aucune notarisation Apple et aucun import dans Dante Controller n'ont été revendiqués.

## Validation de la V3.08 officielle du 2026-07-12

Source validée avant ajout de ce relevé :

- branche : `main` ;
- commit : `552b2e3837b4de9fb2dcb631bce14c16e0fd38a0` ;
- système local : Windows `10.0.26200`, `win-x64` ;
- .NET SDK `8.0.422`, MSBuild `17.11.48` ;
- Inno Setup `6.7.3`.

Résultats locaux :

- 89 tests Core et contrats Windows réussis, 0 échec, 0 ignoré ;
- 8 tests Avalonia sans écran réussis, 0 échec, 0 ignoré ;
- builds WPF et Avalonia Release réussis, 0 warning, 0 erreur ;
- aucun package vulnérable signalé pour les applications Windows et Mac avec les sources NuGet consultées le 2026-07-12 ;
- Quick Start FR/EN : 1 page chacun ; notices complètes FR/EN : 5 pages chacune ;
- rendu Poppler et texte extrait contrôlés sans chevauchement, caractère `Ø` parasite ni caractère Unicode de remplacement.

Installateur Windows officiel :

- fichier : `dist/DanteConfigEditorV3_08_Installer.exe` ;
- taille : `66 858 808` octets ;
- version produit : `3.08`, version fichier : `3.8.0.0` ;
- SHA-256 : `29A699E485933E42CF138B5CDDE3898AAA3C5CA639DEED7533386DCD851C39EE` ;
- signature Authenticode : absente (`NotSigned`) ;
- deux passages d'installation réussis, avec une seule entrée V3.08 et un raccourci Menu Démarrer valide ;
- lancement réel de l'exécutable installé réussi, titre `Dante Config Editor V3.08`, processus répondant ;
- ancienne V3.07 ensuite désinstallée à la demande de l'utilisateur, sans suppression de fichiers XML personnels.

GitHub Actions sur macOS :

- Windows CI `29188564426` : réussi en 1 min 2 s ;
- macOS CI `29188564417` : réussi en 1 min 22 s ;
- `Info.plist` validé, application signée ad hoc, contrôle `codesign` réussi et `hdiutil verify` déclaré `VALID` pour les deux DMG ;
- Apple Silicon : `49 819 194` octets, SHA-256 `763F255EC70C05BD634711F274624D3B468F2BECEC16164739BE58719BC664DD` ;
- Intel : `51 050 377` octets, SHA-256 `E56B86B751CB1377F50A713B83C0D9F1FDC337F82F666BB6211A719305BD5BAF`.

Limites de cette validation : aucun lancement manuel sur un Mac physique, aucune notarisation Apple, aucun import dans Dante Controller et aucun test sur matériel Dante réel. Les deux DMG sont signés ad hoc et peuvent déclencher Gatekeeper au premier lancement.

## Promotion de la V3.08 Beta dans main du 2026-07-12

- ancienne tête de `main` : `903ed94114ee419e4063cbf07e55ff8626006a7a` (V3.07) ;
- tag annoté de sauvegarde : `safety-v3.07-before-v3.08-main-20260712` ;
- promotion par avance rapide, sans conflit ni réécriture d'historique ;
- nouvelle base de `main` avant mise à jour documentaire : `9a17e3ea20191e2543be9dd31a4408f89d2944af` ;
- branche `3.08-beta` et anciennes Releases conservées ;
- 88 tests Windows réussis et build Release sans warning sur `main` ;
- installateur principal reconstruit : `66 861 369` octets, SHA-256 `3893DA578196B3B5A327D51BA222DDD92D3BFA82EFA48D8D306C9F695CDFA09A` ;
- mise à niveau locale V3.08 réussie, notices installées identiques au dépôt ;
- V3.07 locale conservée séparément, sans suppression destructive.

## Validation ergonomique Easy patch du 2026-07-12

Source fonctionnelle testée :

- branche : `3.08-beta` ;
- commit fonctionnel : `cc403f190e12be081c682e8439346a2377382a51` ;
- fixture anonymisée : `representative-preset.xml` ;
- aucune sauvegarde ni modification du fichier XML de la fixture.

Résultats :

- 88 tests Release réussis, 0 échec, 0 ignoré ;
- build WPF Release réussi, 0 warning, 0 erreur ;
- rendu Easy patch contrôlé en thème sombre à `1600 x 820` et `1200 x 650` ;
- rendu Easy patch contrôlé en thème clair à `1600 x 820` ;
- menus des machines RX/TX conservés à l'écran pendant la prévisualisation ;
- listes RX/TX et panneau central dotés de leurs propres ascenseurs lorsque l'espace manque ;
- colonnes `RX cible`, `Source actuelle`, `Nouvelle source` et `Action` lisibles sans ascenseur horizontal de page ;
- commandes `Prévisualiser`, `Appliquer`, `Ajouter au lot`, `Appliquer ces changements` et `Appliquer tout le lot` visibles ;
- les conflits de remplacement restent soumis à un choix explicite.

Paquet Windows contrôlé :

- installateur autonome construit en 43,064 s ;
- fichier : `dist/DanteConfigEditorV3_08_Beta_Installer.exe` ;
- taille : `66 859 156` octets ;
- SHA-256 : `9EE2355D983969167AA51CA002D844AF55BE6D34FAEEA5F9D9F62422B94DBF12` ;
- somme `.sha256` concordante ;
- mise à niveau silencieuse V3.08 réussie avec code retour 0 ;
- V3.07 stable toujours présente dans son dossier séparé ;
- notices FR installées identiques aux PDF du dépôt ;
- exécutable installé démarré avec la fixture : 3 devices, 3 TX, 4 RX et 3 patchs actifs détectés.

Ces contrôles ne constituent pas une preuve d'import dans Dante Controller ni un test sur matériel Dante réel.

## Validation V3.08 Beta Windows du 2026-07-12

Source fonctionnelle testée :

- branche : `3.08-beta` ;
- commit source du paquet : `faa5a8d9ece26e4b7c726121252d3a19f9498736` ;
- système : Windows `10.0.26200`, `win-x64` ;
- .NET SDK `8.0.422`, MSBuild `17.11.48` ;
- Inno Setup `6.7.3` ;
- aucune compilation ni aucun test macOS exécuté pour cette bêta.

| Étape | Temps | Code retour | Résultat |
|---|---:|---:|---|
| Restore application Windows | 0,906 s | 0 | réussi |
| Restore tests Core/contrats Windows | 0,878 s | 0 | réussi |
| Tests Release | 9,938 s | 0 | 86 réussis, 0 échec, 0 ignoré |
| Build Windows Release | 2,921 s | 0 | 0 warning, 0 erreur |
| Restore RID `win-x64` | 1,123 s | 0 | réussi |
| Publish Windows `win-x64` framework-dependent | 2,486 s | 0 | réussi |
| Installateur Windows autonome | 39,980 s | 0 | réussi, 0 erreur Inno Setup |
| Installation puis mise à niveau de contrôle | 9,390 s | 0 | deux passages réussis |

Installateur validé :

- fichier : `dist/DanteConfigEditorV3_08_Beta_Installer.exe` ;
- version : `3.08-beta`, version fichier `3.8.0.0` ;
- taille : `66 858 195` octets ;
- SHA-256 : `BEDAC3F0A28D1BFFFED7F26E3700C98C68FEC6C1DCAD438ED5D0D8A59A2BCBB2` ;
- somme `.sha256` concordante ;
- signature Authenticode : absente (`NotSigned`).

Installation vérifiée :

- dossier : `C:\Program Files\Dante Config Editor V3.08\` ;
- une seule entrée V3.08 Beta après deux passages de l'installateur ;
- une entrée V3.07 stable conservée séparément ;
- deux installations Dante Config Editor V3 au total ;
- raccourci Menu Démarrer et quatre notices PDF FR/EN présents ;
- démarrage réel de l'exécutable installé réussi.

Contrôles fonctionnels et visuels réalisés sur les fixtures anonymisées :

- onglets principaux `Patch` et `Easy patch` présents et lisibles en thème sombre ;
- onglet actif Easy patch bleu avec texte lisible ;
- machines et canaux RX à gauche, machines et canaux TX à droite ;
- menus RX/TX et flèches précédent/suivant visibles, accessibles et fonctionnels ;
- passage RX de `DEVICE-A` à `DEVICE-B` avec mise à jour immédiate des canaux ;
- sélecteur de machine visible en haut de `Détail machine` ;
- passage de `DEVICE-A` à `DEVICE-D` sans changement en attente ;
- alerte appliquer/abandonner/annuler affichée après une modification non appliquée ;
- test annulé puis fenêtre fermée sans appliquer : le projet est resté `Non modifié` ;
- contrôle du choix de résolution des conflits et du texte sombre sur fond clair ;
- aucune sauvegarde de fixture ni modification d'un XML de test.

Documentation vérifiée :

- Quick Start FR/EN : 1 page chacun ;
- notice complète FR/EN : 5 pages chacune ;
- `Easy patch`, `V3.08` et `By Mamat` présents dans les quatre PDF ;
- aucun caractère `Ø` parasite ni caractère de remplacement Unicode ;
- rendu Poppler contrôlé page par page, sans zone noire réelle, texte coupé ou coin non blanc.

Le scan `dotnet list package --vulnerable --include-transitive` ne signale aucun package vulnérable dans l'application Windows, le Core, les tests Windows, ValidationPack et Benchmarks avec les sources NuGet du 2026-07-12.

CI distante du commit de validation `4b648e756fe78bfd7b7c5d4e5031ed43135266a9` :

- Windows CI, run `29171972920` : réussi en 1 min 3 s ; restore, 86 tests, build, publish et artifacts inclus ;
- aucun workflow macOS déclenché pour la branche `3.08-beta`.

Aucun import dans Dante Controller et aucun test sur matériel Dante réel n'ont été exécutés pendant cette validation.

## Validation V3.07 officielle

Source fonctionnelle testée :

- branche : `main` ;
- commit : `bdb1de9ae7b6b80342e18d1f2172d83e74b7bc43` ;
- système : Windows `10.0.26200`, `win-x64` ;
- .NET SDK `8.0.422`, MSBuild `17.11.48` ;
- Inno Setup `6.7.3`.

| Étape | Temps | Code retour | Résultat |
|---|---:|---:|---|
| Restore application Windows | 1,361 s | 0 | réussi |
| Restore tests Core | 1,227 s | 0 | réussi |
| Restore tests UI Mac | 1,288 s | 0 | réussi |
| Tests Core Release | 13,112 s | 0 | 67 réussis, 0 échec, 0 ignoré |
| Tests UI Mac Release | 17,712 s | 0 | 7 réussis, 0 échec, 0 ignoré |
| Build Windows Release | 3,151 s | 0 | 0 warning, 0 erreur |
| Build interface Mac Release | 1,673 s | 0 | 0 warning, 0 erreur |
| Publish Windows `win-x64` framework-dependent | 4,366 s | 0 | réussi |
| Publish Mac autonome `osx-arm64` | 8,217 s | 0 | réussi |
| Publish Mac autonome `osx-x64` | 7,953 s | 0 | réussi |
| Installateur Windows autonome | 71,965 s | 0 | réussi, 0 erreur Inno Setup |
| Installation puis mise à niveau de contrôle | 18,732 s | 0 | deux passages, une seule installation conservée |

Installateur validé :

- fichier : `dist/DanteConfigEditorV3_Installer.exe` ;
- version : `3.07` ;
- taille : `66 840 678` octets ;
- SHA-256 : `9F052C09391A274A044B44336C86893967FA64F10C8867266353A3E0AA352CCF` ;
- signature Authenticode : absente (`NotSigned`).

Installation vérifiée :

- dossier : `C:\Program Files\Dante Config Editor V3\` ;
- entrée de désinstallation : `Dante Config Editor V3.07 version 3.07` ;
- version de fichier : `3.7.0.0` ;
- SHA-256 de l'exécutable installé : `13B8FD4F6C2CF489C9A05FC78F885E36EB5C9F9F3C0867E12A516BA9C164A324` ;
- deux passages de l'installateur conservent une seule installation V3 ;
- démarrage réel réussi, titre `Dante Config Editor V3.07` ;
- fixture anonymisée ouverte : 3 devices, 3 TX, 4 RX, 3 patchs actifs ;
- modes Simple et Expert contrôlés : le mode Expert affiche bien les colonnes techniques supplémentaires ;
- fermeture propre après le contrôle.

Le scan `dotnet list package --vulnerable --include-transitive`, rejoué après restauration sur les sept projets du dépôt, ne signale aucun package vulnérable avec les sources NuGet du 2026-07-11.

CI distante du commit officiel `9ba35639e417cfb26e5249caa6375c94faf026a7` :

- Windows CI, run `29168999560` : réussi, tests, build, publish et artifacts inclus ;
- macOS CI, run `29168999565` : réussi, tests Core/UI et deux DMG inclus.

Release publique stable : [`v3.07`](https://github.com/Mamat79/Dante-Config-Editor/releases/tag/v3.07), publiée le 2026-07-11 avec six assets. Les digests GitHub concordent avec les fichiers vérifiés :

- Windows : `9f052c09391a274a044b44336c86893967fa64f10c8867266353a3e0aa352ccf` ;
- macOS Apple Silicon : `ea560fabe9a6d83da705d9529baa09c4eb74b5351dddc9407dbef40a146fb959` ;
- macOS Intel : `f6be8875e152fe8b31656a6c679e526c0a09649b8c0bc51e92f3f79ebb44fcdd`.

La Release est non brouillon, non prerelease et déclarée comme dernière version du dépôt. Aucun import Dante Controller n'a été exécuté pendant cette validation.

## Validation de la V3.07 Beta du 2026-07-11

Source fonctionnelle testée :

- branche : `main` ;
- commit : `840566b5451d7ddd3985cc1abdc82277a6efa986` ;
- système : Windows `10.0.26200`, `win-x64` ;
- .NET SDK `8.0.422`, MSBuild `17.11.48` ;
- Inno Setup `6.7.3`.

| Étape | Temps | Code retour | Résultat |
|---|---:|---:|---|
| Restore application Windows | 0,862 s | 0 | réussi |
| Restore tests Core | 0,878 s | 0 | réussi |
| Restore tests UI Mac | 0,729 s | 0 | réussi |
| Tests Core Release | 6,946 s | 0 | 63 réussis, 0 échec, 0 ignoré |
| Tests UI Mac Release | 8,919 s | 0 | 7 réussis, 0 échec, 0 ignoré |
| Build Windows Release | 1,785 s | 0 | 0 warning, 0 erreur |
| Build interface Mac Release | 1,040 s | 0 | 0 warning, 0 erreur |
| Publish Windows `win-x64` framework-dependent | 2,468 s | 0 | réussi |
| Publish Mac autonome `osx-arm64` | 3,631 s | 0 | réussi |
| Publish Mac autonome `osx-x64` | 3,505 s | 0 | réussi |
| Pack de validation XML | 1,424 s | 0 | 8 scénarios, rapports, checklist et hashes produits |
| Installateur Windows autonome | 32,764 s | 0 | réussi, 0 erreur Inno Setup |
| Installation puis mise à niveau de contrôle | 13,229 s | 0 | deux passages, une seule installation conservée |

Installateur validé :

- fichier : `dist/DanteConfigEditorV3_Installer.exe` ;
- version : `3.07-beta` ;
- taille : `66 842 463` octets ;
- SHA-256 : `3D06B9EF344A2153AFD67CE8296EEB1FA8EDCB75710C2738BB46EEF9DB5C7FAE` ;
- signature Authenticode : absente (`NotSigned`).

Installation vérifiée :

- dossier : `C:\Program Files\Dante Config Editor V3\` ;
- version de fichier : `3.7.0.0` ;
- SHA-256 de l'exécutable installé : `11BA21D50308C5A5FB69C962E91E4DE71F4B52E7CA39EEFFF1DA36DE1DA004CA` ;
- une seule entrée de désinstallation V3 ;
- raccourci Menu Démarrer, désinstallateur et quatre notices FR/EN présents ;
- démarrage réel réussi, titre `Dante Config Editor V3.07 Beta` ;
- fixture anonymisée ouverte : 3 devices, 3 TX, 4 RX, 3 patchs actifs, état `Non modifié` ;
- fermeture propre et aucune erreur Application Windows liée au processus pendant le contrôle.

Les quatre projets inspectés par `dotnet list package --vulnerable --include-transitive` ne signalent aucun package vulnérable avec les sources NuGet du jour. Les bundles Mac autonomes sont publiables depuis Windows, mais les DMG exigent macOS et sont construits par le workflow macOS.

CI distante du commit de livraison `a9fec0d76154d8a707fce3d8c484ff433f5f544f` :

- Windows CI, run `29167796316` : réussi en 1 min 1 s, tests, build, publish et artifacts inclus ;
- macOS CI, run `29167796298` : réussi, tests UI/Core et deux DMG inclus.

Release publique : `v3.07-beta`, publiée le 2026-07-11 avec six assets. Les digests GitHub concordent avec les fichiers vérifiés localement :

- Windows : `3d06b9ef344a2153afd67ce8296eeb1fa8edcb75710c2738bb46eef9db5c7fae` ;
- macOS Apple Silicon : `1e02b6b8a52d919463240e28c1a87c53b4734023f2e3a278b115db2ccb5b6784` ;
- macOS Intel : `626bd222470d31246ee2b5fe2aff3d2838ec71b10e9566e9c7105e0b9646f37b`.

Aucun import Dante Controller n'a été exécuté pendant cette validation.

## Baseline historique du 2026-07-11

Source testée :

- branche : `main` ;
- commit source après nettoyage : `4c286ab27ea614770565a10562e7ba464afceae0` ;
- tag de sécurité : `safety-v3.07-before-validation-20260711` ;
- système : Windows `10.0.26200`, `win-x64` ;
- SDK sélectionné par `global.json` : .NET SDK `8.0.422` ;
- MSBuild : `17.11.48` ;
- runtime .NET 8 : `8.0.28` ;
- Inno Setup : `6.7.3`.

Les temps ci-dessous proviennent d'une seule exécution locale après suppression de toutes les sorties `bin`, `obj`, `dist` et `tmp`. Ils ne constituent pas des seuils de performance.

| Étape | Temps | Code retour | Résultat |
|---|---:|---:|---|
| Restore application Windows | 0,874 s | 0 | réussi |
| Restore tests Core | 1,162 s | 0 | réussi |
| Restore tests UI Mac | 1,252 s | 0 | réussi |
| Tests Core Release | 10,626 s | 0 | 38 réussis, 0 échec, 0 ignoré |
| Tests UI Mac Release | 14,291 s | 0 | 2 réussis, 0 échec, 0 ignoré |
| Build Windows Release | 2,221 s | 0 | 0 warning, 0 erreur |
| Build interface Mac Release | 1,091 s | 0 | 0 warning, 0 erreur |
| Publish Windows `win-x64` framework-dependent | 2,716 s | 0 | réussi |
| Installateur Windows autonome | 63,089 s | 0 | réussi |

Installateur de baseline :

- nom : `DanteConfigEditorV3_Installer.exe` ;
- version : `3.07-beta` ;
- taille : `66 819 004` octets ;
- SHA-256 : `77A196ED5923240F8059EE120775BFC778A6AABB96959A9F340970186B24093D`.

L'installateur est une sortie locale régénérable et n'est pas suivi dans Git.

## Benchmarks V3.07 après patch visuel

Mesures locales du 2026-07-11 au commit fonctionnel `ff704bf`, trois exécutions par taille et médiane retenue. Chaque device synthétique possède 64 TX et 64 RX. Les allocations sont celles du thread mesuré par le banc ; la mémoire de travail est celle du processus après le scénario. Ces valeurs sont des observations, pas des seuils contractuels.

| Devices | XML | Chargement | Allocation chargement | Édition groupée | Allocation édition | Garde-fou | Sauvegarde | Mémoire de travail |
|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| 10 | 0,156 Mio | 32,210 ms | 2,783 Mio | 34,855 ms | 20,075 Mio | 6,064 ms | 71,934 ms | 49,410 Mio |
| 50 | 0,780 Mio | 73,121 ms | 13,735 Mio | 157,599 ms | 97,378 Mio | 31,875 ms | 283,507 ms | 85,945 Mio |
| 200 | 3,122 Mio | 207,301 ms | 54,822 Mio | 269,810 ms | 387,250 Mio | 50,649 ms | 641,289 ms | 179,512 Mio |

Le garde-fou n'a signalé aucune erreur inattendue sur ces trois scénarios. L'allocation de l'édition et de la sauvegarde croît avec le nombre de canaux ; les gros presets restent donc un point de surveillance, même si le scénario de 200 devices termine correctement.

Commande reproductible :

```powershell
dotnet run --project .\benchmarks\DanteConfigEditorV3.Benchmarks\DanteConfigEditorV3.Benchmarks.csproj -c Release -- --phase v3.07-final --commit ff704bf --output .\tmp\benchmarks\v3.07-final-ff704bf.json
```

Le JSON brut est une sortie locale régénérable ignorée par Git.

## Commandes de référence

```powershell
dotnet restore .\DanteConfigEditorV3.csproj
dotnet restore .\tests\DanteConfigEditorV3.Tests\DanteConfigEditorV3.Tests.csproj
dotnet restore .\tests\DanteConfigEditor.Mac.Tests\DanteConfigEditor.Mac.Tests.csproj

dotnet test .\tests\DanteConfigEditorV3.Tests\DanteConfigEditorV3.Tests.csproj -c Release --no-restore
dotnet test .\tests\DanteConfigEditor.Mac.Tests\DanteConfigEditor.Mac.Tests.csproj -c Release --no-restore

dotnet build .\DanteConfigEditorV3.csproj -c Release --no-restore
dotnet build .\src\DanteConfigEditor.Mac\DanteConfigEditor.Mac.csproj -c Release --no-restore

dotnet publish .\DanteConfigEditorV3.csproj -c Release -r win-x64 --self-contained false
.\installer\build_installer.ps1
```

L'installateur utilise un second publish `win-x64` autonome, mono-fichier et compressé. Le publish framework-dependent ci-dessus reste utile pour contrôler séparément la compilation de publication.

## État CI au début de la baseline

- macOS CI, run `29147477519` : succès sur le commit `46dbfcc` ; les tests et les deux DMG ont été produits.
- Windows CI, run `29147477510` : échec `NETSDK1004` sur le commit `46dbfcc`.

Cause Windows confirmée : après l'extraction du projet Core, le restore des tests ne restaurait plus `DanteConfigEditorV3.csproj`, alors que le build utilisait `--no-restore`. Le workflow a été corrigé pour restaurer explicitement l'application puis les tests.

Le runner Windows sélectionnait également son SDK 10 préinstallé. `global.json` fixe désormais le major .NET 8 et autorise uniquement le dernier patch de la feature band `8.0.400`.

Ces corrections ne seront déclarées validées dans GitHub Actions qu'après un run distant réussi. La réussite locale seule ne vaut pas preuve de CI.

## Baseline des dépendances

Versions directes initiales :

- Avalonia `11.3.13` ;
- Microsoft.NET.Test.Sdk `17.8.0` ;
- xUnit `2.5.3` ;
- xunit.runner.visualstudio `2.5.3` ;
- coverlet.collector `6.0.0`.

Le scan `dotnet list package --vulnerable --include-transitive` a signalé :

| Package transitif | Version | Gravité annoncée | Advisory |
|---|---:|---|---|
| `System.Net.Http` | 4.3.0 | High | `GHSA-7jgj-8wvc-jh57` |
| `System.Text.RegularExpressions` | 4.3.0 | High | `GHSA-cmhx-cq75-c4mj` |
| `Tmds.DBus.Protocol` | 0.21.2 | High | `GHSA-xrw6-gwf8-vvr9` |

Cette table décrit l'état initial, pas l'état final attendu. Les dépendances seront mises à jour séparément, puis le scan et tous les tests seront rejoués.

### Résolution des advisories

Mise à jour appliquée dans un commit séparé :

- Avalonia `11.3.18` et Avalonia Headless `11.3.18` ;
- `Avalonia.Controls.DataGrid` reste en `11.3.13`, dernière version publiée sur la ligne 11 et compatible avec Avalonia 11.3.x ;
- Microsoft.NET.Test.Sdk `18.7.0` ;
- xUnit `2.9.3` et runner Visual Studio `3.1.5` ;
- coverlet.collector `10.0.1`.

Après mise à jour :

- `dotnet list package --vulnerable --include-transitive` ne signale plus aucun package vulnérable dans les deux projets de tests ;
- 38 tests Core et 2 tests UI Mac réussissent ;
- les builds Windows et Mac réussissent sans warning ;
- les publications autonomes `osx-arm64` et `osx-x64` réussissent.

xUnit v2 est désormais une ligne de maintenance dépréciée au profit de xUnit v3. La migration v3 n'est pas mélangée à cette mise à jour corrective, car Avalonia Headless 11 reste intégré au modèle xUnit v2. Elle doit faire l'objet d'un chantier séparé avec validation CI Windows/macOS.

## Ce qui n'est pas validé par cette baseline

- aucun import dans Dante Controller n'a été exécuté pendant cette baseline ;
- aucun modèle matériel réel n'est déclaré compatible sans preuve ;
- le publish Windows framework-dependent n'est pas le paquet transmis aux utilisateurs ; l'installateur final utilise un publish self-contained ;
- les DMG du run macOS sont signés ad hoc, pas notariés par Apple ;
- les tests d'accessibilité et d'échelle d'affichage sont documentés séparément.

## Couverture fonctionnelle actuelle

La suite Core couvre notamment :

- tous les aliases de subscription reconnus par l'application ;
- patch local `.`, TX absent, canal absent et presets partiels ;
- machines sans TX ou sans RX ;
- plusieurs interfaces IPv4 et conservation de l'interface secondaire ;
- zéro, un ou plusieurs preferred masters ;
- namespace par défaut, réordonnancement de balises et valeurs inconnues préservées ;
- Unicode, noms longs, sauvegardes successives, fusion et récupération ;
- presets synthétiques 10/50/200 en 64 TX / 64 RX ;
- contrat de mise à niveau de l'installateur ;
- planification séquentielle et session en attente du patch visuel.

Les tests Mac couvrent la structure du rail latéral, les alertes, le focus initial, les dimensions `1366 x 768` et `1920 x 1080`, ainsi que l'ouverture et l'utilisation minimale de l'atelier de patch. Les contrôles matériels et lecteurs d'écran restants sont détaillés dans `ACCESSIBILITY.md`.
