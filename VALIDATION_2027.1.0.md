# DCE 2027.1.0 - recu de validation / validation receipt

Qualification realisee le 14 septembre 2026. Cette version commune Windows
et macOS ameliore les ecrans compacts, le demarrage d'un projet vide, les
propositions de noms Yamaha et le parcours de licence sans modifier les formats
ou licences existants.

## Artefacts

| Plateforme | Fichier | Taille | SHA-256 |
| --- | --- | ---: | --- |
| Windows x64 | `DanteConfigEditor2027_Installer.exe` | 85 547 760 | `b3aec3c129f2f1335d8ddd7acc2fd06cea7c479f4dbd54d2a3315e4663f1c958` |
| macOS Apple Silicon | `DanteConfigEditor2027_macOS_AppleSilicon.dmg` | 79 097 047 | `7e562560abadd84697b81953fcdf65d901531b3d7a9e2d1b6c5b63205562b80c` |
| macOS Intel | `DanteConfigEditor2027_macOS_Intel.dmg` | 80 413 095 | `ece45a7524d15950d4a2982861b4c3e41d8d76fd18ef55fe997b799453bbe67b` |

- Source privee des executables : commit produit
  `a189a6a1da9a3062a51e1577f404c42ec3223ae4`, tag `v2027.1.0`.
- Distribution Windows et documentation initiale : commit public `d53db3a`.
- Fabrication Codemagic `6aa86917688764aa17c34577` terminee avec succes.
  Les deux DMG telecharges depuis la release ont ete verifies contre leurs
  fichiers SHA-256 publies.
- Le depot public contient uniquement les artefacts compiles, notices, medias,
  banques et controles de telechargement. Aucun code source n'y est publie.

## Controles executes

| Controle | Resultat |
| --- | --- |
| Suite Windows et moteur partage | 686 tests reussis |
| Suite interface macOS/Avalonia | 35 tests reussis |
| Worker de licence | 5 tests reussis |
| Compilation Release Windows et macOS | 0 erreur, 0 avertissement |
| Ecrans compacts Windows | Machines, Patch, Easy Patch, Synoptique et inspecteur verifies en resolution compacte |
| Nouveau projet | configuration vide immediate, banque accessible et sauvegarde differee |
| Nommage Yamaha | propositions Y001/Y002 sans collision et sans renommage des imports |
| Licence | activation libere immediatement l'interface ; redemarrage hors ligne V1/V2 valide |
| Compatibilite | DCEP1, DCEF1, licences V2, cles publiques et stockage stable preserves |
| Installation locale Windows | une seule version 2027.1.0 enregistree, lancement reussi, etat de licence inchange |
| macOS Apple Silicon | compilation, tests et lancement non visuel natif sur runner M2 reussis |
| macOS Intel | compilation croisee et architecture x64 verifiees |
| Reprise Codemagic | paire DMG + SHA existante verifiee et reutilisee ; paire incomplete reparee sans ecrasement aveugle |

## Limites honnetes

Les DMG ne sont pas signes Developer ID ni notarises par Apple. Le controle
Intel est un controle de compilation et d'architecture, pas un essai sur un Mac
Intel physique. Cette livraison n'a pas fait l'objet d'une nouvelle recette
visuelle macOS.

DCE reste un editeur hors ligne et ne revendique pas le pilotage en temps reel
d'un reseau Dante. Les tests ne remplacent pas la verification finale dans
Dante Controller et sur le materiel cible.
