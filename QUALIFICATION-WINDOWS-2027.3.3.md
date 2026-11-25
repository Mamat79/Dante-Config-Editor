# DCE 2027.3.3 Windows — vérifications

- Source privée finale figée : 7660e30e4882eb2740f9f826ffa745a6d122131c.
- 1 079 tests du moteur et 93 tests de compatibilité d’interface partagée ont réussi.
- Le harnais WPF a passé 238 contrôles, notamment la fermeture sans déconnexion, le retour hors ligne, la fermeture de DCE et la préparation d’un remplacement portant un autre nom.
- Le remplacement vérifie les canaux disponibles et sélectionne les noms, patchs et abonnements dépendants pour relecture avant l’envoi.
- Les essais du navigateur simulé couvrent le démarrage replié, les sélections des préréglages, Tout cocher / Tout décocher, le dialogue XML et les informations réseau non renseignées.
- Cinq tests Python et les essais HTTP du compagnon simulé ont vérifié la disponibilité des observations et l’export de préréglages.
- La capture XML a passé export, relecture, conservation des absents et patchs préparés, récupération des flux multicast observés et annulation.
- Les notices Windows FR/EN ont été régénérées, leur navigation vérifiée et les pages modifiées rendues en images.

Les appareils nouvellement capturés sont des rôles génériques. Une identité matérielle non attestée n’est pas fabriquée. Dante Controller peut demander l’affectation des rôles au chargement. Une information non remontée par un appareil apparaît comme non renseignée. La capture de réglages manquants demande confirmation.

Le réseau Dante physique, les anciens appareils ne fournissant pas leur horloge et l’import du XML dans Dante Controller sur le matériel concerné restent à vérifier. Ces essais n’ont effectué aucune écriture sur le réseau de production.
