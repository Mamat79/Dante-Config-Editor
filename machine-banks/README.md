# Banques de machines DCE / DCE device banks

## Français

Ce dossier publie des banques pour Dante Config Editor. La version minimale
requise est indiquée dans `catalog.json` pour chaque banque.
Une banque complète utilise l'extension `*.dce-bank.zip`. DCE vérifie son
manifeste, ses empreintes SHA-256, ses modèles et leur structure avant de
l'installer dans un dossier neuf ou vide.

### Banque fournie

- **DCE Generic Roles 2026.3** : rôles hors ligne génériques 8x8 et 32x32 pour la
  préparation, les essais et la formation.

Ces modèles ne représentent aucun appareil Dante réel. Ils ne contiennent ni
`instance_id`, ni `device_id`, ni adresse IP, ni abonnement. Leur présence dans
ce dépôt ne constitue pas une validation d'import par Dante Controller.

### Banque communautaire

- **DCE Community Devices 2026.3** : 77 modèles illustrés provenant de
  configurations représentatives ou de caractéristiques constructeur
  vérifiées. La banque couvre notamment Allen & Heath, Audinate, Clear-Com,
  d&b, DiGiCo, Focusrite, Fohhn, Glensound, Lab Gruppen, Lake, Neutrik,
  Powersoft, RAMI, RDL, RME, Sennheiser, Shure, Studio Technologies, TASCAM,
  Tieline, Wisycom et Yamaha.
  Les familles Yamaha CL/QL/DM/TF/RIVAGE, Allen & Heath dLive/Avantis/SQ et
  DiGiCo sont représentées par leur appareil, carte ou interface Dante. Les
  capacités vont de 0 à 256 canaux TX ou RX et tous les labels sont génériques.
  Les 57 modèles historiques ont été testés sur matériel réel par le
  mainteneur. Les 20 nouveaux profils reposent sur des caractéristiques
  constructeur officielles et sont marqués `StructurallyValidated`.

Les identités matérielles, paramètres réseau et abonnements du projet source
ont été retirés, de même que les flows et références de patch. Les images
proviennent de pages produit officielles ou de visuels dont la publication a
été autorisée ; leur source est conservée dans les métadonnées. Les marques et
visuels restent la propriété de leurs détenteurs respectifs. Ces modèles
génériques doivent être vérifiés dans Dante Controller avant usage sur un
projet réel.

### Modèles XML 2.1

- **DCE Legacy XML 2.1 Models** : six rôles génériques identifiés par leur
  fabricant, leur modèle et leur nombre de canaux dans un preset XML 2.1.0 :
  HOLOPHONIX Ultra, Powersoft Ottocanali et X Series, Yamaha CL5, Rio1608-D2 et
  Rio3224-D2. Aucun nom de machine, label de production, identifiant matériel,
  adresse, flux ou abonnement du preset source n'est repris. Ces rôles sont
  réservés aux projets XML 2.1.0, sans conversion implicite vers 3.0.0.
  Leur structure est vérifiée hors ligne, pas leur import dans Dante Controller.

### Télécharger et installer

1. Téléchargez dans le tableau ci-dessous le fichier `*.dce-bank.zip` sans le
   décompresser.
2. Dans DCE, ouvrez **Banque de machines**.
3. Cliquez sur **Importer une banque**.
4. Choisissez un dossier neuf ou vide. DCE ne remplace jamais une banque
   existante.

| Banque | Contenu | Téléchargement | SHA-256 |
|---|---|---|---|
| DCE Generic Roles 2026.3 | Rôles génériques 8x8 et 32x32 | [Télécharger](DCE_Generic_Roles_2026_3.dce-bank.zip) | `1f0afe83224499e2ebd8813cc0e91007083900f124f1789a7684951c5234b74b` |
| DCE Community Devices 2026.3 | 77 modèles assainis : 57 testés sur matériel, 20 validés structurellement | [Télécharger](DCE_Community_Devices_2026_3.dce-bank.zip) | `219cbd128c33ff334972c870c2f4735199fcd507935c56be65ec7943a2c5d50f` |
| DCE Legacy XML 2.1 Models | 6 rôles génériques pour projets XML 2.1.0 | [Télécharger](DCE_Legacy_XML_2_1_Models.dce-bank.zip) | `0c0437ab55a776678cac14fcb9f9d0a0fdb5450bf3f796a626cdcb6917d29a2e` |

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

