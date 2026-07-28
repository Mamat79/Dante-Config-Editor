# Banques locales et catalogue GitHub

## État livré dans 2026.1 Beta

L'installateur Windows permet déjà de choisir séparément :

- le dossier de la banque active ;
- le dossier dans lequel copier les banques fournies ;
- l'installation de `DCE Generic Roles 2026.1` ;
- l'installation de `DCE Community Devices 2026.1`.

La banque communautaire 2026.1 contient 41 modèles illustrés et assainis. Les
descriptions françaises et anglaises sont affichées selon la langue active.

Chaque banque fournie est copiée dans un dossier distinct. Un dossier existant
n'est jamais écrasé : un nouveau nom est choisi. La bibliothèque personnelle
reste indépendante de l'application et peut être placée dans un dossier local,
partagé ou synchronisé.

Depuis DCE, l'utilisateur peut :

- changer de banque active ;
- ouvrir son dossier ;
- exporter une banque en `.dce-bank.zip` ;
- importer une archive dans un dossier neuf ou vide ;
- ouvrir le catalogue public GitHub.

## Évolution proposée : catalogue intégré

Le bouton `Banques GitHub` pourra afficher directement le fichier public
`machine-banks/catalog.json` sans authentification.

Le parcours recommandé :

1. téléchargement explicite du catalogue par HTTPS ;
2. affichage du nom, de la description, de la version et de la compatibilité ;
3. comparaison avec les banques locales ;
4. téléchargement sur action de l'utilisateur ;
5. contrôle de la taille et du SHA-256 déclaré ;
6. validation complète de l'archive dans un dossier temporaire ;
7. aperçu des modèles et du rapport d'assainissement ;
8. installation dans un nouveau dossier de bibliothèque ;
9. activation facultative, jamais automatique.

Le cache local doit rester supprimable. L'absence de réseau ne doit jamais
empêcher d'utiliser les banques déjà installées.

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
- une banque téléchargée n'écrase jamais une banque locale ;
- le dépôt public reste la source de vérité du catalogue ;
- toute contribution publique passe par une revue humaine.
