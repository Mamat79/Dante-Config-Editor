# Notes techniques 2026.1 Beta

Version préparée : `2026.1.0-beta.1`

Ce document complète les notes utilisateur
[`RELEASE_NOTES.md`](../../RELEASE_NOTES.md).

## Périmètre

- base : V3.6 stabilisée au commit `25a1e7c` ;
- branche publiée : `main` ;
- ancienne branche de travail `2026.1` supprimable après validation de `main` ;
- Release bêta conservée sous le tag `v2026.1` ;
- installation côte à côte avec la V3.6.

## Changements structurants

- couches Domain, DanteXml, Application et Infrastructure ;
- commandes métier transactionnelles et session centrale ;
- paquet `.dceproj` sécurisé ;
- profil XML et capacités ;
- migration locale et banque format 2 ;
- shell Windows, Patch unifié, synoptique et Centre de validation ;
- indexation des machines/canaux et cache de validation.
- vue globale dédupliquée des banques personnelle et fournies ;
- inspecteur synchronisé avec la dernière machine parcourue dans Machines,
  Patch et Easy Patch.

## Compatibilité

La 2026.1 a été testée avec succès dans Dante Controller par le mainteneur. Les
tests automatisés vérifient en complément la conservation sémantique du corpus
synthétique et le garde-fou XML. Un contrôle reste recommandé pour chaque
nouvelle structure de preset avant exploitation.

## Distribution

L’installateur Windows et les deux DMG utilisent une identité distincte. Le
profil V3.6 n’est ni modifié ni supprimé. Les données utilisateur et banques
ne sont jamais supprimées par la désinstallation. Les 43 modèles fournis sont
également embarqués dans les installateurs et restent séparés de la banque
personnelle.
