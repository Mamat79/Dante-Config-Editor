# Version macOS - 2026.1.1

La version macOS utilise Avalonia et compile le même moteur XML, les mêmes
services de projet, la même validation et les mêmes migrations que la version
Windows. L'interface reste adaptée à macOS et n'est pas une reproduction pixel
par pixel du shell WPF Windows.

## Paquets utilisateurs

Deux DMG autonomes sont produits :

- `DanteConfigEditor2026_1_1_macOS_AppleSilicon.dmg` pour les Mac Apple
  Silicon ;
- `DanteConfigEditor2026_1_1_macOS_Intel.dmg` pour les Mac Intel 64 bits.

Le runtime .NET 8, les notices FR/EN et les banques publiques assainies sont
inclus. L'utilisateur ouvre le DMG puis glisse
`Dante Config Editor 2026.1` vers `Applications`.

L'identifiant du bundle est
`fr.mamat.danteconfigeditor.y2026-1`. Il reste distinct de la V3.6.

## Construction vérifiée

Depuis la racine du dépôt, sur macOS :

```bash
bash packaging/macos/build-macos.sh osx-arm64
bash packaging/macos/build-macos.sh osx-x64
```

Les DMG et leurs sommes SHA-256 sont créés dans `dist/macos`.

Le workflow macOS du commit `7a50b3c` a réussi :

- 364 tests du moteur partagé ;
- 20 tests Avalonia sans écran ;
- publication autonome Apple Silicon et Intel ;
- création et vérification des deux DMG.

Artefacts vérifiés après téléchargement :

| Architecture | Taille | SHA-256 |
|---|---:|---|
| Apple Silicon | 52 850 639 octets | `b1774f3eb710853b289242b3a090544438ff1b5d2ba5cfdd51fb03f9223cd206` |
| Intel | 54 282 491 octets | `0e4c9b52930ac8191b77270d5cab487cf70e970b326c0c5a927410806c9097a6` |

## Signature et Gatekeeper

Le script utilise `codesign --sign -`, donc une signature ad hoc. Les paquets
ne sont ni signés avec un certificat Apple Developer ID ni notariés. Au premier
lancement, macOS peut demander un clic droit sur l'application, puis
`Ouvrir`.

Une distribution sans cet avertissement exige un compte Apple Developer, une
signature `Developer ID Application`, le hardened runtime et une notarisation
Apple.

## Limites de validation

- Aucun Mac physique n'a été utilisé dans ce cycle.
- Les tests Avalonia sans écran valident la structure, les commandes et les
  traductions, mais ne remplacent pas un essai VoiceOver.
- La compatibilité XML automatisée est partagée avec Windows. L'import final
  d'une nouvelle structure de preset doit néanmoins être vérifié dans Dante
  Controller.
