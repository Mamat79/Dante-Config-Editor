# Dante Config Editor v2027

[English release notes](RELEASE_NOTES_2027.2.3_EN.md)

## Windows et macOS 2027.2.3

- Flip échange les deux machines du carré choisi : les TX restent en haut et les
  RX à gauche. Sur une machine sans TX, un en-tête « 0 TX » indique qu'aucun
  patch n'est possible depuis cette intersection.
- Le déplacement du pointeur vers Flip ne change plus la paire choisie. Les
  canaux déjà dépliés restent ouverts ; un second Flip inverse de nouveau la paire.
- Dans la matrice, le pavé tactile défile verticalement à deux doigts. Un
  pincement (ou Ctrl + molette) zoome autour du pointeur sans modifier le patch.
- Les formats XML et les règles de validation ne changent pas. Contrôlez toujours
  le fichier exporté dans Dante Controller avant de l'utiliser sur le réseau.

## Windows et macOS 2027.2.2

- Reunit la correction de detection ci-dessous et la gestion des limites GitHub.
- En cas de 403/429 temporaire, DCE respecte le delai du serveur, meme apres
  relancement. La recherche manuelle donne une information avec l'heure de
  reprise ; la verification automatique reste silencieuse.

### Correction de detection

- Correction du rappel de mise a jour qui reproposait la 2027.2.0 deja installee.
  Le controle utilise la version de l'application Windows ou Mac lancee.

## Windows et macOS 2027.2.0

- La matrice montre tout le projet, avec les machines repliées au départ et
  des en-têtes distincts pour les machines et leurs canaux.
- Un glissement propose une diagonale ou une colonne automatiquement. L'aperçu
  orange reste visible même si le pointeur ne suit pas une diagonale parfaite.
  Relâchez pour appliquer, Échap pour annuler ; une action annule tout le geste.
- Les clics conservent la matrice en place et ne redessinent plus le synoptique
  masqué. Les contrôles XML et l'historique transactionnel restent actifs.
- Double-cliquez un label pour le renommer directement ; tirez sa poignée pour
  prolonger une série. Le renommage en série, Flip, la recherche des sources et
  destinations, et le double-clic sur une machine restent disponibles.
- Multicast récapitule les flux de tout le projet : choisissez une machine,
  cochez les TX audio, puis créez, modifiez ou supprimez un flux.
- Sous Windows, le synoptique reste dans Outils. Une roue dentée à côté d'Espace de travail
  permet de choisir les raccourcis de navigation.

Le multicast concerne les flux Dante audio simples des presets 3.0.0. Les
formes inconnues restent intactes et non modifiables. Supprimer tous les flux
d'un preset ne garantit pas leur suppression sur le réseau : contrôlez ce point
dans Dante Controller. DCE reste un éditeur hors ligne.

La même matrice est portée dans l'interface native Avalonia sur Mac, avec ses
gestes automatiques, l'aperçu, le renommage direct, la poignée de série, la
recherche des liaisons, l'ouverture des machines et le gestionnaire multicast.
Les paquets Apple Silicon et Intel sont fabriqués depuis les mêmes sources.

## Windows et macOS 2027.1.1

Cette version simplifie le travail quotidien et adapte automatiquement DCE aux
écrans plus petits, sans réduire la taille du texte ni ajouter un réglage à
gérer.

- DCE choisit une disposition large, compacte ou étroite selon l'espace
  réellement disponible. Le bandeau se compacte, les panneaux Machines et
  Easy Patch se réorganisent et le défilement reste un dernier recours.
- Les mots ne sont plus coupés au milieu dans les boutons et les libellés. Les
  commandes restent lisibles en français et en anglais sur un écran 1024×640.
- La navigation principale est ramenée à six intentions claires : **Projet**,
  **Vue d'ensemble**, **Machines**, **Patch DCE**, **Synoptique** et **Outils**.
  Import/export, validation, historique, banques, rapports et formation restent
  accessibles dans **Outils**.
- **Nouveau projet DCE** devient un assistant en deux étapes. La première
  définit le fichier, le nom et la description ; la seconde prépare la première
  machine et sa configuration.
