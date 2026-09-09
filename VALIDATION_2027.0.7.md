# DCE 2027.0.7 - reçu de validation / validation receipt

Qualification réalisée le 9 septembre 2026. Cette version commune Windows et
macOS corrige l'export XML depuis un projet StageFlow et étend Easy Patch au
glisser-déposer séquentiel bidirectionnel.

## Artefacts

| Plateforme | Fichier | Taille | SHA-256 |
| --- | --- | ---: | --- |
| Windows x64 | `DanteConfigEditor2027_Installer.exe` | 85 632 299 | `eb7caa3dc1c29dc0e7ab9c680a8f06b0853907112eef8088da1b565c8e99d9df` |
| macOS Apple Silicon | `DanteConfigEditor2027_macOS_AppleSilicon.dmg` | 79 103 164 | `832aaf39a77a1f55a291602298b1375f9062fefa5cbcc54e2c40174165b7b690` |
| macOS Intel | `DanteConfigEditor2027_macOS_Intel.dmg` | 79 458 101 | `76a0ec0a39aab5f744f2d14294231b795007e6151b2fa71179e612abb31c50a9` |

- Source privée des installateurs : commit `e07b46a0b31acfebb00eb919baaa28eb0ce0b3be`,
  tag de fabrication `v2027.0.7-macos-retry3`, incluant la correction portable
  des noms de fichiers. Le tag initial `v2027.0.7` précède cette correction.
- Fabrication macOS Codemagic `6aa15433688764aa17b0f1c4` terminée avec succès
  le 9 septembre 2026 à 12:50:55 UTC. Les deux DMG téléchargés ont été vérifiés
  contre les sommes SHA-256 publiées.
- Le dépôt public contient uniquement les artefacts compilés, notices, médias,
  banques et contrôles de téléchargement. Aucun code source n'y est publié.
- L'installateur Windows est autonome et embarque .NET.
- Les DMG ne sont pas signés Developer ID ni notariés par Apple.

## Contrôles exécutés

| Contrôle | Résultat |
| --- | --- |
| Suite Windows et moteur XML partagé | 660 tests réussis |
| Suite interface macOS/Avalonia | 34 tests réussis |
| Contrats de version et packaging macOS | 13 tests réussis |
| Worker de licence | 5 tests réussis |
| Compilation Release Windows | 0 erreur, 0 avertissement |
| Export XML depuis StageFlow | chemin virtuel du paquet neutralisé avant ouverture du dialogue Windows |
| Cycle XML sans modification | document original et document réenregistré identiques octet par octet |
| Scénarios XML modifiés | renommage machine/TX, patch, latence, preferred master et IP : 0 erreur bloquante |
| Références TX absentes d'un preset partiel | classées comme informations, sans masquer les vraies erreurs XML |
| Easy Patch | sélections multiples TX vers RX et RX vers TX planifiées séquentiellement |
| Installation locale Windows | version 2027.0.7 enregistrée et lancement réel réussi |
| Noms de fichiers portables | caractères Windows interdits et contrôles ASCII neutralisés sur chaque OS ; 3 tests supplémentaires |
| macOS Apple Silicon | compilation, tests et lancement non visuel natif sur le runner M2 réussis |
| macOS Intel | compilation croisée et architecture x64 vérifiées ; pas de lancement natif Intel |

## Limites honnêtes

Cette livraison n'a pas fait l'objet d'une nouvelle recette visuelle macOS.
Le contrôle Intel est un contrôle de compilation et d'architecture, pas un essai
sur un Mac Intel physique.

Les XML produits par DCE ont déjà été importés avec succès dans Dante Controller
par le mainteneur. Cette corrective n'a pas fait l'objet d'un nouvel essai sur
un réseau Dante physique. Les contrôles automatisés ne simulent ni firmware,
ni horloge réelle, ni câblage, ni comportement matériel. Les références vers un
TX absent restent utiles dans un preset partiel et sont donc signalées comme
informations ; elles doivent être résolues ou supprimées si le preset final ne
doit contenir aucune souscription en attente.
