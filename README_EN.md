# Dante Config Editor 2026.1

[Documentation française](README.md)

A local, offline Dante Controller preset XML editor developed by Mamat with
the assistance of development agents.

> **Status: official release published from the `main` branch.**
> DCE is an unofficial third-party tool and is not affiliated with Audinate.
> It does not control a live Dante network and uses no Audinate SDK or API.
> Work on a copy and review the final XML in Dante Controller before operation.

The maintainer successfully imported 2026.1 output into Dante Controller.
Structural and semantic tests complement those real imports. Reviewing the
final file remains recommended for each new preset structure before operation.

## Documentation

- [Complete 2026.1.1 visual guide - English (MKV, 11 min 22 sec)](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.1.1/dce-2026-1-guide-visual-en.mkv)
  Both the recorded interface and the selectable subtitle track are in English.
- [Notice visuelle complète 2026.1.1 - français (MKV, 11 min 22 s)](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.1.1/dce-2026-1-guide-visuel-fr.mkv)
  The selectable French subtitle track can be disabled in the media player.
- [2026.1 presentation video - English](docs/media/dce-2026-1-presentation-en.mp4)
- [Vidéo de présentation 2026.1 - français](docs/media/dce-2026-1-presentation-fr.mp4)
- [English quick start](docs/QuickStart_DanteConfigEditorV3_EN.pdf)
- [Full English guide](docs/Notice_DanteConfigEditorV3_EN.pdf)
- [Démarrage rapide FR](docs/QuickStart_DanteConfigEditorV3_FR.pdf)
- [Notice complète FR](docs/Notice_DanteConfigEditorV3_FR.pdf)
- [2026.1 architecture](docs/2026.1/ARCHITECTURE_2026_1.md)
- [`.dceproj` format](docs/2026.1/DCEPROJECT_FORMAT.md)
- [Device-bank format](docs/2026.1/DEVICE_LIBRARY_FORMAT.md)
- [Migration from V3.6](docs/2026.1/MIGRATION_V3_6_TO_2026_1.md)
- [Performance report](docs/2026.1/PERFORMANCE_REPORT.md)
- [Dante Controller checklist](docs/2026.1/DANTE_CONTROLLER_MANUAL_VALIDATION.md)
- [Known limitations](KNOWN_LIMITATIONS.md)

## Why DCE exists

DCE started from a field need: review a Dante configuration quickly without
opening every Dante Controller page in sequence. It brings devices, latency,
sample rates, encoding, network modes, Preferred Masters, IP configuration,
and subscriptions into one application, then allows recognized values to be
corrected.

The second need was renaming on an already-patched network. When a device or TX
channel is renamed, DCE updates recognized XML references to preserve the
patch. DCE also makes it possible to prepare, merge, document, and review a
project while disconnected from the Dante network.

The original application was written manually as a small XML editor. Current
development agents then helped accelerate safety work, tests, installers,
documentation, and progressive engine separation. Mamat remains responsible
for field requirements and functional decisions.

## What 2026.1 adds

- progressive Domain, DanteXml, Application, and Infrastructure layers;
- central project session and transactional business commands;
- bounded Undo/Redo and readable history;
- versioned `.dceproj` workspace package, separate from Dante XML;
- capability-aware XML profiles and read-only mode;
- consistent Windows and macOS side navigation with contextual inspector;
- one Patch workspace with Matrix, Easy patch, and Rx-to-Tx list views;
- interactive synoptic synchronized with selection and subscriptions;
- searchable, navigable, and exportable Validation Center;
- device-bank format 2 and non-destructive V3.6 migration;
- isolated 2026.1 local profile;
- indexes and caches invalidated on every XML mutation;
- wider synthetic XML corpus and 10/50/200-device benchmarks.

The macOS build now follows the same functional organization as Windows, with
the same main workflows, shared engine, 2026.1 profile, and separate package
identity. A few controls retain their native platform rendering.

## Three different file concepts

### Dante XML

XML remains the file intended for Dante Controller. DCE performs targeted
changes to the original document to preserve unknown nodes, attributes,
namespaces, values, and extensions. Saving uses a temporary file, reload,
validation, backup, and safe destination replacement.

