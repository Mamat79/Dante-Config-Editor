# Rapport de validation - Dante Config Editor 2026.1 Beta

Date : 2026-07-29

## Références

- version : `2026.1.0-beta.1` ;
- branche : `2026.1` ;
- base V3.6 : `25a1e7cc0568b86a56bdf039ecce060c8eeea1ec` ;
- commit de sécurité XML validé :
  `02765f4` ;
- commit final de correction ergonomique :
  `30aa7216fe24e6e056edad823fd2cbd5f1e7f4cd` ;
- identité Windows : `Dante Config Editor 2026.1 Beta` ;
- profil local : `%LOCALAPPDATA%\DanteConfigEditor2026.1` ;
- bundle macOS : `fr.mamat.danteconfigeditor.y2026-1-beta`.

La branche n'est pas fusionnée dans `main`. La prérelease GitHub `v2026.1`
reste distincte de la V3.6 stable et conserve une identité d'installation
séparée.

## Audit et architecture

La base V3.6 concentrait encore une part importante des responsabilités dans
les fenêtres et dans `DanteProject`. La 2026.1 introduit progressivement des
frontières testables sans réécriture totale :

- `DanteConfigEditor.Domain` : capacités, sélection, historique et résultats
  de validation ;
- `DanteConfigEditor.DanteXml` : détection de profil et adaptation du document
  Dante existant ;
- `DanteConfigEditor.Application` : session, commandes transactionnelles,
  navigation, Patch unifié et validation ;
- `DanteConfigEditor.Infrastructure` : paquets `.dceproj`, récupération et
  migrations ;
- interfaces Windows et macOS conservées comme couches de présentation.

La comparaison entre la base et le commit validé porte sur 142 fichiers,
14 241 ajouts et 1 379 suppressions. Ce volume vient principalement de
l'extraction de services, de nouveaux tests, des fixtures anonymisées et de la
documentation ; il ne correspond pas à une reconstruction du XML Dante.

## Fonctions sécurisées ou ajoutées

- session centrale avec commandes annulables ;
- identité stable pour les opérations sur machines et canaux ;
- format `.dceproj` versionné, borné et validé ;
- récupération atomique et migration du profil V3.6 sans écrasement ;
- banque de machines format 2 avec migration explicite ;
- installation séparée des banques fournies et import manuel depuis le
  catalogue public GitHub ;
- espace Windows 2026.1 réorganisé ;
- liste des machines repliée au démarrage, avec flèche compacte centrée ; une
  fois dépliée, la grille occupe toute la hauteur disponible et restitue les
  réglages dans leur état précédent lorsqu'elle est refermée ;
- Patch unifié avec RX à gauche, TX à droite et application immédiate ;
- avertissement activé par défaut lors du remplacement d'un RX déjà patché ;
- synchronisation Patch, Easy patch, sélections et synoptique ;
- centre de validation filtrable ;
- profils XML et capacités explicites ;
- refus de créer un réglage technique absent du rôle Dante source ;
- actions globales limitées aux machines qui exposent réellement le réglage,
  avec compte rendu des machines ignorées ;
- index de machines/canaux et cache de validation.

## Intégrité XML

Les opérations continuent à modifier le document XML d'origine de façon
ciblée. Les tests couvrent notamment :

- namespaces, y compris namespace par défaut ;
- nœuds, attributs et valeurs inconnus ;
- ordre technique sans faux positif injustifié ;
- aliases de subscription reconnus et patch local `.` ;
- interfaces IPv4 multiples sans modification implicite de l'interface
  secondaire, du DNS ou de la passerelle ;
- renommages et références croisées ;
- Unicode et noms longs ;
- sauvegarde atomique et récupération ;
- duplication et insertion de rôles sans recopier `instance_id` ni
  `device_id` ;
- absence de création implicite de `redundancy`, `preferred_master`,
  `samplerate`, `encoding`, `unicast_latency` ou `ipv4_address` ;
- cycles ouverture, sauvegarde, comparaison sémantique et réouverture.

Les chemins techniques inconnus restent bloqués par défaut lorsqu'une
modification ne peut pas être démontrée sûre.

## Tests et builds

Environnement local :

- Windows `10.0.26200` ;
- .NET SDK `8.0.423` ;
- runtime .NET `8.0.29` ;
- MSBuild `17.11.48` ;
- Inno Setup `6.7.3`.

