<p align="center">
  <img src="media/dce-app-icon.png" width="112" alt="Icône Dante Config Editor">
</p>

<h1 align="center">Dante Config Editor</h1>

<p align="center">
  <strong>Préparez et modifiez vos configurations Dante hors ligne, dans une vue claire.</strong>
</p>

<p align="center">
  <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.10/DanteConfigEditor2026_10_Installer.exe"><strong>⬇ Windows 2026.10</strong></a>
  ·
  <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0/DanteConfigEditor2027_macOS_AppleSilicon.dmg"><strong>macOS Apple Silicon v2027</strong></a>
  ·
  <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0/DanteConfigEditor2027_macOS_Intel.dmg"><strong>macOS Intel v2027</strong></a>
  ·
  <a href="manuals/Notice_DanteConfigEditorV3_FR.pdf">Notice Mac v2027</a>
  ·
  <a href="manuals/Guide-Suite-SiLeMIO-FR.pdf">Guide de la suite</a>
  ·
  <a href="README_EN.md">English</a>
</p>

<p align="center">
  <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.10/dante-config-editor-presentation-fr.mp4"><img src="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.10/dante-config-editor-presentation-fr-poster.png" width="820" alt="Présentation vidéo Dante Config Editor"></a><br>
  <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.10/dante-config-editor-presentation-fr.mp4">Présentation · FR</a>
  · <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.10/dante-config-editor-presentation-fr.vtt">Sous-titres FR</a>
  · <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.10/dante-config-editor-presentation-en.mp4">Presentation · EN</a>
</p>

---

## Dante Config Editor, à quoi ça sert ?

Dante Config Editor, ou **DCE**, est un logiciel de préparation hors ligne pour
les réseaux audio Dante.

Il permet d’ouvrir un XML Dante Controller, de comprendre rapidement le contenu
d’une installation, de corriger les noms, de préparer le patch et de fusionner
plusieurs projets sans devoir connecter les machines.

**Renommez sans perdre le patch.** Quand vous renommez une machine ou un canal,
DCE met à jour les références de subscriptions concernées. Vous pouvez aussi
partir de zéro avec la banque de machines, puis exporter votre configuration
vers Dante Controller.

DCE est particulièrement utile pour préparer une installation avant d’arriver
sur site, documenter un réseau existant, appliquer de nombreux changements
répétitifs ou vérifier un fichier avant de le remettre à Dante Controller.

[![Vue d’ensemble de Dante Config Editor](media/fr/overview.png)](media/fr/overview.png)

Les captures de présentation illustrent l'interface Windows. Les fonctions
métier existent aussi sur Mac, avec une disposition propre à macOS.

## Un exemple concret

Vous devez préparer un festival avec une console, plusieurs racks de scène, un
ordinateur d’enregistrement et des amplificateurs réseau :

1. ouvrez le XML de référence dans DCE ;
2. visualisez les machines, leurs canaux TX/RX et les subscriptions ;
3. renommez les rôles et les canaux, à l’unité ou en série ;
4. préparez le patch dans la matrice ou avec Easy Patch ;
5. ajoutez les machines manquantes depuis la banque ;
6. lancez la vérification du projet ;
7. exportez le XML final et ouvrez-le dans Dante Controller avant exploitation.

Vous pouvez préparer l’essentiel au bureau et réserver le temps sur site aux
contrôles réels : câblage, horloge, réseau et audio.

## Ce que DCE permet de faire

### Voir toute l’installation

DCE rassemble dans une seule interface les machines, canaux TX/RX,
subscriptions, formats audio, latences, fréquences d’échantillonnage,
Preferred Masters et informations réseau disponibles dans le fichier.

Les propriétés reconnues peuvent être modifiées pour une machine ou par des
actions globales : fréquence, latence, format audio, mode redondant/daisychain,
horloge et adressage IP. Un réglage que le profil ne permet pas de modifier
reste indisponible avec une explication ; DCE n'invente pas les capacités du
matériel.

La vue d’ensemble aide à repérer rapidement les machines sans patch, les
réglages différents ou les références à contrôler.

### Renommer rapidement

- Renommer les machines et les canaux directement.
- Appliquer une série numérique à une sélection.
- Conserver les zéros dans des noms comme <code>Mic 01</code>.
- Prolonger des paires stéréo comme <code>FX-1L</code>,
  <code>FX-1R</code>, <code>FX-2L</code>, <code>FX-2R</code>.
- Dupliquer ou réorganiser un rôle sans recommencer tout le projet.

[![Réglages des machines](media/fr/devices.png)](media/fr/devices.png)

### Préparer le patch

