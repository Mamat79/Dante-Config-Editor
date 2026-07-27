# Notes techniques 2026.1 Beta

Version préparée : `2026.1.0-beta.1`

Ce document complète les notes utilisateur
[`RELEASE_NOTES.md`](../../RELEASE_NOTES.md).

## Périmètre

- base : V3.6 stabilisée au commit `25a1e7c` ;
- branche : `2026.1` ;
- aucune fusion automatique dans `main` ;
- aucune Release GitHub automatique ;
- installation côte à côte avec la V3.6.

## Changements structurants

- couches Domain, DanteXml, Application et Infrastructure ;
- commandes métier transactionnelles et session centrale ;
- paquet `.dceproj` sécurisé ;
- profil XML et capacités ;
- migration locale et banque format 2 ;
- shell Windows, Patch unifié, synoptique et Centre de validation ;
- indexation des machines/canaux et cache de validation.

## Compatibilité

La V3.6 a été testée avec succès dans Dante Controller par le mainteneur. Les
tests 2026.1 vérifient la conservation sémantique du corpus synthétique et le
garde-fou XML.

**Validation manuelle Dante Controller requise** pour la sortie 2026.1 avant
promotion en version stable.

## Distribution

L’installateur Windows et les deux DMG utilisent une identité distincte. Le
profil V3.6 n’est ni modifié ni supprimé. Les données utilisateur et banques
ne sont jamais supprimées par la désinstallation.