- Easy Patch distingue la sélection des appareils, le parcours
  **Source / destination** et le mode **Hybride recommandé**. Les sélections et
  glisser-déposer multiples conservent les règles de patch existantes.
- Les mêmes principes sont appliqués à Windows, macOS Apple Silicon et macOS
  Intel. Les formats XML/DCE/StageFlow, les banques et les licences DCEP1,
  DCEF1 et V2 ne changent pas.

## Windows et macOS 2027.1.0

Cette version améliore l'utilisation sur les écrans compacts et harmonise le
rappel de licence sans bloquer durablement le travail.

- Les zones Machines, Patch, Easy Patch et Synoptique se réorganisent selon la
  largeur disponible. Le défilement n'apparaît qu'en recours, sans réduire le
  texte ni couper les mots.
- Les commandes de bas de page peuvent revenir à la ligne. La navigation
  compacte affiche entièrement ses libellés, y compris Centre de validation.
- Les choix de thème et les boutons désactivés restent lisibles dans les thèmes
  clair et sombre.
- **Nouveau projet DCE** ouvre immédiatement une configuration vide en mémoire,
  sans demander de nom, de chemin, de XML ni de première machine. Les machines
  s'ajoutent ensuite depuis la banque et la destination est choisie à
  l'enregistrement.
- Avant de remplacer un travail modifié, DCE propose Enregistrer, Ne pas
  enregistrer ou Annuler. `Ctrl+N`, `Ctrl+S` et `Ctrl+Maj+S` sont alignés entre
  Windows et macOS.
- Pour les modèles Yamaha de type console ou boîtier d'entrées/sorties, la
  banque propose des noms machine `Y001-`, `Y002-`, etc. La séquence évite les
  collisions, accepte les identifiants à plusieurs chiffres et reste entièrement
  modifiable. Elle ne renomme jamais un équipement existant ou importé.
- Pendant les 30 jours d'essai, le rappel est immédiatement refermable. Après
  expiration, DCE attend 60 secondes au démarrage puis rend toutes les fonctions
  accessibles ; une activation valide libère immédiatement l'interface.
- Le délai ne recommence pas lors de l'ouverture d'une seconde fenêtre dans le
  même processus. Les licences DCEP1, DCEF1 et V2 restent compatibles.
- Une reprise Codemagic accepte une paire macOS `DMG + SHA-256` déjà publiée
  lorsqu'elle est complète et cohérente, même si le DMG reconstruit contient
  des métadonnées différentes. Une paire incomplète est réparée sans écrasement.

## Windows et macOS 2027.0.7

Cette corrective fiabilise l'export d'un projet StageFlow et étend le patch
séquentiel, sans modifier le format XML, les projets, les banques ni les licences.

- **Exporter le XML Dante** fonctionne depuis un projet `.stageflow` : le chemin
  interne du paquet DCE n'est plus transmis à la fenêtre Windows Enregistrer sous.
- Les boîtes de dialogue proposent un nom et un dossier physiques valides, avec
  un repli sûr si Windows refuse malgré tout le chemin initial.
- Dans **Easy Patch**, plusieurs TX déposés sur le premier RX alimentent les RX
  suivants ; dans l'autre sens, plusieurs RX déposés sur le premier TX reçoivent
  successivement ce TX puis les TX suivants.
- Chaque RX conserve exactement une source. La série s'arrête proprement à la
  fin de la machine et DCE indique les canaux restés sans affectation.
- Une référence vers une machine ou un canal TX absent d'un preset partiel est
  désormais classée comme information. Elle reste conservée dans le XML et ne
  demande plus d'acquittement comme un avertissement.
- Les notices et comportements sont alignés en français et en anglais sous
  Windows et macOS.

## Windows et macOS 2027.0.6

Cette corrective rétablit les réglages réseau réellement décrits par certains
presets et accélère le patch séquentiel sans modifier les règles de compatibilité
XML, les licences, les banques personnelles ni les formats de projet.

- **Redondant** et **Daisychain** restent modifiables lorsque la machine contient
  une balise `redundancy`, même si elle ne contient aucune balise `ipv4_address`.
- L'onglet IP reste volontairement désactivé lorsqu'aucune section IPv4 n'existe
  dans le rôle. Un texte explique la limite : DCE préserve le document d'origine
  et n'invente jamais une capacité technique absente.
