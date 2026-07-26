# Dante Config Editor V3.6 - développement

[English release notes](RELEASE_NOTES_EN.md)

## Statut

V3.6 est une version de développement Windows et macOS issue de la V3.5. Dante Config Editor reste un outil tiers non officiel Audinate. Travaillez sur une copie et validez toujours le XML généré par un import dans la version de Dante Controller réellement utilisée.

## Sécurité et fidélité XML

- Modification ciblée du document XML d'origine afin de conserver nœuds, attributs, namespaces, ordre et valeurs inconnues.
- Validation renforcée des identifiants, références de canaux, subscriptions, structures réseau et ajouts de nœuds.
- Sauvegarde sous atomique avec relecture du temporaire, comparaison sémantique et sauvegarde de la destination existante.
- Tests de cycle import/export/import, namespace par défaut, Unicode, valeurs inconnues et gros presets.
- Corpus local de dix XML : 176 machines et 5 004 labels chargés, validés, sauvegardés, comparés sémantiquement et relus sans toucher aux originaux.

## Duplication et banque de machines

- Duplication d'une machine sous forme de rôle générique indépendant.
- Les identifiants matériels `instance_id` et `device_id` ne sont jamais recopiés ni inventés.
- Réseau, subscriptions, flows, Preferred Master et réglages sensibles restent exclus par défaut.
- Banque versionnée et partageable avec métadonnées, tags, labels modifiables et image PNG/JPEG/WebP facultative copiée dans le modèle.
- Recherche, filtres, modification, duplication, suppression confirmée, import/export ZIP, sauvegarde et restauration complète.
- Catalogue GitHub bilingue et archive `*.dce-bank.zip` vérifiée pour télécharger ou partager une banque complète.
- Banque fournie `DCE Generic Roles 3.6` avec rôles génériques 8x8 et 32x32, sans identité matérielle, réseau ni abonnement.
- Banque communautaire optionnelle `DCE Community Devices 3.6`, illustrée et assainie, avec Yamaha QL1 et Rio1608-D2, Fohhn DI4.1000, Lake LM 44 et RME Digiface Dante.
- Ajout transactionnel d'une instance indépendante depuis un modèle.
- Nouveau projet minimal 3.0.0 expérimental, vide ou amorcé par un modèle.

## Diagnostic et interface

- Journaux techniques quotidiens accessibles depuis l'application.
- Commandes équivalentes Windows/macOS pour dupliquer, administrer la banque, ajouter un modèle et créer un projet.
- Les comportements existants de patch, zoom, renommage, Entrée, Tab et Maj+Tab restent couverts.
- La comparaison XML, ses statuts et ses résultats sont désormais réellement affichés en anglais lorsque cette langue est active, sur Windows et macOS.

## Validation automatisée

- 272 tests Core/Windows réussis.
- 20 tests Avalonia/macOS sans écran réussis.
- Builds Windows et macOS Release sans warning.
- Aucun package NuGet vulnérable signalé par la commande d'audit.

## Distribution

- Installateur Windows x64 autonome : `DanteConfigEditorV3_6_Installer.exe`, runtime .NET 8 et notices FR/EN inclus.
- Choix du dossier de banque actif et du dossier des banques fournies ; les banques générique et communautaire sont sélectionnables séparément, sans remplacement des banques existantes.
- Les DMG macOS contiennent les deux archives de banques dans un dossier `Machine Banks`.
- La V3.6 met à niveau la ligne de développement V3.5 et laisse la V3.4.2 stable intacte.
- Paquets macOS prévus pour Apple Silicon et Intel sous le nom V3.6.

## Limites

- Aucun fichier V3.6 n'a encore été importé réellement dans Dante Controller : les tests automatisés et comparaisons structurelles ne constituent pas une garantie terrain.
- Un rôle générique de preset n'est pas l'identité d'un appareil Dante physique.
- La création d'un projet complet reste expérimentale.
- L'installateur Windows n'est pas signé Authenticode.
- Les DMG Mac sont signés ad hoc, sans certificat Apple Developer ID ni notarisation.