Les archives publiées ici sont accompagnées de leur empreinte SHA-256 dans
`catalog.json`. La banque personnelle ne fait pas partie de ce dépôt.

## English

This folder publishes banks for Dante Config Editor. The minimum DCE version
for each bank is listed in `catalog.json`. A full
bank uses the `*.dce-bank.zip` extension. DCE verifies its manifest, SHA-256
hashes, templates and structure before installing it into a new or empty
folder.

### Included bank

- **DCE Generic Roles 2026.3**: generic offline 8x8 and 32x32 roles for
  preparation, testing and training.

These templates do not represent real Dante hardware. They contain no
`instance_id`, `device_id`, IP address or subscription. Their presence in this
repository is not proof of a successful Dante Controller import.

### Community bank

- **DCE Community Devices 2026.3**: 77 illustrated templates identified from
  representative configurations or verified manufacturer specifications. The
  bank covers Allen & Heath, Audinate, Clear-Com, d&b, DiGiCo, Focusrite,
  Fohhn, Glensound, Lab Gruppen, Lake, Neutrik, Powersoft, RAMI, RDL, RME,
  Sennheiser, Shure, Studio Technologies, TASCAM, Tieline, Wisycom, and Yamaha.
  Yamaha CL/QL/DM/TF/RIVAGE, Allen & Heath dLive/Avantis/SQ, and DiGiCo are
  represented through the relevant Dante device, card, or interface.
  Capacities range from 0 to 256 Tx or Rx channels and all labels are generic.
  The 57 historical templates were tested on real hardware by the maintainer.
  The 20 new profiles are based on official manufacturer specifications and
  are marked `StructurallyValidated`.

Hardware identities, network settings and source-project subscriptions were
removed, together with flows and patch references. Images come from official
product pages or visual assets approved for publication, and their sources are
recorded in metadata. Trademarks and visual assets remain the property of
their respective owners. Verify these generic templates in Dante Controller
before using them in a real project.

### XML 2.1 models

- **DCE Legacy XML 2.1 Models**: six generic roles based on manufacturer,
  model and channel count found in an XML 2.1.0 preset: HOLOPHONIX Ultra,
  Powersoft Ottocanali and X Series, Yamaha CL5, Rio1608-D2 and Rio3224-D2.
  No production device name, channel label, hardware identity, address, flow
  or subscription is retained. These roles are only for XML 2.1.0 projects;
  there is no implicit conversion to 3.0.0. Their structure was checked
  offline, not imported into Dante Controller.

### Download and install

1. Download the required `*.dce-bank.zip` file from the table below without
   extracting it.
2. In DCE, open **Device bank**.
3. Select **Import bank**.
4. Choose a new or empty folder. DCE never replaces an existing bank.

| Bank | Contents | Download | SHA-256 |
|---|---|---|---|
| DCE Generic Roles 2026.3 | Generic 8x8 and 32x32 roles | [Download](DCE_Generic_Roles_2026_3.dce-bank.zip) | `1f0afe83224499e2ebd8813cc0e91007083900f124f1789a7684951c5234b74b` |
| DCE Community Devices 2026.3 | 77 sanitized templates: 57 hardware-tested, 20 structurally validated | [Download](DCE_Community_Devices_2026_3.dce-bank.zip) | `219cbd128c33ff334972c870c2f4735199fcd507935c56be65ec7943a2c5d50f` |
| DCE Legacy XML 2.1 Models | 6 generic roles for XML 2.1.0 projects | [Download](DCE_Legacy_XML_2_1_Models.dce-bank.zip) | `0c0437ab55a776678cac14fcb9f9d0a0fdb5450bf3f796a626cdcb6917d29a2e` |

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

The published archives have SHA-256 hashes in `catalog.json`. The personal
bank is not part of this repository.