### `.dceproj` project

A `.dceproj` file is a DCE workspace container. It can hold the Dante XML,
project name, layout, annotations, history, bank references, and DCE assets.
Never import it directly into Dante Controller: export its Dante XML first.

### Device bank

A bank holds reusable and shareable templates. Insertion creates an independent
instance and does not bind a project to the source template. Hardware identity,
IP configuration, flows, and subscriptions are not copied by default.
`DCE Generic Roles 2026.1` and `DCE Community Devices 2026.1` are bundled with
the application. Together they provide 45 templates, including 43 illustrated,
sanitized community templates. The Devices page lets users select a bank, add
a template to the project, or manage its contents. The window now combines the
personal and bundled banks in one deduplicated list. A selector can isolate one
bank, and the `Bank` column identifies each template's source. Bundled
templates remain read-only and may be duplicated into the personal bank.
Official banks are managed in `Documents/Dante Config Editor/Included Machine Banks`.
The `Update banks` button checks GitHub, verifies SHA-256, backs up the previous
copy, and installs the new one without ever replacing the personal bank.
When adding devices, the quantity defaults to `1` and may be increased up to
`100`. DCE previews the generated names (`Name`, `Name-2`, `Name-3`, and so on),
validates the complete batch before changing the XML, and keeps the bank open
for subsequent additions.

At startup, DCE also checks silently for a newer GitHub Release. When an update
is available, it offers to download and launch the verified installer. A manual
check remains available under `Help > Check for updates`.

### Merging two XML files and role identity

`Add XML to project` keeps the first XML as the baseline and imports compatible
roles from the second file. DCE compares both names and the technical
`device_id` / `process_id` pair.

- **Import unique only** reuses the role already present when the same
  technical identity is found. Imported subscriptions are redirected to its
  current name.
- **Automatic or manual rename** retains a second independent role. DCE makes
  it generic by removing hardware identity, network interfaces, multicast
  flows, and the Preferred Master state inherited from the other project.
- DCE never creates a fake `device_id`. Dante Controller can then assign the
  role to the original device or another compatible device.

This distinction merges two installations without producing two roles with
the same hardware identity and without losing recognized patch references.

## Main features

- open, inspect, compare, and merge XML files;
- direct or series device, RX, and TX renaming;
- recognized subscription updates after TX renaming;
- table, selection, matrix, drag, and 1:1 patch workflows;
- cross-navigation in the matrix to locate an Rx source or select one of a Tx
  channel's destinations;
- detachable matrix in a large window retaining Rx, Tx, FLIP, 1:1, and zoom;
- FLIP of the currently displayed RX/TX roles in Easy Patch;
- targeted latency, audio format, network, and Preferred Master changes;
- profiles and global actions on an unlocked selection;
- cautious role deletion and duplication;
- transactional insertion from a device bank;
- offline creation of a minimal XML 3.0.0 project;
- JSON, CSV, DMT XLSX/ODS, A&H dLive/Avantis, and Yamaha CL/QL label exchange;
- TXT/PDF reports, patchbooks, and before/after comparison;
- SVG/PDF synoptic with locations and grouped cables;
- automatic recovery and safe saving;
- French/English UI and light/dark themes;
- light theme on first launch, then restoration of the last selected theme and
  language;
- standard top menu for quick access to File, Edit, Devices, View, Tools, and
  Help commands;
- fully offline Atomic Bomb training tool with a safety key that automatically
  opens the cover, then enables ARM, LOCK, and FIRE.

## Importing and exporting labels

DCE exchanges labels through generic JSON/CSV, DMT XLSX/ODS workbooks for
dLive and Avantis, native Allen & Heath dLive and Avantis CSV files, and
Yamaha CL/QL packages. Native templates are bundled with the application, and
every export creates a new file without modifying its source template.

