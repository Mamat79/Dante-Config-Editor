# Banques de machines DCE / DCE device banks

## Français

Ce dossier publie des banques compatibles avec Dante Config Editor V3.6.
Une banque complète utilise l'extension `*.dce-bank.zip`. DCE vérifie son
manifeste, ses empreintes SHA-256, ses modèles et leur structure avant de
l'installer dans un dossier neuf ou vide.

### Banque fournie

- **DCE Generic Roles 3.6** : rôles hors ligne génériques 8x8 et 32x32 pour la
  préparation, les essais et la formation.

Ces modèles ne représentent aucun appareil Dante réel. Ils ne contiennent ni
`instance_id`, ni `device_id`, ni adresse IP, ni abonnement. Leur présence dans
ce dépôt ne constitue pas une validation d'import par Dante Controller.

### Banque communautaire

- **DCE Community Devices 3.6** : Yamaha QL1 (32 TX / 32 RX), Yamaha
  Rio1608-D2 (16 TX / 8 RX), Fohhn DI4.1000 (0 TX / 4 RX), Lake LM 44
  (0 TX / 4 RX) et RME Digiface Dante (64 TX / 64 RX). Les labels sont
  génériques et les cinq modèles incluent une image.

Les identités matérielles, paramètres réseau et abonnements du projet source
ont été retirés. Les images ont été fournies et autorisées à la publication par
le contributeur ; les marques et visuels restent la propriété de leurs
détenteurs respectifs. Ces modèles n'ont pas encore fait l'objet d'une
validation matérielle ou d'un import confirmé dans Dante Controller.

### Télécharger et installer

1. Téléchargez dans le tableau ci-dessous le fichier `*.dce-bank.zip` sans le
   décompresser.
2. Dans DCE, ouvrez **Banque de machines**.
3. Cliquez sur **Importer une banque**.
4. Choisissez un dossier neuf ou vide. DCE ne remplace jamais une banque
   existante.

| Banque | Contenu | Téléchargement | SHA-256 |
|---|---|---|---|
| DCE Generic Roles 3.6 | Rôles génériques 8x8 et 32x32 | [Télécharger](DCE_Generic_Roles_3_6.dce-bank.zip) | `bd24239205a83cb9aec60938bf10d5eeab798385775e945cf5ec0051ee32d8df` |
| DCE Community Devices 3.6 | Cinq modèles illustrés | [Télécharger](DCE_Community_Devices_3_6.dce-bank.zip) | `2913c2bf943de925932e8eac55c32032f09bf4fd7a2fb294b6fa12ca33b854c7` |

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

This folder publishes banks compatible with Dante Config Editor V3.6. A full
bank uses the `*.dce-bank.zip` extension. DCE verifies its manifest, SHA-256
hashes, templates and structure before installing it into a new or empty
folder.

### Included bank

- **DCE Generic Roles 3.6**: generic offline 8x8 and 32x32 roles for
  preparation, testing and training.

These templates do not represent real Dante hardware. They contain no
`instance_id`, `device_id`, IP address or subscription. Their presence in this
repository is not proof of a successful Dante Controller import.

### Community bank

- **DCE Community Devices 3.6**: Yamaha QL1 (32 Tx / 32 Rx), Yamaha
  Rio1608-D2 (16 Tx / 8 Rx), Fohhn DI4.1000 (0 Tx / 4 Rx), Lake LM 44
  (0 Tx / 4 Rx), and RME Digiface Dante (64 Tx / 64 Rx). Labels are generic
  and all five templates include an image.

Hardware identities, network settings and source-project subscriptions were
removed. The images were supplied and approved for publication by the
contributor; trademarks and visual assets remain the property of their
respective owners. These templates have not yet been validated against
physical hardware or through a confirmed Dante Controller import.

### Download and install

1. Download the required `*.dce-bank.zip` file from the table below without
   extracting it.
2. In DCE, open **Device bank**.
3. Select **Import bank**.
4. Choose a new or empty folder. DCE never replaces an existing bank.

| Bank | Contents | Download | SHA-256 |
|---|---|---|---|
| DCE Generic Roles 3.6 | Generic 8x8 and 32x32 roles | [Download](DCE_Generic_Roles_3_6.dce-bank.zip) | `bd24239205a83cb9aec60938bf10d5eeab798385775e945cf5ec0051ee32d8df` |
| DCE Community Devices 3.6 | Five illustrated templates | [Download](DCE_Community_Devices_3_6.dce-bank.zip) | `2913c2bf943de925932e8eac55c32032f09bf4fd7a2fb294b6fa12ca33b854c7` |

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
