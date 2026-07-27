# Corpus de compatibilité 2026.1

## Principe

Le dépôt ne contient que des fixtures synthétiques et anonymisées. Les corpus
réels éventuels restent locaux, sont lus depuis des copies temporaires via
`DANTE_REAL_XML_ROOT` et ne sont jamais commités.

## Couverture versionnée

| Besoin | Fixture ou génération | Test principal |
|---|---|---|
| XML simple et complet | `representative-preset.xml` | `CompatibilityCorpus2026_1Tests` |
| XML partiel | `compat-partial-one-way.xml` | `CompatibilityCorpus2026_1Tests` |
| TX uniquement / RX uniquement | `compat-partial-one-way.xml` | `CompatibilityCorpus2026_1Tests` |
| Source absente | `compat-subscription-edges.xml` | `CompatibilityCorpus2026_1Tests` |
| Canal TX absent | `compat-subscription-edges.xml` | `CompatibilityCorpus2026_1Tests` |
| Subscription locale `.` | `compat-subscription-edges.xml` | `CompatibilityCorpus2026_1Tests` |
| Namespace par défaut | `compat-namespace-unknown.xml` | `CompatibilityCorpus2026_1Tests` |
| Balise inconnue | `compat-namespace-unknown.xml` | `CompatibilityCorpus2026_1Tests` |
| Ordre de balises différent | `compat-namespace-unknown.xml` | garde-fou et corpus |
| Plusieurs interfaces IPv4 | `compat-namespace-unknown.xml`, `compat-mixed-network-audio.xml` | corpus et tests réseau |
| Unicode | `compat-namespace-unknown.xml` | corpus et tests d'export |
| Formats audio et modes réseau mixtes | `compat-mixed-network-audio.xml` | corpus et validation |
| 10 / 50 / 200 machines, 64 x 64 | `SyntheticPresetFactory` | `SyntheticPresetTests`, benchmarks |
| XML créé par DCE | `official-preset-creator-custom.xml` et génération temporaire | `ProjectCreationServiceTests` |
| Duplication | sortie temporaire | `MachineRoleV36Tests`, commandes 2026.1 |
| Fusion | `merge-preset.xml` et sortie temporaire | tests d'import/fusion |
| Projet `.dceproj` | paquet temporaire | `DceProjectPackageTests` |

Chaque fixture versionnée passe le cycle :

1. ouverture ;
2. validation sans erreur bloquante ;
3. sauvegarde sans modification ;
4. comparaison XML sémantique ;
5. réouverture ;
6. nouveau passage du garde-fou.

## Corpus local facultatif

```powershell
$env:DANTE_REAL_XML_ROOT = 'D:\chemin\vers\copies-anonymisees'
$env:DANTE_REAL_XML_REQUIRED = '1'
dotnet test .\tests\DanteConfigEditorV3.Tests\DanteConfigEditorV3.Tests.csproj -c Release --filter Category=LocalIntegration
```

Le test limite l'inventaire à 100 XML et travaille exclusivement dans des
répertoires temporaires. Le chemin, les noms et les contenus ne sont pas écrits
dans les rapports publics.
