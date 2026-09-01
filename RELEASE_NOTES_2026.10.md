# Dante Config Editor - version 2026.10

[English release notes](RELEASE_NOTES_2026.10_EN.md)

## Interface SiLeMIO harmonisée

Dante Config Editor adopte le système visuel commun SiLeMIO v1 : une barre
d'identité plus compacte, les commandes Annuler/Rétablir toujours visibles,
le thème sous forme d'icône, la langue compacte puis le bouton contour
`? Aide` toujours accessibles à droite, une navigation plus calme et des
panneaux sans effets décoratifs inutiles. Les thèmes clair et sombre utilisent
désormais les palettes Atelier hybride et Studio Graphite.

Les champs, listes, boutons et dialogues partagent les mêmes hauteurs, rayons
et alignements. Cette évolution est strictement visuelle : le moteur XML, les
banques, StageFlow, les licences et les formats de projet restent inchangés.

## Créer Dante depuis un projet StageFlow vide

Après l'ouverture d'un projet `.stageflow` sans domaine Dante, DCE propose
directement **Créer depuis zéro**, **Ouvrir un XML Dante** ou **Plus tard**.
Le premier choix ouvre l'assistant de la première machine, personnalisée ou
issue de la banque. DCE ajoute uniquement `dante/dante.json` et son paquet ;
les domaines Patch, CAD, SMT et StageMark restent inchangés.

L'ancien parcours reste disponible : ouvrir le projet StageFlow vide, ouvrir
un XML Dante existant, puis utiliser **Enregistrer**. Une fois une machine RX
présente, **Associer le patch StageFlow aux canaux RX** est accessible depuis
Projet et depuis **Import / Export > Labels**. Cette action reprend les noms
Source, Micro, Source + micro ou Libellé StageFlow du groupe choisi avec aperçu
Avant / Après.

## Console locale StageFlow sous Windows

StageFlow peut détecter DCE, ouvrir le projet courant dans l'instance
unique existante, afficher Patch / RX ou le centre de validation et demander
l'enregistrement du domaine Dante. La présence utilise un bail court et une
écriture atomique ; le pipe est limité à l'utilisateur courant. DCE vérifie le
nonce d'instance ainsi que les UUID du projet et de la session LIVE.

Cette console ne publie aucune capacité de commande du réseau Dante. Une
ouverture ou un rechargement reste une opération de projet hors ligne et ne
déclenche jamais d'action matérielle. Si le projet courant contient des
modifications, DCE présente **Enregistrer / Abandonner / Annuler** avant toute
bascule. L'ouverture interactive accepte jusqu'à cinq minutes ; les commandes
rapides restent limitées à deux secondes et aucune seconde instance n'est créée.

Lors d'un enregistrement partagé, DCE prend `dante.lock` puis `project.lock`,
relit les deux documents sous verrou et ajoute sa référence au dernier manifeste
disponible. Les ajouts concurrents des autres outils sont ainsi conservés ; une
collision sur le domaine Dante est refusée explicitement.

Le guide commun FR/EN et ses schémas d'architecture et de parcours sont fournis
avec la documentation. Ils distinguent le travail autonome dans chaque outil,
le projet commun `.stageflow` utilisable sans StageFlow et la console
centrale facultative de StageFlow.

Le menu **Aide** et le centre de validation proposent aussi **Guide de la suite
SiLeMI/O**. DCE ouvre alors le PDF local français ou anglais selon la langue
active, sans remplacer le démarrage rapide ni la notice complète propres à DCE.

## Nouveau : suivi StageFlow LIVE V1

Quand un projet `.stageflow` est orchestré par StageFlow, DCE reconnaît
désormais son bail LIVE court et affiche un état explicite : connecté,
disponible avec suivi désactivé, autonome ou conflit. Le suivi est activé par
défaut, mémorisé localement et peut être désactivé depuis la page Labels.

Une association RX mémorise maintenant l'UUID du groupe, l'UUID de la ligne,
le mode de nommage, la machine cible et le `DanteId` RX. En LIVE, seuls ces
canaux sont recalculés. DCE n'utilise jamais de rapprochement par texte. Une
règle absente, un hash concurrent ou un travail local non enregistré bloque
toute la transaction et conserve le dernier état valide.

Le bail est validé avant utilisation : protocole et version, `projectId`,
horodatages, durée maximale, capacité d'événements, taille et absence de lien
symbolique sur `.live`. À son expiration, DCE revient automatiquement en mode
autonome. DCE ne crée pas de session et ne commande jamais le réseau Dante
réel : il modifie uniquement son projet hors ligne et son propre domaine.

## Nouveau : patch StageFlow vers canaux RX

Depuis un projet `.stageflow` ouvert, DCE peut maintenant utiliser directement
un groupe du patch pour nommer tout ou partie des canaux RX d'une machine Dante.
La commande se trouve dans **Import / Export > Labels > Associer le patch
StageFlow aux canaux RX**.

