# Journal des versions DCE

Toutes les dates utilisent le calendrier ISO. Les versions antérieures à
2026.1 sont conservées dans [CHANGELOG_V3.md](CHANGELOG_V3.md).

## [2026.1.1] - 2026-08-02

### Ajouté

- vérification automatique et silencieuse de la dernière Release GitHub au
  démarrage sur Windows et macOS ;
- commande `Aide > Rechercher les mises à jour` pour vérifier manuellement,
  télécharger l'installateur adapté, contrôler son SHA-256 puis le lancer ;
- mise à jour directe des banques officielles depuis la fenêtre Banque ;
- installation des banques officielles dans `Documents/Dante Config Editor/Included Machine Banks`,
  sans modifier la banque personnelle ;
- remplacement transactionnel d'une banque officielle avec sauvegarde de la
  copie précédente et rejet de toute archive dont le SHA-256 est incorrect.

### Modifié

- la fenêtre Banque reste ouverte après un ajout afin d'enchaîner plusieurs
  modèles sans la rouvrir ;
- l'ajout depuis une banque accepte désormais de 1 à 100 instances en une seule
  opération, avec aperçu des noms, suffixes `-2`, `-3`, etc., validation globale
  avant mutation et une seule action Annuler ;
- une banque officielle gérée dans Documents est prioritaire sur la copie de
  secours livrée dans le dossier de l'application ;
- ajout des modèles assainis Glensound Paradiso 32x32 et Tieline Gateway 16x16,
  portant la banque communautaire à 43 modèles et l'ensemble fourni à 45.

## [2026.1.0] - 2026-07-29

### Banque communautaire - mise à jour du 2026-08-02

- ajout des rôles assainis Glensound Paradiso 32x32 et Tieline Gateway 16x16 ;
- la banque communautaire contient désormais 43 modèles illustrés, soit 45
  modèles fournis avec les deux rôles génériques ;
- correction du contrôle de réussite du script de reconstruction des banques.

### Modifié

- promotion de 2026.1 en version officielle sans changer son périmètre métier ;
- suppression de la mention Beta dans l'application, les notices, les
  installateurs, les paquets macOS et la présentation ;
- noms de paquets simplifiés, tout en conservant l'AppId Windows pour mettre à
  niveau l'installation 2026.1 existante ;
- vidéo de présentation réorganisée pour expliquer les principaux écrans et
  parcours en français et en anglais.
- fusion de deux XML corrigée lorsque des rôles partagent la même identité
  technique : le rôle courant peut être réutilisé, ou le rôle importé peut
  devenir générique après renommage sans recevoir de faux `device_id` ;
- subscriptions importées remappées vers le rôle réutilisé ou le nouveau nom ;
- notices complètes FR/EN enrichies avec des parcours guidés pour la banque,
  le nouveau projet, la fusion, le patch, le renommage et le synoptique.
- navigation directe depuis un RX vers sa source TX et depuis un TX vers
  chacune de ses destinations RX dans la matrice ;
- matrice détachable dans une grande fenêtre conservant les sélecteurs, FLIP,
  Patch 1:1 et le zoom.

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
- navigation, réglages Machines et inspecteur ouverts à chaque lancement avec
  une flèche de repli toujours accessible ;
- accès aux banques déplacé dans la page Machines et assistant Nouveau projet
  rendu adaptatif ;
- 43 modèles fournis intégrés aux installateurs Windows et macOS sans écraser
  la banque personnelle.

### Corrigé le 2026-07-28

- ajout d'un menu d'application standard sur Windows et macOS sans retirer
  l'inspecteur contextuel rétractable ;
- thème clair au premier lancement, puis conservation du dernier thème et de
  la dernière langue choisis sur Windows et macOS ;
- intégration de la page Atomic Bomb au shell avec une séquence visuelle :
  clé de sécurité, ouverture automatique du capot, ARM, LOCK, FIRE ;
- la banque personnelle et les deux banques fournies sont visibles ensemble
  dans une liste dédupliquée de 43 modèles, avec filtre par banque et origine ;
- la page Machines ouvre directement cette vue globale depuis ses deux boutons,
  sans sélecteur de source redondant, et regroupe la liste rapide dans une barre
  compacte sous les actions globales ;
- le panneau Actions globales s'aligne sur la hauteur de Machine/Canaux et
  affiche Réseau/audio sans ascenseur interne sur un écran standard ;
- la fenêtre Banque s'ouvre en grand format et s'ajuste automatiquement à la
  surface de travail disponible ;
- retrait du dépôt des vidéos et captures V3.3/V3.5, des archives publiques
  V3.6 non cataloguées et de l'ancienne description de Release V3.4 ;
- la flèche de repli des réglages Machines est centrée sur leur séparation et
  reste accessible après le repli ;
- les modèles fournis sont protégés en lecture seule mais restent exportables,
  ajoutables au projet et duplicables dans la banque personnelle ;
- l'inspecteur de droite suit désormais la dernière machine choisie dans
  Machines, Patch ou Easy Patch et conserve ce contexte au changement de vue ;
- les fenêtres de banque Windows et macOS disposent du même parcours global.

### Corrigé le 2026-07-29

- les réglages techniques ne créent plus de balise absente du rôle Dante
  d'origine : `redundancy`, `preferred_master`, `samplerate`, `encoding`,
  `unicast_latency` et `ipv4_address` doivent déjà être présents ;
- les actions globales modifient uniquement les machines qui exposent le
  réglage demandé et annoncent clairement les machines ignorées ;
- les commandes indisponibles sont désactivées avec une explication visible
  et une bulle d'aide en français ou en anglais ;
- le garde-fou bloque aussi l'ajout indirect d'une balise technique, notamment
  lors d'une duplication ou d'une sauvegarde ;
- le bandeau et les sélecteurs de la matrice Patch ont été compactés sans
  masquer les libellés RX/TX, le zoom, FLIP ni Patch 1:1 ;
- « Voir dans Patch » ouvre la matrice sur la dernière machine sélectionnée ;
- les flèches persistantes des panneaux gauche et droit sont plus grandes ;
- les panneaux Machines restent utilisables à `1536 x 864`, correspondant à
  l'espace logique d'un écran Full HD réglé à 125 %.
- la liste générale des machines démarre repliée derrière une barre titrée et
  sa flèche persistante, afin de réserver la hauteur aux réglages ;
- les réglages Machines tiennent sans ascenseur à `1536 x 864` ; un défilement
  de secours reste disponible aux résolutions inférieures ;
- toutes les infobulles déclarées dans les interfaces Windows et macOS ont une
  traduction anglaise et utilisent un affichage multilignes non tronqué.

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

La 2026.1 a été testée avec succès dans Dante Controller par le mainteneur.
La checklist manuelle reste recommandée pour chaque fichier de production.

### Distribution

- Windows : `DanteConfigEditor2026_1_Installer.exe` ;
- macOS Apple Silicon et Intel : DMG autonomes dédiés ;
- Release officielle : <https://github.com/Mamat79/Dante-Config-Editor/releases/tag/v2026.1> ;
- branche officielle : `main`.
