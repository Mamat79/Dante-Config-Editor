# Rapport de validation - Dante Config Editor 2026.1 Beta

Date : 2026-07-28

## Références

- version : `2026.1.0-beta.1` ;
- branche : `2026.1` ;
- base V3.6 : `25a1e7cc0568b86a56bdf039ecce060c8eeea1ec` ;
- commit de code et de paquets validé :
  `361aab9c98b8382addf45f74eb2e7861f5128b24` ;
- identité Windows : `Dante Config Editor 2026.1 Beta` ;
- profil local : `%LOCALAPPDATA%\DanteConfigEditor2026.1` ;
- bundle macOS : `fr.mamat.danteconfigeditor.y2026-1-beta`.

La branche n'a pas été fusionnée dans `main` et aucune Release GitHub n'a été
créée. La V3.6 stable reste indépendante.

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
- Patch unifié avec RX à gauche, TX à droite et application immédiate ;
- avertissement activé par défaut lors du remplacement d'un RX déjà patché ;
- synchronisation Patch, Easy patch, sélections et synoptique ;
- centre de validation filtrable ;
- profils XML et capacités explicites ;
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

Résultats locaux finaux du 28 juillet 2026 :

| Contrôle | Résultat |
|---|---:|
| Tests Core/Windows | 386 réussis, 0 échec, 3 s |
| Tests Avalonia/macOS sans écran | 22 réussis, 0 échec, 20 s |
| Build Windows Release | 0 warning, 0 erreur, 1,40 s |
| Build macOS Release | 0 warning, 0 erreur, 1,03 s |
| Publish Windows autonome | réussi, 20,3 s |
| Installateur Windows autonome | réussi, 41,08 s, 74 163 527 octets |
| SHA-256 installateur | `a9bcb6d0c7347a12bfda9de1d24df1e7a58605af238303a0739b598b31550ef6` |

GitHub Actions de la base 2026.1 :

- [Windows CI, exécution 30298793379](https://github.com/Mamat79/DanteConfigEditorV3/actions/runs/30298793379) :
  succès ;
- [macOS CI, exécution 30298792909](https://github.com/Mamat79/DanteConfigEditorV3/actions/runs/30298792909) :
  succès ;
- les fichiers TRX téléchargés confirment 364/364 tests Windows, 364/364 tests
  Core sur macOS et 20/20 tests Avalonia.

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
| Installateur Windows local testé | 69 759 045 octets | `2ad5fa4ab53b1621738c1c30841e5f1a0af4f89eb928e9f41482f305d8404b90` |
| Installateur Windows CI | 69 767 745 octets | `72b84b0e6effb03d534d2a68b94ad5119231c7a0237077081e0fe56ef6fa044b` |
| DMG Apple Silicon CI | 52 850 639 octets | `b1774f3eb710853b289242b3a090544438ff1b5d2ba5cfdd51fb03f9223cd206` |
| DMG Intel CI | 54 282 491 octets | `0e4c9b52930ac8191b77270d5cab487cf70e970b326c0c5a927410806c9097a6` |

Chaque empreinte a été recalculée après téléchargement et correspond au
fichier `.sha256` livré avec l'artefact concerné.

L'installateur Windows local a été exécuté deux fois. Le contrôle confirme :

- installation dans
  `C:\Program Files\Dante Config Editor 2026.1 Beta\` ;
- version `2026.1.0-beta.1`, fichier `2026.1.0.0` ;
- une seule inscription bêta après mise à niveau ;
- inscription de la V3.6 stable toujours présente ;
- lancement réussi de l'application installée.

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
- lancement de l'exécutable installé.

Aucun XML de production n'a été ouvert pendant cette validation.

## Dante Controller : preuve et limite

Le mainteneur a confirmé un import réel réussi de fichiers V3.6 modifiés avec
DCE dans Dante Controller. Cette preuve est conservée dans l'historique du
projet.

Pour le commit 2026.1 validé ici :

- les tests XML structurels et sémantiques sont verts ;
- les nœuds inconnus et références testées sont préservés ;
- aucun nouvel import manuel dans Dante Controller n'a été consigné pendant
  ce cycle.

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
- échelles Windows exactes 125 %, 150 % et 200 % encore à contrôler
  manuellement ;
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
