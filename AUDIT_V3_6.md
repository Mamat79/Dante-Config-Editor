# Audit technique Dante Config Editor V3.6

Date de l'audit : 25 juillet 2026  
Branche de départ : `v3.5`  
Commit de départ : `09985050aab242682bab82b792276c68ef3693fb`  
Système de mesure : Windows 10.0.26200, .NET SDK 8.0.423, runtime 8.0.29

## Portée et niveau de preuve

Cet audit couvre le code source, les tests, les builds Windows et macOS, les
fixtures anonymisées et dix presets réels traités uniquement depuis des copies
temporaires.

Trois niveaux de preuve doivent rester distincts :

1. **Validé automatiquement** : tests unitaires et d'intégration, garde-fou XML,
   comparaison sémantique, compilation.
2. **Vérifié structurellement** : cycle chargement, sauvegarde, comparaison et
   rechargement sur des copies de presets réels.
3. **Validé dans Dante Controller** : import manuel réalisé dans le logiciel
   Audinate avec observation du résultat.

Le niveau 3 n'avait pas été exécuté dans l'environnement d'audit. Mise à jour du
27 juillet 2026 : le mainteneur confirme un import et un essai réussis dans
Dante Controller avec des XML modifiés par la V3.6. La version de Dante
Controller et les scénarios anonymisés restent à consigner ; les nouveaux types
de preset doivent donc toujours suivre la checklist manuelle.

## Baseline V3.5

Commandes exécutées avant modification :

```powershell
dotnet restore
.\tests\run-tests.ps1
dotnet build .\DanteConfigEditorV3.csproj -c Release
```

Résultats :

- restauration : réussie en 3,66 s ;
- tests métier et XML : 212 réussis, 0 échec ;
- tests macOS headless : 16 réussis, 0 échec ;
- build Windows Release : réussi, 0 warning, 0 erreur, 11,12 s ;
- CI Windows du commit de départ : réussie ;
- CI macOS du commit de départ : réussie.

Après le lot V3.6 final :

- tests métier, XML et contrats Windows : 258 réussis, 0 échec ;
- tests macOS headless : 16 réussis, 0 échec ;
- build Windows Release : réussi, 0 warning, 0 erreur ;
- build macOS Release : réussi, 0 warning, 0 erreur ;
- publication autonome Windows `win-x64` : réussie ;
- publications croisées macOS `osx-arm64` et `osx-x64` : réussies ;
- installateur Inno Setup 6.7.3 : compilé et installé deux fois sans doublon ;
- lancement de l'exécutable autonome et de l'exécutable installé : processus
  répondant, titre V3.6 et version fichier 3.6.0.0.

## Architecture constatée

Le projet utilise :

- C# et .NET 8 ;
- WPF pour Windows ;
- Avalonia 11.3 pour macOS ;
- LINQ to XML pour conserver et modifier le document source ;
- xUnit pour les tests ;
- Inno Setup pour l'installateur Windows.

Le projet `DanteConfigEditor.Core` compile les mêmes fichiers `Models` et
`Services` que l'application WPF. L'interface macOS référence ce coeur partagé.
Cette organisation limite les divergences métier entre les deux plateformes.

### Fichiers trop volumineux

Les plus grands fichiers au moment de l'audit sont :

| Fichier | Lignes | Risque |
|---|---:|---|
| `MainWindow.xaml.cs` | 4 276 | logique UI, orchestration et erreurs fortement couplées |
| `PatchWorkspaceView.xaml.cs` | 2 358 | patch, navigation et édition réunis |
| `src/DanteConfigEditor.Mac/MainWindow.axaml.cs` | 2 221 | duplication de logique UI |
| `MainWindow.xaml` | 1 873 | coût élevé de modification et de revue visuelle |
| `Models/DanteProject.cs` | 1 750 | trop de responsabilités métier dans une classe |
| `MachineBankRepository.cs` | 734 | persistance, images et archives encore regroupées |
| `DanteXmlChangeGuardService.cs` | 672 | comparaison de sécurité complexe |

La V3.6 extrait la création, la duplication, l'identité de rôle, la banque,
l'intégrité et la comparaison sémantique dans des services distincts. Une
réécriture générale n'est pas justifiée ; les extractions doivent rester
progressives et couvertes par des tests.

## Ce qui fonctionne correctement

- Le XML est chargé avec conservation des espaces et informations de ligne.
- Les modifications métier agissent sur le document d'origine au lieu de
  reconstruire un preset simplifié.
- La sauvegarde utilise un fichier temporaire et un remplacement atomique.
- Une destination existante est conservée dans un backup.
- Les nœuds et attributs inconnus sont bloqués s'ils sont modifiés sans
  autorisation.
