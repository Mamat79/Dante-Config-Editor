<p align="center">
  <img src="media/dce-app-icon.png" width="112" alt="Icône Dante Config Editor">
</p>

<h1 align="center">Dante Config Editor</h1>

<p align="center">
  <strong>Préparez et modifiez vos configurations Dante hors ligne, dans une vue claire.</strong>
</p>

<p align="center">
  <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0.2/DanteConfigEditor2027_Installer.exe"><strong>Windows 2027.0.2</strong></a>
  ·
  <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0/DanteConfigEditor2027_macOS_AppleSilicon.dmg"><strong>macOS Apple Silicon 2027.0</strong></a>
  ·
  <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0/DanteConfigEditor2027_macOS_Intel.dmg"><strong>macOS Intel 2027.0</strong></a>
  ·
  <a href="manuals/Notice_DanteConfigEditorV3_FR.pdf">Notice Windows</a>
  ·
  <a href="manuals/Notice_DanteConfigEditor_macOS_FR.pdf">Notice Mac</a>
  ·
  <a href="manuals/Guide-Suite-SiLeMIO-FR.pdf">Guide de la suite</a>
  ·
  <a href="README_EN.md">English</a>
</p>

<p align="center">
<a href="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0.2/dante-config-editor-presentation-fr.mp4"><img src="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0.2/dante-config-editor-presentation-fr-poster.png" width="820" alt="Présentation vidéo Dante Config Editor"></a><br>
  <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0.2/dante-config-editor-presentation-fr.mp4">Présentation · FR</a>
  · <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0.2/dante-config-editor-presentation-fr.vtt">Sous-titres FR</a>
  · <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0.2/dante-config-editor-presentation-en.mp4">Presentation · EN</a>
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

DCE peut travailler seul sur une configuration XML Dante, ouvrir un dossier
<code>.stageflow</code> local ou rejoindre volontairement une session
StageFlow LIVE. Ces trois parcours restent distincts : StageFlow est gratuit
et facultatif, jamais nécessaire pour préparer votre configuration Dante.

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

DCE tient compte des paires communes, des valeurs spécifiques au groupe et des
paires masquées. Les cellules vides sont ignorées et rien n'est modifié avant
votre validation. Ce parcours fonctionne aussi lorsque DCE est utilisé seul,
sans StageFlow.

### Une connexion StageFlow explicite

Sous Windows, le bouton permanent **Connexion StageFlow**, en haut de la
fenêtre, ouvre le centre de connexion. L'état est affiché séparément :
**Autonome**, **Session disponible**, **Connecté à…** ou **Déconnecté**.
Le centre garde le projet courant visible et propose **Retour au projet**.
Il peut rester ouvert pendant que vous consultez votre configuration.
Trois usages restent distincts :

- **DCE autonome** : préparez votre configuration sans StageFlow.
- **Projet local `.stageflow`** : partagez un dossier de projet en conservant
  les données propres à chaque logiciel.
- **Session LIVE temporaire** : choisissez l'hôte StageFlow sur ce PC ou sur
  le réseau local, vérifiez le nom du projet, puis saisissez son code à six chiffres.

Si aucune session n'apparaît, vérifiez que StageFlow est ouvert et diffuse
son projet, puis actualisez. La saisie de l'adresse IPv4 privée et du port
reste disponible dans les détails techniques. Une erreur de code reste
affichée pour permettre un nouvel essai. Quitter une session est explicite ;
DCE ne rejoint pas automatiquement une session après une coupure.

Le rail **StageFlow LIVE / Télécommande Dante Config Editor** distingue
l'association au projet de l'accès mobile. DCE n'héberge pas de télécommande
mobile autonome : le QR, son activation et ses droits se gèrent dans
**StageFlow, sur l'ordinateur hôte**. Le protocole reçu par DCE ne fournit
ni lien mobile ni état actif/arrêté ; aucun QR n'est inventé à partir d'une IP.
Avec les droits nécessaires et un projet chargé, la télécommande de la suite
peut ouvrir l'espace Patch / RX ou le centre de validation de DCE. Elle ne
modifie pas le patch Dante et ne commande pas le matériel.

