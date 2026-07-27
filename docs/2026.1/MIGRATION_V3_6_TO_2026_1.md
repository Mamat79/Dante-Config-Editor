# Migration V3.6 vers 2026.1

## Principe

La version 2026.1 utilise un profil local distinct :

- V3.6 : `%LOCALAPPDATA%\DanteConfigEditorV3.2`
- 2026.1 : `%LOCALAPPDATA%\DanteConfigEditor2026.1`

Le nom historique du dossier V3.6 est conservé tel quel afin de ne pas casser
les installations existantes. La 2026.1 ne modifie jamais ce dossier.

## Premier démarrage

Avant de charger la langue et les préférences, DCE :

1. inventorie les données V3.6 reconnues ;
2. crée une archive de sauvegarde vérifiée dans le profil 2026.1 ;
3. copie chaque fichier vers un temporaire ;
4. compare son empreinte SHA-256 avec la source ;
5. déplace la copie validée à sa destination ;
6. écrit un rapport JSON atomique.

Une destination 2026.1 déjà présente est toujours conservée. La migration est
idempotente : un rapport terminé empêche de refaire les copies aux démarrages
suivants.

## Données reprises

Fichiers :

- langue ;
- état développé ou réduit des éditeurs de configuration ;
- fichiers récents ;
- chemin de la banque de machines ;
- préférence du rappel de soutien.

Dossiers :

- récupérations XML V3.6 ;
- mises en page de synoptique V3.6.

La banque de machines elle-même n'est ni copiée ni déplacée. Seul son chemin
configuré est repris. Une banque personnelle, partagée ou synchronisée reste
donc à son emplacement et n'est jamais remplacée par les banques intégrées.

## Sauvegarde et rapport

Les fichiers locaux sont créés sous :

```text
%LOCALAPPDATA%\DanteConfigEditor2026.1\
├── MigrationBackups\
│   └── V36_Settings_*.zip
└── migration-v3.6-to-2026.1.json
```

Le rapport indique :

- les chemins source et destination ;
- les dates de début et de fin ;
- l'archive de sauvegarde ;
- l'empreinte et la taille de chaque fichier ;
- les fichiers copiés ;
- les destinations déjà présentes et conservées ;
- les éventuels échecs.

En cas d'échec partiel, le rapport reste incomplet et une nouvelle tentative
peut reprendre les fichiers manquants. Les copies déjà validées ne sont pas
écrasées.

## Retour à la V3.6

La V3.6 continue à lire son propre profil inchangé. Désinstaller ou tester la
2026.1 n'altère pas ses préférences, ses récupérations, ses XML ni sa banque.
Supprimer le profil 2026.1 ne supprime aucune donnée du profil V3.6.

