# DCE 2027.1.1 - reçu de validation / validation receipt

Qualification réalisée le 19 septembre 2026. Cette version commune Windows et
macOS adapte automatiquement l'interface aux écrans compacts et clarifie les
parcours Projet, Patch et Outils sans modifier les formats ni les licences.

## Artefacts vérifiés

| Plateforme | Fichier | Taille | SHA-256 |
| --- | --- | ---: | --- |
| Windows x64 | `DanteConfigEditor2027_Installer.exe` | 85 551 996 | `c9ab0c793dc5514510fbc9cab7928097d911764dfd99af2a6222045fef749bd0` |
| macOS Apple Silicon | `DanteConfigEditor2027_macOS_AppleSilicon.dmg` | 79 103 253 | `418904fff1f5074d739c5b9de7d9e82d61848b9091509e2faa5ead05617a2d54` |
| macOS Intel | `DanteConfigEditor2027_macOS_Intel.dmg` | 80 407 200 | `96d123e73830711e102249746e08ae443bd948ffe780595fcae74fc79649060e` |

- Source privée : commit `cd914a30818deecb6dde5defce4fb93b7c7c4bf6`.
- Le dépôt public contient uniquement les artefacts compilés, notices, médias,
  banques et contrôles de téléchargement. Aucun code source n'y est publié.

## Contrôles exécutés

| Contrôle | Résultat |
| --- | --- |
| Suite Windows et moteur partagé | 694 tests réussis |
| Suite interface macOS/Avalonia | 35 tests réussis |
| Worker de licence | 5 tests réussis |
| Compilation Release Windows | 0 erreur, 0 avertissement |
| Interface compacte Windows | Captures 1024 x 640 FR/EN : Projet, Vue d'ensemble, Machines, Easy Patch, Outils et assistant vérifiés |
| Notices | 46 pages Windows et 23 pages macOS par langue, rendues et contrôlées ; démarrages rapides sur une page |
| Installation locale Windows | version 2027.1.1 enregistrée, exécutable 2027.1.1.0 lancé, un seul raccourci Bureau |
| Compatibilité | DCEP1, DCEF1, licences V2, clés publiques et stockage stable préservés |
| macOS Apple Silicon | Codemagic : tests, compilation arm64, smoke test et DMG terminés avant publication de l'asset |
| macOS Intel | Codemagic : compilation x64 et architecture vérifiée ; DMG et SHA-256 publiés |

Les deux DMG ont été fabriqués par Codemagic à partir du tag source
`v2027.1.1`, puis retéléchargés depuis la release. Chaque fichier correspond à
son fichier SHA-256 publié.

## Limites honnêtes

Les DMG ne sont pas signés Developer ID ni notarisés par Apple. Le contrôle
Intel est un contrôle de compilation et d'architecture, pas un essai sur un Mac
Intel physique. Aucune recette visuelle sur un Mac physique n'est revendiquée.

DCE reste un éditeur hors ligne. Les tests ne remplacent pas la vérification
finale dans Dante Controller et sur le matériel cible.