- Les namespaces par défaut sont pris en charge.
- Les alias de subscription connus sont couverts par les tests existants.
- Les interfaces IPv4 secondaires ne sont pas modifiées implicitement.
- Les opérations groupées évitent plusieurs reconstructions du modèle.
- La pile d'annulation est limitée.
- La récupération automatique est différée et asynchrone.

## Problèmes corrigés immédiatement

### Identité d'une machine

Le nom visible ne suffit pas comme identité : il est précisément destiné à être
renommé. La V3.6 ajoute une identité de rôle de session non sérialisée. Les
machines physiques continuent de conserver leur `device_id` et leur
`process_id` d'origine.

### Ajout arbitraire d'un `<device>`

Le garde-fou bloquait correctement les chemins techniques modifiés, mais devait
reconnaître explicitement les ajouts créés par les nouveaux services. Un ajout
de machine non autorisé reste bloqué. Une duplication ou une insertion depuis
la banque fournit au garde-fou une baseline assainie précise.

### Duplication d'identité matérielle

Copier `instance_id/device_id` aurait produit deux instances prétendant être le
même matériel. La duplication V3.6 retire `instance_id` et `default_name`.
L'instance devient un rôle générique nommé par `friendly_name`, comme les
Custom Devices observés dans Dante Preset Creator.

### Sauvegarde

La validation complète était répétée inutilement après la sérialisation. Le
fichier temporaire est maintenant :

1. relu ;
2. comparé sémantiquement au document en mémoire ;
3. validé pour l'intégrité ;
4. comparé au document source par le garde-fou ;
5. seulement ensuite déplacé ou remplacé atomiquement.

### Fusion de noms longs

Le suffixe automatique pouvait dépasser la limite de 31 caractères. La base est
maintenant nettoyée et tronquée avant ajout du suffixe et du compteur.

### Banque de machines

La banque possède un format versionné, des empreintes SHA-256, des écritures
temporaires, des backups de manifestes, des suppressions récupérables et une
restauration uniquement vers un dossier vide.

## Risques XML encore présents

### Compatibilité matérielle

Un rôle générique ne contient pas d'identité matérielle. Dante Controller devra
encore associer ce rôle à un matériel compatible au moment d'appliquer le
preset. DCE ne peut pas déduire les capacités réelles d'un appareil absent.

### Champs spécifiques aux fabricants

Les nœuds inconnus sont conservés dans une duplication. Ils peuvent néanmoins
représenter un état propre à une instance matérielle. La neutralisation de
`instance_id`, du réseau, des subscriptions et des flows réduit le risque sans
permettre de certifier chaque extension constructeur.

### Versions de preset

La banque V1 refuse l'insertion entre deux versions de preset différentes. Il
est préférable de bloquer plutôt que de convertir silencieusement une structure
2.1.0 vers 3.0.0.

### Nouveau projet

Le projet minimal 3.0.0 reproduit la forme des Custom Devices du Preset Creator
officiel. Il reste expérimental tant qu'un import réel dans Dante Controller
n'a pas été enregistré comme preuve.

## Validation XML V3.6

Les contrôles automatiques couvrent notamment :

- racine, version et namespace ;
- déclaration XML ;
- présence et unicité des `danteId` ;
- stabilité de `mediaType` ;
- identités techniques dupliquées ;
- format inhabituel de `device_id` ;
- IP fixes dupliquées ;
- références de slots `txflow` ;
- paires de subscriptions incomplètes ;
- appareil ou canal TX absent ;
- noms de machines dupliqués ;
- perte, ajout ou modification de chemins XML non autorisés ;
- perte de commentaires, nœuds inconnus ou attributs ;
- cohérence du nombre de canaux d'un modèle de banque ;
- empreinte du modèle et namespace déclaré.

Les messages contiennent l'élément, la machine ou le canal concerné lorsque ces
informations sont disponibles, puis une correction suggérée.

## Presets réels

Dix fichiers XML non versionnés ont été lus depuis un corpus local, copiés dans
un répertoire temporaire, sauvegardés, comparés sémantiquement puis rechargés.

Échantillon observé :

- 176 machines ;
- versions de preset 2.1.0 et 3.0.0 ;
- 5 004 labels de canaux ;
- subscriptions `subscribed_device` et `subscribed_channel` ;
- fichiers sans namespace et cas de namespace testés séparément ;
- identifiants techniques de 16 caractères hexadécimaux dans ce corpus.

Résultat structurel : 10 fichiers sur 10 ont terminé le cycle automatisé. Les
originaux n'ont jamais été écrits. Ce résultat ne constitue pas une preuve
d'import dans Dante Controller.

