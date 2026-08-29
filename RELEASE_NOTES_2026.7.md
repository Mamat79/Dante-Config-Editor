# DCE 2026.7

## Français

Cette version étend la banque de machines fournie sans modifier le moteur XML,
le format des projets ni le système de licence.

### Nouveautés

- 77 modèles physiques dans `DCE Community`, plus 2 rôles génériques ;
- 20 nouveaux profils : Yamaha RIVAGE PM10, PM7, PM5 et PM3 avec HY144-D,
  HY144-D-SRC, Rio3224-D3, Rio1608-D3, Tio1608-D2, RUio16-D, DME7,
  Focusrite RedNet A16R MkII, D16R MkII, A8R, HD32R, D64R, MP8R, X2P et AM2,
  ainsi que Neutrik NA-2I2O-DLINE ;
- notices française et anglaise régénérées en édition 2026.7 ;
- liens de téléchargement et sommes SHA-256 actualisés.

Les 57 modèles déjà testés sur du matériel conservent le statut
`HardwareTested`. Les 20 nouveaux profils sont marqués
`StructurallyValidated` : leurs capacités proviennent des spécifications
fabricant, mais ils n'ont pas tous été contrôlés sur l'appareil physique.

### Validation

- 487 tests Core/Windows réussis ;
- 22 tests Avalonia/macOS sans écran réussis ;
- 5 tests du Worker de licence réussis ;
- build Windows Release sans avertissement ;
- installation locale et lancement visuel de l'exécutable 2026.7 réussis ;
- 77 identifiants de modèles uniques, sans référence manquante.

Les formats de licence `DCEP1` et `DCEF1`, les licences V2, le produit signé
`DCE` et le stockage local restent inchangés. Toutes les licences existantes
continuent de fonctionner. Le tarif reste un achat unique de 29 EUR TTC via
Stripe et DCE reste intégralement utilisable après les 30 jours gratuits.

L'installateur Windows n'est pas signé Authenticode. Les paquets macOS 2026.7
ne sont pas publiés dans cette Release : ils doivent être reconstruits sur un
runner Apple, actuellement indisponible, avant d'être proposés.

---

## English

This release expands the bundled device bank without changing the XML engine,
project format, or licensing system.

### What's new

- 77 physical templates in `DCE Community`, plus 2 generic roles;
- 20 new profiles: Yamaha RIVAGE PM10, PM7, PM5, and PM3 with HY144-D,
  HY144-D-SRC, Rio3224-D3, Rio1608-D3, Tio1608-D2, RUio16-D, DME7,
  Focusrite RedNet A16R MkII, D16R MkII, A8R, HD32R, D64R, MP8R, X2P, and AM2,
  plus Neutrik NA-2I2O-DLINE;
- regenerated French and English 2026.7 manuals;
- updated direct download links and SHA-256 checksums.

The 57 templates already tested on hardware retain the `HardwareTested`
status. The 20 new profiles are marked `StructurallyValidated`: their
capacities were checked against manufacturer specifications, but not on every
physical device.

### Validation

- 487 Core/Windows tests passed;
- 22 headless Avalonia/macOS tests passed;
- 5 license Worker tests passed;
- warning-free Windows Release build;
- successful local installation and visual launch of the 2026.7 executable;
- 77 unique template identifiers with no missing references.

The `DCEP1` and `DCEF1` formats, V2 licenses, signed `DCE` product, and stable
local storage remain unchanged. Every existing license continues to work. The
price remains a one-time EUR 29 purchase including French VAT through Stripe,
and DCE remains fully usable after the 30-day free period.

The Windows installer is not Authenticode-signed. 2026.7 macOS packages are not
included in this Release: they must be rebuilt on an Apple runner, currently
unavailable, before publication.
