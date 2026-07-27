# ADR 0001 - Séparation progressive des couches

Statut : accepté  
Date : 27 juillet 2026

## Contexte

La V3.6 possède un moteur XML fiable mais encore concentré dans
`DanteProject`, tandis que Windows recompilait auparavant les mêmes sources que
le Core macOS. Une réécriture immédiate augmenterait fortement le risque de
modifier les XML produits.

## Décision

Windows référence désormais le même assemblage Core que macOS. Les nouvelles
couches Domain, DanteXml, Application et Infrastructure sont ajoutées autour du
moteur existant. Les responsabilités sont ensuite extraites une par une avec
tests sémantiques avant/après.

## Conséquences

- une seule implémentation métier est compilée pour les deux plateformes ;
- le Core historique reste temporairement plus large que la cible ;
- les nouvelles API ne doivent pas exposer LINQ to XML à l'interface ;
- chaque suppression d'une responsabilité du Core exige un test de
  non-régression XML.
