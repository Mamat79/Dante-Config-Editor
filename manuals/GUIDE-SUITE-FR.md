# La suite SiLeMIO — Guide de démarrage

**Du premier patch à l’exploitation, avec un seul logiciel ou toute la suite.**

Édition v2027 - guide utilisateur. Chaque logiciel est livré séparément :
installez uniquement une version publiée pour votre plateforme et consultez
ses notes de version. Les limites Windows et macOS sont précisées ci-dessous.

StageFlow est gratuit et facultatif. Chaque logiciel SiLeMIO reste utilisable
seul, avec son projet natif. Lorsqu’on veut faire circuler le même show
entre plusieurs outils, chaque logiciel peut aussi créer, ouvrir et enregistrer
un projet StageFlow `.stageflow`, même si StageFlow n’est pas installé.

Découvrir StageFlow et ses versions publiées : [page officielle StageFlow](https://github.com/Mamat79/StageFlow).

## Disponibilité Windows et macOS

| Logiciel | Windows | macOS |
|---|---|---|
| StageFlow | Application native* | Portage en cours - non disponible |
| StageDesk | Application native* | Native Intel / Apple Silicon* |
| StageMark | Application native* | Native Intel / Apple Silicon* |
| StageMon | Application native* | Portage en cours - non disponible |
| Dante Config Editor | Application native* | Native Intel / Apple Silicon* |
| AutoCAD | Connecteur StageFlow 2026 pour Windows* | Connecteur StageFlow non disponible |

*La présence d'un installateur v2027 sur la page Releases du logiciel confirme
sa disponibilité pour la plateforme. Une application native construite ou une
édition Windows ne garantit pas une version Mac publiée. Ce guide ne remplace
ni les notes de version ni la recette de votre matériel.*

Le maître StageFlow fonctionne actuellement sous **Windows**. Les éditions Mac
compatibles de StageDesk, StageMark et Dante Config Editor peuvent le rejoindre
sur le réseau local avec son code à six chiffres, sans installer StageFlow.

Les cartes de lancement et raccourcis de la console décrits ici concernent le
poste Windows. Un canal de commande **sur le même Mac** est implémenté dans
StageDesk, mais pas dans StageMark ni Dante Config Editor. Ces deux logiciels
peuvent néanmoins rejoindre une **Session StageFlow LIVE** par le réseau.
Seules les commandes réellement proposées par le logiciel connecté sont actives.
StageFlow et StageMon pour Mac restent en portage, **non disponibles**.

![Architecture de la suite SiLeMIO](../media/ecosystem/suite-architecture-fr.svg)

## Choisir son parcours

Les trois parcours portent les mêmes noms dans toute la suite :

- **Projet <Application>** : créer, ouvrir et enregistrer le projet natif du
  logiciel, en fonctionnement autonome ;
- **Projet StageFlow local** : ouvrir un projet `.stageflow` stocké sur ce
  poste, sans avoir besoin de l'application StageFlow ;
- **Session StageFlow LIVE** : découvrir le show et le poste hôte sur le réseau
  local, puis rejoindre explicitement la session avec son code à six chiffres.

### J’utilise un seul logiciel

Créez et enregistrez son projet natif comme d’habitude. Vous n’avez besoin ni de
StageFlow, ni des autres logiciels. Si vous installez un autre outil de
la suite plus tard, vous pourrez alors créer ou ouvrir un projet `.stageflow`
depuis le logiciel que vous possédez déjà.

### Je veux partager un show entre plusieurs logiciels

Créez un projet `.stageflow` depuis StageFlow, StageDesk, StageMark, StageMon
ou Dante Config Editor. Chaque application écrit uniquement
dans sa partie du projet et préserve le travail des autres.

### Je veux un poste de travail central

Ouvrez le projet dans StageFlow. Sa console affiche les logiciels
installés, ouverts, connectés au projet ou en mode LIVE. Elle peut les ouvrir
directement sur le show courant et donner accès à quelques commandes
essentielles. Les cartes inutiles peuvent être masquées dans **Réglages** sans
modifier le projet.

## Le parcours conseillé

![Parcours conseillé de la préparation à l’exploitation](../media/ecosystem/suite-workflow-fr.svg)

Toutes les étapes sont facultatives : on peut passer directement du patch à
StageMark, n’utiliser que StageMon, ou préparer Dante sans utiliser
StageDesk.

## 1. Commencer le patch

Trois portes d’entrée sont prévues.

### Depuis StageFlow

1. Cliquez sur **Nouveau**.
2. Donnez un nom au show.
3. Choisissez librement le nombre de paires et de groupes.
4. Remplissez le **Commun à tous les groupes** pour les sources réutilisées.
5. Ouvrez chaque groupe pour ajouter ses particularités.
6. Enregistrez le dossier `NomDuShow.stageflow`.

La saisie suit les habitudes d’Excel : **Entrée** descend, **Tab** passe à la
cellule suivante, `Ctrl+C` et `Ctrl+V` copient et collent, et la poignée de
recopie prolonge une liste vers le bas.

### Depuis Excel

1. Dans n’importe quel logiciel compatible, choisissez **Modèle Excel** ou
   **Créer le classeur**.
2. Indiquez le nombre de paires et de groupes : le classeur crée une feuille
   centrale et une feuille par groupe.
3. Remplissez le classeur dans Excel.
4. Ouvrez ou importez ce classeur dans StageFlow, StageDesk ou StageMon.
5. Enregistrez ensuite le nouveau projet `.stageflow` si vous voulez le partager.

Chaque feuille de groupe est réglée pour tenir sur une page A4 paysage. Le
classeur conserve des identifiants invisibles et stables afin que les logiciels
reconnaissent les groupes et les paires après une modification.

### Depuis un autre logiciel SiLeMIO

Choisissez **Nouveau projet StageFlow** dans le logiciel concerné. Il crée un
projet StageFlow valide avec un patch vide ou existant, puis ajoute son propre
domaine. StageFlow n’a pas besoin d’être lancé.

## 2. Organiser le commun et les groupes

Le **Commun à tous les groupes** contient ce qui peut être réutilisé : source,
micro et commentaire. Dans chaque groupe, la colonne **Commun** décide, paire
par paire, si cette valeur doit apparaître.

- **Tout afficher** active les paires communes pour le groupe courant.
- **Tout masquer** les retire de ce groupe sans effacer le patch commun.
- Une valeur locale remplace la valeur commune uniquement dans ce groupe.
- Une paire commune masquée et sans valeur locale reste vide sur la page du
  groupe et dans son export Excel/A4.

## 3. Préparer les autres métiers

### StageDesk

Utilisez StageDesk pour récupérer un patch de console, modifier les labels,
couleurs et réglages utiles, puis l’envoyer vers une autre console ou un autre
logiciel. Il peut travailler sur son projet natif ou sur sa propre partie du
projet `.stageflow`.

### Dante Config Editor

Utilisez Dante Config Editor pour préparer hors ligne les machines Dante,
leurs canaux TX/RX, le patch et les contrôles de cohérence. Depuis le projet
StageFlow, vous pouvez associer les lignes du patch aux canaux RX d’une machine
et réutiliser les noms. Dante Config Editor ne commande pas le réseau Dante en
exploitation.

### AutoCAD et le plan de scène

Le plan simple peut être réalisé directement dans StageFlow. Pour un plan
technique plus poussé, le connecteur AutoCAD ouvre le même projet, place les
sources et republie la géométrie dans la partie CAD. StageFlow reste utilisable
sans AutoCAD.

Le connecteur distingue le dessin autonome, le **Projet StageFlow local** et
la **Session StageFlow LIVE**. En LIVE réseau, choisissez l'hôte et saisissez
son code à six chiffres. Le patch reçu reste en mémoire ; son actualisation
est explicite et seule la partie CAD est publiée. La recette de cette version
dans AutoCAD réel reste requise avant exploitation.

## 4. Passer à l’exploitation

### StageMark

Préparez l’implantation, les plans, les repères et les cues, puis utilisez les
commandes **AFFICHER** et **BLACKOUT**. La sécurité locale de StageMark reste
prioritaire, même lorsque le logiciel est ouvert depuis StageFlow.

### StageMon

Préparez les affectations et les entrées, puis exploitez les écoutes A/B,
présentes par défaut. StageMon peut proposer de deux à six circuits : C à F
n'apparaissent qu'après leur configuration. Les commandes rapides de la console StageFlow peuvent notamment
effacer un cue. Un mute appliqué localement dans StageMon ne peut jamais
être retiré à distance par StageFlow.

## 5. Utiliser la console StageFlow

La zone inférieure de StageFlow présente une carte par logiciel :

- **grisée** : logiciel absent, avec accès au téléchargement ;
- **installé · fermé** : prêt à être lancé ;
- **ouvert** : application détectée, pas encore reliée au projet ;
- **autonome · hors ligne** (gris) : aucun lien LIVE ;
- **disponible** (orange) : une Session StageFlow LIVE peut être rejointe ;
- **connecté · synchronisé** (vert) : même projet et même session LIVE ;
- **conflit** ou **connexion perdue** (rouge) : action nécessaire avant de
  poursuivre.

Sous Windows, sélectionnez plusieurs cartes puis **Ouvrir la sélection** pour
lancer les outils utiles. Avec un connecteur local compatible, une application
déjà ouverte reçoit le projet dans sa fenêtre existante. Les limites Mac
figurent dans la matrice de disponibilité. Le bouton **Réglages**
permet de masquer les cartes dont vous ne vous servez pas. **Console compacte**
réduit StageFlow aux commandes de la suite ; revenez à la fenêtre complète
pour travailler sur le patch.

## 6. Piloter le poste avec un seul QR code

1. Enregistrez le projet StageFlow puis ouvrez le centre **Session StageFlow
   LIVE** avec le grand bouton d'état en haut.
2. Dans **Téléphones et tablettes**, choisissez l’interface Wi-Fi ou Ethernet
   du même réseau que le téléphone.
3. Cliquez sur **Créer le QR** : StageFlow active LIVE et affiche le QR code.
4. Scannez-le avec le téléphone ou la tablette.

L’onglet **StageFlow** permet de changer de groupe, modifier les sources,
micros et commentaires, régler l’affichage du commun, ajouter, renommer ou
supprimer un groupe, puis enregistrer. La suppression demande toujours une
confirmation et le groupe commun est protégé.

Les onglets **StageMark** et **StageMon** ouvrent leurs télécommandes normales,
pas une version réduite. Ils deviennent disponibles lorsque le logiciel est
ouvert sur le même projet et la même session LIVE et que sa télécommande locale
est prête. Les règles `BLACKOUT`, projection, mute et niveaux restent gérées par
le logiciel propriétaire. Cliquez sur **Arrêter** dans StageFlow pour révoquer
immédiatement le portail. Sur téléphone ou tablette, et lorsque le logiciel
cible est sur une autre adresse réseau, sa télécommande s'ouvre en page entière.
Le bouton **Retour** du navigateur permet de retrouver StageFlow.

StageDesk et Dante Config Editor n'ont pas de télécommande QR propre : ils
rejoignent la session pour travailler sur le projet. StageMark et StageMon
conservent aussi leur propre accès QR lorsqu'ils sont utilisés sans StageFlow.

Plusieurs téléphones et tablettes peuvent utiliser le même QR code en même
temps. Chaque contrôleur possède son propre bail court et son propre
acquittement des alertes ; StageFlow limite la session à 24 contrôleurs.

## 7. Un centre de connexion, trois parcours clairs

![Connexion des logiciels et des mobiles](../media/ecosystem/suite-connections-fr.svg)

Un dossier **Projet StageFlow local** n'est pas une connexion réseau. Ouvrir le
même dossier sur le PC ne signifie pas rejoindre une **Session StageFlow LIVE**.
Le projet natif, le dossier local et la session LIVE restent des choix distincts.

### Mode manuel

Chaque application enregistre et recharge lorsque vous le décidez. C’est le
mode le plus simple pour préparer seul un projet ou pour travailler sans
synchronisation automatique.

### Mode LIVE

Ouvrez la **Session StageFlow LIVE** lorsque plusieurs logiciels doivent suivre le même
show pendant la préparation ou l’exploitation. Les changements valides
sont publiés et rechargés rapidement. Une application qui contient des
modifications locales non enregistrées ne les écrase pas : elle signale un
conflit et demande un choix.

Le grand bouton reste toujours visible. Le centre **Session StageFlow LIVE**
réunit l'activation générale, les autres postes et le QR code. Il distingue les
compteurs **postes / logiciels / mobiles** : plusieurs applications d'un même
ordinateur ne deviennent pas plusieurs ordinateurs. Le texte de l'état reste
lisible sans devoir interpréter une couleur.

### Héberger ou rejoindre

1. Sur le poste principal, ouvrez le projet dans StageFlow puis le centre
   **Session StageFlow LIVE**. Choisissez le réseau et **Autoriser et diffuser**.
2. Sur un autre logiciel, choisissez **Session StageFlow LIVE**. Sélectionnez
   le show et l'ordinateur hôte découverts, ou utilisez l'adresse indiquée par
   l'hôte si la découverte réseau est indisponible.
3. Vérifiez le show et l'hôte, puis recopiez les **six chiffres** du code,
   y compris les zéros au début. Aucun chemin de dossier distant n'est demandé.
4. Attendez **connecté · synchronisé** avant de travailler en équipe. StageFlow
   liste les logiciels reliés ; il héberge la session et ne se rejoint pas lui-même.
5. Pour vous détacher, choisissez **Déconnecter** dans votre logiciel. En cas de
   coupure, l'état devient **connexion perdue** : la reprise est volontaire.

Le code reste valable pendant la session. Arrêter puis redémarrer la session
produit un nouveau code. Un code incorrect peut être corrigé sans relancer le
logiciel ; si la session a changé, sélectionnez-la à nouveau. Le code est
réservé à l'équipe sur un réseau local de confiance, jamais publié sur Internet.

LIVE ne contourne jamais les sécurités métier. BLACKOUT, mutes, arrêt audio et
autres états critiques restent contrôlés par l’application concernée. Coupez
LIVE à tout moment pour revenir au fonctionnement manuel.

Plusieurs ordinateurs du même réseau local peuvent rejoindre la session LIVE
StageFlow. Chaque poste affiche clairement s’il est connecté au projet StageFlow
ou s’il travaille de manière autonome ; il peut se détacher sans bloquer les
autres applications. Quitter la session rend immédiatement le logiciel
autonome et ne déclenche aucune reconnexion automatique.

### Alertes de labels

Le bouton **Alertes labels** reste directement accessible sur la page principale.
Le maître choisit quand émettre les notifications ; chaque logiciel qui rejoint
la session les reçoit **par défaut**. Le bandeau persistant affiche
**ancien → nouveau**, l'origine, l'heure, un compteur et la liste des changements.

L'**acquittement local**, unitaire ou groupé, ne concerne que les alertes
présentes au clic. Il ne les efface ni chez les autres destinataires, ni pour une
nouvelle alerte reçue ensuite. Chaque destinataire peut couper sa réception
sans affecter les autres ; les alertes ignorées ne sont pas rejouées à la reprise.

Le maître peut **Suspendre les notifications de modifications**, puis les
réactiver **sans arrêter LIVE**. Les changements réalisés pendant cette pause
ne sont pas rappelés. Ce n'est pas un acquittement pour toute l'équipe.
Seuls les labels de sources déclenchent les alertes : jamais les micros,
commentaires, groupes, balances ou autres réglages.

## 8. Piloter avec OSC, MIDI ou Stream Deck

Dans **StageFlow sous Windows**, ouvrez **Outils > OSC · MIDI · Stream Deck**.
Pour les contrôles autonomes, vérifiez les fonctions de votre plateforme dans
la notice du logiciel. StageFlow écoute OSC localement
sur `127.0.0.1:18040` par défaut. Choisissez explicitement une interface réseau
seulement lorsqu’un autre poste de confiance doit envoyer des commandes. Le
journal montre les messages reçus, acceptés ou refusés, et les retours d’état
peuvent être activés commande par commande.

Pour le MIDI, choisissez l’entrée, cliquez sur **Apprendre le prochain
message**, puis actionnez le bouton à associer. Une sortie MIDI facultative peut
renvoyer l’état. Le plugin Stream Deck fourni s’installe depuis le même écran :
placez l’action SiLeMIO sur une touche, puis choisissez la commande.

Le catalogue commun pilote StageFlow, StageMark et StageMon uniquement lorsque
l’application propriétaire a rejoint le projet LIVE. Ces contrôles restent
facultatifs et chaque logiciel conserve ses commandes locales.

## 9. Sauvegarde et conflits

Un projet `.stageflow` est un dossier. Ne déplacez pas un de ses fichiers JSON
séparément : déplacez ou sauvegardez toujours le dossier entier.
Pour un échange ou une archive sous la forme d’un fichier unique, créez un
paquet `.stageflowpack`, puis importez-le pour reconstruire le dossier complet.

Chaque logiciel enregistre atomiquement sa partie et vérifie qu’elle n’a pas été
modifiée entre-temps. Des changements indépendants, par exemple deux paires ou
deux champs différents, sont fusionnés automatiquement puis affichés dans
l’éditeur. En cas de modification différente de la même valeur :

1. ne forcez pas l’écrasement ;
2. lisez le nom du domaine et du logiciel signalés ;
3. comparez ou rechargez la version la plus récente ;
4. réappliquez votre modification si nécessaire ;
5. enregistrez de nouveau.

## 10. Quel outil faut-il vraiment installer ?

| Besoin | Outil conseillé |
|---|---|
| Patch, groupes, Excel A4, plan simple | StageFlow |
| Conversion entre consoles et logiciels audio | StageDesk |
| Implantation, cues et projection | StageMark |
| Écoutes A/B, jusqu'à six circuits configurés | StageMon |
| Préparation et contrôle Dante hors ligne | Dante Config Editor |
| Plan technique DWG | AutoCAD avec le connecteur StageFlow |

Installez uniquement ce qui vous sert. StageFlow devient intéressant
lorsque vous voulez voir et piloter plusieurs outils depuis le même écran, mais
il n’est jamais obligatoire pour utiliser les autres logiciels ou le format
`.stageflow`.

---

**SiLeMIO / By Mamat----[]---**