- Matrice de patch compacte.
- Easy Patch par sélection ou plage 1:1.
- Patch à l’unité, vertical ou diagonal.
- Recherche de la source d’un RX.
- Affichage des destinations d’un TX.
- FLIP RX/TX entre deux machines.
- Réinitialisation ciblée d’une partie du patch.

[![Matrice de patch](media/fr/patch.png)](media/fr/patch.png)

### Fusionner plusieurs projets

DCE peut ajouter le contenu d’un second XML dans le projet ouvert. Il vous aide
à résoudre les conflits de noms et à réutiliser ou renommer les rôles importés.

Cette fonction est pratique pour réunir des préparations provenant de plusieurs
équipes ou plusieurs zones d’une installation.

### Préparer des machines sans les avoir sous la main

La banque de machines permet d’ajouter des modèles réutilisables pour préparer
un projet hors ligne. La banque communautaire comprend des profils de consoles,
racks, interfaces, amplificateurs et équipements réseau audio.

[![Banque de machines](media/fr/device-bank.png)](media/fr/device-bank.png)

### Vérifier et documenter

- Assistant de contrôle avant export.
- Erreurs et avertissements regroupés par gravité.
- Rapports TXT ou PDF.
- Patchbooks et comparaisons avant/après.
- Import et export de labels avec Excel, CSV, JSON et ODS.
- Synoptique exportable en PDF ou SVG.

[![Synoptique de Dante Config Editor](media/fr/synoptic.png)](media/fr/synoptic.png)

## Un seul projet avec les autres outils SiLeMI/O

DCE utilise le dossier <code>.stageflow</code> comme projet natif. Il peut créer
et ouvrir ce projet de manière autonome : StageFlow n’est pas obligatoire.

Si le projet StageFlow ne contient pas encore de configuration Dante, DCE
propose immédiatement de **créer depuis zéro**, d'**ouvrir un XML Dante** ou de
continuer plus tard. La création depuis zéro démarre avec une première machine
personnalisée ou issue de la banque. À l'enregistrement, DCE ajoute uniquement
son domaine Dante et conserve les données des autres logiciels.

Le même projet peut ensuite être utilisé avec StageDesk, sous-titré **Save My Time**, StageMark,
StageFlow, StageMon et AutoCAD sans multiplier les fichiers contradictoires.

### Envoyer le patch StageFlow vers les RX Dante et suivre le projet en LIVE

Un groupe de patch StageFlow peut servir directement à nommer
tout ou partie des canaux RX d'une machine Dante :

1. ouvrez le projet `.stageflow` dans DCE ;
2. choisissez le groupe et le nom à utiliser : source, micro, source + micro ou
   libellé StageFlow ;
3. choisissez la machine, le premier RX et le nombre de canaux ;
4. contrôlez l'aperçu Avant / Après, puis appliquez.

DCE tient compte des paires communes, des surcharges propres au groupe et des
paires masquées. Les cellules vides sont ignorées et rien n'est modifié avant
votre validation. Ce parcours fonctionne aussi lorsque DCE est utilisé seul,
sans StageFlow.

### Nouveau sur Mac dans v2027 : un centre de connexion clair

Le bouton **StageFlow LIVE**, toujours accessible en haut de la fenêtre Mac,
ouvre le centre de connexion. Trois usages restent
distincts :

- **DCE autonome** : préparez votre configuration sans StageFlow.
- **Projet local `.stageflow`** : partagez un dossier de projet en conservant
  les données propres à chaque logiciel.
- **Session LIVE temporaire** : choisissez l'hôte StageFlow sur votre réseau
  local, vérifiez le nom du projet, puis saisissez son code à six chiffres.

Si la découverte ne fonctionne pas, vous pouvez renseigner l'adresse IPv4
privée et le port de l'hôte. Une erreur de code reste affichée dans la fenêtre
pour permettre un nouvel essai. Quitter une session est explicite ; DCE ne
rejoint pas automatiquement une session après une coupure.

Les changements liés aux RX sont suivis par leurs **UUID explicites**, jamais
par un simple rapprochement de noms. Une règle manquante, des modifications
locales non enregistrées ou un conflit refusent la transaction entière et
conservent le dernier état valide.

Un **bandeau orange, visible sur toutes les pages**, présente les changements
reçus avec l'ancien et le nouveau label, leur origine et leur heure. Acquitter
une alerte ou toutes les alertes visibles ne concerne que votre poste ; une
nouvelle alerte arrivée ensuite reste à traiter. Vous pouvez couper la
réception des notifications sans quitter LIVE. Une pause décidée par l'hôte
est indiquée séparément et n'interrompt pas la connexion.

**LIVE synchronise le projet, pas le matériel Dante.** Utilisez cette liaison
sur un réseau local de confiance. La console locale StageFlow, qui commande
les logiciels du même poste, reste propre à Windows ; les projets locaux et
le centre de connexion LAN DCE existent également sur Mac.

