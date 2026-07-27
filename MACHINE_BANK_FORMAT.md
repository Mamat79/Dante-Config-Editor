# Format de la banque de machines DCE

Version du format : 1

## Objectifs

La banque doit être :

- lisible sans base de données ;
- copiable sur un partage ou un répertoire synchronisé ;
- versionnée ;
- vérifiable ;
- extensible ;
- indépendante des projets qui utilisent ses modèles.

## Emplacement

Par défaut sous Windows et macOS, dans le dossier Documents de l'utilisateur :

```text
Documents/
└── Dante Config Editor/
    └── Machine Bank/
```

Le chemin choisi est enregistré dans les réglages locaux de DCE. La banque
elle-même reste dans un emplacement visible et choisi par l'utilisateur.

## Arborescence

```text
Machine Bank/
├── bank.json
├── machines/
│   └── 8d9d65b0-.../
│       ├── machine.json
│       ├── template.xml
│       └── image.png
└── Backups/
    ├── bank_*.json
    ├── DeletedModels/
    └── ReplacedModels/
```

`Backups` n'est pas nécessaire pour partager la banque active, mais permet une
récupération manuelle après modification ou suppression.

## `bank.json`

Le manifeste contient :

- `formatVersion` ;
- `bankId` ;
- dates de création et de mise à jour UTC ;
- liste des `templateId`.

Le manifeste est écrit dans un fichier temporaire puis remplacé atomiquement.
L'ancien manifeste est conservé dans `Backups`.

## `machine.json`

Chaque modèle contient :

- version du format ;
- identifiant UUID du modèle ;
- nom du modèle ;
- fabricant ;
- modèle matériel ;
- description ;
- catégorie ;
- tags ;
- nombres TX et RX ;
- version du preset source ;
- namespace XML source ;
- version de DCE ayant créé le modèle ;
- dates de création et modification UTC ;
- SHA-256 de `template.xml` ;
- nom de l'image facultative.

Le fichier utilise JSON UTF-8 indenté afin de rester relisible.

## `template.xml`

Le document a une racine `<device>` et contient un fragment assaini :

- aucune identité matérielle ;
- aucune interface réseau ;
- aucune subscription ;
- aucun flow multicast copié par défaut ;
- aucun `default_name` ;
- labels TX/RX réutilisables ;
- propriétés constructeur inconnues conservées.

Le nombre de canaux et le namespace doivent correspondre à `machine.json`.

## Image

Formats acceptés :

- PNG ;
- JPEG ;
- WebP.

Taille maximale : 10 Mio. Le contenu du fichier est vérifié par signature, puis
copié dans le dossier du modèle sous un nom stable. Aucun chemin vers une image
externe n'est conservé.

## Écriture transactionnelle

### Nouveau modèle

1. validation en mémoire ;
2. écriture dans `.staging-*` ;
3. calcul de l'empreinte ;
4. déplacement atomique vers `machines/{templateId}` ;
5. mise à jour atomique du manifeste ;
6. retour arrière si le manifeste échoue.

### Modification

1. préparation du nouveau dossier dans `.staging-*` ;
2. déplacement de l'ancien modèle dans `.rollback-*` ;
3. installation du nouveau modèle ;
4. mise à jour du manifeste ;
5. conservation de l'ancien dans `Backups/ReplacedModels` ;
6. restauration de l'ancien si une étape échoue.

### Suppression

La suppression retire l'identifiant du manifeste puis déplace le dossier vers
`Backups/DeletedModels`. L'interface doit demander une confirmation.

## Archives

### Un modèle

Extension conseillée :

```text
*.dce-machine.zip
```

L'archive contient directement `machine.json`, `template.xml` et l'image
facultative. L'import refuse un identifiant déjà présent et n'écrase rien.

### Banque complète

Extension conseillée :

```text
*.dce-bank.zip
```

L'archive contient `bank.json` et `machines`. Les backups locaux ne sont pas
exportés. Tous les modèles sont relus et vérifiés avant création de l'archive.

La restauration :

- limite le nombre et la taille des entrées ;
- bloque les chemins sortant de l'archive ;
- valide le manifeste, chaque empreinte et chaque modèle ;
- exige un dossier de destination neuf ou vide ;
- n'écrase jamais une banque existante.

## Distribution GitHub et banque fournie

Le dossier public [`machine-banks`](machine-banks/README.md) contient :

- un catalogue `catalog.json` versionné ;
- les archives téléchargeables `*.dce-bank.zip` ;
- le SHA-256 de chaque archive ;
- les consignes bilingues de téléchargement et de contribution.

La banque `DCE Generic Roles 3.6` est générée de façon reproductible par
`tools/Build-BundledMachineBanks.ps1`. Elle contient uniquement des rôles
génériques 8x8 et 32x32 dépourvus d'identité matérielle, de réseau, de flows et
de subscriptions. Le script produit à la fois la banque intégrée à
l'installateur et son archive GitHub.

Le même script vérifie puis archive de façon reproductible la banque
communautaire `DCE Community Devices 3.6`. Il contrôle les noms attendus, le
nombre de canaux, les images, les empreintes XML et l'absence d'identités,
d'interfaces, d'adresses, de flows ou de subscriptions propres au projet
source. Un échec de contrôle bloque la construction de l'installateur.

L'installateur Windows demande :

1. le dossier de la banque active, mémorisé dans les réglages locaux ;
2. le dossier réservé aux banques fournies ;
3. si la banque générique doit être installée ;
4. si la banque communautaire doit être installée.

Une banque fournie existante n'est jamais modifiée. Si l'utilisateur demande
une nouvelle copie, l'installateur choisit un nom de dossier libre. Sur macOS,
les deux archives sont également placées dans le dossier `Machine Banks` du
DMG ; elles restent accessibles depuis le bouton `Banques GitHub`, puis
installables depuis la fenêtre Banque de machines.

Le workflow `.github/workflows/machine-bank-audit.yml` exécute chaque semaine
la génération reproductible et les tests de banque. Ses permissions GitHub
sont limitées à la lecture : l'ajout d'un nouveau modèle public reste une
action volontaire, examinée et commitée.

## Migration

La V3.6 crée le format 1. Aucun format historique maîtrisé n'existe encore.
`MachineBankMigrationService` constitue le point d'entrée unique :

- la version 1 est acceptée ;
- une version antérieure ou future est bloquée ;
- les fichiers ne sont pas réécrits ;
- le message indique qu'aucune migration sûre n'est disponible.

Une migration future devra :

1. reconnaître explicitement la version source ;
2. sauvegarder la banque ;
3. convertir dans une zone temporaire ;
4. valider tous les modèles ;
5. laisser l'original intact en cas d'échec.

## Partage et concurrence

La banque peut résider dans un dossier synchronisé ou partagé. La V1 protège
chaque écriture locale, mais ne fournit pas de verrou distribué entre deux
ordinateurs. Deux utilisateurs ne doivent pas modifier simultanément la même
banque. Pour un usage collaboratif, effectuer une sauvegarde avant une série de
modifications et laisser la synchronisation se terminer.

## Indépendance des instances

L'ajout d'un modèle dans un projet réalise une copie profonde. Le projet ne
conserve aucun lien dynamique avec le dossier de banque. Modifier les labels ou
le patch de la nouvelle machine ne change donc ni `machine.json` ni
`template.xml`.