The DMT integration was initially designed with
[dLive MIDI Tools](https://github.com/togrupe/dlive-midi-tools). It remains an
offline file exchange: DCE does not communicate directly with DMT or a console.

## XML safety

DCE blocks saving by default when the guard detects an unauthorized technical
mutation. Recognized sensitive fields and identities are tracked by stable
identity, not only by name. Unknown tags are preserved; an unknown fundamental
structure causes limited editing or read-only mode.

For hardware-dependent settings, the element must already exist in the loaded
role before DCE treats that capability as editable. DCE therefore does not
create `redundancy`, `preferred_master`, `samplerate`, `encoding`,
`unicast_latency`, or `ipv4_address` when they are absent. The command is
disabled or rejected with a precise explanation; a global action skips
unsupported devices and reports how many were excluded.

Automated coverage includes:

- open, unchanged save, and reopen cycles;
- semantic XML comparison;
- default namespaces and unknown tags;
- Unicode and element ordering;
- local `.` subscriptions and missing sources/channels;
- multiple IPv4 interfaces and secondary-interface preservation;
- rejection of missing technical-setting elements and read-only integration
  checks against a local corpus of 11 XML files;
- rename, patch, merge, recovery, duplication, and bank workflows;
- synthetic 10, 50, and 200-device presets with 64 TX and 64 RX each.

These tests do not replace final import in Dante Controller.

## Install DCE 2026.1.1

The [DCE 2026.1.1 Release](https://github.com/Mamat79/Dante-Config-Editor/releases/tag/v2026.1.1)
contains the Windows and macOS installers, guides, videos, and bundled banks.
GitHub Actions also retains branch artifacts after a successful run.

### Windows 11 x64

Artifact: `DCE-2026.1.1-Windows-Installer`

File: `DanteConfigEditor2026_1_1_Installer.exe`

The self-contained installer includes .NET 8 and the French/English guides.
Its default folder is
`C:\Program Files\Dante Config Editor 2026.1\`. Its AppId, shortcuts, and
`%LOCALAPPDATA%\DanteConfigEditor2026.1` profile are separate from V3.6.
Uninstalling DCE 2026.1 does not remove XML files, projects, banks, or the V3.6
profile.

### macOS

- `DanteConfigEditor2026_1_1_macOS_AppleSilicon.dmg`
- `DanteConfigEditor2026_1_1_macOS_Intel.dmg`

The .NET runtime is bundled. Packages are ad hoc signed but not notarized by
Apple, so the first launch may require an explicit Open action from Finder.

## Build and test

The .NET 8 SDK is required. Inno Setup 6 is also required for the Windows
installer. DMG packages must be built on macOS.

```powershell
dotnet restore .\DanteConfigEditorV3.csproj
dotnet test .\tests\DanteConfigEditorV3.Tests\DanteConfigEditorV3.Tests.csproj -c Release
dotnet test .\tests\DanteConfigEditor.Mac.Tests\DanteConfigEditor.Mac.Tests.csproj -c Release
dotnet build .\DanteConfigEditorV3.csproj -c Release
dotnet publish .\DanteConfigEditorV3.csproj -c Release -r win-x64 --self-contained true
.\installer\build_installer.ps1
```

## Limitations

- no live Dante network control;
- no Audinate SDK or API;
- project creation is currently limited to the supported XML 3.0.0 profile;
- duplicated or bank-created roles have no real hardware identity;
- unknown XML profiles are limited or read-only;
- minor native rendering differences remain between Windows and macOS;
- Windows installer is not Authenticode signed;
- DMGs are not notarized;
- a Dante Controller review is recommended for each important new preset
  structure.

## Support DCE

Dante Config Editor remains completely free, and every feature is available
without a contribution. Optional ways to support the project are described in
[docs/SUPPORT_DCE.md](docs/SUPPORT_DCE.md).

## Acknowledgements

Thanks to [Tobi / @togrupe](https://github.com/togrupe), author of
[dLive MIDI Tools](https://github.com/togrupe/dlive-midi-tools), for his
feedback, patch-workflow ideas, and help with DMT label exchange.

Thanks to Charles Bouticourt for the `Atomic Bomb` training-function idea.

---

**By Mamat**<br>
*et ses agents*<br>
`-------[]--`
