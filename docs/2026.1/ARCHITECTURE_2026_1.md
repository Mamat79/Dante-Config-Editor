# Architecture Dante Config Editor 2026.1

## Objectif

La version 2026.1 sépare progressivement l'interface, les cas d'usage et le
moteur XML sans réécrire brutalement la V3.6. Le document XML original reste la
référence de compatibilité. Les extractions doivent conserver les mutations
ciblées, le garde-fou et la sauvegarde atomique déjà éprouvés.

## Couches

| Projet | Responsabilité | Dépendances autorisées |
|---|---|---|
| `DanteConfigEditor.Domain` | contrats métier sans XML, fichier ni interface | bibliothèque standard uniquement |
| `DanteConfigEditor.Core` | moteur V3.6 historique partagé pendant la migration | aucun framework UI |
| `DanteConfigEditor.DanteXml` | détection de profil, capacités et accès contrôlé au moteur XML | Domain, Core |
| `DanteConfigEditor.Application` | session, commandes, transactions, historique | Domain, DanteXml, Core pendant la transition |
| `DanteConfigEditor.Infrastructure` | paquets `.dceproj`, fichiers, migration et récupération | Domain, Application, DanteXml |
| application Windows WPF | vues, navigation, ViewModels et composition | Application, Infrastructure |
| application macOS Avalonia | interface existante maintenue, sans refonte 2026.1 | Core et contrats partagés utiles |

`DanteConfigEditor.Core` est un pont de migration. Il contient encore
`DanteProject`, des modèles liés à `XElement` et plusieurs services de fichiers.
Il ne représente pas l'architecture cible finale, mais permet de déplacer une
responsabilité à la fois avec comparaison XML avant/après.

## Règles de dépendance

### Autorisé

- Domain ne dépend d'aucun autre projet DCE.
- DanteXml peut manipuler LINQ to XML et le moteur V3.6.
- Application utilise des contrats métier et appelle le moteur par des
  adaptateurs ou commandes explicites.
- Infrastructure effectue les accès fichiers pour le compte de l'application.
- Windows compose les services et affiche les états traduits.

### Interdit

- Domain ne référence ni LINQ to XML, ni WPF, ni Avalonia, ni accès fichier.
- Une vue ou un code-behind ne manipule pas `XDocument`, `XElement`,
  `XAttribute` ou `XNamespace`.
- Core ne référence pas WPF ou Avalonia.
- Une commande ne modifie pas un fichier directement.
- Infrastructure ne déclenche pas une boîte de dialogue.
- Une capacité absente ne doit pas être contournée par l'interface.

Ces règles sont vérifiées progressivement par
`ArchitectureBoundaryTests`.

## Flux d'ouverture XML

1. L'infrastructure vérifie le chemin et ouvre une copie de travail.
2. `DanteXmlProjectAdapter` charge le projet avec le moteur V3.6.
3. `DanteXmlProfileDetector` inspecte la structure sans lui attribuer une
   version officielle inventée.
4. Le détecteur produit un `DanteXmlProfileDescriptor`.
5. La session expose le modèle, le profil et ses capacités aux vues.
6. L'interface désactive les actions non autorisées et explique la raison.

Les profils initiaux sont :

- `recognized-complete` : structure reconnue et édition complète disponible ;
- `recognized-partial` : structure lisible, édition restreinte ;
- `unknown-read-only` : structure non reconnue, aucune écriture.

Les balises inconnues n'entraînent pas automatiquement une perte de capacité :
elles restent conservées et protégées par le garde-fou. Une structure
fondamentale inconnue entraîne en revanche le mode lecture seule.

## Flux d'une commande

1. La vue crée une commande métier avec des identifiants stables.
2. Le dispatcher valide la commande contre la session et les capacités.
3. La commande fournit son aperçu, les avertissements et les éléments touchés.
4. Le dispatcher capture l'état nécessaire à l'annulation.
5. La commande est exécutée comme une transaction unique.
6. Le moteur reconstruit au maximum une fois les index nécessaires.
7. La validation est actualisée.
8. Une entrée lisible est ajoutée à l'historique.
9. Les vues reçoivent une notification de modèle modifié.