## Performances

Scénario : presets synthétiques de 10, 50 et 200 machines, 64 TX et 64 RX par
machine, trois exécutions, médiane, configuration Release.

### Baseline V3.5

| Machines | XML | Chargement | Modification groupée | Garde-fou | Sauvegarde | Allocation sauvegarde |
|---:|---:|---:|---:|---:|---:|---:|
| 10 | 0,156 Mio | 40,0 ms | 40,9 ms | 9,6 ms | 110,8 ms | 30,1 Mio |
| 50 | 0,780 Mio | 88,6 ms | 148,9 ms | 26,8 ms | 203,4 ms | 147,2 Mio |
| 200 | 3,122 Mio | 365,8 ms | 580,3 ms | 99,7 ms | 1 032,5 ms | 586,2 Mio |

### Mesure V3.6 finale

| Machines | Chargement | Modification groupée | Garde-fou | Sauvegarde | Allocation sauvegarde |
|---:|---:|---:|---:|---:|---:|
| 10 | 57,4 ms | 109,8 ms | 18,8 ms | 206,5 ms | 22,6 Mio |
| 50 | 188,8 ms | 393,8 ms | 31,9 ms | 386,1 ms | 110,6 Mio |
| 200 | 433,8 ms | 1 081,4 ms | 142,8 ms | 996,9 ms | 440,8 Mio |

La mesure finale confirme surtout une baisse reproductible de l'allocation de
sauvegarde d'environ 25 % : 586,2 Mio en V3.5 contre 440,8 Mio en V3.6 sur 200
machines. La sauvegarde finale est légèrement plus rapide que la baseline sur
les trois tailles, mais les temps de chargement et de modification restent
variables selon la charge de la machine. Aucune accélération générale n'est
présentée comme garantie.

Le coût principal restant est la copie complète du document pour l'annulation
et certaines validations. À 200 machines, une modification groupée alloue
environ 391 Mio. Une annulation différentielle serait plus économe, mais elle
augmenterait le risque de restauration partielle ; elle est différée.

## Dépendances

- Le coeur Windows n'ajoute aucune dépendance NuGet métier.
- Avalonia reste en 11.3.18 et DataGrid en 11.3.13.
- La version 12 d'Avalonia est une migration majeure, non intégrée dans cette
  version de sécurisation.
- `dotnet list package --vulnerable --include-transitive` ne signale aucune
  vulnérabilité connue lors de l'audit.

## Accessibilité et ergonomie

Déjà présent :

- édition au clavier des labels ;
- navigation `Entrée`, `Tab`, `Maj+Tab` ;
- thèmes clair et sombre ;
- panneaux adaptatifs ;
- tests headless de l'interface Mac.

À vérifier manuellement avant publication :

- échelle Windows 125 %, 150 % et 200 % ;
- résolution 1366 x 768 et 1920 x 1080 ;
- contraste élevé ;
- ordre de tabulation des nouveaux dialogues ;
- lecteur d'écran ;
- troncatures en français et en anglais ;
- apparence native réelle sur macOS Intel et Apple Silicon.

Une tentative de contrôle visuel automatisé Windows a été interrompue faute
d'autorisation de prise de contrôle dans le délai imparti. Les tests de
structure, de dimensions, de lancement et de processus sont réussis, mais les
nouveaux dialogues ne sont pas déclarés validés manuellement sur les échelles
ci-dessus.

## Recommandations

1. Exécuter la checklist d'import Dante Controller avec un preset 3.0.0 créé,
   une duplication et une insertion depuis la banque.
2. Conserver le statut expérimental de « Nouveau projet » jusqu'à cette preuve.
3. Valider manuellement les nouveaux dialogues à 125 %, 150 %, 200 % et sur
   macOS réel.
4. Scinder progressivement `MainWindow.xaml.cs` et
   `MachineBankRepository.cs`.
5. Ajouter ultérieurement des migrations seulement à partir de vrais anciens
   formats, jamais par supposition.

## Sources officielles consultées

- [Audinate - Presets](https://dev.audinate.com/GA/dante-controller/userguide/webhelp/content/presets.htm)
- [Audinate - Applying presets](https://dev.audinate.com/GA/dante-controller/userguide/webhelp/content/applying_presets.htm)
- [Audinate - Preset elements](https://dev.audinate.com/GA/dante-controller/userguide/webhelp/content/preset_elements_-_configuration_parameters.htm)
- [Dante Preset Creator](https://www.getdante.com/products/software-essentials/dante-preset-creator/)
- [Preset Creator support](https://support.getdante.com/hc/en-gb/articles/5767408604191-Dante-Preset-Creator)
