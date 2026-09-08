# DCE 2027.0.5 - reçu de validation / validation receipt

Qualification terminée le 8 septembre 2026. Cette publication commune Windows
et macOS clarifie les projets XML DCE, préserve le synoptique dans StageFlow et
supprime les alertes sans action utile avant export.

## Artefacts

| Plateforme | Fichier | Taille | SHA-256 |
| --- | --- | ---: | --- |
| Windows x64 | `DanteConfigEditor2027_Installer.exe` | 85 623 009 | `7b2740305344d46852a5a63e05a3a9dff842f2005feaa8b7475cad5ff50fb062` |
| macOS Apple Silicon | `DanteConfigEditor2027_macOS_AppleSilicon.dmg` | 79 126 529 | `ca6b6305ff2f19b3e3da3f8e7763081c42c46b634a20df47a7fe22fb25d8a001` |
| macOS Intel | `DanteConfigEditor2027_macOS_Intel.dmg` | 80 409 023 | `34559fdb1b25a0f2167a55aa024e1673d2024d85868205ecf2c65f89233b790d` |

- Source privée : commit `bd7ac1867c11483ca35ff4d1b6a0bf1e954e8eee`, tag `v2027.0.5`.
- Le dépôt public contient uniquement les artefacts compilés, notices, médias,
  banques et contrôles de téléchargement. Aucun code source n'y est publié.
- L'installateur Windows est autonome et embarque .NET.
- Les DMG ne sont pas signés Developer ID ni notariés par Apple.

## Contrôles exécutés

| Contrôle | Résultat |
| --- | --- |
| Suite Windows et moteur XML partagé | 650 tests réussis |
| Suite interface macOS/Avalonia | 34 tests réussis |
| Contrats de version et packaging macOS | 13 tests réussis |
| Génération et contrôle des notices | 7 tests réussis |
| Compilation Release Windows | 0 erreur, 0 avertissement |
| Compilation Release macOS | 0 erreur, 0 avertissement |
| Probes WPF Atomic Bomb et synoptique | FR/EN, clair/sombre : 4 scénarios réussis |
| Codemagic Apple Silicon | tests, packaging, smoke natif et SHA-256 réussis |
| Codemagic Intel | packaging, architecture x86_64 et SHA-256 réussis |
| Installation locale Windows | version 2027.0.5 enregistrée, lancement réel réussi |

Le cycle StageFlow testé couvre l'enregistrement et la réouverture des
emplacements, de l'ordre, de la visibilité et des positions manuelles du
synoptique, sans modification du XML Dante exporté. Les domaines étrangers du
projet StageFlow restent hors de la responsabilité de DCE.

## Limites honnêtes

Les XML produits par DCE ont déjà été importés avec succès dans Dante Controller
par le mainteneur. Cette mise à jour n'a pas fait l'objet d'un nouvel essai sur
un réseau Dante physique. Les contrôles automatisés ne simulent ni firmware,
ni horloge réelle, ni câblage, ni comportement matériel. Le DMG Intel a été
vérifié architecturalement sur le runner Apple Silicon, sans smoke natif Intel.
