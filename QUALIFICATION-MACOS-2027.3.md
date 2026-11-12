# Dante Config Editor 2027.3 — qualification macOS

DMG Apple Silicon et Intel produits depuis le commit source `2d6b2ceefd46c33cb1d783afffc74bc84e006038`. Version technique 2027.3.0. macOS 15 minimum déclaré.

- Le build natif Apple Silicon a réussi les 1 060 tests Core, les 92 tests d’interface et les 25 vérifications d’outillage.
- Le DMG Apple Silicon a été monté et l’application a été lancée dans des scénarios non visuels avec XML et projet StageFlow. L’écriture et la lecture du Trousseau entre processus, la reconnaissance hors ligne et la preuve de désactivation ont été vérifiées avec des échanges d’activation simulés.
- Le DMG Intel a été inspecté pour l’architecture x86_64 et le contenu. L’exécution et le Trousseau sur un Mac Intel restent à vérifier.
- Les deux images contiennent les notices et guides rapides en français et en anglais. Leurs fichiers correspondent aux quatre PDF macOS distribués.
- La signature du paquet est ad hoc. Les images ne sont pas notarisées. Cette version macOS propose l’édition XML hors ligne.

| Image | Taille | SHA-256 |
| --- | ---: | --- |
| Apple Silicon | 82 912 194 | `655be044c692bfdf8bdbec76da6b488957e18b1cc3e2f2271d3bf912c8d36e62` |
| Intel | 84 264 634 | `aa75831ba2f7d0a8be039590eb6f719df48b84fe8b84d723fba40dc91dfa9b23` |
