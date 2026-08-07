# Banques locales et catalogue GitHub

## État livré dans 2026.1.1

L'installateur Windows permet déjà de choisir séparément :

- le dossier de la banque active ;
- le dossier dans lequel copier les banques fournies ;
- l'installation de `DCE Generic Roles 2026.1` ;
- l'installation de `DCE Community Devices 2026.1`.

La banque communautaire 2026.1 contient 44 modèles illustrés et assainis. Les
descriptions françaises et anglaises sont affichées selon la langue active.
Deux rôles LM44 sont proposés séparément : `8 TX / 4 RX` et `0 TX / 4 RX`.

L'application conserve une copie de secours de chaque banque fournie dans son
dossier d'installation. Les copies officielles mises à jour sont enregistrées
dans `Documents/Dante Config Editor/Included Machine Banks` et sont prioritaires
sur ces copies de secours. La bibliothèque personnelle reste indépendante et
peut être placée dans un dossier local, partagé ou synchronisé.

Depuis DCE, l'utilisateur peut :

- afficher toutes les banques installées dans une liste globale dédupliquée ;
- isoler une banque et identifier l'origine de chaque modèle ;
- changer de banque active ;
- ouvrir son dossier ;
- exporter une banque en `.dce-bank.zip` ;
- importer une archive dans un dossier neuf ou vide ;
- vérifier et installer directement les mises à jour des banques officielles ;
- ouvrir le catalogue public GitHub.

Dans la vue globale, un modèle présent à la fois dans la banque personnelle et
dans une banque fournie n'apparaît qu'une fois : la copie personnelle est
prioritaire. Les modèles fournis sont en lecture seule ; ils peuvent toutefois
être ajoutés à un projet, exportés ou dupliqués dans la banque personnelle.

## Mise à jour directe depuis GitHub

Le bouton `Mettre à jour` ou `Update banks` consulte directement le fichier
public `machine-banks/catalog.json`, sans authentification. Le bouton
`Banques GitHub` ouvre séparément la page publique du catalogue.

Le parcours mis en œuvre est le suivant :

1. téléchargement du catalogue par HTTPS après action de l'utilisateur ;
2. contrôle du format, des doublons et de la version minimale de DCE ;
3. comparaison du SHA-256 avec le marqueur de la copie installée ;
4. confirmation du nombre de banques à mettre à jour ;
5. téléchargement de chaque archive avec limite de taille ;
6. contrôle du SHA-256 déclaré ;
7. restauration et validation dans un dossier temporaire ;
8. sauvegarde horodatée de la copie officielle précédente ;
9. remplacement de la copie officielle puis rechargement de la liste.

Une coupure réseau ou une archive incorrecte laisse la copie précédente intacte.
La banque personnelle n'est jamais une destination de cette opération. Les
sauvegardes sont conservées dans
`Documents/Dante Config Editor/Included Machine Banks/Backups`.

## Évolution proposée : soumettre une banque

Une publication silencieuse directe n'est pas retenue. Une banque personnelle
peut contenir par erreur une identité matérielle, une adresse, une subscription
ou une donnée de production.

Le premier parcours sûr sera :

1. `Proposer cette banque sur GitHub` ;
2. export d'une copie assainie dans un dossier choisi ;
3. validation des identifiants, adresses, flows et subscriptions ;
4. génération d'un rapport lisible et d'un SHA-256 ;
5. confirmation explicite de la liste des modèles et images ;
6. ouverture d'un formulaire GitHub prérempli dans le navigateur ;
7. ajout manuel de l'archive par l'utilisateur ;
8. revue humaine avant intégration au catalogue public.

DCE ne stockera alors aucun mot de passe, jeton GitHub ou cookie.

Une automatisation complète par OAuth Device Flow pourra être étudiée plus
tard. Elle nécessitera une application OAuth dédiée, un stockage sécurisé du
jeton dans le coffre du système, des permissions minimales, une révocation
facile et la création d'une pull request plutôt qu'une écriture directe dans
`main`.

## Règles de sécurité

- aucune banque personnelle n'est envoyée sans action et confirmation ;
- aucun XML de production n'est publié ;
- `instance_id`, `device_id`, réseau, flows et subscriptions sont absents des
  modèles publics ;
- le SHA-256 du catalogue est vérifié avant installation ;
- une archive est extraite avec protection contre les chemins sortants et les
  tailles excessives ;
- une banque téléchargée ne remplace qu'une copie officielle gérée par DCE,
  après sauvegarde ;
- le dépôt public reste la source de vérité du catalogue ;
- toute contribution publique passe par une revue humaine.
