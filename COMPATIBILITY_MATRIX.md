# Matrice de compatibilité Dante XML

## Règles de lecture

Cette matrice sépare strictement trois niveaux de preuve :

- **Réussi - automatisé** : le moteur a ouvert, modifié ou sauvegardé le fichier dans un test reproductible ;
- **Réussi - import prouvé** : le fichier généré a réellement été importé dans une version identifiée de Dante Controller et le résultat a été contrôlé ;
- **Non testé / à vérifier** : aucune conclusion de compatibilité terrain ne peut être tirée.

Une réussite automatisée ne vaut jamais preuve d'import Dante Controller. Les colonnes `Résultat d'import`, `Version Dante Controller`, `Date` et `Testeur` doivent être complétées avec une preuve manuelle réelle.

## Validation terrain déclarée pour la V3.6

Le 27 juillet 2026, le mainteneur a confirmé avoir importé et utilisé avec
succès dans Dante Controller des XML modifiés par Dante Config Editor V3.6.
Cette confirmation remplace l'ancienne mention générale « aucun test réel ».

La version exacte de Dante Controller, les opérations couvertes et les
caractéristiques anonymisées du preset n'ayant pas encore été consignées, les
lignes détaillées ci-dessous conservent leur statut individuel jusqu'à ce que
ces informations soient relevées. Aucune donnée de production ne doit être
ajoutée au dépôt pour compléter cette preuve.

La branche 2026.1 conserve ce socle et ajoute un corpus synthétique plus large.
Ses sorties n'ont pas encore fait l'objet d'un import Dante Controller consigné
pour ce commit précis. La checklist est disponible dans
`docs/2026.1/DANTE_CONTROLLER_MANUAL_VALIDATION.md`.

## Matrice actuelle

| Cas / fichier | Version Dante Controller | Structure XML | Fabricant | Modèle | Devices | TX / RX | Namespace | Interfaces réseau | Structure des subscriptions | Complet / partiel | Ouverture | Sauvegarde sans modification | Modifications testées | Résultat d'import | Anomalies | Date / testeur |
|---|---|---|---|---|---:|---:|---|---|---|---|---|---|---|---|---|---|
| `representative-preset.xml` | Inconnue - fixture synthétique | `<preset version="3.0.0">`, devices directs | Test Manufacturer | Test TX / RX / IO | 3 | 3 / 4 | Aucun | `network=0`, IPv4 dynamique et fixe | `subscribed_device`, `subscribed_channel`, source locale `.` et source externe | Fixture représentative | Réussi - automatisé et application installée | Réussi - pack automatisé | renommage device/TX, patchs simples et visuels, profils, IP, suppression, récupération, garde-fou, pack de 8 scénarios | **Non testé** | mélange 48/96 kHz, 24/32 bit, modes réseau, IP fixe volontaire | 2026-07-11 / tests et contrôle installé Codex |
| `merge-preset.xml` | Inconnue - fixture synthétique | `<preset version="3.0.0">`, devices directs | Test | Duplicate / Imported | 2 | 1 / 1 | Aucun | IPv4 dynamique `network=0` | abonnement externe simple | Fixture partielle d'import | Réussi via scénario de fusion | Non testé isolément | fusion, doublon, renommage importé, conservation des patchs | **Non testé** | DEVICE-A est volontairement en doublon avec la fixture principale | 2026-07-11 / tests automatisés Codex |
| preset synthétique 10 devices | Sans objet | généré en mémoire, 64 TX + 64 RX par device | Synthetic | Synthetic | 10 | 640 / 640 | Aucun | interface principale synthétique | alias principal de subscription | Complet pour charge | Réussi - automatisé | Réussi - automatisé | édition groupée, garde-fou, SaveAs, rechargement | **Non testé** | aucun matériel réel | 2026-07-25 / tests automatisés Codex |
| preset synthétique 50 devices | Sans objet | généré en mémoire, 64 TX + 64 RX par device | Synthetic | Synthetic | 50 | 3 200 / 3 200 | Aucun | interface principale synthétique | alias principal de subscription | Complet pour charge | Réussi - automatisé | Réussi - automatisé | édition groupée, garde-fou, SaveAs, rechargement | **Non testé** | aucun matériel réel | 2026-07-25 / tests automatisés Codex |
| preset synthétique 200 devices | Sans objet | généré en mémoire, 64 TX + 64 RX par device | Synthetic | Synthetic | 200 | 12 800 / 12 800 | Aucun | interface principale synthétique | alias principal de subscription | Complet pour charge | Réussi - automatisé | Réussi - automatisé | édition groupée, garde-fou, SaveAs, rechargement | **Non testé** | aucun matériel réel | 2026-07-25 / tests automatisés Codex |
| preset avec namespace par défaut | Sans objet | `<preset xmlns="urn:test:dante:preset">` | Synthetic | Synthetic | 1 minimum | variable | `urn:test:dante:preset` | principale synthétique | éléments enfants dans le même namespace | Fixture ciblée | Réussi - automatisé | Réussi - automatisé | renommage, duplication générique, sauvegarde et rechargement sans élément hors namespace | **Non testé** | aucun matériel réel | 2026-07-25 / tests automatisés Codex |
| `official-preset-creator-custom.xml` | Structure issue d'un exemple Preset Creator | `<preset version="3.0.0">`, custom role sans identité matérielle | Custom | Custom Device | 1 | fixture ciblée | Aucun | aucune identité réseau physique | rôle sans abonnement actif | Fixture de structure | Réussi - automatisé | Réussi - automatisé | chargement, analyse de rôle, duplication sans `instance_id/device_id` | **Non testé** | ne prouve pas qu'un projet créé par DCE est accepté | 2026-07-25 / tests automatisés Codex |
| `compat-partial-one-way.xml` | Sans objet - fixture synthétique | preset partiel | Synthetic | TX only / RX only | 2 | 1 / 1 | Aucun | absentes | aucun abonnement actif | Partiel | Réussi - automatisé | Réussi - comparaison sémantique | ouverture, validation, sauvegarde, réouverture | **Non testé** | absence volontaire des champs optionnels | 2026-07-27 / tests automatisés Codex |
| `compat-subscription-edges.xml` | Sans objet - fixture synthétique | preset partiel | Synthetic | Subscription edges | 3 | 1 / 3 | Aucun | absentes | source locale `.`, device absent, canal absent | Partiel | Réussi - automatisé | Réussi - comparaison sémantique | conservation des trois structures de subscription | **Non testé** | avertissements attendus, non bloquants | 2026-07-27 / tests automatisés Codex |
| `compat-namespace-unknown.xml` | Sans objet - fixture synthétique | preset complet ciblé | Synthetic | Namespace IO | 1 | 1 / 1 | `urn:dce:test:preset` + extension constructeur | interfaces `network=0` et `network=1` | source locale `.`, ordre de balises inversé | Ciblé | Réussi - automatisé | Réussi - comparaison sémantique | namespace, Unicode, balise inconnue, interface secondaire | **Non testé** | extension `vendor:opaque` volontaire | 2026-07-27 / tests automatisés Codex |
| `compat-mixed-network-audio.xml` | Sans objet - fixture synthétique | preset complet ciblé | Synthetic | Mixed IO | 2 | 2 / 2 | Aucun | dynamique, fixe, interface secondaire | abonnement externe simple | Ciblé | Réussi - automatisé | Réussi - comparaison sémantique | 48/96 kHz, 24/32 bit, redondant/daisychain, IPv4 | **Non testé** | avertissements mixtes attendus | 2026-07-27 / tests automatisés Codex |
| duplication V3.6 | À relever | rôle générique cloné depuis un device existant | Hérité de la source | Hérité de la source | 1 ajouté | mêmes structures TX/RX | Conservé | retirées par défaut | retirées par défaut ; locales réécrites si conservation explicite | Scénario ciblé | Réussi - automatisé | Réussi - automatisé | identités retirées, labels/options, unicité du nom, annulation, garde-fou | **Non testé** | nécessite association manuelle au matériel lors de l'application du preset | 2026-07-25 / tests automatisés Codex |
| insertion depuis banque V3.6 | À relever | fragment `<device>` assaini, format de banque 1 | Métadonnée du modèle | Métadonnée du modèle | 1 ajouté | comptages vérifiés | Adapté au projet | retirées par défense en profondeur | retirées par défaut | Scénario ciblé | Réussi - automatisé | Réussi - automatisé | empreinte, version, namespace, copie indépendante, rollback | **Non testé** | versions de preset différentes bloquées | 2026-07-25 / tests automatisés Codex |
| nouveau projet V3.6 | À relever | `<preset version="3.0.0">` minimal, vide ou avec un rôle générique | Variable | Variable | 0 ou 1 | variable | Aucun ou celui du modèle | aucune par défaut | aucune par défaut | Expérimental | Réussi - automatisé dans DCE | Réussi - automatisé dans DCE | création atomique, relecture, validation, refus d'écrasement | **Non testé** | import Dante Controller obligatoire avant toute utilisation | 2026-07-25 / tests automatisés Codex |
| corpus local non versionné | À relever fichier par fichier | exports réels hors dépôt, versions 2.1.0 et 3.0.0 | À relever | À relever | 176 au total | 5 004 labels au total | avec et sans namespace selon cas | variable | `subscribed_device` / `subscribed_channel` | variable | 10 XML chargés depuis des copies temporaires | 10/10 sauvegardés, comparés sémantiquement et relus | aucune modification métier ; cycle sans perte significative détectée | **Non testé** | détails volontairement absents du dépôt public, originaux inchangés | 2026-07-25 / contrôle local automatisé |

## Ligne à dupliquer pour chaque validation réelle

| Cas / fichier anonymisé | Version Dante Controller | Structure XML | Fabricant | Modèle | Devices | TX / RX | Namespace | Interfaces réseau | Structure des subscriptions | Complet / partiel | Ouverture | Sauvegarde sans modification | Modifications testées | Résultat d'import | Anomalies | Date / testeur |
|---|---|---|---|---|---:|---:|---|---|---|---|---|---|---|---|---|---|
| `ID_INTERNE_SANS_DONNEE_SENSIBLE` | `x.y.z` | à décrire | à renseigner | à renseigner | 0 | 0 / 0 | aucun / URI | primaire, secondaire, modes IP | noms exacts des balises/attributs | complet / partiel | réussi / échec | réussi / échec | liste précise | réussi / échec / non testé | observations et logs | AAAA-MM-JJ / nom |

## Preuves à conserver hors du dépôt public

Pour chaque import manuel :

1. conserver le XML original et le XML généré dans un espace de test non public ;
2. noter la version exacte de Dante Controller et le système d'exploitation ;
3. conserver une capture ou un journal de l'import ;
4. contrôler les devices, Dante Id, mediaType, patchs, formats audio, preferred masters et interfaces ;
5. reporter uniquement des informations anonymisées dans cette matrice ;
6. ne jamais committer le XML de production.