- Dans **Easy Patch**, sélectionnez un ou plusieurs TX avec Ctrl ou Maj, puis
  déposez-les sur le premier RX. Le premier TX est affecté au RX visé et les
  suivants aux RX successifs, immédiatement.
- Les remplacements de RX déjà patchés conservent l'avertissement choisi par
  l'utilisateur. Plusieurs TX ne peuvent jamais alimenter le même RX.
- Le comportement et les explications sont disponibles en français et en
  anglais sous Windows et macOS.

## Windows et macOS 2027.0.5

Cette version clarifie les projets DCE et StageFlow, fiabilise le synoptique et
recentre la validation sur les problèmes réellement utiles avant export.

- Un projet autonome DCE reste un fichier XML Dante : il peut être créé de zéro,
  ouvert, modifié puis exporté pour Dante Controller sans dépendre de StageFlow.
- Dans un projet `.stageflow`, **Enregistrer** conserve désormais dans le domaine
  Dante les emplacements, l'ordre, la visibilité et les positions manuelles du
  synoptique. Ces données de travail ne sont jamais ajoutées au XML Dante exporté.
- **Import / Export** expose directement l'import d'un XML Dante Controller et
  l'export vers Dante Controller, en plus du menu Fichier.
- Le centre de validation ne signale plus les RX libres ni les machines légitimes
  sans TX ou sans RX. Les contrôles bloquants de structure, d'identités et de
  références restent actifs.
- Le stockage du synoptique utilise une identité portable pour retrouver chaque
  machine après fermeture et réouverture du projet StageFlow.
- Les libellés, tests, notices et numéros de version sont alignés entre Windows,
  macOS Apple Silicon et macOS Intel.

## Windows 2027.0.4

Cette corrective Windows rend les champs plus lisibles et les commandes de
formation et de synoptique plus accessibles. Les données de projet, licences,
banques et formats existants restent inchangés.

- Les listes déroulantes et champs de saisie partagés retrouvent un centrage
  vertical cohérent, y compris après un changement de thème.
- Le titre Connexion StageFlow reste contrasté dans le thème clair.
- Atomic Bomb est à nouveau accessible depuis la navigation ; son pupitre
  conserve la séquence clé, capot, ARM, LOCK, FIRE. La clé et le capot ne sont
  plus rognés dans la disposition standard.
- Le synoptique rend explicites les commandes Détacher et Rattacher, conserve
  sa disposition et applique correctement le thème dans sa fenêtre séparée.

## Windows 2027.0.2

Cette mise à jour clarifie les commandes de préparation, de connexion à la suite
et d'export. DCE reste autonome et prépare les configurations Dante hors ligne.

### Un bandeau commun, toujours accessible

- L'icône et le nom de DCE restent visibles avec les menus métier.
- Les commandes suivent l'ordre Connexion StageFlow, Alertes, Thème, Langue,
  Guide DCE et Aide de DCE. Elles passent sur une seconde ligne lorsque
  la largeur disponible ne suffit pas.
- Le thème indique le choix courant, Clair ou Sombre. Le thème et la langue
  sont mémorisés. Les commandes disposent de noms accessibles et d'infobulles.
- Les champs de thème et de langue ont le même centrage vertical que les
  boutons voisins, sans marge interne héritée qui décale leur fond.
- Annuler et Rétablir restent dans le bandeau. Atomic Bomb se trouve dans
  **Outils > Formation** et conserve sa séquence de sécurité.

### Aide et notices

- **Guide** ouvre désormais la notice complète de DCE dans la langue choisie.
  **Aide** ouvre **Découvrir DCE** ; le guide commun est disponible uniquement
  dans **Aide > Guide de la suite SiLeMI/O**, sans bouton dédié supplémentaire.
- Les notices produit FR/EN ont un sommaire compact sur une page, avec liens
  et signets conservés. Les légendes expliquent les écrans, les liens utilisateur
  conduisent au site SiLeMI/O et les valeurs spécifiques à un groupe sont expliquées.
- Les procédures distinguent Enregistrer, Enregistrer sous et Exporter le XML
  Dante, pour conserver le projet tout en produisant le preset destiné au matériel.

### Connexion StageFlow et alertes

