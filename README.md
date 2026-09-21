# Dante Config Editor

**DCE permet de préparer, vérifier, modifier, fusionner, patcher et documenter
des configurations Dante hors ligne, sans avoir les machines connectées.**

DCE propose trois parcours explicites : créer ou ouvrir un **projet Dante Config
Editor** autonome en XML, ouvrir un **projet StageFlow local** accessible sur
l'ordinateur, ou rejoindre volontairement une **session StageFlow LIVE** sur le
réseau local avec son code à six chiffres. StageFlow est gratuit et facultatif.
Un projet StageFlow sans configuration Dante peut être initialisé directement
depuis zéro dans une configuration vide, puis recevoir les machines voulues
depuis la banque.

[English version](README_EN.md)

## Télécharger, voir, apprendre

La 2027.2.2 corrige le rappel de mise a jour repetitif : le controle utilise la
version de l'application lancee pour ne pas reproposer une version deja installee.
Les limites temporaires GitHub sont respectees, avec un delai de reprise
memorise et une simple information lors d'une recherche manuelle.

**Version stable commune : 2027.2.2 pour Windows, macOS Apple Silicon et macOS Intel.**

| Plateforme | Téléchargement direct |
| --- | --- |
| Windows 11 x64 | [Installateur autonome 2027.2.2 `.exe`](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.2/DanteConfigEditor2027_Installer.exe) |
| macOS Apple Silicon | [Image disque `.dmg`](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.2/DanteConfigEditor2027_macOS_AppleSilicon.dmg) |
| macOS Intel | [Image disque `.dmg`](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.2/DanteConfigEditor2027_macOS_Intel.dmg) |
| Contrôle des téléchargements | [Sommes SHA-256](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.2/SHA256SUMS.txt) |
| Projet StageFlow facultatif | [StageFlow, gratuit](https://github.com/Mamat79/StageFlow) |

| Découvrir Dante Config Editor | Français | English |
| --- | --- | --- |
| Présentation rapide | [Vidéo MP4](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.2/dante-config-editor-presentation-fr.mp4) | [MP4 video](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.2/dante-config-editor-presentation-en.mp4) |
| Démarrage rapide Windows | [PDF français](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.2/QuickStart_DanteConfigEditorV3_FR.pdf) | [English PDF](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.2/QuickStart_DanteConfigEditorV3_EN.pdf) |
| Notice complète Windows | [PDF français](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.2/Notice_DanteConfigEditorV3_FR.pdf) | [English PDF](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.2/Notice_DanteConfigEditorV3_EN.pdf) |
| Démarrage rapide macOS | [PDF français](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.2/QuickStart_DanteConfigEditor_macOS_FR.pdf) | [English PDF](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.2/QuickStart_DanteConfigEditor_macOS_EN.pdf) |
| Notice complète macOS | [PDF français](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.2/Notice_DanteConfigEditor_macOS_FR.pdf) | [English PDF](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.2/Notice_DanteConfigEditor_macOS_EN.pdf) |
| Guide commun de la suite SiLeMI/O | [PDF français](docs/guides/Guide-Suite-SiLeMIO-FR.pdf) | [English PDF](docs/guides/SiLeMIO-Suite-Guide-EN.pdf) |

Les vidéos présentent les principes du logiciel. Les notices 2027.2.2 décrivent
les parcours autonomes et StageFlow, le synoptique persistant et l'export vers Dante Controller.
Windows et Mac proposent la matrice de tout le projet : machines repliées,
diagonales et colonnes automatiques avec aperçu, Flip, renommage direct et
poignées de série, recherche des sources/destinations et double-clic machine.
Le gestionnaire Multicast permet de choisir les TX audio et de créer, modifier
ou supprimer les flux simples d'un preset 3.0.0. Les formats inconnus restent
préservés. La suppression des flux du preset doit être vérifiée sur le réseau
dans Dante Controller.

La 2027.2.2 adapte automatiquement l'interface aux fenêtres larges, compactes
ou étroites, sans réduire le texte. La navigation principale est recentrée sur
Projet, Vue d'ensemble, Machines, Patch DCE et Outils. Sous Windows, une roue
dentée personnalise les raccourcis et le synoptique reste dans Outils. Le nouveau
projet suit un assistant en deux étapes et Easy Patch distingue clairement le
parcours Source / destination du mode Hybride recommandé.

> DCE est un outil tiers non officiel, sans affiliation avec Audinate. Il ne
> pilote pas le réseau Dante en direct et n’utilise ni SDK ni API Audinate.
> Travaillez sur une copie et contrôlez le fichier final dans Dante Controller
> avant son utilisation sur une installation.

Les XML générés par cette génération ont été importés avec succès dans Dante
Controller par le mainteneur. Des tests structurels, sémantiques et de
non-régression complètent ces essais réels.

## Bien démarrer

### Mise à jour Windows depuis 2026.10

Enregistrez vos projets, fermez DCE et utilisez le lien Windows direct ci-dessus.
Les anciens clients 2026.10 peuvent ne pas voir ce correctif avec leur vérification
automatique. L'installateur préserve projets, banques, préférences et licences.
Contrôlez ensuite `Aide > À propos` : le dossier Program Files historique peut
conserver « 2026.3 » sans représenter la version installée. Les paquets Windows et
Mac sont numérotés séparément ; l'étiquette GitHub Latest n'est pas le sélecteur
de plateforme. Le nouvel updater choisit le paquet stable adapté.

### Bandeau et préparation

Le bandeau commun donne accès à **Connexion StageFlow**, **Alertes**, au thème
**Clair/Sombre**, à **FR/EN**, au **Guide DCE** et à l'**Aide DCE**.
**Guide** ouvre la notice complète du logiciel dans la langue choisie, sans
connexion Internet. **Aide** ouvre **Découvrir DCE** pour choisir un point de
départ. Le document commun reste dans **Aide > Guide de la suite SiLeMI/O**.
Les [notices et ressources officielles](https://www.silemio.com/logiciels/dante-config-editor#guides)
sont également accessibles sur le site SiLeMI/O.
Les alertes distinguent les contrôles XML et les changements StageFlow à acquitter.
Préparez les **Machines**, puis le **Patch**, ouvrez le **Centre de validation**
et utilisez **Exporter vers Dante Controller** pour produire le XML final.
**Enregistrer** conserve dans le projet StageFlow le domaine Dante et les données
de travail du synoptique : emplacements, ordre, visibilité et positions manuelles.
**Atomic Bomb** reste accessible dans la navigation latérale. Le parcours
`Outils > Formation > Atomic Bomb` et la séquence de sécurité restent inchangés.
Dans **Import / Export > Synoptique**, **Détacher le synoptique** ouvre la vue
séparée ; **Rattacher** revient à la fenêtre principale en conservant les
déplacements des machines et des zones, sans modifier le XML.

La page **Projet** adapte ses compteurs, ses commandes et sa liste de fichiers
récents à la place disponible. Dans une grande fenêtre, seuls les récents ont
besoin de défiler. Dans une petite fenêtre, le défilement de la page reste
disponible pour accéder à toutes les commandes, sans réduire la taille du texte.

## Pourquoi utiliser DCE ?

DCE est né d’un besoin de terrain : **voir rapidement l’ensemble d’une
installation Dante sans parcourir toutes les pages de Dante Controller**. Dans
une seule interface, il réunit les machines, canaux, subscriptions, latences,
sample rates, encodages, modes réseau, Preferred Masters et adresses IP.

Il répond aussi à deux opérations habituellement longues :

- **renommer une machine ou ses TX sur un réseau déjà patché** : DCE met à jour
  les références reconnues afin de conserver les subscriptions ;
- **fusionner plusieurs installations** : un second XML peut être ajouté au
  projet, avec détection des doublons et renommage automatique ou manuel en cas
  de conflit.

DCE peut partir d’une sauvegarde existante ou d’un projet hors ligne créé avec
des rôles issus de la banque de machines. Il sert autant à préparer une future
installation qu’à contrôler, corriger, documenter ou réorganiser rapidement un
projet existant.

## Ce que DCE sait faire

### Préparer et organiser un projet

- créer de zéro un projet Dante Config Editor autonome et son XML ;
- ouvrir un projet StageFlow local sans installer ni lancer StageFlow ;
- rejoindre explicitement une session StageFlow LIVE avec son code à six chiffres ;
- suivre automatiquement une session StageFlow LIVE valide, avec état visible,
  retour immédiat au mode autonome à l'expiration du bail et possibilité de
  désactiver localement le suivi ;
- ouvrir un XML Dante, le réimporter après modification et comparer avant/après ;
- créer un projet hors ligne minimal, puis ajouter des rôles depuis une banque ;
- ajouter un XML dans le projet ouvert pour fusionner deux installations ;
- résoudre les conflits de noms et d’identités pendant la fusion ;
- dupliquer, supprimer ou réinitialiser prudemment une machine ;
- migrer les anciens espaces de travail `.dceproj` sans les rendre obligatoires.

### Modifier les machines et leurs canaux

- renommer directement les machines, RX et TX ;
- renommer une série en prolongeant un suffixe numérique, zéros compris, ou des paires stéréo `1L`, `1R`, `2L`, `2R` ;
- préserver les subscriptions reconnues lors du renommage d’une machine ou d’un TX ;
- modifier, lorsque le XML de la machine le permet, latence, sample rate,
  encodage, Preferred Master, mode redondant/daisychain et paramètres IP ;
- conserver les commandes Redondant/Daisychain lorsque la balise `redundancy`
  existe, même si le rôle ne contient aucune section IPv4 ;
- appliquer un profil ou une même action globale à une sélection ou à toutes
  les machines non verrouillées ;
- signaler clairement les propriétés absentes ou non modifiables au lieu
  d’inventer des balises techniques.

### Patcher rapidement

- utiliser la Matrice, Easy Patch ou la liste détaillée RX vers TX ;
- patcher à l’unité, par glisser, par plage ou en série 1:1 ;
- dans Easy Patch, sélectionner un ou plusieurs TX puis les déposer sur le
  premier RX : DCE les affecte immédiatement aux RX successifs ;
- ou sélectionner un ou plusieurs RX puis les déposer sur le premier TX : DCE
  utilise immédiatement ce TX et les TX suivants pour alimenter chaque RX ;
- inverser les machines affichées avec FLIP RX/TX ;
- retrouver la source d’un RX ou toutes les destinations d’un TX ;
- détacher et agrandir la matrice tout en conservant filtres, FLIP, 1:1 et zoom ;
- déconnecter les RX, les TX ou l’ensemble des patchs d’une machine.

### Vérifier, exporter et documenter

- afficher alertes, incohérences, différences de formats audio et de réseau ;
- contrôler chaque sauvegarde avec un assistant de validation avant export :
  erreurs bloquantes, avertissements à confirmer, informations et navigation
  directe vers l'élément concerné ;
- conserver comme simples informations les références vers un TX absent d'un
  preset partiel, sans les confondre avec une incompatibilité XML bloquante ;
- générer rapports TXT/PDF, patchbooks et comparaisons avant/après ;
- produire un synoptique coloré en PDF ou SVG avec lieux et liaisons regroupées ;
- importer et exporter des labels en JSON, CSV, XLSX et ODS, notamment pour
  DMT, Allen & Heath dLive/Avantis et Yamaha CL/QL ;
- utiliser directement un groupe du patch StageFlow pour nommer tout ou partie
  des canaux RX d'une machine Dante, avec choix Source, Micro, Source + micro ou
  Libellé StageFlow et aperçu Avant / Après ;
- conserver pour chaque association LIVE l'UUID du groupe, l'UUID de la ligne,
  le mode de nommage et le canal RX cible : aucun rapprochement automatique par
  texte n'est effectué ;
- gérer des banques personnelles, fournies ou partagées et les actualiser
  depuis GitHub ;
- distinguer dans la banque la provenance et le niveau de vérification de chaque
  modèle ; 57 modèles communautaires ont été testés sur du matériel réel par
  le mainteneur et 20 profils supplémentaires ont été validés structurellement
  d'après les caractéristiques officielles des fabricants ;
- préparer une contribution de banque assainie et partageable, sans identité
  matérielle, adresse réseau, subscription ni donnée de projet ;
- récupérer une session, annuler/rétablir les actions et enregistrer avec
  validation, sauvegarde et remplacement sûr de la destination.

Au premier lancement, un accueil guidé distingue clairement **Projet Dante
Config Editor**, **Projet StageFlow local** et **Session StageFlow LIVE**, puis
donne accès à la banque de machines et à la notice complète.
Il peut être rouvert depuis `Aide > Découvrir DCE`. Pour faciliter le support,
`Outils > Créer un dossier de support` produit une archive de diagnostic
assainie avec manifeste et empreinte SHA-256, sans XML Dante ni code de licence.

DCE propose une interface française/anglaise, les thèmes clair/sombre et des
versions Windows et macOS. L’outil **Atomic Bomb**, volontairement à part,
permet de fabriquer un projet de dépannage chaotique pour la formation.
Sur Mac, la fenêtre s'adapte au démarrage à la zone disponible et à l'échelle
de l'écran. Le défilement garde les commandes accessibles sur un petit écran.

## Documentation technique

- [Architecture 2026.1](docs/2026.1/ARCHITECTURE_2026_1.md)
- [Intégration StageFlow et domaine Dante](docs/STAGEFLOW_INTEGRATION.md)
- [Guide de démarrage de la suite SiLeMIO](docs/guides/GUIDE-SUITE-FR.md)
- [Format de projet `.dceproj`](docs/2026.1/DCEPROJECT_FORMAT.md)
- [Format des banques](docs/2026.1/DEVICE_LIBRARY_FORMAT.md)
- [Migration depuis la V3.6](docs/2026.1/MIGRATION_V3_6_TO_2026_1.md)
- [Performance](docs/2026.1/PERFORMANCE_REPORT.md)
- [Checklist Dante Controller](docs/2026.1/DANTE_CONTROLLER_MANUAL_VALIDATION.md)
- [Limites connues](KNOWN_LIMITATIONS.md)

## Quatre formats à ne pas confondre

### Projet StageFlow local `.stageflow`

Un projet StageFlow local est un dossier `.stageflow` commun aux outils de la suite.
DCE n'y modifie que `dante/dante.json` et les paquets placés sous `dante/`.
Les domaines de StageFlow, StageDesk, StageMark, AutoCAD ou d'autres outils sont
préservés. L'écriture prend d'abord le verrou Dante puis le verrou du manifeste,
relit les deux documents, conserve les ajouts concurrents des autres outils et
remplace atomiquement le seul domaine Dante. StageFlow est gratuit et
facultatif.

Le même projet peut aussi être ouvert par
[StageDesk](https://github.com/Mamat79/StageDesk), sans modifier
les identifiants historiques du domaine technique `smt`.

Pour ajouter Dante à un projet `.stageflow` qui n'en contient pas encore,
ouvrez-le dans DCE. Un choix immédiat propose **Créer depuis zéro**, **Ouvrir un
XML Dante** ou **Plus tard** :

- cliquez sur **Créer la configuration Dante** pour ouvrir immédiatement une
  configuration vide en mémoire, puis ajoutez les machines voulues depuis la
  banque ; aucun nom, chemin, XML ou première machine n'est imposé ;
- ou ouvrez un XML Dante existant, puis cliquez sur **Enregistrer** pour
  l'ajouter au projet.

Dans les deux cas, **Enregistrer** met à jour uniquement le domaine Dante du
projet StageFlow. Sous Windows, **Enregistrer sous** permet notamment d'exporter
un nouveau XML ou de créer un autre projet ; il refuse d'écraser un dossier
`.stageflow` existant. Sur Mac, **Fichier > Exporter vers Dante Controller** produit une
copie XML séparée, hors du dossier StageFlow, sans quitter le projet ni effacer
l'historique. **Enregistrer** et **Exporter** sont deux opérations distinctes.

Une fois la configuration créée, le bouton **Associer le patch StageFlow aux
canaux RX** apparaît directement sur la page Projet. La même action reste
disponible dans **Import / Export > Labels**. Elle permet de choisir un groupe,
un mode de nommage, une machine et une plage. DCE tient compte des paires
communes et des surcharges du groupe, ignore les cellules vides et ne modifie
rien avant l'aperçu puis **Appliquer**.

Lorsqu'une session StageFlow LIVE valide est présente, DCE affiche clairement
**LIVE connecté** et peut suivre les évolutions du patch. Seuls les canaux RX
déjà associés à un UUID explicite et à une règle de nommage enregistrée sont
mis à jour. Le suivi est activé par défaut, peut être coupé avec **Suivre les
sessions StageFlow LIVE**, et redevient strictement manuel si le bail disparaît
ou expire. Une modification locale non enregistrée, une règle devenue
orpheline ou un conflit de hash bloque l'automatisme et conserve le dernier
état valide. DCE ne crée jamais la session LIVE et ne pilote jamais le réseau
Dante réel.

Le bouton **Connexion StageFlow** garde son nom. Une seconde ligne indique
**Autonome**, **Session disponible**, **Connecté à [session]** ou **Déconnecté**.
Le même centre réunit **StageFlow LIVE** et **Télécommande Dante Config Editor**
dans un menu latéral, avec le projet courant et **Retour au projet**.
Dans la rubrique **StageFlow LIVE**, choisissez une session détectée
sur ce PC ou le réseau local,
vérifiez le projet et l'hôte, puis saisissez le code à six chiffres affiché par
StageFlow et cliquez sur **Rejoindre**. Les zéros initiaux sont conservés, y
compris lorsqu'un code est collé avec des espaces ou un tiret.

Si la liste est vide, vérifiez d'abord que StageFlow est ouvert et diffuse sa
session, puis actualisez. **Rechercher par adresse** accepte l'**IPv4 privée et le port** affichés
par StageFlow, puis cliquez sur **Rechercher**. Un code refusé ne ferme pas la
fenêtre : corrigez-le puis réessayez. DCE ne rejoint jamais une session tout
seul. Après une coupure ou un arrêt de l'hôte, une nouvelle connexion doit être
demandée explicitement. **Retour au projet** ferme le centre sans quitter une session
active ; **Se déconnecter** quitte la session et conserve le travail local.
La connexion ne nécessite aucun compte cloud ni abonnement. Ce transport LAN
doit être utilisé uniquement sur un réseau local de confiance ; ce n'est pas
une connexion distante chiffrée ni une télécommande du réseau Dante.

La rubrique **Télécommande Dante Config Editor** précise la cible réelle :
la **suite StageFlow**, pas une télécommande DCE autonome. Le QR, son activation,
ses droits et sa révocation se gèrent dans StageFlow, sur l'hôte de la session.
DCE ne reçoit ni ce lien ni son état actif/arrêté et n'affiche donc pas de faux QR.
Avec un projet chargé et les droits requis, les commandes existantes de la suite
peuvent ouvrir Patch / RX ou le centre de validation ; elles ne pilotent pas le
matériel Dante et ne remplacent pas l'édition du patch dans DCE.

Le projet publié par StageFlow peut être ouvert par cette connexion. S'il ne possède
pas encore de domaine Dante, DCE propose le même parcours **Créer depuis zéro /
Ouvrir un XML Dante / Plus tard**. Lors de l'enregistrement, le paquet `.dceproj`
est envoyé avant `dante/dante.json`, puis relu uniquement si son chemin, sa
taille et son SHA-256 correspondent au manifeste. Les domaines Patch, CAD, StageDesk,
StageMark et futurs restent inchangés.

Les **alertes labels** sont activées par défaut dès qu'un poste rejoint la
session LIVE. Les renommages humains TX/RX sont présentés avec l'ancien et le
nouveau label, l'élément concerné, l'origine, l'heure et un compteur persistant
dans un bandeau orange accessible depuis chaque écran. **Acquitter** traite le
changement affiché ; **Tout acquitter** traite uniquement les alertes présentes
au clic, jamais celles qui arrivent ensuite. **Détails** ouvre la liste. Chaque ordinateur
acquitte les alertes qu'il reçoit et peut les couper localement : DCE synchronise
alors ce choix avec StageFlow, acquitte le stock de ce seul destinataire et ne
lui accumule aucune alerte pendant la coupure. À la réactivation, seules les
nouvelles alertes arrivent. Les autres logiciels ne sont pas affectés. L'hôte
peut aussi suspendre les seules notifications de modifications : DCE l'indique
distinctement d'une réception coupée localement. La synchronisation LIVE reste
active, sans reconnexion ni rattrapage d'alertes à la reprise. Les messages de
connexion et de sécurité restent actifs. Un serveur qui refuse les alertes ne
bloque jamais le renommage ni l'enregistrement. Cette liaison transporte le
projet de préparation, pas des commandes vers les équipements Dante.

Sous Windows, StageFlow peut aussi détecter l'instance Dante Config Editor ouverte et
lui demander d'afficher Patch / RX, le centre de validation ou d'enregistrer
son domaine Dante. Cette console locale est facultative : elle utilise un pipe
réservé à l'utilisateur courant, un nonce propre à l'instance et vérifie les
UUID du projet et de la session. Elle n'expose aucune commande réseau Dante et
ne remplace ni le travail autonome dans Dante Config Editor ni l'ouverture
directe d'un `.stageflow`. Si un autre projet contient des modifications, le
choix **Enregistrer / Abandonner / Annuler** reste entièrement dans Dante Config
Editor avant toute bascule.

![Architecture de la suite SiLeMIO](docs/media/ecosystem/suite-architecture-fr.svg)

![Parcours conseillé de la suite SiLeMIO](docs/media/ecosystem/suite-workflow-fr.svg)

### XML Dante

Le XML reste le fichier destiné à Dante Controller. DCE modifie le document
d’origine de manière ciblée afin de conserver les nœuds, attributs,
namespaces, valeurs et extensions inconnues. La sauvegarde utilise un fichier
temporaire, une relecture, une validation, une copie de sécurité puis un
remplacement sécurisé de la destination.

### Ancien projet `.dceproj`

Le `.dceproj` est l'ancien conteneur de travail propre à DCE. Il reste lisible
pour assurer la migration. Il peut conserver le
XML, le nom du projet, la disposition, les annotations, le journal, les
références de banque et les ressources DCE. Il ne doit jamais être importé
directement dans Dante Controller : exportez d’abord son XML Dante.

### Banque de machines

Une banque contient des modèles réutilisables et partageables. Une insertion
crée une instance indépendante ; elle ne lie pas le projet au modèle source.
Les identités matérielles, IP, flows et subscriptions ne sont pas recopiés par
défaut. Les banques `DCE Generic` et `DCE Community` sont intégrées à
l'application. Elles fournissent 79 modèles, dont 77 modèles communautaires
illustrés et assainis. Parmi eux, 57 ont été testés sur matériel réel et 20
reposent sur des caractéristiques constructeur avec validation structurelle
hors ligne. Les dossiers livrés restent versionnés, mais DCE ne
présente qu'une banque logique par famille et sélectionne automatiquement sa
génération la plus récente.
La banque couvre notamment Yamaha CL/QL/DM/TF/RIVAGE, Allen & Heath
dLive/Avantis/SQ, DiGiCo, Focusrite RedNet et Neutrik via leurs appareils,
cartes ou interfaces Dante documentés.
Les rôles Yamaha DM7 et DM7 Compact sont inclus en 144 TX / 144 RX.
La banque contient deux rôles LM44 distincts : `8 TX / 4 RX` et `0 TX / 4 RX`.
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
Pour les modèles Yamaha classés comme console ou boîtier d'entrées/sorties,
DCE propose le préfixe de nom machine `Y001-`, puis le prochain identifiant
libre. Cette aide suit la convention décrite par Yamaha pour certaines fonctions
d'identification et de contrôle entre équipements compatibles ; ce n'est pas
une règle universelle Dante. Le nom reste modifiable, la séquence évite les
collisions et aucun appareil existant ou importé n'est renommé.
Références constructeur : [guide système Yamaha CL/QL](https://download.yamaha.com/files/tcm%3A39-1251310)
et [manuel Yamaha Rio-D3](https://usa.yamaha.com/files/download/other_assets/7/2345707/Rio3224-D3_reference_manual_En_A0.pdf).

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
- création, ouverture et sauvegarde autonome de projets `.stageflow` ;
- synchronisation du seul domaine Dante avec verrou, hash de base et reprise
  après modification externe ;
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
  ouverture manuelle du capot, puis ARM, LOCK et FIRE.

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

## Installer Dante Config Editor sous Windows

La [Release 2027.2.2](https://github.com/Mamat79/Dante-Config-Editor/releases/tag/v2027.2.2)
regroupe l'installateur Windows, les deux paquets Mac, les notices et les banques
fournies. Les artefacts intermédiaires de construction ne constituent pas une distribution.

### Windows 11 x64

Artefact : `DCE-2027-Windows-Installer`

Fichier : `DanteConfigEditor2027_Installer.exe`

### macOS 2027

- `DanteConfigEditor2027_macOS_AppleSilicon.dmg`
- `DanteConfigEditor2027_macOS_Intel.dmg`

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

## Licence et essai

Dante Config Editor propose un essai de 30 jours. Son rappel au démarrage est
immédiatement refermable pendant l'essai. Après cette période, un décompte de
60 secondes apparaît une fois par lancement, puis toutes les fonctions restent
accessibles. Une activation valide libère immédiatement l'interface. Une licence
commerciale ou offerte, vérifiée localement par signature cryptographique,
supprime le rappel et l'attente.
Le fonctionnement complet et les limites de confidentialité sont décrits dans
[docs/LICENSING_DCE.md](docs/LICENSING_DCE.md).

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