Les opérations de masse doivent former une seule commande et une seule entrée
Annuler/Rétablir.

## Flux de validation

La validation combine :

1. le profil et ses capacités ;
2. l'intégrité du modèle reconnu ;
3. les identités et références ;
4. le garde-fou comparant le document source au document courant ;
5. la validation du paquet DCE lorsqu'un `.dceproj` est utilisé.

Les résultats sont convertis en problèmes de domaine contenant niveau,
catégorie, cible, détail technique et éventuel chemin XML. Le Centre de
validation distingue toujours validation interne DCE et validation manuelle
Dante Controller.

## Flux de sauvegarde XML

1. La session refuse l'action si le profil n'autorise pas la sauvegarde.
2. Le moteur valide le document en mémoire.
3. Il écrit un fichier temporaire dans le dossier cible.
4. Il relit et compare sémantiquement ce fichier.
5. Il exécute l'intégrité et le garde-fou.
6. Il sauvegarde la destination existante.
7. Il effectue un remplacement atomique.
8. La session met à jour son chemin et son état uniquement après réussite.

Le fichier original n'est jamais remplacé implicitement lors de la première
ouverture.

## Annulation et historique

La pile Annuler/Rétablir appartient à la session courante et reste bornée.
L'historique lisible peut être persisté dans un projet DCE. Une entrée
historique persistée décrit une opération passée mais ne promet pas qu'elle
reste annulable après réouverture.

Pendant la migration, certaines commandes complexes peuvent encore utiliser un
snapshot complet du moteur V3.6. Les commandes simples doivent évoluer vers des
deltas ciblés afin de réduire les allocations.

## Format `.dceproj`

Le paquet DCE est un conteneur ZIP de données, jamais un exécutable. Il contient
le XML Dante et les informations propres à DCE dans des entrées séparées. La
mise en page, les annotations, les filtres et les images ne sont jamais écrits
dans le XML Dante.

L'infrastructure doit vérifier les chemins internes, tailles, versions et
manifestes avant de charger une entrée. L'écriture suit la même stratégie
temporaire, validation, backup et remplacement atomique que le XML.

Le format détaillé est défini dans `DCEPROJECT_FORMAT.md`.

## Banque de machines

La banque V3.6 reste lisible. Son dépôt est une infrastructure distincte de la
session de projet. L'insertion crée une copie indépendante et passe par une
commande transactionnelle. Une modification ultérieure du modèle de banque ne
modifie jamais une machine déjà insérée.

La migration 2026.1 lit l'ancien format, sauvegarde l'original puis écrit une
copie migrée. Aucun répertoire personnel n'est remplacé par une banque fournie
avec l'installateur.

## État de migration

Réalisé :

- Windows et macOS consomment le même assemblage Core ;
- aucun code-behind desktop ne manipule LINQ to XML ;
- couche Domain indépendante ;
- couche DanteXml et profils de capacités ;
- session de projet centrale ;
- dispatcher de commandes transactionnelles ;
- Annuler, Rétablir et historique borné ;
- commandes typées pour renommages, plages, patch, suppression et format audio ;
- paquet `.dceproj` 1.0 avec manifeste, espace de travail, journal, validation
  et images ;
- contrôle des chemins internes, tailles et empreintes du paquet ;
- sauvegarde atomique du projet DCE avec backup de la destination ;
- profil local 2026.1 séparé et migration V3.6 non destructive, sauvegardée,
  vérifiée et idempotente ;
- tests d'architecture et de profils.

À poursuivre :

- branchement progressif de toutes les vues sur la session ;
- remplacement progressif des snapshots par des deltas ciblés ;
- récupération des projets DCE ;
- branchement progressif des vues Windows sur la session ;
- moteur Patch unique ;
- Centre de validation et navigation par cible ;
- réduction progressive du Core historique.
