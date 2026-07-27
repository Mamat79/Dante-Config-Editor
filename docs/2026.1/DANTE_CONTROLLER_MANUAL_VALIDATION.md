# Validation manuelle Dante Controller pour 2026.1

## Niveau de preuve

Le mainteneur a confirmé le 27 juillet 2026 que des XML produits par la V3.6
ont été importés et utilisés avec succès dans Dante Controller. La version
exacte de Dante Controller et le détail anonymisé des scénarios n'ont pas
encore été consignés.

Les tests automatisés 2026.1 prouvent la conservation structurelle et
sémantique des fixtures, mais ne remplacent pas un nouvel import humain de la
sortie 2026.1. Ne marquer une ligne comme validée dans Dante Controller qu'après
un import réel.

Un fichier `.dceproj` est un conteneur de travail propre à DCE. Dante
Controller doit recevoir le XML exporté depuis ce projet, jamais le
`.dceproj`.

## Identification

- commit DCE :
- version affichée :
- fichier de référence anonymisé :
- SHA-256 original :
- SHA-256 export 2026.1 :
- version Dante Controller :
- système :
- date :
- testeur :
- preuve conservée hors dépôt public :

## Préparation

- [ ] Travailler sur une copie, jamais sur l'original.
- [ ] Vérifier les SHA-256 avant et après le scénario.
- [ ] Lire le Centre de validation et exporter son rapport.
- [ ] Conserver le rapport avant/après.
- [ ] Utiliser un environnement de test hors réseau Dante de production.
- [ ] Ne publier aucun XML de production dans Git.

## Cycle sans modification

- [ ] Ouvrir le XML dans DCE 2026.1.
- [ ] Ne réaliser aucune mutation.
- [ ] Enregistrer sous un nouveau nom.
- [ ] Comparer sémantiquement l'original et la sortie.
- [ ] Réouvrir la sortie dans DCE.
- [ ] Importer la sortie dans Dante Controller.
- [ ] Vérifier que Dante Controller ne signale aucune perte inattendue.

## Structure et identité

- [ ] Le nombre de machines est identique au rapport.
- [ ] Les noms et friendly names attendus sont présents.
- [ ] Les `instance_id`, `device_id` et `process_id` non concernés sont
  inchangés.
- [ ] Les `Dante Id` et `mediaType` non concernés sont inchangés.
- [ ] Le namespace est conservé.
- [ ] Les balises et attributs inconnus sont conservés.
- [ ] Un ordre différent de balises n'est pas traité comme une perte.

## Renommage et patch

- [ ] Renommer une machine et contrôler toutes les références.
- [ ] Renommer un TX et contrôler tous les RX abonnés.
- [ ] Renommer un RX.
- [ ] Renommer une série TX et RX.
- [ ] Créer un patch simple.
- [ ] Créer un patch 1:1.
- [ ] Remplacer un patch existant.
- [ ] Supprimer un patch.
- [ ] Vérifier le marqueur local `subscribed_device="."`.
- [ ] Vérifier une source absente dans un preset partiel.
- [ ] Vérifier un canal TX absent dans un preset partiel.

## Audio, horloge et réseau

- [ ] Contrôler latence, sample rate et encodage de chaque machine modifiée.
- [ ] Contrôler le ou les Preferred Masters.
- [ ] Contrôler redondant et daisychain.
- [ ] Contrôler l'IPv4 principale.
- [ ] Vérifier que DNS et passerelle ne changent pas implicitement.
- [ ] Vérifier que l'interface secondaire reste strictement inchangée.

## Machines et banque

Tester chaque cas séparément :

- [ ] suppression d'une machine et de ses subscriptions associées ;
- [ ] duplication en rôle générique avec identité matérielle neutralisée ;
- [ ] ajout depuis une banque assainie et de version compatible ;
- [ ] fusion de deux XML avec résolution des noms en conflit ;
- [ ] export XML depuis un `.dceproj` contenant une disposition et des notes ;
- [ ] création expérimentale d'un nouveau projet, si cette capacité est
  activée par le profil XML.

Pour une duplication ou une insertion depuis la banque :

- [ ] aucune identité matérielle source n'est réutilisée ;
- [ ] aucune IP, subscription ou flow source n'est recopié par défaut ;
- [ ] le nombre de TX/RX, les Dante Id et les mediaType correspondent ;
- [ ] la machine source n'est pas modifiée.

## Résultat

- résultat : réussi / échec / partiel / interrompu ;
- avertissements Dante Controller :
- différences inattendues :
- captures ou journaux :
- décision :

Reporter uniquement le résultat anonymisé dans `COMPATIBILITY_MATRIX.md`.
