# Banque de machines DCE 2026.1

## Rôle

La banque stocke des modèles de rôles Dante hors ligne réutilisables. Un modèle
n'est pas une identité matérielle et une machine ajoutée au projet devient une
instance indépendante.

La banque peut rester dans un dossier local, partagé ou synchronisé. DCE ne la
déplace jamais implicitement et l'installateur ne remplace jamais une banque
personnelle.

## Arborescence

```text
Machine Bank/
├── bank.json
├── machines/
│   └── 00000000-0000-0000-0000-000000000000/
│       ├── machine.json
│       ├── template.xml
│       └── image.png
├── Backups/
└── migration-v1-to-v2.json
```

`Backups` et le rapport de migration sont locaux. Les archives de banque
partageables contiennent uniquement `bank.json` et `machines/`.

## Versions

### Format 1

Le format V3.6 reste lisible et utilisable. Il contient :

- un identifiant de banque ;
- les identifiants de modèles ;
- un manifeste par modèle ;
- l'empreinte SHA-256 de `template.xml` ;
- une image facultative.

### Format 2

Le format 2026.1 ajoute :

- l'empreinte SHA-256 de l'image facultative ;
- la conservation des propriétés JSON inconnues dans `bank.json` et
  `machine.json` ;
- des limites renforcées sur l'import des archives de modèles.

Une nouvelle banque créée par 2026.1 utilise le format 2. Une banque format 1
n'est jamais modifiée automatiquement.

## Migration non destructive

Dans la fenêtre Banque de machines, `Migrer une copie` apparaît pour une banque
format 1. L'utilisateur choisit un dossier neuf ou vide.

La migration :

1. ouvre et valide tous les modèles de la banque source ;
2. exporte une archive de sauvegarde vérifiée ;
3. restaure cette archive dans un dossier temporaire ;
4. transforme uniquement les JSON du temporaire ;
5. calcule les empreintes d'images ;
6. valide tous les modèles au format 2 ;
7. compare les XML sémantiquement ;
8. vérifie que les empreintes de la source n'ont pas changé ;
9. publie le dossier temporaire comme nouvelle banque.

Le `bankId`, les `templateId`, les XML, les images, les dates et les champs JSON
inconnus sont conservés. La banque source reste bit à bit identique.

## Contenu neutralisé

Lors de l'enregistrement d'une machine dans la banque, DCE retire :

- `instance_id` ;
- `default_name` ;
- interfaces et adresses réseau ;
- subscriptions RX ;
- flows multicast TX ;
- état Preferred Master propre au projet.

Le nom est remplacé par un rôle générique. Les labels TX/RX peuvent être
personnalisés avant l'enregistrement.

Le modèle ne contient donc ni adresse matérielle inventée ni identité technique
du matériel source.

## Ajout au projet

L'ajout depuis la banque passe par une commande transactionnelle :

- validation de la capacité du profil XML ;
- validation du nom et de son unicité ;
- contrôle de la structure source ;
- adaptation du namespace au projet ;
- neutralisation défensive de l'identité et du réseau ;
- insertion comme rôle générique ;
- validation du projet ;
- une seule entrée d'historique ;
- Annuler/Rétablir en une étape.

Modifier ensuite le modèle ne modifie pas la machine déjà ajoutée. Modifier la
machine ajoutée ne modifie pas la banque.

La duplication d'une machine suit la même transaction. Par défaut, réseau,
subscriptions, flows multicast et Preferred Master ne sont pas recopiés.

## Images

Formats acceptés :

- PNG ;
- JPEG ;
- WebP.

La taille maximale est 10 Mio. DCE vérifie l'extension, la signature du fichier
et, en format 2, son empreinte SHA-256. L'image est copiée dans le dossier du
modèle ; aucun chemin externe fragile n'est conservé.

## Archives

Un modèle peut être exporté dans une archive `.dce-machine.zip`. Une banque
complète utilise `.dce-bank.zip`.

À l'import, DCE contrôle :

- le path traversal ;
- les doublons de chemins ;
- le nombre d'entrées ;
- la taille par entrée ;
- la taille totale décompressée ;
- les manifestes ;
- les identifiants ;
- les empreintes ;
- la structure XML neutralisée.

Une collision de `templateId` ne remplace jamais silencieusement un modèle.