- Le bouton **Connexion StageFlow** conserve son nom ; son état s'affiche sur
  une seconde ligne, avec la session rejointe lorsqu'elle est connectée.
- Le centre reprend le menu latéral commun **StageFlow LIVE / Télécommande
  Dante Config Editor**, le projet courant et **Retour au projet**. Il reste
  non modal ; l'édition locale est temporairement protégée pendant la connexion.
- La rubrique mobile identifie la cible **suite StageFlow**. Le QR, son état et
  ses droits se gèrent dans StageFlow sur l'hôte : DCE n'invente ni serveur mobile
  autonome, ni lien, ni bouton d'activation. Sans session détectée, le centre
  invite à ouvrir StageFlow et à diffuser le projet sur ce PC ou le réseau local.
- **Connexion StageFlow** distingue le projet local de la session LIVE. L'état
  autonome, la connexion en cours, la perte de connexion et les erreurs sont
  indiqués explicitement. Le nom complet du projet reste consultable.
- Le centre permet d'ouvrir un dossier `.stageflow` local sans lancer StageFlow.
  Une session LIVE doit être déconnectée avant cette ouverture ; les changements
  non enregistrés font l'objet d'un avertissement.
- La découverte, le code à six chiffres et la déconnexion sont conservés.
  L'adresse IPv4 et les détails techniques se trouvent dans une zone dépliable.
- **Alertes** sépare la validation XML des changements StageFlow non acquittés.
  Consulter les alertes ne les acquitte pas et ne désactive pas leur réception.

### Préparer, vérifier, exporter

- La page Projet donne accès aux machines, au patch, à la validation et à l'export.
- Les compteurs tiennent sur une ligne en grande largeur. Les fichiers récents
  utilisent la hauteur restante et défilent dans leur propre liste ; la page
  conserve un défilement de secours dans une petite fenêtre, sans couper les commandes.
- **Fichier > Exporter vers Dante Controller** et **Import / Export > Exporter le XML
  Dante** ouvrent le contrôle avant export, puis enregistrent une copie XML.
- Cette copie conserve le projet ouvert, ses modifications et son historique
  d'annulation. Le fichier source n'est pas écrasé. Une destination existante
  reste protégée par la confirmation et les sauvegardes du moteur existant.
- L'export refuse les extensions autres que `.xml` et les chemins à l'intérieur
  du dossier StageFlow courant, y compris les jonctions et liens symboliques.
- **Enregistrer** conserve son rôle de sauvegarde du projet StageFlow. Une
  exportation XML ne remplace pas cette sauvegarde.

Le moteur de modification XML n'est pas réécrit. Les banques, le patch, le
renommage en série, la fusion XML, le synoptique et les licences sont préservés.
StageFlow est gratuit et facultatif. Une connexion LIVE à la suite ne constitue
pas un contrôle du réseau ou du matériel Dante.

## Documentation et mise à niveau

L'installateur Windows inclut les notices et démarrages rapides FR/EN ainsi que
la paire exacte des guides communs SiLeMI/O 2027.3 validés par leur propriétaire.

Depuis une version 2026.10 qui ne propose pas v2027, téléchargez manuellement
l'installateur Windows depuis le dépôt public. Les versions récentes choisissent
la mise à jour correspondant à leur plateforme et vérifient taille et SHA-256.

La mise à niveau conserve les profils, banques personnelles, projets et licences.
Un seul raccourci Bureau est utilisé : **Dante Config Editor v2027**. Un ancien
nom de dossier d'installation, tel que `Dante Config Editor 2026.3`, peut être
conservé ; la version effective figure dans À propos et dans l'exécutable.

## Windows et Mac

Cette livraison vise **2027.1.1**, version binaire **2027.1.1.0**, avec des
paquets distincts pour Windows, macOS Apple Silicon et macOS Intel.

Les tests Avalonia sans écran exécutés sous Windows ne constituent pas une
recette native Mac. Les vérifications d'installation et les contrôles visuels
sont rapportés séparément des tests automatisés. Aucun nouvel essai Dante
Controller ou matériel physique n'est revendiqué par ce lot. La signature
commerciale Windows et la notarisation Apple ne font pas partie de cette mise à jour.
