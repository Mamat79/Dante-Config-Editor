# Windows 2027.2.9 — contrôles et limites / checks and limits

## Français

- Source privée figée : `153d63e681190339b9cb579f783c723a4a094d0b`, branche `main`. Le code produit qualifié est celui de `103b9c26757861e164dd56607c87e3d5b5478b10` ; les changements suivants concernent la documentation et le routage.
- 938 tests source réussis réutilisés sans nouvelle exécution : 878 tests cœur, 55 tests Avalonia et 5 tests Node. Le build Windows ajoute 20 contrôles du contrat de version réussis.
- Construction locale Windows avec la recette officielle. Les 283 fichiers extraits de l’installateur correspondent à ses entrées. L’installation réelle s’est terminée avec le code 0 ; les 283 fichiers installés, la version du registre, la version de l’EXE et le raccourci Bureau unique ont été contrôlés.
- Sauvegardes privées indépendantes sur C: et D: et restauration de contrôle vérifiées avant remplacement. Les documents et banques personnels mesurés sont restés identiques après installation. Un ancien groupe du menu Démarrer reste conservé.
- L’assistant a effectivement exécuté son lancement final. Cette première instance était déjà fermée avant le contrôle suivant. Une instance distincte a ensuite été lancée sans projet, observée dans le contexte utilisateur normal et fermée normalement avec le code 0. À propos affiche 2027.2.9 / 2027.2.9.0 ; la révision source n’est pas renseignée dans cette boîte et provient des preuves de construction.
- L’écran français, thème clair courant, distingue utilisation gratuite sans code et code permanent acheté ou offert ; Continuer est visible et permet de revenir à l’application sans activation. Aucun réglage de langue ou de thème n’a été changé pour cette observation.
- Sur le profil présent après installation, tous les champs de licence restent identiques pendant ce démarrage, sauf l’horodatage monotone du dernier lancement. Les autres fichiers de profil restent identiques. Ce profil était absent dans les contextes mesurés avant installation : cela ne prouve pas la migration d’un ancien profil non observé. Aucune licence n’a été activée ou réinitialisée par le contrôle.
- Quatre PDF Windows qualifiés : notices FR/EN de 47 pages et démarrages rapides FR/EN d’une page. Les 96 pages ont été rendues ; revue globale en planches et inspection de pages ciblées. Les illustrations de licence des notices sont synthétiques et identifiées comme telles.

Ces résultats ne constituent pas une nouvelle recette matérielle Dante, un import réel dans Dante Controller, un test d’achat ou d’activation, une preuve d’isolation réseau, une mesure native du décompte d’essai expiré, ni une nouvelle campagne native dans toutes les langues, thèmes et échelles. Les paquets macOS 2027.2.8 restent inchangés et ne sont pas requalifiés par cette livraison.

## English

The pinned private source is `153d63e681190339b9cb579f783c723a4a094d0b` on `main`; qualified product code is `103b9c26757861e164dd56607c87e3d5b5478b10`. The 938 successful source tests were reused, not rerun (878 core, 55 Avalonia, 5 Node). This Windows build added 20 successful version-contract checks.

The official local recipe produced an installer whose 283 application files match its inputs. Actual installation exited 0; all 283 installed files, registry/EXE versions and the single desktop shortcut were checked. Recoverable independent C:/D: backups and a restoration rehearsal preceded replacement; measured personal documents and banks stayed unchanged. An older Start Menu group was retained.

Setup did run its postinstall application entry. That first instance was already absent before the controlled launch. A separate normal-user instance was then launched without a project and closed normally with exit 0. About shows 2027.2.9 / 2027.2.9.0; source identity comes from build evidence rather than an embedded About revision. The current French/light license dialog visibly separates free use from an optional purchased/gifted permanent code; Continue returned to the application without activation. Existing language/theme preferences were not changed for this observation.

Across this controlled startup, all current license fields were preserved except monotone LastSeen, and other current profile files were unchanged. The profile was absent in the measured preinstallation contexts, so continuity of an unobserved older profile is not asserted. No license was activated or reset by the check.

The four Windows guides contain 47/1/47/1 pages; all 96 pages were rendered, reviewed in contact sheets and inspected at selected full-size pages. License illustrations are explicitly synthetic. These checks do not constitute new hardware/Dante Controller import, payment/activation, network-isolation, expired-trial native timing or every-language/theme/DPI tests. Existing macOS 2027.2.8 packages are unchanged and not requalified here.