Résultats locaux finaux du 29 juillet 2026 :

| Contrôle | Résultat |
|---|---:|
| Tests Core/Windows | 419 réussis, 0 échec, 0 ignoré |
| Tests Avalonia/macOS sans écran | 22 réussis, 0 échec, 0 ignoré |
| Corpus XML local en lecture seule | 3 tests, 11 fichiers, 0 modification |
| Build Windows Release | 0 warning, 0 erreur, 5,434 s |
| Build macOS Release | 0 warning, 0 erreur, 1,367 s |
| Installateur Windows GitHub | réussi, 74 222 465 octets |
| SHA-256 installateur | `8cb27c432adb34d45b6b83c74147ebc5b05d61416fb604b00806de6f077503b7` |

GitHub Actions au commit `30aa721` :

- [Windows CI, exécution 30434168531](https://github.com/Mamat79/Dante-Config-Editor/actions/runs/30434168531) :
  succès, 419 tests et installateur produit ;
- [macOS CI, exécution 30434167263](https://github.com/Mamat79/Dante-Config-Editor/actions/runs/30434167263) :
  succès, 419 tests Core, 22 tests Avalonia et deux DMG produits.

## Performances

Les mesures complètes sont publiées dans
[`PERFORMANCE_REPORT.md`](PERFORMANCE_REPORT.md).

Sur le preset synthétique de 200 machines, 64 TX et 64 RX par machine :

- édition groupée : `317,410 ms` vers `38,092 ms`, soit -88,0 % ;
- allocations de l'édition : `390,759 Mio` vers `29,358 Mio`, soit -92,5 % ;
- validation initiale : `86,948 ms` vers `36,062 ms`, soit -58,5 % ;
- allocations de validation : -80,5 % ;
- patch : -38,7 % ;
- sauvegarde XML : -27,6 % ;
- chargement : +5,9 %, dans la limite de régression fixée à 10 % ;
- second appel du garde-fou à 200 machines : environ `0,009 ms`.

La matrice et le synoptique restent sensibles aux dimensions et à la densité
du preset ; leurs mesures présentent davantage de variabilité.

## Paquets

| Paquet | Taille | SHA-256 |
|---|---:|---|
| Installateur Windows GitHub | 74 222 465 octets | `8cb27c432adb34d45b6b83c74147ebc5b05d61416fb604b00806de6f077503b7` |
| DMG Apple Silicon CI | 63 431 643 octets | `d099f868d63624c354d6953bd4e07dd4322b3cb5e63a317f47a6d66c481b8541` |
| DMG Intel CI | 65 238 307 octets | `5719db32796daaefbb81442d0de38f52a680c4607a3b08ae7bba3c59133551d7` |
| Notice française | 1 052 955 octets | `f9a3b0f566fb70836f0e67506096c2af59a439e3e007dcb573efb07f153f1ed6` |
| Notice anglaise | 1 005 326 octets | `344b8d40be56adb74c310558f6b8d6efcb64dfede539669c54696013bd643277` |
| Démarrage rapide français | 44 953 octets | `9978c80f2fbc55a78d2dd2e962798b921bc6284a012a815f694af78acf86e401` |
| Quick Start anglais | 44 229 octets | `07785ea32fb66254d895cdc8122accea687d505356eb436439e79a6ef19d3fab` |

Les trois paquets, leurs sommes et les quatre PDF sont publiés dans la
prérelease `v2026.1`. Les vidéos, sous-titres et banques restent attachés sans
modification.

L'installateur Windows local a été exécuté en mise à niveau. Le contrôle
confirme :

- installation dans
  `C:\Program Files\Dante Config Editor 2026.1 Beta\` ;
- version `2026.1.0-beta.1`, fichier `2026.1.0.0` ;
- exécutable installé : 72 710 378 octets, SHA-256
  `d87f8695ba4929b298c3f3283ecf4ce84b4b82c26009f88e5fd48e4873052a61` ;
- une seule inscription bêta après mise à niveau ;
- raccourcis Bureau et menu Démarrer présents ;
- 41 modèles communautaires et 2 rôles génériques installés ;
- banque personnelle inchangée, avec 77 fichiers, 1 587 873 octets et aucune
  entrée ajoutée ou retirée ;
- lancement réussi et application répondante avec la fixture représentative.

Le contrôle de l'installation précédente a confirmé le chargement de la fixture
représentative et l'état replié initial. Les tests de contrat Windows vérifient
la flèche compacte centrée et l'affectation de toute la hauteur restante à la
liste ; les tests Avalonia exécutent réellement le dépliage, contrôlent que la
zone des réglages passe à zéro et que la grille reste dans la fenêtre. La
capture automatisée de la fenêtre WPF installée est restée blanche dans l'outil
de capture utilisé ; ce point est signalé comme une limite de l'outil et n'est
pas présenté comme une validation visuelle par capture.

L'installateur n'est pas signé Authenticode. Les DMG sont signés ad hoc mais
ne sont pas notariés.

## Vérification visuelle Windows

Seule la fixture synthétique
`tests/DanteConfigEditorV3.Tests/Fixtures/representative-preset.xml` a été
ouverte.

Contrôles effectués :

- réglages visibles par défaut ;
- Vue d'ensemble et Machines lisibles ;
- Patch avec RX à gauche, TX à droite, FLIP et 1:1 visibles ;
- remplacement d'un patch avec avertissement activé par défaut ;
- application immédiate, puis restauration avec Annuler ;
- Synoptique et ses commandes de zoom/export ;
- Centre de validation ;
- thèmes sombre et clair ;
- interface française et anglaise ;
- contrôles à `1920 x 1024` et `1536 x 864`, cette seconde taille représentant
  l'espace logique d'un écran Full HD à 125 % ;
- lancement de l'exécutable installé.

Les captures utilisent uniquement la fixture synthétique. Les 11 XML du corpus
local ont été ouverts exclusivement par les tests d'intégration en lecture
seule ; aucun original n'a été modifié ou ajouté au dépôt.

## Dante Controller : preuve et limite

Le mainteneur a confirmé des imports réels réussis dans Dante Controller de
fichiers modifiés avec DCE, y compris avec la version 2026.1. En complément :

- les tests XML structurels et sémantiques sont verts ;
- les nœuds inconnus et références testées sont préservés ;
- les réglages techniques absents ne sont plus inventés ;
- les 11 fichiers du corpus local passent ouverture et contrôle en lecture
  seule.

Il serait donc incorrect d'annoncer une garantie universelle pour toutes les
versions de Dante Controller et toutes les extensions constructeur. La
checklist
[`DANTE_CONTROLLER_MANUAL_VALIDATION.md`](DANTE_CONTROLLER_MANUAL_VALIDATION.md)
doit être utilisée avant promotion de la bêta.

## Retour arrière

La bêta peut être désinstallée depuis les Applications Windows sans supprimer
la V3.6, les XML, les projets `.dceproj` ni les banques utilisateur. Les
données 2026.1 restent dans leur profil distinct et peuvent être sauvegardées
avant désinstallation.

Sur macOS, supprimer l'application 2026.1 Beta n'efface pas les documents ou
banques stockés ailleurs.

## Limites restantes

- aucun contrôle sur un Mac physique dans ce cycle ;
- aucun essai VoiceOver, Narrator, NVDA ou contraste élevé ;
- échelles Windows natives 125 %, 150 % et 200 % encore à contrôler
  manuellement ; la taille logique équivalente à 125 % a été vérifiée ;
- interface macOS non identique au shell Windows ;
- création complète de projet toujours expérimentale ;
- absence de signature Authenticode et de notarisation Apple ;
- validation manuelle Dante Controller 2026.1 encore à consigner ;
- catalogue GitHub actuellement ouvert dans le navigateur : téléchargement
  intégré et proposition contrôlée d'une banque sont documentés mais pas encore
  implémentés.

## Documentation livrée

- README français et anglais ;
- CHANGELOG et notes de bêta ;
- baseline et architecture ;
- format `.dceproj` ;
- format et migration de banque ;
- architecture proposée pour les banques locales et le catalogue GitHub ;
- Patch unifié, synoptique et centre de validation ;
- performances et corpus de compatibilité ;
- notices complètes FR/EN de 27 pages ;
- Quick Start FR/EN ;
- limites, accessibilité, tests et procédure macOS ;
- vidéos de présentation françaises et anglaises en H.264, 1920 × 1080,
  30 images/s et 1 min 48 s, sans piste audio ;
- sous-titres corrigés incrustés dans chaque vidéo et conservés séparément au
  format SRT ;
- captures réalisées uniquement avec le preset synthétique anonymisé, avec
  masquage des chemins locaux avant versionnement.
