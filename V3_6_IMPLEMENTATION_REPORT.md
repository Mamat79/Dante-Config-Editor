# Rapport final d'implémentation V3.6

Date : 25 juillet 2026  
Branche : `v3.6`  
Base V3.5 : `09985050aab242682bab82b792276c68ef3693fb`  
Commit fonctionnel validé avant ajout de ce rapport : `5f51e7e`

## 1. Résumé de l'audit

L'audit a couvert l'architecture, le moteur XML, les sauvegardes, les
subscriptions, les interfaces réseau, les performances, les dépendances, les
tests, les interfaces Windows/macOS, l'installation et la documentation.

Le moteur modifie déjà le document LINQ to XML d'origine au lieu de reconstruire
un preset simplifié. Cette base a été conservée. Les nouvelles fonctions sont
isolées dans des services et n'écrivent pas de propriété DCE dans le XML Dante.

Trois niveaux de preuve sont distingués :

1. tests automatisés et compilation ;
2. comparaison structurelle et sémantique sur des copies de XML ;
3. import réel dans une version identifiée de Dante Controller.

Le niveau 3 n'a pas été exécuté.

## 2. Problèmes trouvés

- Le nom visible d'une machine ne pouvait pas servir d'identité stable pendant
  un renommage ou une duplication.
- Copier une machine brute aurait dupliqué son `instance_id/device_id`.
- Le garde-fou devait distinguer un ajout autorisé par un service d'un ajout
  arbitraire de nœud `<device>`.
- Les noms fusionnés pouvaient dépasser la limite de 31 caractères.
- La validation après sérialisation répétait des opérations coûteuses.
- Aucun format maîtrisé ne permettait de stocker, vérifier, migrer et partager
  des modèles de machines.
- Les échecs d'import, banque et validation n'avaient pas de journal technique
  persistant facilement accessible.
- Les fonctions prévues n'avaient pas de commandes équivalentes sur les deux
  interfaces.
- Les documents V3.5 ne décrivaient évidemment pas les nouveaux risques V3.6.

## 3. Problèmes corrigés

- Identité de rôle de session stable, portée par une annotation non sérialisée.
- Duplication sous forme de rôle générique sans identité matérielle inventée.
- Autorisation précise des ajouts assainis dans le garde-fou XML.
- Validation d'intégrité du document candidat avant insertion définitive.
- Sauvegarde temporaire relue, comparée sémantiquement, validée puis remplacée
  atomiquement.
- Suffixes de fusion nettoyés et tronqués avant numérotation.
- Banque versionnée avec empreintes, staging, rollback et backups.
- Archives protégées contre les chemins sortants, volumes excessifs et doublons.
- Journaux quotidiens et bouton d'ouverture du dossier.
- Dialogues Windows et macOS pour duplication, banque, insertion et nouveau
  projet.
- Installateur V3.6 autonome avec mise à niveau de la ligne V3.5.

## 4. Risques encore présents

- Aucun XML V3.6 n'a été importé réellement dans Dante Controller.
- Une extension constructeur conservée peut contenir une donnée propre à
  l'instance source que DCE ne sait pas interpréter.
- DCE ne connaît pas hors ligne les capacités matérielles réelles.
- La création de projet 3.0.0 reste expérimentale.
- La banque V1 ne convertit pas entre versions de preset.
- Deux utilisateurs ne doivent pas modifier simultanément une banque partagée.
- Les nouvelles fenêtres n'ont pas été revues manuellement à toutes les échelles
  Windows ni sur un Mac réel.

## 5. Fichiers modifiés

Les changements sont regroupés par responsabilité :

- moteur : `Models/DanteProject.*`, `Models/Machine*`,
  `Services/DanteXml*`, `Services/Machine*`, `Services/ProjectCreationService.cs` ;
- diagnostic : `Services/DiagnosticLogService.cs`, `App.xaml.cs` ;
- Windows : `MainWindow.*`, fenêtres `Machine*Window` et `NewProjectWindow` ;
- macOS : `src/DanteConfigEditor.Mac/MainWindow.*`, dialogues `Machine*Dialog`
  et `NewProjectDialog` ;
- tests : nouvelles suites `*V36Tests.cs` et fixture de rôle custom ;
- paquetage : projets `.csproj`, workflows, Inno Setup et script macOS ;
- documentation : audit, structure XML, format de banque, guides, limites,
  matrice de compatibilité, checklist et README bilingues.

## 6. Nouveaux modules

- `MachineRoleIdentityService` : identité stable de session.
- `MachineRoleInstantiationService` : mécanisme commun de création d'instance.
- `MachineTemplateService` : conversion entre rôle assaini et modèle.
- `MachineBankRepository` : persistance transactionnelle.
- `MachineBankArchiveService` : import/export et sauvegarde/restauration.
- `MachineBankMigrationService` : contrôle central de version.
- `MachineBankLocationService` : emplacement visible et configurable.
- `ProjectCreationService` : création expérimentale minimale.
- `XmlSemanticComparisonService` : détection de pertes réelles de contenu.
- `DiagnosticLogService` : journalisation locale.

## 7. Architecture de la banque

L'interface ne manipule pas directement les fragments XML. Elle appelle le
service de modèle, puis le dépôt de banque. L'ajout au projet réutilise le même
service d'instanciation que la duplication.

Une instance ajoutée est une copie profonde sans lien dynamique avec le modèle.
La banque peut donc être copiée ou partagée sans rendre les projets dépendants
de son emplacement.

## 8. Format de stockage

Format V1 lisible :

```text
Machine Bank/
├── bank.json
├── machines/
│   └── {templateId}/
│       ├── machine.json
│       ├── template.xml
│       └── image.png|jpg|webp
└── Backups/
```

