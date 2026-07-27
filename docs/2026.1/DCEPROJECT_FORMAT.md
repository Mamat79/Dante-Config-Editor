# Format de projet DCE 2026.1

## Rôle

Un fichier `.dceproj` conserve un espace de travail Dante Config Editor. Il ne
remplace pas le XML Dante et n'est pas destiné à Dante Controller. Le XML Dante
reste exportable séparément et continue à pouvoir être ouvert directement dans
DCE.

Le conteneur est un ZIP de données sans code exécutable. DCE ne l'extrait pas
dans un dossier temporaire lors de l'ouverture.

## Schéma 1.0

Entrées obligatoires :

| Entrée | Contenu |
|---|---|
| `manifest.json` | version, dates, application créatrice et empreintes |
| `dante/project.xml` | document XML Dante préservé |
| `workspace/metadata.json` | nom, description et extensions du projet DCE |
| `workspace/layout.json` | positions et ordre du synoptique |
| `workspace/annotations.json` | annotations et machines masquées |
| `workspace/history.json` | journal descriptif persistant |
| `workspace/settings.json` | page, filtres et dimensions de colonnes |

Entrées facultatives :

| Entrée | Contenu |
|---|---|
| `reports/last-validation.json` | dernier état de validation connu |
| `assets/**/*.png` | image intégrée au projet |
| `assets/**/*.jpg` ou `.jpeg` | image intégrée au projet |
| `assets/**/*.webp` | image intégrée au projet |

Le journal persistant décrit les opérations passées. Il ne restaure pas la pile
Annuler/Rétablir d'une ancienne session.

## Manifeste

Le manifeste contient au minimum :

- `schemaVersion` : actuellement `1.0` ;
- `createdWithVersion` : version de DCE ayant écrit le paquet ;
- `createdAt` et `modifiedAt` en UTC ;
- `projectName` et `description` ;
- `danteXmlEntry` : actuellement `dante/project.xml` ;
- `contentSha256` : empreinte SHA-256 de chaque autre entrée.

Les propriétés inconnues du manifeste sont conservées lors d'une ouverture puis
d'une nouvelle sauvegarde. Une version majeure inconnue est refusée. Une future
version mineure `1.x` pourra être lue si les entrées indispensables restent
compatibles.

## Sécurité

Avant de lire les données, DCE contrôle :

- la présence des entrées obligatoires ;
- l'absence de doublons ;
- les chemins absolus, `..`, `.`, antislashs et caractères NUL ;
- les empreintes de toutes les entrées autres que le manifeste ;
- la version du schéma ;
- le nom de l'entrée XML ;
- le format JSON ;
- la taille de chaque entrée et la taille totale ;
- les extensions autorisées pour les images.

Limites initiales :

- 200 entrées ;
- 100 Mio non compressés au total ;
- 25 Mio pour le XML ;
- 5 Mio par entrée JSON ;
- 10 Mio par image.

Ces limites protègent contre les archives pathologiques. Elles pourront devenir
configurables sans modifier le schéma.

## Écriture atomique

Une sauvegarde suit cet ordre :

1. validation du projet Dante en mémoire ;
2. création d'un paquet temporaire dans le dossier cible ;
3. réouverture et validation complète du paquet temporaire ;
4. création d'une sauvegarde si la destination existe ;
5. remplacement atomique de la destination ;
6. suppression du temporaire restant.

Une erreur avant le remplacement laisse la destination précédente intacte. La
sauvegarde de l'ancienne destination est placée dans
`DanteConfigEditor_Backups`.

## Fidélité XML

Le XML contenu est sérialisé depuis le document original modifié par mutations
ciblées. Les éléments, attributs, namespaces et valeurs inconnus ne sont pas
transférés dans un modèle simplifié du paquet. À la réouverture, le même
détecteur de profil et les mêmes restrictions de capacités que pour un XML
direct sont appliqués.

La compatibilité de la V3.6 avec Dante Controller a été testée avec succès par
le mainteneur. Toute évolution du moteur XML en 2026.1 doit néanmoins refaire
les tests automatisés et la validation manuelle Dante Controller avant sa
publication.

## Évolution

Une modification incompatible impose une nouvelle version majeure du schéma.
Une propriété ou entrée facultative compatible peut utiliser une version
mineure. Une migration doit toujours :

- lire sans modifier le fichier source ;
- produire une nouvelle copie ;
- conserver une sauvegarde ;
- fournir un rapport ;
- ne jamais masquer une donnée non comprise.

