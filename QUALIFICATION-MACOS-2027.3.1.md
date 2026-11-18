# Dante Config Editor 2027.3.1 · qualification macOS

Source privée 964f2610ca0a1c3b6bd8e0560869c4bb23f9941a, job Codemagic 6afcedf74b6487a1a8ac467c du 18 novembre 2026.

- 1 071 tests du moteur, 93 tests d’interface et 25 contrôles de packaging ont réussi.
- Les libellés français et anglais à 79 € TTC, le lien vers la boutique centrale et les mentions StageFlow optionnel ont été contrôlés dans les DLL livrées. Les guides partagés sont ceux de l’édition approuvée 2027.7.
- Sur Apple Silicon, le paquet a passé les contrôles du Trousseau en trois processus, du lancement et de la fermeture natifs, de l’ouverture XML et StageFlow, ainsi que du moteur réseau et de WebKit en simulation.
- Le paquet Intel a été vérifié pour son architecture x86_64 et son contenu. Son exécution native reste à vérifier.
- Les essais de réseau portent sur des appareils fictifs. Le matériel Dante physique et la présentation visuelle native complète restent à qualifier.
- macOS 15 minimum, signature ad hoc sans notarisation.

| Paquet | Taille en octets | SHA-256 |
| --- | ---: | --- |
| Apple Silicon | 142410099 | b6976475fd840954b4d818061f4be91adfb6a65755a9b3fdd9456475b0dac8ac |
| Intel | 151836523 | 68259b6aaa0b694d6019f13cf828696ecd4a7774b37d2ccc472869a2177ce603 |