`machine.json` contient version, UUID, fabricant, modèle, catégorie, tags,
comptages, version de preset, namespace, dates et SHA-256. `template.xml`
contient un seul rôle assaini. Les images sont copiées et limitées à 10 Mio.

## 9. Régénération des identifiants

DCE ne fabrique pas de faux `device_id`.

- `instance_id` et `default_name` sont retirés du clone.
- Le rôle reçoit une identité de session DCE aléatoire et non sérialisée.
- Les `danteId` et `mediaType` des canaux restent ceux du rôle.
- Le nom est validé et rendu unique dans le projet.
- Les références locales conservées explicitement suivent le nouveau nom.

Cette méthode suit la forme des Custom Devices observés dans Dante Preset
Creator, sans prétendre remplacer la validation de Dante Controller.

## 10. Préservation de la compatibilité XML

- modifications ciblées du document source ;
- conservation des nœuds, attributs, commentaires, namespaces et ordre ;
- chemins inconnus bloqués par défaut s'ils changent ;
- aucune donnée de banque, image ou identité de session écrite dans le preset ;
- réseau, subscriptions, flows et Preferred Master neutralisés par défaut ;
- document candidat validé avant mutation du projet ;
- SaveAs atomique avec backup et comparaison sémantique ;
- version de preset et namespace contrôlés avant insertion.

## 11. Tests ajoutés

Les nouvelles suites couvrent :

- identité stable, duplication et options ;
- absence de duplication d'identité matérielle ;
- ajout autorisé contre ajout arbitraire bloqué ;
- custom role de Preset Creator ;
- namespace par défaut et valeurs inconnues ;
- banque, empreintes, images, import/export et rollback ;
- archives malveillantes ou trop volumineuses ;
- emplacement, sauvegarde et restauration ;
- nouveau projet et refus d'écrasement ;
- corpus réel traité depuis des copies temporaires ;
- journaux de diagnostic ;
- commandes et dimensions macOS ;
- versionnement et contrats installateur/workflows.

## 12. Tests exécutés

- 258 tests Core/Windows : réussis ;
- 16 tests Avalonia/macOS sans écran : réussis ;
- builds Windows et macOS Release : 0 warning, 0 erreur ;
- publications `win-x64`, `osx-arm64`, `osx-x64` : réussies ;
- audit NuGet : aucun package vulnérable signalé ;
- installateur Inno Setup : compilé, installé deux fois, aucun doublon ;
- exécutable autonome et installé : démarrage et réponse confirmés ;
- quatre PDF : texte extrait et 32 pages rendues puis inspectées ;
- dix XML locaux : 176 machines et 5 004 labels, cycle sans perte sémantique
  détectée, originaux inchangés.

Benchmark médian final, trois passages, 64 TX et 64 RX par machine :

| Machines | Chargement | Modification | Garde-fou | Sauvegarde |
|---:|---:|---:|---:|---:|
| 10 | 57,4 ms | 109,8 ms | 18,8 ms | 206,5 ms |
| 50 | 188,8 ms | 393,8 ms | 31,9 ms | 386,1 ms |
| 200 | 433,8 ms | 1 081,4 ms | 142,8 ms | 996,9 ms |

## 13. Test réel dans Dante Controller

**Aucun test réel n'a été effectué.**

Il n'est donc pas affirmé que la duplication, l'insertion depuis la banque ou le
nouveau projet sont garantis importables. La procédure exacte est dans
`MANUAL_DANTE_CONTROLLER_TESTS.md`.

## 14. Vérifications structurelles seulement

Ont été vérifiés automatiquement :

- lecture, modification, sauvegarde et relecture DCE ;
- conservation sémantique ;
- intégrité des identifiants et références reconnues ;
- forme des rôles génériques ;
- atomicité et sauvegardes ;
- indépendance modèle/instance.

Ces résultats sont utiles mais ne remplacent pas l'interprétation du fichier
par Dante Controller.

## 15. Limites connues

Voir `KNOWN_LIMITATIONS.md`. Les principales limites sont l'absence de contrôle
réseau en temps réel, l'absence d'API Audinate, la dépendance au format XML
réel, l'absence de validation matérielle et la non-notarisation des DMG.

## 16. Recommandations

1. Importer dans Dante Controller un XML inchangé, une duplication, une
   insertion depuis la banque et un projet neuf.
2. Renseigner version, testeur, captures et anomalies dans la matrice.
3. Maintenir Nouveau projet en statut expérimental jusqu'à plusieurs preuves.
4. Tester les dialogues à 125 %, 150 %, 200 %, avec lecteur d'écran et sur Mac.
5. Scinder progressivement les fenêtres principales et le dépôt de banque.

## 17. Version finale

Version applicative : **3.6**  
Version fichier : **3.6.0.0**  
Installateur : `DanteConfigEditorV3_6_Installer.exe`

## 18. Branche et commits

- branche : `v3.6` ;
- base : `09985050aab242682bab82b792276c68ef3693fb` ;
- code XML et création : `afca736` ;
- banque : `a6a2ff1` ;
- diagnostic : `b1f6342` ;
- documentation technique : `1603581` ;
- interface Windows : `ee0052b` ;
- interface macOS : `831231b` ;
- paquetage V3.6 : `3751691` ;
- guides et release notes : `9cb7ccf` ;
- validation finale avant rapport : `5f51e7e`.

Le hash du commit contenant ce rapport est à relever avec `git rev-parse HEAD`
après son ajout ; il ne peut pas être inscrit dans son propre contenu sans
changer ce même hash.
