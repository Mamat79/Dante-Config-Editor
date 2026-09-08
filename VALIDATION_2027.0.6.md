# DCE 2027.0.6 - recu de validation / validation receipt

Qualification terminee le 8 septembre 2026. Cette publication commune Windows
et macOS retablit l'edition Redundant/Daisychain pour les profils XML qui
declarent cette capacite sans section IPv4, et ajoute l'affectation sequentielle
par glisser-deposer dans Easy Patch.

## Artefacts

| Plateforme | Fichier | Taille | SHA-256 |
| --- | --- | ---: | --- |
| Windows x64 | `DanteConfigEditor2027_Installer.exe` | 85 625 091 | `6f6ffa05c9ba450523d7e765a17a8c4bef6d16fb7ec666c70418beb396b1a513` |
| macOS Apple Silicon | `DanteConfigEditor2027_macOS_AppleSilicon.dmg` | 79 126 671 | `7120f38c7a75167d229fad96aa085da5d55a525f877926f5745dce64f4213187` |
| macOS Intel | `DanteConfigEditor2027_macOS_Intel.dmg` | 80 409 444 | `5e7c241106171cf9b0cd1bada03e29a4230de9e61293b28fd566db2f394a2353` |

- Source privee : commit `18502824e155b9b421214f982be5fedb345eb3e7`, tag `v2027.0.6`.
- Build Codemagic macOS : `6aa008fca21832a6ee3d3298`.
- Le depot public contient uniquement les artefacts compiles, notices, medias,
  banques et controles de telechargement. Aucun code source n'y est publie.
- L'installateur Windows est autonome et embarque .NET.
- Les DMG ne sont pas signes Developer ID ni notarises par Apple.

## Controles executes

| Controle | Resultat |
| --- | --- |
| Suite Windows et moteur XML partage | 651 tests reussis |
| Suite interface macOS/Avalonia | 34 tests reussis |
| Contrats de version et packaging macOS | 13 tests reussis |
| Generation et controle des notices | 7 tests reussis |
| Worker de licence | 5 tests reussis |
| Compilation Release Windows | 0 erreur, 0 avertissement |
| Profil XML Redundant sans IPv4 | reconnu comme editable sans mutation du document |
| Easy Patch par glisser-deposer | selection multiple, plan sequentiel et conflits couverts par tests |
| Codemagic Apple Silicon | tests, packaging, lancement natif et SHA-256 reussis |
| Codemagic Intel | packaging, architecture x86_64 et SHA-256 reussis |
| Telechargement public des DMG | tailles et SHA-256 reverifies apres publication |
| Installation locale Windows | version 2027.0.6 enregistree, lancement reel reussi |

Le controle XML a egalement ete execute sur le fichier de recuperation du projet
Nancy : sept machines exposent la redondance sans noeud IPv4 ; le profil est
maintenant reconnu comme editable et le document source n'est pas reconstruit.

## Limites honnetes

Les XML produits par DCE ont deja ete importes avec succes dans Dante Controller
par le mainteneur. Cette corrective n'a pas fait l'objet d'un nouvel essai sur
un reseau Dante physique. Les controles automatises ne simulent ni firmware,
ni horloge reelle, ni cablage, ni comportement materiel. Le DMG Intel a ete
verifie architecturalement sur le runner Apple Silicon, sans lancement natif
sur un Mac Intel.