Le dialogue propose le groupe StageFlow, quatre modes de nommage - Source,
Micro, Source + micro ou Libellé StageFlow -, la machine cible, le premier canal
du patch, le premier RX Dante et le nombre de canaux. L'aperçu Avant / Après est
obligatoire. Les paires communes, les surcharges et les paires masquées sont
résolues pour le groupe choisi ; les cellules vides sont affichées puis ignorées
sans bloquer les lignes remplies.

DCE reste autonome et hors ligne. Aucun XML n'est modifié avant **Appliquer**,
les canaux TX ne sont pas proposés dans ce parcours, et `patch.json` n'est
jamais réécrit. Les UUID associés sont conservés dans le domaine Dante lors de
l'enregistrement du projet.

Les interfaces Windows et macOS exposent le même flux en français et en anglais.

## Correctif 2026.8.1

Lorsqu'un projet StageFlow ne contient pas encore de domaine Dante, DCE permet
soit de créer sa configuration de zéro avec **Nouveau projet**, soit d'ouvrir
un XML Dante puis d'utiliser **Enregistrer**. La commande **Enregistrer sous**
reste réservée à la création d'un autre projet et refuse volontairement un
dossier `.stageflow` existant.

Le libellé commun Windows/macOS et le flux réel sont couverts par un test de
contrat. Celui-ci vérifie aussi que l'ajout de `dante/dante.json` et du paquet
`.dceproj` conserve `patch`, `CAD` et `SMT` octet par octet. Le moteur XML, les
licences et le format StageFlow restent inchangés.

## Projet natif `.stageflow`

DCE utilise désormais le dossier `.stageflow` comme format de projet par
défaut. L'application sait le créer, l'ouvrir et l'enregistrer de façon
autonome. StageFlow reste gratuit et facultatif ; il n'est pas requis
pour utiliser DCE.

L'import et l'export XML Dante restent disponibles. Les anciens fichiers XML
et paquets `.dceproj` restent lisibles. Le moteur XML, ses mutations ciblées et
ses garde-fous de compatibilité ne sont pas reconstruits par cette évolution.

## Projet partagé sans conflit

- DCE ne possède que `dante/dante.json` et les paquets sous `dante/` ;
- `patch.json` est lu par UUID explicites, jamais par rapprochement de labels ;
- les domaines CAD, SMT, StageMark et les domaines inconnus ne sont ni réécrits
  ni supprimés ;
- l'enregistrement utilise un verrou de domaine, un hash de base, une écriture
  temporaire et un remplacement atomique ;
- un watcher temporisé recharge les changements externes valides et conserve
  le dernier état valide si une autre écriture est incomplète ;
- un conflit concurrent bloque l'enregistrement avec une explication plutôt que
  d'écraser le travail d'une autre application.

Le projet QA commun a été copié puis enrichi avec le domaine Dante. Les fichiers
`patch.json`, `cad/cad.json`, `cad/plan.svg` et `smt/smt.json` sont restés
strictement identiques octet pour octet.

## Interface et documentation

- Nouveau projet et Enregistrer sous proposent `.stageflow` par défaut ;
- un projet StageFlow vide peut recevoir un XML Dante par Ouvrir XML puis
  Enregistrer ;
- fichiers récents, arguments de démarrage et réouverture acceptent les dossiers
  `.stageflow` ;
- le premier lancement explique le format natif et conserve un accès direct au
  XML historique, aux banques et à la notice ;
- les interfaces Windows et macOS exposent le même parcours ;
- les notices FR/EN incluent le schéma « Un seul projet, plusieurs outils » et
  documentent les verrous, UUID, hashes et limites de propriété.

## Licence et compatibilité

Les formats `DCEP1` et `DCEF1`, les licences V2, le produit signé `DCE`, les
clés publiques et le stockage local restent inchangés. Toutes les licences déjà
émises continuent de fonctionner. Le tarif reste un achat unique de 29 EUR TTC
en France via Stripe, après une période gratuite de 30 jours non bloquante.

DCE reste un éditeur hors ligne tiers, sans affiliation avec Audinate et sans
contrôle en direct du réseau Dante.

## Validation

- 540 tests Core/Windows réussis, avec les parcours StageFlow ciblés ;
- 22 tests Avalonia/macOS sans écran réussis ;
- build Windows et build macOS/Avalonia Release sans erreur ;
- tests dédiés au cycle StageFlow, à l'intégrité inter-domaines, aux UUID et aux
  conflits de hash ;
- tests dédiés aux jonctions Windows, aux liens sortant du projet et aux
  enveloppes étrangères dont le hash ou le `projectId` est invalide ;
- contrôle visuel Windows avant publication ;
- quatre notices PDF bilingues régénérées et contrôlées.

L'installateur Windows n'est pas signé Authenticode. Les paquets macOS restent
non notariés. L'export XML final doit toujours être contrôlé dans Dante
Controller avant exploitation.
