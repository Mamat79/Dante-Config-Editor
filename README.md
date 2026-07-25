# Dante Config Editor V3.6 - branche de développement

## Français

La V3.6 est développée et testée dans la branche `v3.6`, à partir de la V3.5. La V3.4.2 officielle reste publiée dans `main`.

**Version stable : [Release V3.4.2 Windows et macOS](https://github.com/Mamat79/DanteConfigEditorV3/releases/tag/v3.4.2)**

> **Statut : V3.6 en développement. Outil tiers non officiel Audinate.**
> Cette branche peut encore contenir des bugs. Travaillez toujours sur une copie et validez le XML généré dans les outils Dante officiels.

**Documentation V3.6 :**
[notice complète FR](docs/Notice_DanteConfigEditorV3_FR.pdf) ·
[full English guide](docs/Notice_DanteConfigEditorV3_EN.pdf)

**Présentations historiques V3.5 :**
[vidéo FR](docs/media/dce-v35-presentation-fr.mp4) ·
[English video](docs/media/dce-v35-presentation-en.mp4)

> **Import et export de labels en JSON, CSV, DMT XLSX/ODS pour dLive et Avantis, A&H CSV et Yamaha CL/QL ZIP/CSV.** Les modèles natifs sont inclus dans l'application : aucun fichier modèle externe n'est nécessaire pour exporter.

## Nouveautés V3.6

- audit documenté de la fidélité XML, des performances et des risques dans [AUDIT_V3_6.md](AUDIT_V3_6.md) ;
- bilan détaillé des choix, tests et limites dans [V3_6_IMPLEMENTATION_REPORT.md](V3_6_IMPLEMENTATION_REPORT.md) ;
- validation renforcée des identifiants, références, canaux, subscriptions, namespaces et ajouts de nœuds ;
- duplication prudente d'une machine sous forme de rôle générique, sans recopier les identifiants matériels ;
- banque de machines versionnée et partageable avec métadonnées, labels, image facultative, recherche, import/export et catalogue GitHub ;
- ajout transactionnel d'une instance indépendante depuis la banque ;
- création expérimentale d'un projet minimal 3.0.0, à valider impérativement par un import réel dans Dante Controller ;
- journaux techniques accessibles depuis l'application ;
- 271 tests Core/Windows et 20 tests Mac sans écran.

La duplication et la banque ne fabriquent pas un appareil Dante réel. Elles créent des rôles de preset génériques dépourvus de `instance_id` et `device_id`. Aucun import V3.6 n'ayant encore été effectué dans Dante Controller, la compatibilité terrain n'est pas présentée comme garantie.

## Origine et développement assisté

Dante Config Editor est né d'une tentative de pallier ce qui me manquait dans Dante Controller. À l'origine, c'était un petit logiciel personnel écrit manuellement pour répondre à un besoin de terrain : vérifier rapidement une configuration Dante sans devoir ouvrir successivement toutes les pages du logiciel. L'objectif était de disposer d'une vue d'ensemble unique des devices, latences, fréquences d'échantillonnage, modes réseau, Preferred Master, adresses IP, etc, puis de pouvoir corriger au besoin les valeurs présentes dans le preset.

Un autre problème récurrent concernait les renommages sur un réseau déjà patché. Changer le nom d'une machine ou de canaux TX peut obliger à reprendre les subscriptions concernées et à repasser par une phase de patch. L'éditeur a donc été conçu pour mettre à jour les références XML reconnues lors d'un renommage et préserver le patch autant que la structure du preset le permet.

Enfin, le workflow hors ligne n'existant pas dans Dante Controller, ce soft tente de répondre au besoin d'une préparation globale et rapide. Pouvoir contrôler, modifier, fusionner et préparer un preset sans être connecté au réseau Dante est ainsi devenu l'un des objectifs centraux du projet.

L'arrivée des agents de développement actuels a permis de faire évoluer ce simple éditeur XML beaucoup plus rapidement : sécurisation des sauvegardes, tests de non-régression, interface bilingue, installateurs autonomes, version macOS, rapports et outils de patch plus élaborés. Les besoins métier, les choix fonctionnels et la validation d'usage restent dirigés par Mamat ; les agents participent à l'analyse, au développement, aux tests et à la documentation.

## Import et export de labels

L'espace `Import / Export` regroupe l'échange de noms de canaux pour une ou plusieurs machines. L'utilisateur peut exporter les labels TX ou RX en JSON ou CSV, les réimporter avec choix des machines et des plages, puis vérifier chaque correspondance avant application. Les renommages TX continuent de mettre à jour les subscriptions XML reconnues.

Le CSV générique est proposé en premier et demande uniquement un nom de fichier et un dossier de destination. Il sert aux échanges avec Dante Config Editor et d'autres outils génériques ; il ne doit pas être importé directement dans dLive Director. Les formats natifs DMT, A&H et Yamaha utilisent les modèles dLive, Avantis, CL ou QL inclus dans l'application et demandent seulement le nom du nouveau fichier.

Les formats JSON et CSV restent génériques. Des profils natifs permettent aussi les échanges avec les consoles et outils suivants :

- **DMT → Dante Config Editor** : lecture directe de la feuille `Channels` d'un classeur XLSX ou ODS DMT, puis affectation des labels aux TX ou RX d'une ou plusieurs machines Dante.
- **DMT 2.14.0-RC1 → Dante Config Editor** : les exports JSON et CSV de la branche `feature/add-json-export` sont couverts par des fixtures reproduisant exactement la sortie des exporteurs DMT au commit `3c34052`.
- **Dante Config Editor → DMT** : création directe d'un classeur XLSX ou ODS dLive/Avantis depuis l'un des quatre modèles DMT inclus, avec désactivation des lignes absentes de la sélection.
- **Projet DMT** : [togrupe/dlive-midi-tools](https://github.com/togrupe/dlive-midi-tools).
- **Allen & Heath dLive / Avantis** : lecture d'un export CSV existant ou création directe d'un nouveau CSV natif depuis le modèle inclus ; seule la colonne des noms `Input` est modifiée.
- **Yamaha CL / QL** : lecture d'un package ZIP ou d'un fichier `InName.csv`, et création directe d'un nouveau ZIP complet depuis le modèle inclus ; les huit autres CSV du package restent inchangés.

Les modèles internes ne sont jamais modifiés : chaque export crée un nouveau fichier. Une machine sans TX mais avec des RX bascule automatiquement sur RX, et les machines sans canal dans le sens choisi ne sont pas sélectionnables. Toute adaptation en ASCII sur huit caractères est affichée dans l'aperçu et doit être activée explicitement ; JSON et CSV génériques conservent les labels complets Unicode. Il s'agit d'une passerelle de fichiers hors ligne, pas d'une connexion directe ou temps réel entre les logiciels.

## Synoptique visuel

La V3.2 ajoute un synoptique en couleur dans `Import / Export > Synoptique`. Chaque machine peut recevoir un emplacement physique, être affichée ou masquée en un clic et être réordonnée. Les emplacements déjà saisis restent disponibles dans une liste. Les subscriptions consécutives entre deux machines sont regroupées dans un seul câble, par exemple `TX 1-32 vers RX 1-32`, et les liaisons nombreuses sont réparties sur des points d'arrivée distincts.

Le synoptique peut être ouvert dans une fenêtre séparée, avec un zoom qui conserve toujours ses proportions, puis exporté en SVG ou en PDF vectoriel. Ces exports et le fichier local de mise en page ne modifient jamais le XML Dante chargé.

Les emplacements et choix de présentation sont enregistrés dans un petit fichier local séparé. Ils ne sont jamais ajoutés au XML Dante. L'export SVG contient les machines, les câbles numérotés et une légende détaillée ; il peut être ouvert dans un navigateur, imprimé ou intégré à un dossier technique.

## Banques de machines partageables

La fenêtre `Banque de machines` permet d'exporter une banque complète dans une archive vérifiée `*.dce-bank.zip`, puis d'importer une banque téléchargée dans un dossier neuf ou vide. L'import contrôle le manifeste, les empreintes SHA-256 et chaque modèle avant de changer de banque ; aucun fichier existant n'est remplacé.

Le bouton `Banques GitHub` ouvre le [catalogue public des banques V3.6](machine-banks/README.md). La banque fournie `DCE Generic Roles 3.6` contient deux rôles génériques 8x8 et 32x32 pour les essais et la formation. Ils ne représentent aucun matériel réel et ne contiennent ni identité matérielle, ni IP, ni abonnement.

L'installateur Windows propose séparément le dossier de banque actif et le dossier où placer les banques fournies. Il réutilise le chemin déjà configuré lors d'une mise à jour, ne touche jamais au contenu d'une banque existante et choisit un nouveau nom de dossier si la banque fournie est déjà présente.

## Notices

- **[Lire la notice complète en français (PDF)](docs/Notice_DanteConfigEditorV3_FR.pdf)** ou le [démarrage rapide](docs/QuickStart_DanteConfigEditorV3_FR.pdf).
- **[Read the full English guide (PDF)](docs/Notice_DanteConfigEditorV3_EN.pdf)** or the [English quick start](docs/QuickStart_DanteConfigEditorV3_EN.pdf).

Les captures et notices utilisent uniquement un preset synthétique anonymisé. Elles ne contiennent aucun nom de machine, fichier ou chemin de production.

## Vidéos de présentation

- **[Présentation française de DCE v3.5](docs/media/dce-v35-presentation-fr.mp4)**
- **[English presentation of DCE v3.5](docs/media/dce-v35-presentation-en.mp4)**

Chaque vidéo dure 55 secondes, sans voix ni piste audio, avec le texte directement intégré à l'image. Les fichiers `.srt` séparés et les sommes SHA-256 sont fournis dans `docs/media`. Les écrans ont été réalisés avec un preset synthétique anonymisé.

## Ce que fait l'application

- Ouvre des fichiers XML de configuration Dante hors ligne.
- Affiche les devices, canaux TX/RX, latences, mode réseau et preferred master.
- Renomme les devices.
- Supprime un device et nettoie les subscriptions/patchs qui pointent vers lui.
- Duplique un device sous forme de rôle générique indépendant, avec conservation optionnelle des labels et réglages sûrs.
- Enregistre, recherche, modifie et partage des modèles ou des banques complètes, avec accès au catalogue GitHub.
- Ajoute au projet une nouvelle instance indépendante issue de la banque.
- Crée expérimentalement un projet minimal 3.0.0 vide ou amorcé depuis la banque.
- Ajoute les devices d'un second XML dans le projet ouvert, avec import des machines uniques même en présence de doublons.
- Propose de renommer automatiquement ou manuellement les machines en doublon pendant l'import XML.
- Renomme les canaux TX/RX.
- Renomme des plages de canaux en série.
- Importe et exporte des labels de canaux pour une ou plusieurs machines en JSON, CSV, classeur DMT XLSX/ODS, CSV A&H dLive/Avantis ou package Yamaha CL/QL, avec plages et prévisualisation.
- Regroupe les échanges de labels, les rapports, les patchbooks et le synoptique dans un onglet `Import / Export` organisé en trois sous-onglets.
- Produit un synoptique SVG en couleur, avec emplacements, machines masquables, ordre personnalisable et câbles consécutifs regroupés.
- Conserve les informations de mise en page du synoptique hors du XML Dante dans un fichier local séparé.
- Réinitialise les noms de canaux.
- Modifie les paramètres réseau et audio exposés par les fichiers XML reconnus.
- Affiche une page Patch pour visualiser et modifier les abonnements RX vers TX lorsque le format XML le permet.
- Ajoute l'onglet Windows `Easy patch` avec RX à gauche, TX à droite, navigation rapide entre machines, application immédiate, plages strictes, alerte optionnelle pour les RX déjà patchés et matrice interactive compacte avec glissement en série.
- Dans `Easy patch`, propose un bouton `FLIP TX ⇄ RX` très visible, un patch `1:1` accessible depuis `Sélection et plage` ou directement depuis la grille, et la navigation Tab/Maj+Tab pendant le renommage.
- Dans la matrice `Easy patch`, un clic sur un libellé TX vertical ouvre son renommage direct ; Entrée valide, Tab/Maj+Tab naviguent, Échap annule.
- La poignée de recopie n'apparaît que pour un nom terminé par un nombre et conserve les zéros initiaux (`Mic 04` devient `Mic 05`, `Mic 06`, etc.).
- Met à jour les patchs RX quand un canal TX utilisé est renommé.
- Crée une sauvegarde du fichier source et de toute destination existante avant sauvegarde.
- Sauvegarde via un fichier temporaire relu puis un remplacement atomique de la destination.
- Vérifie la compatibilité XML de base avant sauvegarde.
- Bloque les modifications interdites des zones sensibles du XML Dante grâce au garde-fou de changements XML.
- Permet de modifier l'interface après ouverture d'un XML, mais impose une sauvegarde sous un autre nom pour protéger le fichier original.
- Propose un choix de langue Français / Anglais dans l'application.
- Affiche les latences Dante en ms tout en conservant les valeurs XML brutes.
- Affiche et modifie les sample rates et les bits par échantillon machine par machine ou globalement.
- Signale les projets qui mélangent plusieurs sample rates ou plusieurs encodages.
- Peut remettre les adresses IPv4 des machines en automatique quand le XML contient ces informations.
- Peut fixer une IP manuelle sur une machine ou générer une plage d'IP fixes en série.
- Ouvre une fiche machine au double-clic pour modifier les formats, l'IP, les canaux TX/RX et les patchs de ses entrées RX.
- Filtre, sélectionne et verrouille des machines pour contrôler précisément les actions globales.
- Réinitialise les patchs RX/TX d'une machine, ou seulement ses RX / seulement ses TX.
- Ouvre une notice rapide PDF et une notice complète PDF dans la langue active de l'application.
- Affiche des info-bulles sur les principales fonctions.
- Permet de cliquer sur un point important pour filtrer immédiatement les machines concernées.
- Filtre les machines modifiées et affiche une comparaison détaillée avant / après.
- Enregistre automatiquement une copie de récupération asynchrone après un court délai suivant les modifications.
- Applique des profils rapides 48/96 kHz, 24 bit, latence, mode réseau et IP automatique à la cible choisie.
- Affiche une page `Santé du fichier` avec les points à vérifier.
- Annule la dernière action.
- Recherche globalement dans les machines, canaux et patchs.
- Exporte un rapport TXT ou PDF.
- Exporte un patchbook TXT organisé par device RX.
- Exporte un patchbook CSV en lecture seule.
- Affiche un rapport compatibilité Dante Controller et une topologie simple TX vers RX.
- Génère, avec `Atomic Bomb`, un preset d'exercice volontairement désorganisé après trois confirmations ; chaque catégorie peut être décochée pour protéger les éléments que le formateur veut conserver.
- Compare le fichier ouvert avec un autre XML.
- Renomme des canaux en série.
- Signale en gros les mélanges redondant/daisychain, les machines détectées en IP fixe et les formats audio mélangés.
- Affiche les fichiers récents.
- Propose un thème sombre et un thème clair.

## Limites importantes

- L'application ne pilote pas un réseau Dante en direct.
- Elle n'utilise pas de SDK ou d'API Audinate.
- Elle travaille uniquement sur des fichiers XML hors ligne.
- Elle ne contourne aucune protection Audinate et ne réimplémente pas de protocole propriétaire.
- La compatibilité dépend de la structure réelle du XML fourni.
- Les rôles génériques ne portent pas d'identité matérielle et doivent être validés dans Dante Controller.
- La création d'un nouveau projet est expérimentale et ne constitue pas une garantie d'import.
- Certains champs de patch peuvent ne pas être détectés si le fichier utilise une structure différente de celles actuellement reconnues.
- `subscribed_device="."` est interprété comme une source locale, c'est-à-dire le device RX lui-même.
- Un device TX absent du preset est un avertissement, pas forcément une erreur bloquante, car un preset peut être partiel.
- Les Dante Id sont préservés. L'interface écrit `Dante Id`, mais l'attribut XML reste exactement `danteId`.

## Télécharger / installer

La [Release GitHub V3.4.2](https://github.com/Mamat79/DanteConfigEditorV3/releases/tag/v3.4.2) reste la version stable. Les paquets de développement V3.6 sont générés dans les exécutions `Windows CI` et `macOS CI` de la branche `v3.6`.

- Windows x64 : artefact `DCE-v3.6-Windows-Installer`, contenant `DanteConfigEditorV3_6_Installer.exe` et sa somme SHA-256.

La version autonome inclut le runtime .NET nécessaire. Sur une machine Windows x64, il ne devrait pas être nécessaire d'installer .NET séparément pour utiliser l'application.

L'assistant permet de choisir le dossier de banque actif et le dossier des banques fournies. L'installation de `DCE Generic Roles 3.6` est proposée par défaut lors d'une première installation et reste facultative. Une mise à jour conserve le chemin existant et ne remplace aucune banque.

### macOS

Deux DMG autonomes sont fournis :

- `DanteConfigEditorV3_6_macOS_AppleSilicon.dmg` pour les Mac M1, M2, M3, M4 et suivants ;
- `DanteConfigEditorV3_6_macOS_Intel.dmg` pour les Mac Intel 64 bits.

Ouvrir le DMG, puis glisser `Dante Config Editor V3.6` dans `Applications`. Le runtime .NET 8 et les notices FR/EN sont inclus. Son bundle distinct permet de conserver la V3.4.2.

La distribution Mac n'est pas encore notariée avec un compte Apple Developer. Au premier lancement, faire un clic droit sur l'application dans `Applications`, choisir `Ouvrir`, puis confirmer l'ouverture. Les détails de compilation, signature et notarisation sont documentés dans `MACOS_BUILD.md`.

L'installateur contient uniquement l'application autonome et la documentation utilisateur. Les sources du projet ne sont pas installees sur la machine de l'utilisateur.

En fin d'installation, il peut proposer d'ouvrir les release notes, le quick start PDF et la notice complète PDF dans la langue choisie dans l'installateur. Les quatre PDF français/anglais restent installés et accessibles depuis le menu Démarrer.

Notices fournies :

- `QuickStart_DanteConfigEditorV3_FR.pdf` et `Notice_DanteConfigEditorV3_FR.pdf` ;
- `QuickStart_DanteConfigEditorV3_EN.pdf` et `Notice_DanteConfigEditorV3_EN.pdf`.

Dans l'application, les boutons d'aide ouvrent automatiquement les fichiers FR ou EN selon la langue active.

L'installateur V3.6 utilise l'AppId de la ligne de développement : il remplace une V3.5 déjà installée, nettoie ses anciens raccourcis et conserve les données locales de travail. Une installation neuve utilise `C:\Program Files\Dante Config Editor V3.6\`. L'AppId distinct de la V3.4.2 stable reste intact.

## Version distribuée

- La branche `main` contient la V3.4 officielle pour Windows et macOS.
- La branche `v3.6` contient la version de développement et ses workflows Windows/macOS ; elle ne remplace pas encore la release stable.
- Le tag immuable `v3.4.2` identifie le code source de la Release officielle actuellement marquée `Latest`.
- Chaque version utilise un tag immuable distinct selon [la politique de publication](RELEASE_POLICY.md).
- L'historique fonctionnel reste également consultable dans les commits de `main` et dans `CHANGELOG_V3.md`.

## Utilisation rapide

1. Lancer l'application.
2. Cliquer sur `Ouvrir XML`.
3. Sélectionner une copie du fichier de configuration Dante.
4. Vérifier les devices et paramètres détectés.
5. Modifier les champs souhaités.
6. Dans l'onglet `Easy patch`, choisir les machines RX et TX, puis cliquer ou glisser dans la grille : chaque point de patch est appliqué immédiatement. `PATCH 1:1` applique directement une série.
7. Laisser `M'avertir si le RX est déjà patché` coché pour confirmer chaque remplacement, ou le décocher pour remplacer sans cette alerte.
8. Utiliser `Annuler action` pour revenir sur une opération de patch si nécessaire.
9. Si besoin, utiliser `Ajouter XML au projet` pour importer les devices d'un autre export XML.
10. Sauvegarder sous un nouveau nom.
11. Valider le fichier généré dans l'outil Dante officiel approprié avant usage terrain.

## Atomic Bomb : exercice de dépannage

`Atomic Bomb`, rangé dans son propre onglet après `Sécurité et journal`, sert à préparer un réseau volontairement en désordre pour une formation. Son panneau **Générateur d'expérience horrible (mais pédagogique)** permet de décocher les catégories à épargner avant les trois confirmations. Il agit uniquement sur la copie XML chargée en mémoire.

Merci à **Charles Bouticourt** pour l'idée de cette fonction de formation.

Les machines reçoivent des noms uniques choisis dans un catalogue mythologique, audio et volontairement décalé, comme `ATHENA`, `RAVENNA`, `PYRAMIX`, `INFERNO` ou `PATCHOS`. Les noms ne suivent donc pas un préfixe uniforme facilement reconnaissable.

- Les noms des machines et des canaux TX/RX sont remplacés par des noms d'exercice.
- Les modes redondant/daisy-chain, Preferred Master, latences, sample rates, encodages et modes IP sont volontairement mélangés.
- Les patchs sont redistribués et environ un quart des RX sont laissés libres.
- Une graine affichée dans le résumé permet de reproduire exactement un scénario pendant les tests automatisés.
- Les identifiants techniques `device_id`, `danteId` et `mediaType`, ainsi que le DNS, les passerelles et les interfaces secondaires, restent protégés.
- Toute l'opération correspond à une seule étape d'annulation.
- Le fichier source n'est jamais écrit : utilisez `Enregistrer sous` pour créer le preset destiné aux stagiaires.

Toutes les cases sont cochées par défaut. Il est possible de protéger indépendamment les noms de machines, labels TX, labels RX, patchs, modes réseau, Preferred Master, latences, fréquences, bits et IP principales. Si les patchs sont protégés pendant un renommage, les références reconnues suivent les nouveaux noms afin de conserver le routage.

Le résultat est volontairement incohérent sur le plan fonctionnel. Il reste indispensable de l'importer dans l'outil Dante officiel approprié avant de l'utiliser comme support d'exercice.

## Nouveautés V3.4

- Renommage direct des machines et canaux depuis Patch et des canaux TX/RX depuis Easy patch.
- Extension de séries numériques de noms avec une poignée de glisser, sur le principe d'Excel.
- Grille Easy patch affichée en premier, puis mode `Sélection et plage`.
- Filtres Patch réorganisés avec RX au-dessus de TX.
- Action globale pour conserver une seule machine comme Preferred Master.
- Reset du synoptique : suppression des déplacements manuels et reconstruction d'un ordre propre par emplacement.
- Une seule ligne à deux flèches représente désormais les flux réellement bidirectionnels entre deux machines.
- Réglages Configuration visibles au premier lancement et contrôles agrandis pour améliorer la lecture à 125 % sous Windows.
- Zone Atomic Bomb agrandie.

## Nouveautés V3.3

- Import direct des fichiers DMT XLSX et ODS, avec export vers quatre modèles dLive/Avantis intégrés.
- Préservation des feuilles et styles hors `Channels` dans les copies ODS générées.
- Choix détaillé des catégories modifiées par `Atomic Bomb`, toutes cochées par défaut.
- Corrections macOS Full HD, traductions et explication du bouton `Appliquer` désactivé après un import identique.
- Installateur V3.3 remplaçant la V3.2 tout en conservant les données locales.

## Nouveautés V3.2

- Nouvel onglet principal `Import / Export`, organisé en `Labels`, `Rapports et patchbook` et `Synoptique`.
- Création d'un synoptique visuel en couleur à partir du projet ouvert.
- Affectation d'emplacements physiques, affichage/masquage et ordre personnalisé des machines.
- Regroupement des subscriptions consécutives en câbles synthétiques avec une légende séparée.
- Routage orthogonal, troncs partagés et légende sur deux colonnes pour les synoptiques denses.
- Export SVG autonome ; les informations de mise en page restent dans un fichier local séparé et ne modifient jamais le XML Dante.
- Import/export natif A&H dLive/Avantis CSV et Yamaha CL/QL ZIP/CSV, en plus de DMT XLSX et des formats génériques.
- Installateur V3.2 officiel remplaçant les anciennes installations V3.

## Nouveautés V3.1

- Échange de labels TX/RX pour une ou plusieurs machines avec sélection des plages et aperçu avant application.
- Formats JSON et CSV documentés pour les échanges génériques et les collaborations avec d'autres outils.
- Lecture de classeurs XLSX issus de [dLive MIDI Tools](https://github.com/togrupe/dlive-midi-tools) et export direct d'un modèle dLive ou Avantis inclus, avec adaptation DMT ASCII/8 caractères uniquement sur demande explicite.
- Même workflow de labels sur Windows et macOS, fondé sur le moteur XML partagé.
- Déplacement d'`Atomic Bomb` dans un onglet dédié afin qu'il ne monopolise plus la navigation principale.
- Installateur V3.1 remplaçant proprement les V3.07, V3.08 et V3.09 installées.
- Nouveau tag immuable `v3.1` ; les tags `v3.08` et `v3.09` restent dans l'historique Git, même après le retrait ultérieur de leurs pages de Release en V3.3.

## Nouveautés V3.09

- Générateur d'exercice `Atomic Bomb` partagé par Windows et macOS, protégé par trois confirmations, une sauvegarde sous un autre nom et des tests de non-régression XML.
- Mélange volontaire des noms, canaux, patchs, modes réseau, Preferred Master, latences, sample rates, encodages et IP principales.
- Conservation des identifiants techniques Dante, namespaces, DNS, passerelles et interfaces secondaires.
- Installation V3.09 remplaçant proprement les anciennes V3.07/V3.08.

## Nouveautés V3.08

- Sélection multiple indépendante des canaux TX et RX avec `Ctrl` ou `Maj`, plus commandes `Tout sélectionner`.
- Appariement un-à-un quand les quantités sont égales et diffusion autorisée d'un seul TX vers plusieurs RX.
- Blocage de plusieurs TX vers un seul RX et de toute sélection multiple aux quantités incohérentes.
- Patch par plage avec premier TX, premier RX et nombre exact ; aucune application partielle si la plage dépasse les canaux disponibles.
- Prévisualisation lisible sans ascenseur général : les machines restent visibles et chaque ligne indique le RX, la source actuelle, la nouvelle source et l'action.
- Application directe facultative pour la sélection et le patch par plage.
- Chaque prévisualisation rejoint automatiquement un lot cumulatif visible ; le XML reste inchangé jusqu'à `Appliquer tout le lot`.
- Grille plus dense avec cellules `28 × 22`, numéros TX compacts et noms complets au survol.
- Glissement maintenu dans la grille : horizontal pour une série TX/RX, vertical pour diffuser un TX vers plusieurs RX et diagonal pour une série un-à-un.
- Traitement explicite des RX déjà patchés : annuler le lot, ignorer les conflits ou remplacer les subscriptions.
- Déconnexion groupée de plusieurs RX et matrice unitaire TX vers RX conservée.
- Changements ajoutés en mémoire puis appliqués au XML en un seul lot et une seule étape d'annulation.
- Onglets principaux distincts : `Patch` conserve l'éditeur classique et `Easy patch` accueille le nouveau système.
- Disposition `Easy patch` avec machines et canaux RX à gauche, machines et canaux TX à droite.
- Menus et flèches précédent/suivant pour parcourir rapidement les machines RX et TX.
- Sélecteur de machine dans `Détail machine`, avec confirmation appliquer/abandonner/annuler si des changements sont en attente.
- Onglet `Patch RX` dans `Détail machine`, limité aux entrées de la machine ouverte.
- Identité d'installation, dossier Program Files, raccourcis et données locales séparés de la V3.07.
- Paquets macOS V3.08 autonomes Apple Silicon et Intel construits et vérifiés sur GitHub Actions macOS.
- Le moteur XML est commun aux deux plateformes. macOS conserve son atelier visuel Avalonia ; le nouvel onglet Windows `Easy patch` n'y est pas encore reproduit à l'identique.
- Suites automatisées couvrant notamment la sélection, les plages, les conflits, le rollback, la persistance, l'affichage Easy patch, la navigation du détail machine et l'interface Mac sans écran.

## Nouveautés V3.07

- Garde-fou XML associé aux machines par identité technique stable et non plus par leur nom visible.
- Blocage par défaut des chemins et balises XML inconnus.
- Comparaison XML sémantique qui tolère le réordonnancement de balises inchangées.
- Lecture, modification et sauvegarde des presets utilisant un namespace XML par défaut.
- `Enregistrer sous` atomique : l'ancienne destination reste intacte en cas d'échec et reçoit une copie de sécurité lors d'un remplacement.
- Le fichier créé par `Enregistrer sous` devient la référence de la session et de la récupération automatique.
- Les changements IPv4 ciblent uniquement l'interface principale et ne réécrivent plus implicitement le DNS ou une interface secondaire.
- Mutations groupées avec une seule reconstruction du modèle pour les réglages de détail et les actions globales.
- Récupération automatique asynchrone et temporisée ; historique d'annulation limité à 10 états.
- 67 tests de sécurité et de non-régression, dont les presets synthétiques de 10, 50 et 200 machines en 64 TX / 64 RX.
- Workflow GitHub Actions sur Windows et script `build.ps1` qui vérifie chaque code de retour.
- Installateur de mise à niveau unique : la V3.07 remplace la version déjà installée sans proposer de copie parallèle.
- Nouvelle interface macOS via Avalonia, avec les alertes placées dans la colonne latérale pour préserver la hauteur des tableaux.
- Sept tests d'interface sans écran vérifient la disposition, la navigation clavier, les tailles compactes et l'atelier de patch sur Mac.
- DMG autonomes distincts Apple Silicon et Intel, incluant .NET 8 et les notices françaises/anglaises.
- Les alertes importantes de la version Windows sont également déplacées dans la colonne latérale.
- Atelier de patch visuel partagé : sélection de plusieurs TX, affectation aux RX suivants, glisser-déposer, matrice et application en un seul lot annulable.
- Interface principale plus compacte ; le reset global des canaux est placé sous la latence globale pour réduire la hauteur du panneau.

## Validation et maintenance

- `TESTING.md` : commandes, résultats et mesures synthétiques ;
- `COMPATIBILITY_MATRIX.md` : niveau de preuve par structure XML ;
- `MANUAL_DANTE_CONTROLLER_TESTS.md` : checklist d'import réel ;
- `RC_VALIDATION.md` : preuves historiques de validation de la publication stable V3.07 ;
- `ACCESSIBILITY.md` : contrôles effectués et tests manuels restants ;
- `KNOWN_LIMITATIONS.md` : limites techniques et de distribution ;
- `ARCHITECTURE_REFACTORING.md` : extractions réalisées et suite prudente.

## Nouveautés V3.05

- Les alertes `Points à vérifier` ouvrent la liste des machines concernées et permettent de les filtrer dans le tableau principal.
- Le filtre `Modifiées uniquement` et le bouton `Avant / après` permettent de contrôler précisément les changements depuis l'ouverture du XML.
- Les messages dynamiques de ces nouvelles vues suivent le choix Français / Anglais de l'interface.
- Une récupération automatique protège les modifications non sauvegardées après une fermeture inattendue ; l'utilisateur choisit de restaurer ou d'abandonner la copie temporaire à la prochaine ouverture du même fichier.
- Des profils rapides 48/96 kHz et 24 bit combinent latence, IP automatique et, selon le profil, mode redondant ou daisychain.
- Les profils respectent la cible des actions globales et ignorent les machines verrouillées.
- Une suite de tests vérifie le chargement XML, les renommages et leurs patchs, les imports, la suppression, les profils, la récupération et la sauvegarde sécurisée.
- Un seul bouton `Appliquer les paramètres` valide ensemble le nom, le mode réseau, la latence et le preferred master de la machine sélectionnée.
- Les IP automatique/fixe, sample rate, bits et noms de canaux restent modifiables dans `Détail machine`.
- L'alerte `Points à vérifier` reste compacte et le bouton `Détails` affiche son texte complet.
- Le bouton `Réduire les réglages` agrandit le tableau des machines quand les panneaux supérieurs ne sont pas nécessaires.
- Les resets patch RX/TX, RX, TX et la suppression de machine sont regroupés sur une seule ligne.
- Import d'un second XML corrigé : les machines uniques sont importées même si d'autres machines du fichier sont en doublon.
- Gestion des doublons à l'import : choix entre import des uniques seulement, renommage automatique ou renommage manuel.
- Les patchs/subscriptions du XML importé suivent les nouveaux noms quand des machines importées sont renommées.
- Réglages sample rate et bits par échantillon par machine.
- Actions globales pour appliquer une sample rate ou des bits par échantillon à toutes les machines.
- Fonction pour remettre les adresses IPv4 en automatique, par machine ou globalement, quand le XML expose ces champs.
- Cible d'actions globales : toutes non verrouillées, sélection non verrouillée ou filtre affiché non verrouillé.
- Verrouillage de machines pour éviter qu'une action globale ne les modifie.
- Reset patch RX et reset patch TX séparés.
- Notices rapide et complète en français/anglais, sélectionnées automatiquement selon la langue de l'interface, et info-bulles intégrées.
- Comparaison XML en tableau.
- Installateur avec détection d'une version déjà installée.
- Listes rapides Sample rates, Bits et IP fixes.
- Avertissements renforcés si plusieurs sample rates ou plusieurs encodages coexistent dans le projet.
- Compatibilité de sauvegarde adaptée aux fonctions d'ajout/suppression de machines.
- Page Configuration plus lisible : alerte compacte, panneaux masquables, table des machines gardée visible et recherche globale avec message d'aide.
- Ouverture en plein écran, fiche machine au double-clic, preferred master cochable dans la table, IP fixe en série et reset patch RX/TX par machine.

## Nouveautés V3.04

- Passage en V3.04 dev.
- Interface Français / Anglais, modifiable à tout moment.
- Suppression d'un device avec nettoyage des subscriptions associées.
- Import d'un second XML dans le projet ouvert.
- Ajout d'un garde-fou de changements XML : seules les zones métier autorisées peuvent changer, les zones techniques Dante sensibles bloquent la sauvegarde.
- Ajout d'un rapport `Compatibilité Dante Controller` exportable/copier depuis l'écran Sécurité.
- Mode `Lecture seule` par défaut après ouverture d'un XML, avec bouton `Activer l'édition`.
- Correction des libellés utilisateur : `Dante Id` avec espace dans l'interface, les rapports et la documentation.
- Affichage des latences en ms : `250` devient `0,25 ms`, `1000` devient `1 ms`, tout en gardant la valeur XML brute.
- Prévisualisation des actions globales existantes avant application.
- Preferred master global rendu plus sûr : définir un seul device comme preferred master, ou retirer tous les preferred masters.
- Page Patch : mode simple/expert, colonne `Source complète`, choix TX avec Dante Id, protection des patchs vers devices absents.
- Patchbook enrichi et export CSV lecture seule.
- Page Santé enrichie avec mode Lecture seule/Édition, latences en ms, samplerates et encodages.
- Ajout d'une topologie simple des sources les plus utilisées, receivers les plus patchés et relations TX vers RX.
- Script installateur plus portable pour trouver Inno Setup.
- Option `installer/build_source_zip.ps1` pour créer une archive source propre.

## Nouveautés V3.03

- Page `Santé du fichier` : synthèse du preset, statistiques TX/RX, patchs actifs/libres/locaux, preferred masters, modes réseau, IP fixes et tableau filtrable des points à vérifier.
- Contrôles de compatibilité XML Dante Controller avant sauvegarde : racine `<preset>`, version, devices, canaux `txchannel` / `rxchannel`, attributs XML `danteId` / `mediaType` et balises techniques importantes.
- Gestion correcte des patchs locaux `subscribed_device="."` : affichage comme source locale, pas comme conflit, et conservation du `.` à la sauvegarde quand le fichier source l'utilise.
- Utilisation du Dante Id comme identifiant métier principal des canaux, sans renumérotation.
- Page `Patch` plus lisible : TX brut/résolu/affiché, type de patch, warnings et filtres par état.
- Export `Patchbook TXT` organisé par device RX.
- Comparaison XML plus lisible des canaux et patchs.
- Correction de la lisibilité des listes et menus : fond blanc et texte noir, y compris en thème sombre.
- Correction de la propagation des patchs lors de la réinitialisation des canaux TX.
- Ajout du choix canal début / canal fin pour limiter le renommage en série à une plage.

## Nouveautés V3.02

- Annulation de la dernière action.
- Résumé avant sauvegarde plus lisible.
- Recherche globale.
- Export du rapport en TXT ou PDF.
- Détection visuelle plus forte des conflits de patch.
- Renommage en série des canaux.
- Liste des fichiers récents.
- Comparaison avec un autre XML.
- Sauvegarde sécurisée par fichier temporaire relu avant remplacement.

## Nouveautés V3.01

- Colonne `Projet` plus compacte.
- Navigation par onglets uniquement en haut.
- Page `Configuration` réorganisée pour limiter le défilement vertical.
- Page `Patch` avec filtres TX/RX explicites.
- Ajout des choix `Tous les émetteurs` et `Tous les récepteurs`.
- Renommage RX et TX séparés dans la page `Patch`.

## Renommage des canaux et patchs

Quand un canal TX est renommé, y compris lors d'un renommage en série sur une plage, l'application parcourt les abonnements RX reconnus et remplace l'ancien nom du canal par le nouveau partout où il est utilisé avec le même device TX.

Les champs de patch actuellement reconnus incluent notamment :

- `subscribed_device`
- `subscription_device`
- `tx_device`
- `source_device`
- `subscribed_channel`
- `subscribed_channel_name`
- `subscribed_channel_label`
- `subscribed_tx_channel`
- `subscribed_tx_channel_name`
- `subscribed_label`
- `source_channel`
- `source_channel_name`

## Compiler depuis les sources

Prérequis :

- Windows
- SDK .NET 8

Depuis le dossier du projet :

```powershell
.\build.ps1
```

Pour reconstruire l'installateur autonome :

```powershell
.\installer\build_installer.ps1
```

Pour lancer les tests automatiques :

```powershell
.\tests\run-tests.ps1
```

## Sécurité des fichiers

- Toujours travailler sur une copie.
- Ne jamais écraser un fichier de production sans test.
- Conserver les backups générés automatiquement.
- Tester le fichier final dans les outils Dante officiels avant exploitation.
- L'application vérifie la cohérence du XML généré, mais la validation définitive doit être faite par un import dans Dante Controller avant toute utilisation en production.

## Crédit

**By Mamat**<br>
<sub>et ses agents</sub>

Remerciement à **Charles Bouticourt** pour l'idée de la fonction `Atomic Bomb`.

### Un grand merci à Tobias Grupe

Un grand merci à **[Tobias Grupe (@togrupe)](https://github.com/togrupe)** pour le temps consacré à tester Dante Config Editor, pour ses captures et retours précis, ainsi que pour ses nombreuses idées d'amélioration. Ses propositions sur les en-têtes TX/RX fixes, le patch 1:1, le bouton Flip, la navigation au clavier, les performances de la matrice et les échanges de labels avec DMT ont directement contribué à faire progresser la V3.5.

Tobias développe également **[dLive MIDI Tools](https://github.com/togrupe/dlive-midi-tools)**, projet avec lequel les fonctions d'import et d'export de labels de DCE ont été pensées et testées.

---

## English

The complete English presentation is kept in a separate section so that both languages remain easy to read:

- **[Open the complete English README](README_EN.md)**
- **[Download the stable Dante Config Editor V3.4.2](https://github.com/Mamat79/DanteConfigEditorV3/releases/tag/v3.4.2)**
- **[Read the full English guide (PDF)](docs/Notice_DanteConfigEditorV3_EN.pdf)** or the [English quick start](docs/QuickStart_DanteConfigEditorV3_EN.pdf).
- **[Open the dLive MIDI Tools (DMT) project](https://github.com/togrupe/dlive-midi-tools)**
