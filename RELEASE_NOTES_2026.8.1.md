# Dante Config Editor 2026.8.1

[English release notes](RELEASE_NOTES_2026.8.1_EN.md)

## Correctif 2026.8.1

Lorsqu'un projet StageFlow ne contient pas encore de domaine Dante, DCE indique
maintenant le parcours exact : ouvrir le projet StageFlow, ouvrir le XML Dante,
puis utiliser **Enregistrer**. La commande **Enregistrer sous** reste réservée
à la création d'un nouveau projet et refuse volontairement un dossier
`.stageflow` existant.

Le libellé commun Windows/macOS et le flux réel sont couverts par un test de
contrat. Celui-ci vérifie aussi que l'ajout de `dante/dante.json` et du paquet
`.dceproj` conserve `patch`, `CAD` et `SMT` octet par octet. Le moteur XML, les
licences et le format StageFlow restent inchangés.

## Projet natif `.stageflow`

DCE utilise désormais le dossier `.stageflow` comme format de projet par
défaut. L'application sait le créer, l'ouvrir et l'enregistrer de façon
autonome. StageFlow Desktop reste gratuit et facultatif ; il n'est pas requis
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

- 495 tests Core/Windows réussis ;
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
