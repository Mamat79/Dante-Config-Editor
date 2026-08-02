# Dante Config Editor 2026.1.1

[English release notes](RELEASE_NOTES_EN.md)

## Statut

`2026.1.1` est la version officielle issue de la refondation progressive de
DCE à partir de la V3.6 stabilisée. La V3.6.1 reste disponible comme version
historique ; les identités d’installation et les profils locaux sont distincts.

DCE reste un outil tiers non officiel, sans affiliation avec Audinate. Il
travaille uniquement sur des fichiers hors ligne.

## Principales évolutions

- vérification automatique des nouvelles Releases, sans erreur visible lorsque
  l'ordinateur est hors ligne, et contrôle manuel depuis le menu Aide ;
- téléchargement de l'installateur Windows ou macOS adapté, validation de son
  SHA-256 puis lancement uniquement après confirmation ;
- mise à jour directe des banques officielles depuis GitHub dans Documents,
  avec sauvegarde transactionnelle et sans toucher à la banque personnelle ;
- ajout en lot de 1 à 100 machines depuis la banque, avec aperçu des noms,
  validation XML globale, une seule action Annuler et maintien de la fenêtre
  Banque ouverte pour enchaîner les insertions ;

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
  `45` modèles uniques et protection en lecture seule des modèles fournis ;
- sélection de machine partagée entre Machines, Patch, Easy Patch et
  l'inspecteur, conservée lors des changements de vue ;
- accès direct à la vue globale des 45 modèles depuis Machines, sans sélecteur
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
- navigation croisée dans la matrice : le bouton d’un RX affiche et surligne
  sa source TX ; le bouton d’un TX liste toutes ses destinations RX et ouvre
  celle choisie ;
- poignées de recopie conservées au contact des labels et flèches de ciblage
  déplacées contre la grille, avec un en-tête TX stable en thème clair comme
  en thème sombre ;
- matrice Patch détachable dans une grande fenêtre indépendante qui conserve
  les sélecteurs RX/TX, FLIP, Patch 1:1 et le zoom ;
- fusion XML corrigée lorsque deux presets contiennent la même paire
  `device_id` / `process_id` : réutilisation explicite du rôle existant ou
  création d'un rôle générique indépendant après renommage, sans faux
  identifiant matériel ;
- subscriptions du second XML redirigées vers le rôle existant réutilisé ou
  vers le nouveau nom choisi ;
- notices complètes française et anglaise réorganisées en 38 pages autour du
  parcours réel : réglages généraux, réglages par machine, patch, composition
  du projet, exports, validation et outils avancés, avec davantage de captures ;
- profil local 2026.1 isolé.

## Fidélité XML

Le document original reste la source. DCE effectue des mutations ciblées et
conserve les nœuds, attributs, namespaces, ordre et valeurs inconnues. Une
sauvegarde utilise un temporaire, une relecture, une validation, une copie de
sécurité et un remplacement sécurisé. Une balise technique absente du rôle
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

- 435 tests Core/Windows réussis ;
- 22 tests Avalonia/macOS sans écran réussis ;
- build Windows Release sans warning ;
- corpus synthétique sauvegardé et comparé sémantiquement sans perte ;
- 11 XML locaux contrôlés en lecture seule par les tests d'intégration, sans
  modification des originaux.

Les validations détaillées de la phase de préparation restent consignées dans
les rapports techniques datés du dossier `docs/2026.1`.

## Installation

### Windows

`DanteConfigEditor2026_1_1_Installer.exe` inclut le runtime .NET 8 et les
notices bilingues. Le dossier proposé est
`C:\Program Files\Dante Config Editor 2026.1\`.
Les 45 modèles fournis sont intégrés dans le dossier de l'application. Les
copies officielles mises à jour sont conservées dans Documents, sans remplacer
la banque personnelle.

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
