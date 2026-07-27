# ADR 0003 - Session unique, commandes et historique

Statut : accepté  
Date : 27 juillet 2026

## Contexte

La V3.6 possède une pile d'annulation à sens unique dans `DanteProject` et
plusieurs fenêtres conservent leur propre état. Une commande partiellement
échouée doit pourtant pouvoir restaurer l'ensemble du projet, y compris les
identités de session et les autorisations du garde-fou.

## Décision

`ProjectSession` devient la source de vérité des nouvelles vues. Toute nouvelle
mutation passe par `ProjectCommandDispatcher`.

Le dispatcher :

- valide et prévisualise la commande ;
- capture un état opaque avant l'exécution ;
- exécute une transaction unique ;
- restaure l'état complet en cas d'exception ;
- capture l'état après réussite ;
- alimente Annuler, Rétablir et l'historique lisible ;
- borne la pile d'annulation.

Le snapshot reste privé au Core et ne donne jamais accès au `XDocument` à
l'application. Les annotations d'identité de session sont explicitement
recopiées.

## Conséquences

- Annuler et Rétablir restaurent aussi les références de patch et autorisations
  de nouveaux rôles ;
- une opération de masse forme une seule transaction ;
- l'historique descriptif peut survivre plus longtemps que la pile annulable ;
- le coût mémoire reste celui d'un snapshot complet pendant cette étape ;
- les commandes simples pourront ensuite évoluer vers des deltas ciblés sans
  changer le contrat des vues.
