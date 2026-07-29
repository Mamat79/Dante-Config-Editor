# AGENTS.md - Dante Config Editor

Ce fichier s'applique à tout le dépôt.

## Mission

Dante Config Editor (DCE) est un éditeur local et hors ligne de presets XML
pour Dante Controller. Il ne pilote pas le réseau, n'utilise pas de SDK/API
Audinate et ne doit jamais prétendre créer un appareil Dante réel.

La priorité absolue est de préserver un XML importable dans Dante Controller.
Une fonctionnalité pratique ne justifie jamais une régression de compatibilité.

## Avant toute modification

1. Vérifier la racine avec `git rev-parse --show-toplevel`.
2. Vérifier la branche et `git status --short --branch`.
3. Ne jamais supprimer ni annuler une modification locale inconnue.
4. Lire le code voisin et réutiliser les services existants.
5. Ne jamais utiliser un XML de production comme fixture versionnée.

Ne changez pas le numéro de version, le tag ou la Release sans demande
explicite. Une correction de la 2026.1 remplace la 2026.1 lorsqu'il est demandé
de conserver ce numéro.

## Invariants XML

- Le `XDocument` chargé reste la source de vérité. Effectuer des mutations
  ciblées plutôt que reconstruire le preset avec un modèle simplifié.
- Préserver les namespaces, l'ordre utile, les attributs, les nœuds inconnus,
  l'encodage, les valeurs non gérées et les références croisées.
- Bloquer par défaut un chemin technique inconnu ou une identité incohérente.
- Ne jamais inventer `instance_id`, `device_id`, Dante ID, `mediaType` ou EUI-64.
- Un rôle générique dupliqué, importé ou issu d'une banque ne porte pas
  d'identité matérielle. Dante Controller affecte ensuite le rôle à un appareil
  compatible.
- Lors d'une fusion, la même paire `device_id` / `process_id` doit soit
  réutiliser le rôle existant, soit produire après renommage un rôle générique
  sans identité matérielle.
- Ne pas créer implicitement une balise technique absente (`redundancy`,
  `preferred_master`, `samplerate`, `encoding`, `unicast_latency`,
  `ipv4_address`, etc.). Désactiver l'action et expliquer la limite.
- Pour plusieurs interfaces IPv4, ne modifier que l'interface principale
  reconnue. Ne pas toucher implicitement au DNS, à la passerelle ou à une
  interface secondaire.
- Toute sauvegarde doit rester temporaire, relue, validée, sauvegardée puis
  remplacée de façon sûre. Un échec ne doit pas altérer la destination.

## Architecture

- `Models/` contient encore le modèle XML historique. Réduire progressivement
  ses responsabilités sans réécriture générale.
- `src/DanteConfigEditor.Domain/` porte les concepts métier indépendants.
- `src/DanteConfigEditor.DanteXml/` porte les règles liées au format Dante XML.
- `src/DanteConfigEditor.Application/` porte les cas d'usage et commandes.
- `src/DanteConfigEditor.Infrastructure/` porte fichiers, stockage et services
  externes locaux.
- `MainWindow.xaml.cs` et `src/DanteConfigEditor.Mac/` doivent orchestrer
  l'interface, pas dupliquer la logique XML.
- Toute opération sur plusieurs éléments doit être groupée et ne provoquer
  qu'une reconstruction du modèle lorsque c'est possible.

Ajoutez des commentaires en français seulement lorsqu'ils expliquent une règle
non évidente ou un garde-fou. Évitez les commentaires qui répètent le code.

## Interface et traductions

- Maintenir les mêmes parcours fonctionnels sous Windows et macOS.
- Vérifier thèmes clair/sombre, français/anglais, clavier, focus et mise à
  l'échelle Windows 125 %, 150 % et 200 %.
- Le premier lancement utilise le thème clair. Les lancements suivants
  restaurent la dernière langue et le dernier thème.
- Les réglages importants démarrent visibles. Chaque panneau repliable garde
  une flèche accessible dans les deux états.
- Une commande indisponible doit être désactivée avec une explication et une
  infobulle complète, traduite et non tronquée.
- Toute nouvelle chaîne visible doit être ajoutée en français et en anglais.
- Atomic Bomb est le seul écran volontairement ludique. Le reste de
  l'application doit rester dense, lisible et efficace.

## Banques de machines

- Une banque contient des modèles de rôles assainis, pas des appareils réels.
- Ne jamais publier identité matérielle, IP, interface, flow, subscription ou
  donnée de production dans une banque fournie.
- Ne jamais remplacer la banque personnelle pendant une installation ou une
  mise à jour.
- Les modèles fournis restent en lecture seule et peuvent être copiés dans la
  banque personnelle.
- Les images autorisées sont copiées dans le dossier du modèle ; ne pas
  conserver un chemin externe fragile.

## Tests obligatoires

Exécuter au minimum avant un commit :

```powershell
dotnet restore .\DanteConfigEditorV3.csproj
dotnet test .\tests\DanteConfigEditorV3.Tests\DanteConfigEditorV3.Tests.csproj -c Release
dotnet test .\tests\DanteConfigEditor.Mac.Tests\DanteConfigEditor.Mac.Tests.csproj -c Release
dotnet build .\DanteConfigEditorV3.csproj -c Release
```

Pour toute mutation XML, ajouter un test de non-régression qui sauvegarde puis
recharge le résultat. Couvrir les références, les identités, les namespaces,
les balises inconnues et les erreurs transactionnelles concernées.

Ne jamais écrire « compatible » sur la seule base d'un build réussi. Distinguer
les tests automatisés, la comparaison structurelle et l'import réellement
effectué dans Dante Controller.

## Documentation et PDF

- Conserver français et anglais synchronisés.
- Modifier `docs/generate_guides.py`, régénérer les quatre PDF, puis rendre les
  pages en images pour contrôler coupures, chevauchements et caractères.
- Mettre à jour `README.md`, `README_EN.md`, `RELEASE_NOTES*.md` et
  `CHANGELOG.md` lorsqu'un comportement utilisateur change.
- Les notices doivent expliquer le parcours complet, pas seulement nommer les
  boutons.

## Distribution

- Windows doit être publié en autonome avec le runtime .NET 8 et les banques
  fournies. Construire avec `installer/build_installer.ps1`.
- Lancer réellement l'EXE publié ou installé avant publication.
- Les builds macOS Apple Silicon et Intel sont produits par les workflows
  GitHub sur macOS ; ne pas présenter un paquet comme testé sur Mac sans preuve.
- Chaque Release garde son tag propre. Ne jamais modifier une ancienne Release
  historique sauf demande explicite.
- Lors du remplacement d'une Release portant le même numéro, reconstruire,
  recalculer les SHA-256 et vérifier que chaque fichier correspond au nouveau
  commit.
- Une publication publique DCE implique aussi la mise à jour du sujet
  Audiofanzine en français et du sujet Gearspace en anglais puis français,
  lorsque les accès sont disponibles.

## Git

- Commits séparés par sujet : moteur/tests, interface, documentation,
  distribution.
- Ne pas commiter `bin/`, `obj/`, `dist/`, temporaires, journaux, XML réels,
  banques personnelles ou secrets.
- Ne pousser qu'après réussite des tests requis.
- Finir avec un dépôt propre et indiquer clairement les contrôles qui n'ont pas
  pu être réalisés.
