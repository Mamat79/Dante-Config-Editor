# Banques de machines DCE / DCE device banks

## Français

Ce dossier publie des banques compatibles avec Dante Config Editor 2026.1.
Une banque complète utilise l'extension `*.dce-bank.zip`. DCE vérifie son
manifeste, ses empreintes SHA-256, ses modèles et leur structure avant de
l'installer dans un dossier neuf ou vide.

### Banque fournie

- **DCE Generic Roles 2026.1** : rôles hors ligne génériques 8x8 et 32x32 pour la
  préparation, les essais et la formation.

Ces modèles ne représentent aucun appareil Dante réel. Ils ne contiennent ni
`instance_id`, ni `device_id`, ni adresse IP, ni abonnement. Leur présence dans
ce dépôt ne constitue pas une validation d'import par Dante Controller.

### Banque communautaire

- **DCE Community Devices 2026.1** : 41 modèles illustrés provenant de
  configurations représentatives. La banque couvre notamment Allen & Heath,
  Audinate, Clear-Com, d&b, Fohhn, Glensound, Lab Gruppen, Lake, Powersoft,
  RAMI, RDL, RME, Sennheiser, Shure, TASCAM et Yamaha. Les capacités vont de
  0 à 144 canaux TX ou RX et tous les labels sont génériques.

Les identités matérielles, paramètres réseau et abonnements du projet source
ont été retirés, de même que les flows et références de patch. Les images
proviennent de pages produit officielles ou de visuels dont la publication a
été autorisée ; leur source est conservée dans les métadonnées. Les marques et
visuels restent la propriété de leurs détenteurs respectifs. Ces modèles
génériques doivent être vérifiés dans Dante Controller avant usage sur un
projet réel.

### Télécharger et installer

1. Téléchargez dans le tableau ci-dessous le fichier `*.dce-bank.zip` sans le
   décompresser.
2. Dans DCE, ouvrez **Banque de machines**.
3. Cliquez sur **Importer une banque**.
4. Choisissez un dossier neuf ou vide. DCE ne remplace jamais une banque
   existante.

| Banque | Contenu | Téléchargement | SHA-256 |
|---|---|---|---|
| DCE Generic Roles 2026.1 | Rôles génériques 8x8 et 32x32 | [Télécharger](DCE_Generic_Roles_2026_1.dce-bank.zip) | `0b07af7c63276e3648a03626120b0ba9bbc4b469e67484f0fd8b8cb75d3ea27b` |
| DCE Community Devices 2026.1 | 41 modèles illustrés et assainis | [Télécharger](DCE_Community_Devices_2026_1.dce-bank.zip) | `67fe30378086240b0939ece5d7c7b74e93dfe94d59d53b3561b03e1433a0fb2a` |

### Partager une banque

1. Dans DCE, cliquez sur **Exporter la banque**.
2. Conservez l'archive `*.dce-bank.zip` produite.
3. Avant de la publier, vérifiez qu'elle ne contient aucune donnée de
   production confidentielle.
4. Proposez l'archive dans une issue ou une pull request du dépôt avec une
   description, la version de DCE utilisée et son SHA-256.

Les modèles de matériels réels doivent provenir d'un XML que leur auteur est
autorisé à partager. DCE assainit les identités, le réseau et les abonnements,
mais l'auteur reste responsable du contenu publié.

Le workflow GitHub `Machine-bank audit` reconstruit et vérifie chaque semaine
les archives publiques, leurs empreintes, leurs manifestes et l'absence de
données de projet interdites. Il fonctionne en lecture seule et ne publie
jamais automatiquement une banque personnelle.

## English

This folder publishes banks compatible with Dante Config Editor 2026.1. A full
bank uses the `*.dce-bank.zip` extension. DCE verifies its manifest, SHA-256
hashes, templates and structure before installing it into a new or empty
folder.

### Included bank

- **DCE Generic Roles 2026.1**: generic offline 8x8 and 32x32 roles for
  preparation, testing and training.

These templates do not represent real Dante hardware. They contain no
`instance_id`, `device_id`, IP address or subscription. Their presence in this
repository is not proof of a successful Dante Controller import.

### Community bank

- **DCE Community Devices 2026.1**: 41 illustrated templates identified from
  representative configurations. The bank covers Allen & Heath, Audinate,
  Clear-Com, d&b, Fohhn, Glensound, Lab Gruppen, Lake, Powersoft, RAMI, RDL,
  RME, Sennheiser, Shure, TASCAM, and Yamaha, among others. Capacities range
  from 0 to 144 Tx or Rx channels and all channel labels are generic.

Hardware identities, network settings and source-project subscriptions were
removed, together with flows and patch references. Images come from official
product pages or visual assets approved for publication, and their sources are
recorded in metadata. Trademarks and visual assets remain the property of
their respective owners. Verify these generic templates in Dante Controller
before using them in a real project.

### Download and install

1. Download the required `*.dce-bank.zip` file from the table below without
   extracting it.
2. In DCE, open **Device bank**.
3. Select **Import bank**.
4. Choose a new or empty folder. DCE never replaces an existing bank.

| Bank | Contents | Download | SHA-256 |
|---|---|---|---|
| DCE Generic Roles 2026.1 | Generic 8x8 and 32x32 roles | [Download](DCE_Generic_Roles_2026_1.dce-bank.zip) | `0b07af7c63276e3648a03626120b0ba9bbc4b469e67484f0fd8b8cb75d3ea27b` |
| DCE Community Devices 2026.1 | 41 illustrated sanitized templates | [Download](DCE_Community_Devices_2026_1.dce-bank.zip) | `67fe30378086240b0939ece5d7c7b74e93dfe94d59d53b3561b03e1433a0fb2a` |

### Share a bank

1. In DCE, select **Export bank**.
2. Keep the generated `*.dce-bank.zip` archive.
3. Before publishing it, verify that it contains no confidential production
   data.
4. Submit the archive in a repository issue or pull request with a
   description, the DCE version used and its SHA-256 hash.

Real-hardware templates must come from XML that the author is allowed to
share. DCE removes identities, network settings and subscriptions, but the
publisher remains responsible for the shared content.

The `Machine-bank audit` GitHub workflow rebuilds and validates the public
archives, hashes, manifests, and forbidden project-data checks every week. It
runs read-only and never publishes a personal bank automatically.
