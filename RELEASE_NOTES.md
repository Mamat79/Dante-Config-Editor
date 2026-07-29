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
- vue globale des banques personnelle et fournies, avec déduplication de
  `43` modèles uniques et protection en lecture seule des modèles fournis ;
- sélection de machine partagée entre Machines, Patch, Easy Patch et
  l'inspecteur, conservée lors des changements de vue ;
- accès direct à la vue globale des 43 modèles depuis Machines, sans sélecteur
  de banque redondant, avec une liste rapide compacte intégrée sous les actions
  globales ;
- Actions globales aligné sur la hauteur du groupe Machine/Canaux, sans
  ascenseur interne dans Réseau/audio sur un écran standard ;
- fenêtre Banque agrandie et automatiquement limitée à la surface de travail
  disponible ;
- réglages Machines ouverts au démarrage et repliables par une flèche centrale
  qui reste accessible dans les deux états ;
- liste générale des machines repliée au démarrage derrière une barre titrée,
  pour privilégier les réglages ; elle se déplie avec sa flèche persistante ;
- réglages sans ascenseur à `1536 x 864`, avec défilement de secours aux petites
  résolutions ;
- menu d'application standard sur Windows et macOS, avec accès direct aux
  fichiers, machines, vues, outils et notices ;
- thème clair au premier lancement, puis restauration du dernier thème et de
  la dernière langue choisis ;
- page Atomic Bomb intégrée au shell avec clé de sécurité, ouverture automatique
  du capot, ARM, LOCK puis FIRE ;
- réglages techniques conditionnés par les balises réellement présentes dans
  chaque rôle Dante, sans création automatique de `redundancy`,
  `preferred_master`, `samplerate`, `encoding`, `unicast_latency` ou
  `ipv4_address` ;
- commandes indisponibles désactivées avec une explication et une bulle d'aide
  bilingues ;
- infobulles Windows et macOS auditées en français et en anglais, affichées sur
  plusieurs lignes pour éviter toute phrase tronquée ;
- matrice Patch plus compacte, flèches latérales plus visibles et ouverture de
  « Voir dans Patch » directement sur la machine sélectionnée ;
- profil local 2026.1 isolé.

## Fidélité XML

Le document original reste la source. DCE effectue des mutations ciblées et
conserve les nœuds, attributs, namespaces, ordre et valeurs inconnues. Une
sauvegarde utilise un temporaire, une relecture, une validation, une copie de
sécurité et un remplacement atomique. Une balise technique absente du rôle
d'origine n'est jamais ajoutée pour simuler une capacité non démontrée.

Le corpus automatisé couvre notamment les presets partiels, TX ou RX seuls,
subscriptions locales `.`, sources absentes, canaux absents, namespace par
défaut, Unicode, extensions inconnues, interfaces multiples et modes audio
mixtes.

La version 2026.1 a été importée avec succès dans Dante Controller par le
mainteneur sur ses fichiers de contrôle. La checklist
`docs/2026.1/DANTE_CONTROLLER_MANUAL_VALIDATION.md` reste recommandée pour
chaque structure XML et chaque opération de production.

## Performance

Sur le preset synthétique de 200 machines avec 64 TX et 64 RX chacune :

- édition groupée : `317,410 ms` vers `38,092 ms` ;
- allocations d’édition : `390,759 Mio` vers `29,358 Mio` ;
- validation : `86,948 ms` vers `36,062 ms` ;
- sauvegarde XML : `501,695 ms` vers `363,457 ms`.

## Validation automatisée

- 419 tests Core/Windows réussis ;
- 22 tests Avalonia/macOS sans écran réussis ;
- build Windows Release sans warning ;
- corpus synthétique sauvegardé et comparé sémantiquement sans perte ;
- 11 XML locaux contrôlés en lecture seule par les tests d'intégration, sans
  modification des originaux.

Les nombres finaux de l’artefact livré sont consignés dans le rapport bêta.

## Installation

### Windows

`DanteConfigEditor2026_1_Beta_Installer.exe` inclut le runtime .NET 8 et les
notices bilingues. Le dossier proposé est
`C:\Program Files\Dante Config Editor 2026.1 Beta\`.
Les 43 modèles fournis sont intégrés dans le dossier de l'application, sans
remplacer la banque personnelle.

### macOS

Deux DMG autonomes sont prévus pour Apple Silicon et Intel. Ils sont signés ad
hoc mais non notariés.

## Limites

- création complète de projet toujours expérimentale ;
- aucune identité matérielle réelle créée lors d’une duplication ou insertion
  depuis une banque ;
- quelques contrôles conservent un rendu natif différent entre Windows et macOS ;
- installateur Windows non signé Authenticode ;
- DMG non notariés ;
- publication GitHub réalisée manuellement après validation des artefacts.