[![Un seul projet, plusieurs outils](media/stageflow-suite-workflow.svg)](media/stageflow-suite-workflow.svg)

- [StageFlow — patch, groupes, Excel et plan de scène](https://github.com/Mamat79/StageFlow)
- [StageDesk — Save My Time · transfert entre consoles et logiciels](https://github.com/Mamat79/StageDesk/releases/latest)
- [StageMark — implantation et projection](https://github.com/Mamat79/StageMark/releases/latest)
- [StageMon — matrice d’écoute live](https://github.com/Mamat79/StageMon/releases/latest)

## Les formats à connaître

- **Projet <code>.stageflow</code>** : projet natif partagé avec la suite
  SiLeMI/O.
- **XML Dante** : fichier d’échange destiné à Dante Controller.
- **Projet historique <code>.dceproj</code>** : ancien format DCE toujours
  ouvrable.
- **Banque de machines** : collection de modèles réutilisables pour préparer un
  projet.

## Télécharger et démarrer

**macOS : v2027**, avec deux DMG distincts pour Apple Silicon et Intel.
**Windows 11 x64 : 2026.10**, version stable conservée. Les nouveautés v2027
décrites ci-dessus concernent la publication Mac ; aucun installateur Windows
v2027 n'est distribué dans cette Release.

| Ressource | Lien |
|---|---|
| Installateur Windows 2026.10 | [Télécharger directement](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.10/DanteConfigEditor2026_10_Installer.exe) |
| macOS Apple Silicon | [Télécharger le DMG](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0/DanteConfigEditor2027_macOS_AppleSilicon.dmg) |
| macOS Intel | [Télécharger le DMG](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0/DanteConfigEditor2027_macOS_Intel.dmg) |
| Démarrage rapide v2027 | [PDF français](manuals/QuickStart_DanteConfigEditorV3_FR.pdf) |
| Notice complète v2027 | [PDF français](manuals/Notice_DanteConfigEditorV3_FR.pdf) |
| Notice Windows 2026.10 | [PDF français](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.10/Notice_DanteConfigEditorV3_FR.pdf) |
| Guide de la suite | [PDF français](manuals/Guide-Suite-SiLeMIO-FR.pdf) · [English PDF](manuals/SiLeMIO-Suite-Guide-EN.pdf) |
| Banque communautaire | [Télécharger la banque](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0/DCE_Community_Devices_2026_3.dce-bank.zip) |
| Nouveautés et limites | [Notes de version v2027](RELEASE_NOTES_2027.md) |
| Vérification des paquets Mac | [Tests, provenance et SHA-256](MACOS_VALIDATION_2027.json) |

Au premier lancement, l’écran **Découvrir DCE** permet d’ouvrir un XML, créer un
projet, découvrir la banque ou accéder à la notice.

Les six PDF sont également inclus dans les applications Mac v2027. Les vidéos présentent
les fonctions métier ; la notice actualisée détaille aussi le centre de
connexion LIVE et les notifications. Les versions précédentes restent
accessibles dans [les Releases](https://github.com/Mamat79/Dante-Config-Editor/releases).

Le nom public Mac est **v2027** et son tag technique est **v2027.0**. Les liens
Mac de cette page donnent directement cette version. La Release GitHub marquée
**Latest** reste **v2026.10** pour préserver les mises à jour Windows : les
utilisateurs Mac peuvent télécharger v2027 avec les liens ci-dessus même si
l'ancien gestionnaire de mises à jour ne la propose pas encore.

Les applications Mac ne sont pas encore notariées par Apple. Les notices
v2027 couvrent aussi l'interface Windows en préparation ; utilisez la notice
2026.10 pour les commandes de la version Windows actuellement disponible.

## Utilisation et licence

DCE offre 30 jours sans rappel au premier lancement. Ensuite, le logiciel et ses
fonctions restent utilisables ; un rappel apparaît simplement au démarrage.

Une licence permanente à **29 € TTC**, en paiement unique, supprime ce rappel.

**[Acheter une licence permanente DCE](https://dce-license.mamat79-dce.workers.dev/buy)**

## À savoir avant une exploitation

DCE est un outil tiers indépendant, sans affiliation avec Audinate. Il prépare
des fichiers hors ligne et ne pilote pas directement un réseau Dante.

Travaillez sur une copie, ouvrez le XML obtenu dans Dante Controller et vérifiez
la configuration sur le matériel réel avant toute exploitation importante.

---

<p align="right">
  <strong>SiLeMI/O</strong><br>
  By Mamat<br>
  <code>-------[]--</code>
</p>
