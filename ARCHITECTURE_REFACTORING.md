# Architecture et refactorisation progressive

## Objectif

La V3.6 conserve le projet existant et extrait progressivement les responsabilités risquées. Il n'y a pas de réécriture générale : chaque déplacement doit préserver le XML produit, rester couvert par les tests et faire l'objet d'un commit isolé.

## Projets

| Projet | Rôle |
|---|---|
| `DanteConfigEditorV3.csproj` | interface Windows WPF et assemblage de l'application |
| `src/DanteConfigEditor.Core` | modèle XML et services partagés par Windows et macOS |
| `src/DanteConfigEditor.Mac` | interface macOS Avalonia |
| `tests/DanteConfigEditorV3.Tests` | sécurité XML, persistance, fusion, patch, charge et installateur |
| `tests/DanteConfigEditor.Mac.Tests` | tests Avalonia headless de structure, navigation et dimensions |
| `tools/DanteConfigEditor.ValidationPack` | génération non destructive des scénarios de validation manuelle |
| `benchmarks/DanteConfigEditorV3.Benchmarks` | mesures synthétiques reproductibles |

## Extractions déjà réalisées

| Responsabilité | Fichier principal | Contrat conservé |
|---|---|---|
| sauvegarde atomique et session | `Models/DanteProject.Persistence.cs`, `Services/SafeFileService.cs` | destination précédente intacte en cas d'échec, backup avant remplacement |
| comparaison avant/après | `Models/DanteProject.Comparison.cs` | comparaison métier sans réécriture XML |
| import / fusion | `Models/DanteProject.Import.cs` | import des uniques et renommage explicite des doublons |
| actions globales | `Models/DanteProject.GlobalActions.cs` | lot unique et un seul `ReloadModel` |
| rapports | `Models/DanteProject.Reports.cs`, `Services/ReportExportService.cs` | contenu fonctionnel préservé, exports signés et versionnés |
| récupération | `MainWindow.Recovery.cs`, `Services/SessionRecoveryService.cs` | écriture asynchrone temporisée liée à la destination active |
| garde-fou XML | `Services/DanteXmlChangeGuardService.cs` | chemins inconnus bloqués et identités techniques protégées |
| patch visuel partagé | `Services/PatchAssignmentPlanner.cs`, `Services/PatchWorkspaceSession.cs` | changements en attente puis lot unique et annulable |
| échange de labels | `Services/ChannelLabelExchangeService.cs`, `Services/DmtChannelWorkbookService.cs`, `Services/ChannelLabelTransferPlanner.cs` | aperçu avant application, modèle DMT original intact, lot unique et références TX mises à jour |
| identité de rôle | `Services/MachineRoleIdentityService.cs`, `Models/MachineRoleModels.cs` | identité de session non sérialisée, aucun identifiant matériel inventé |
| duplication et insertion | `Services/MachineRoleInstantiationService.cs`, `Models/DanteProject.MachineRoles.cs` | clone indépendant, options explicites, opération annulable et transactionnelle |
| conversion de modèle | `Services/MachineTemplateService.cs`, `Models/MachineTemplateModels.cs` | neutralisation des données de projet, métadonnées versionnées et labels génériques |
| stockage de banque | `Services/MachineBankRepository.cs`, `Services/MachineBankArchiveService.cs`, `Services/MachineBankMigrationService.cs` | écritures atomiques, empreintes, archives protégées et migration explicite |
| création de projet | `Services/ProjectCreationService.cs`, `Models/DanteProject.Creation.cs` | structure minimale expérimentale 3.0.0 et validation avant remplacement |
| comparaison sémantique | `Services/XmlSemanticComparisonService.cs` | distinction entre mise en forme et perte de contenu XML |
| diagnostic | `Services/DiagnosticLogService.cs` | journal quotidien pour les échecs sans donnée sensible imposée |

Les interfaces `PatchWorkspaceWindow` (WPF) et `PatchWorkspaceDialog` (Avalonia) restent propres à chaque plateforme, mais utilisent le même planificateur et la même session partagée.

## Fichiers encore volumineux

Mesure du 2026-07-25 avant la validation finale :

| Fichier | Lignes | Risque principal |
|---|---:|---|
| `MainWindow.xaml.cs` | 4 276 | orchestration, état UI et commandes Windows encore mêlés |
| `Models/DanteProject.cs` | 1 750 | chargement, mutations unitaires et recherche XML encore concentrés |
| `src/DanteConfigEditor.Mac/MainWindow.axaml.cs` | 2 221 | orchestration Mac et adaptation de vues encore regroupées |
| `Services/LocalizationService.cs` | 843 | dictionnaire bilingue monolithique |
| `Services/DanteXmlChangeGuardService.cs` | 672 | comparaison de sécurité complexe et sensible |

La réduction historique de `DanteProject.cs` depuis 2 449 lignes provient des extractions ciblées. La croissance des fenêtres principales inclut le patch visuel et l'orchestration de la banque ; elle doit être contenue par des contrôleurs de vue, sans déplacer la logique XML vers l'interface.

## Prochaines extractions prudentes

1. Isoler la coordination des actions globales de `MainWindow.xaml.cs` dans un contrôleur testable sans WPF.
2. Extraire la construction des listes et filtres de patch dans un modèle de vue partagé.
3. Déplacer les mutations unitaires restantes de `DanteProject.cs` par domaine : device, canal, réseau, subscription.
4. Découper `LocalizationService` par écran tout en ajoutant un test d'exhaustivité des clés FR/EN.
5. Mutualiser les modèles de vue Windows/Mac seulement lorsque leur comportement est réellement identique.

## Règles pour la suite

- aucun changement métier non demandé pendant une extraction ;
- test de non-régression ajouté avant ou avec l'extraction ;
- comparaison du XML généré avant/après sur fixtures anonymisées ;
- pas de modification implicite des identités, interfaces secondaires ou valeurs inconnues ;
- un commit par responsabilité ;
- validation Windows et macOS avant publication.