Les paquets Mac disponibles restent en **2027.0**, avec leur centre
**StageFlow LIVE** existant. Cette mise à jour du centre Windows ne les
présente pas comme reconstruits.

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

**Dante Config Editor v2027 est disponible sur Windows et Mac.**
La dernière version publiée est **2027.0.2 sur Windows** et **2027.0 sur Mac**
(Apple Silicon et Intel). Les liens ci-dessous donnent les fichiers publics
correspondant à votre ordinateur.

| Ressource | Lien |
|---|---|
| Installateur Windows x64 | [Télécharger directement](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0.2/DanteConfigEditor2027_Installer.exe) |
| macOS Apple Silicon | [Télécharger le DMG](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0/DanteConfigEditor2027_macOS_AppleSilicon.dmg) |
| macOS Intel | [Télécharger le DMG](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0/DanteConfigEditor2027_macOS_Intel.dmg) |
| Démarrage rapide Windows | [PDF français](manuals/QuickStart_DanteConfigEditorV3_FR.pdf) |
| Notice complète Windows | [PDF français](manuals/Notice_DanteConfigEditorV3_FR.pdf) |
| Démarrage rapide Mac | [PDF français](manuals/QuickStart_DanteConfigEditor_macOS_FR.pdf) |
| Notice complète Mac | [PDF français](manuals/Notice_DanteConfigEditor_macOS_FR.pdf) |
| Guide de la suite | [PDF français](manuals/Guide-Suite-SiLeMIO-FR.pdf) · [English PDF](manuals/SiLeMIO-Suite-Guide-EN.pdf) |
| Banque communautaire | [Télécharger la banque](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0.1/DCE_Community_Devices_2026_3.dce-bank.zip) |
| Nouveautés et limites | [Windows 2027.0.2 et versions Mac publiques](RELEASE_NOTES_2027.0.2.md) |
| Vérification des fichiers | [SHA-256](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0.2/SHA256SUMS.txt) |
| Contrôles de publication | [Tests et limites](VALIDATION_2027.0.2.md) |

Au premier lancement, l’écran **Découvrir DCE** permet d’ouvrir un XML, créer un
projet, découvrir la banque ou accéder à la notice.

Le bouton **Guide** ouvre la notice complète de DCE dans la langue choisie.
**Aide** ouvre **Découvrir DCE**. Le guide commun est accessible dans
**Aide > Guide de la suite SiLeMI/O**.

L'installateur Windows 2027.0.2 inclut les notices FR/EN de **46 pages**, avec
sommaire cliquable, les démarrages rapides et les guides communs **2027.3**.
Les liens Mac conservent la
documentation livrée avec Mac 2027.0. Les vidéos présentent les principes
généraux ; consultez la notice correspondant à votre version pour les commandes.

Sur Windows, le renommage en série se fait notamment avec les poignées de
recopie de la matrice. Sur Mac, le panneau de renommage en série propose les
suites numériques et stéréo. Les fonctions métier communes ne supposent pas
une interface identique.

Sur Windows 2027.0.2, **Fichier > Exporter le XML Dante** produit une copie
XML séparée, sans remplacer le projet ouvert. **Enregistrer** met à jour le
projet courant ; **Enregistrer sous** permet de créer une autre copie de projet.
Le correctif Mac reste aligné sur **v2027.0** pour l'instant.

Le nom public commun est **v2027**. Le tag technique est **v2027.0.2** pour
Windows et **v2027.0** pour Mac. Utilisez les liens directs de cette page ;
GitHub `Latest` suit désormais la publication en cours.
Les paquets Windows ne sont pas encore signés commercialement et les
applications Mac ne sont pas encore notariées par Apple.

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
