# Dante Config Editor V3.6 - development branch

[Français](README.md) | **English**

V3.6 is developed and tested on the `v3.6` branch from V3.5. Official V3.4.2 remains published on `main`.

**Stable version: [V3.4.2 Release for Windows and macOS](https://github.com/Mamat79/DanteConfigEditorV3/releases/tag/v3.4.2)**

> **Status: V3.6 in development. Unofficial third-party tool, not affiliated with Audinate.**
> This branch may still contain bugs. Always work on a copy and validate generated XML with official Dante tools.

**V3.6 documentation:**
[full English guide](docs/Notice_DanteConfigEditorV3_EN.pdf) ·
[notice complète FR](docs/Notice_DanteConfigEditorV3_FR.pdf)

**Historical V3.5 presentations:**
[English video](docs/media/dce-v35-presentation-en.mp4) ·
[vidéo FR](docs/media/dce-v35-presentation-fr.mp4)

> **Importing and exporting labels through JSON, CSV, DMT XLSX/ODS for dLive and Avantis, A&H CSV, and Yamaha CL/QL ZIP/CSV.** Native templates are bundled with the application, so no external template file is required for export.

## What is new in V3.6

- documented XML fidelity, performance, and risk audit in [AUDIT_V3_6.md](AUDIT_V3_6.md);
- detailed implementation, test, and limitation report in [V3_6_IMPLEMENTATION_REPORT.md](V3_6_IMPLEMENTATION_REPORT.md);
- stronger validation of identities, references, channels, subscriptions, namespaces, and node additions;
- cautious device duplication as a generic role without copying hardware identifiers;
- versioned, shareable machine bank with metadata, labels, optional image, search, import/export, and a GitHub catalog;
- transactional insertion of an independent instance from the bank;
- experimental creation of a minimal 3.0.0 project, which must be validated by an actual Dante Controller import;
- technical logs available from the application;
- 272 Core/Windows tests and 20 headless Mac tests.

Duplication and the bank do not create real Dante hardware. They create generic preset roles without `instance_id` or `device_id`. No V3.6 output has yet been imported into Dante Controller, so field compatibility is not claimed as guaranteed.

## Origin and agent-assisted development

Dante Config Editor began as an attempt to provide what I personally was missing in Dante Controller. It started as a small personal application written manually to solve a practical field need: checking a Dante configuration quickly without opening each page of the application in turn. The goal was to provide one overview of devices, latency, sample rates, network modes, Preferred Master state, IP addresses, etc., with the ability to correct preset values when needed.

Renaming on an already patched network was another recurring problem. Changing a device name or TX channel names can require revisiting the affected subscriptions and rebuilding part of the patch. The editor was therefore designed to update recognized XML references during renaming and preserve the patch as far as the preset structure allows.

Finally, because Dante Controller has no offline workflow, this software attempts to meet the need for fast, consolidated preparation. Reviewing, editing, merging and preparing a preset without being connected to the Dante network therefore became one of the project's central goals.

Modern development agents then enabled a much larger step forward: safer saving, regression tests, a bilingual interface, self-contained installers, a macOS build, reports and more advanced patching tools. Product needs, functional decisions and field validation remain directed by Mamat; the agents contribute to analysis, implementation, testing and documentation.

## Shareable device banks

The `Device bank` window exports a complete bank as a verified
`*.dce-bank.zip` archive and imports a downloaded bank into a new or empty
folder. Import verifies the manifest, SHA-256 hashes, and every template before
switching banks; existing files are never replaced.

The `GitHub banks` button opens the
[public V3.6 bank catalog](machine-banks/README.md). The included
`DCE Generic Roles 3.6` bank provides generic 8x8 and 32x32 roles for testing
and training. They do not represent real hardware and contain no hardware
identity, IP address, or subscription.

The catalog and new installers also provide `DCE Community Devices 3.6`, an
illustrated community bank containing Yamaha QL1 and Rio1608-D2, Fohhn
DI4.1000, Lake LM 44, and RME Digiface Dante templates.

The Windows installer separately asks for the active bank folder and the
folder used for included banks. `DCE Generic Roles 3.6` and
`DCE Community Devices 3.6` can be selected independently. During an upgrade
the installer reuses the configured path, never changes an existing bank, and
selects a new folder name if an included bank is already present.

## Importing and exporting labels

The `Import / Export` workspace contains channel-label exchange for one or several devices. TX or RX labels can be exported to JSON or CSV, imported with explicit device and range mapping, and checked row by row before application. TX renames still update recognized XML subscriptions.

Generic CSV is selected by default and only asks for an output name and destination folder. It is intended for Dante Config Editor and generic data exchange; it must not be imported directly into dLive Director. Native DMT, A&H, and Yamaha formats use the bundled dLive, Avantis, CL, or QL template and only ask for the new output file name.

JSON and CSV remain generic exchange formats. Native profiles also support these console and tool formats:

- **DMT → Dante Config Editor**: directly read the `Channels` sheet from a DMT XLSX or ODS workbook, then map its labels to the TX or RX channels of one or several Dante devices.
- **DMT 2.14.0-RC1 → Dante Config Editor**: JSON and CSV exports from `feature/add-json-export` are covered by fixtures that exactly reproduce the DMT exporters at commit `3c34052`.
- **Dante Config Editor → DMT**: directly create a dLive/Avantis XLSX or ODS workbook from one of four bundled DMT templates, disabling rows absent from the selection.
- **DMT project**: [togrupe/dlive-midi-tools](https://github.com/togrupe/dlive-midi-tools).
- **Allen & Heath dLive / Avantis**: read an existing console CSV or directly create a new native CSV from the bundled template; only `Input` name fields are changed.
- **Yamaha CL / QL**: read a ZIP package or an individual `InName.csv`, and directly create a complete ZIP from the bundled template; the eight other CSV files remain unchanged.

Bundled templates are never modified: every export creates a new file. A device with RX but no TX automatically switches to RX, and devices without channels in the selected direction cannot be selected. Every eight-character ASCII adaptation is visible in the preview and must be enabled explicitly; generic JSON and CSV retain full Unicode labels. This is an offline file bridge, not a direct or real-time connection between applications.

## Visual synoptic

V3.2 adds a colored synoptic under `Import / Export > Synoptic`. Each device can be assigned to a physical location, shown or hidden with one click, and reordered. Previously entered locations remain available in a list. Consecutive subscriptions between two devices are compressed into one cable, for example `TX 1-32 to RX 1-32`, while dense connections are distributed across distinct connection ports.

The synoptic can be opened in a separate window whose zoom always preserves its proportions, then exported as vector SVG or PDF. Neither export nor the local layout sidecar modifies the loaded Dante XML.

Locations and presentation choices are saved in a separate local sidecar file. They are never inserted into Dante XML. The SVG export contains devices, numbered cables, and a detailed legend and can be opened in a browser, printed, or included in technical documentation.

## User guides

- **[Read the full English guide (PDF)](docs/Notice_DanteConfigEditorV3_EN.pdf)** or the [quick start](docs/QuickStart_DanteConfigEditorV3_EN.pdf).
- **[Lire la notice complète en français (PDF)](docs/Notice_DanteConfigEditorV3_FR.pdf)** ou le [démarrage rapide](docs/QuickStart_DanteConfigEditorV3_FR.pdf).

The screenshots and manuals use only a synthetic anonymized preset. They contain no production device name, file or path.

## Presentation videos

- **[English presentation of DCE v3.5](docs/media/dce-v35-presentation-en.mp4)**
- **[Présentation française de DCE v3.5](docs/media/dce-v35-presentation-fr.mp4)**

Each video lasts 55 seconds, with no voice-over or audio track, and uses text burned directly into the image. Separate `.srt` files and SHA-256 checksums are provided under `docs/media`. Every screen uses a synthetic anonymized preset.

## What the application does

- Opens Dante XML configuration files offline.
- Displays devices, TX/RX channels, latency, network mode and Preferred Master state.
- Renames devices and TX/RX channels, including channel ranges.
- Imports and exports channel labels for one or several devices through JSON, CSV, DMT XLSX/ODS, A&H dLive/Avantis CSV, or Yamaha CL/QL packages, with range mapping and preview.
- Groups label exchange, reports, patchbooks, and the synoptic in one `Import / Export` tab with three clear subtabs.
- Generates a colored SVG synoptic with locations, optional devices, custom ordering, and compressed consecutive cable ranges.
- Keeps synoptic layout information outside Dante XML in a separate local file.
- Updates recognized RX subscriptions when a referenced TX channel is renamed.
- Resets channel names.
- Deletes a device and removes recognized subscriptions that reference it.
- Duplicates a device as an independent generic role, with optional retention of labels and safe settings.
- Saves, searches, edits, and shares templates or complete banks, with access to the GitHub catalog.
- Adds an independent project instance from the bank.
- Experimentally creates an empty or bank-seeded minimal 3.0.0 project.
- Merges devices from a second XML file into the open project.
- Handles duplicate device names with manual or automatic renaming during merge.
- Edits supported audio and network values exposed by recognized XML structures.
- Provides the classic `Patch` view and the Windows `Easy patch` workspace.
- Supports immediate patching, strict ranges, optional warnings for already-patched Rx channels, and explicit conflict handling.
- Displays a compact interactive TX/RX patch matrix with horizontal, vertical and diagonal gestures.
- In `Easy patch`, provides a highly visible `FLIP TX ⇄ RX` button, one-to-one patching from either `Selection and range` or the matrix itself, and Tab/Shift+Tab navigation while renaming.
- In the `Easy patch` matrix, clicking a vertical Tx label opens direct rename; Enter validates, Tab/Shift+Tab navigate, and Escape cancels.
- The fill handle appears only when a name ends with a number and preserves leading zeroes (`Mic 04` becomes `Mic 05`, `Mic 06`, and so on).
- Opens a device details window to edit formats, IP data, channel names and RX patches.
- Applies global actions only to the selected or locked target scope.
- Resets all RX patches, TX references, or both for a device.
- Saves through a validated temporary file and protects an existing destination with a backup.
- Blocks unexpected changes to protected Dante XML areas.
- Preserves default XML namespaces and recognized unknown values.
- Provides French and English interfaces, quick starts and full PDF manuals.
- Exports TXT/PDF reports and read-only patchbooks.
- Uses `Atomic Bomb` to generate a deliberately scrambled troubleshooting preset after three confirmations, with independent exclusions for every category.
- Displays file-health warnings, compatibility information and a simple TX-to-RX topology.
- Provides search, recent files, undo, recovery, dark theme and light theme.

## Important limitations

- The application does not control a live Dante network.
- It does not use an Audinate SDK or API.
- It only works on offline XML files.
- It does not bypass Audinate protections or reimplement a proprietary protocol.
- Compatibility depends on the actual structure of the supplied preset.
- Generic roles carry no hardware identity and must be validated in Dante Controller.
- New project creation is experimental and is not an import guarantee.
- Some subscriptions may not be detected if their XML structure is not currently recognized.
- `subscribed_device="."` is treated as a local source on the RX device.
- A missing TX device is reported as a warning because a preset may be partial.
- Dante IDs are preserved; the UI label is `Dante Id`, while the XML attribute remains `danteId`.
- Only a successful import into Dante Controller can provide final compatibility confirmation.

## Download and install

The [V3.4.2 GitHub Release](https://github.com/Mamat79/DanteConfigEditorV3/releases/tag/v3.4.2) remains stable. V3.6 development packages are generated by the `Windows CI` and `macOS CI` runs for branch `v3.6`.

### Windows x64

Download artifact `DCE-v3.6-Windows-Installer`, containing `DanteConfigEditorV3_6_Installer.exe` and its SHA-256 checksum.

The self-contained installer includes the required .NET 8 runtime, French and English documentation, Start menu and desktop shortcuts, destination selection, and clean uninstall support. A fresh install defaults to `C:\Program Files\Dante Config Editor V3.6\`. It upgrades the V3.5 development line in place, removes obsolete V3.5 shortcuts, preserves local working data, and leaves stable V3.4.2 untouched.

The wizard lets users choose the active device-bank folder and the folder for
included banks. Installing `DCE Generic Roles 3.6` and
`DCE Community Devices 3.6` is proposed by default on a first installation;
each bank remains optional. An upgrade keeps the existing path and never
replaces a bank.

### macOS

- `DanteConfigEditorV3_6_macOS_AppleSilicon.dmg` supports Apple Silicon Macs.
- `DanteConfigEditorV3_6_macOS_Intel.dmg` supports 64-bit Intel Macs.

Each DMG also contains a `Machine Banks` folder with both verified bank
archives, ready to import from the Device bank window.

Open the DMG and drag `Dante Config Editor V3.6` into `Applications`. The .NET runtime and both language manuals are included. Its separate bundle lets it coexist with V3.4.2.

The macOS builds are ad hoc signed but are not notarized with an Apple Developer account. On first launch, you may need to right-click the application, choose `Open`, and confirm. Verify the published SHA-256 checksum before installation.

## Distributed version

- `main` contains the official V3.4 source for Windows and macOS.
- `v3.6` contains the development version and its Windows/macOS workflows; it does not replace the stable release yet.
- Immutable tag `v3.4.2` identifies the source of the official Release currently marked `Latest`.
- Every version uses a separate immutable tag under the [release policy](RELEASE_POLICY.md).
- Each Release contains the files built for its own tagged source.
- Functional history remains available through the commits and `CHANGELOG_V3.md`.

## Quick start

1. Launch the application.
2. Select `Open XML` and choose a copy of a Dante configuration file.
3. Review detected devices and warnings.
4. Make the required changes.
5. In `Easy patch`, choose RX and TX devices, then click or drag in the matrix: each crosspoint is applied immediately. `PATCH 1:1` directly applies a series.
6. Keep `Warn me when the Rx channel is already patched` selected to confirm replacements, or clear it to replace without that warning.
7. Use `Undo` to revert a patch operation when needed.
8. Save under a new name.
9. Import and validate the result in the appropriate official Dante tool before production use.

## Atomic Bomb: troubleshooting exercise

`Atomic Bomb`, placed in its own tab after `Safety and log`, prepares a deliberately disordered network preset for training. Its **Horrible experience generator (but educational)** panel lets users clear every category they want to spare before the three confirmations. It only changes the XML copy loaded in memory.

Thanks to **Charles Bouticourt** for the idea behind this training feature.

Devices receive unique names from a mythological, audio-themed, and deliberately playful catalogue, such as `ATHENA`, `RAVENNA`, `PYRAMIX`, `INFERNO`, or `PATCHOS`. They therefore do not share an obvious uniform prefix.

- Device and TX/RX channel names are replaced with exercise names.
- Redundant/daisy-chain modes, Preferred Master states, latencies, sample rates, encodings, and primary IP modes are deliberately mixed.
- Subscriptions are redistributed and about one quarter of the RX channels are left free.
- The displayed seed identifies the generated scenario and makes automated reproductions possible.
- Technical `device_id`, `danteId`, and `mediaType` identifiers, along with DNS, gateways, and secondary interfaces, remain protected.
- The whole operation creates a single undo step.
- The source file is never overwritten: use `Save as` to create the trainee preset.

The result is intentionally inconsistent at the functional level. Import it into the appropriate official Dante tool before using it as a training exercise.

## What's new in V3.3

- Direct DMT XLSX and ODS import, with export to four embedded dLive/Avantis templates.
- Preservation of sheets and styles outside `Channels` in generated ODS copies.
- Detailed Atomic Bomb category selection, with all options enabled by default.
- Full HD macOS layout, translation, and identical-import feedback fixes.
- V3.3 installer replacing V3.2 while preserving local data.

## What's new in V3.2

- New `Import / Export` main tab organized into `Labels`, `Reports and patchbook`, and `Synoptic`.
- Colored visual synoptic generated from the open project.
- Physical locations, device visibility, and custom device ordering.
- Consecutive subscriptions compressed into synthetic cables with a separate legend.
- Orthogonal routes, shared trunks, and a two-column legend for dense synoptics.
- Standalone SVG export; layout data stays in a separate local sidecar and never modifies Dante XML.
- Native A&H dLive/Avantis CSV and Yamaha CL/QL ZIP/CSV label exchange alongside DMT XLSX and generic formats.
- Official V3.2 installer that replaces older V3 installations.

## What's new in V3.4

- Direct device and channel renaming in Patch, plus direct Tx/Rx channel renaming in Easy patch.
- Excel-style numeric channel-name series extension through a drag handle.
- Easy patch opens on the patch matrix, with `Selection and range` as the second tab.
- Patch filters are reordered with Rx above Tx.
- A global action keeps one selected device as the only Preferred Master.
- Synoptic Reset clears manual positions and rebuilds a clean order per location.
- Real two-way flows between two devices now use one cable with an arrow at each end.
- Configuration controls are visible on first launch and controls are sized for better Windows 125% scaling.
- The Atomic Bomb work area is larger.

## What's new in V3.1

- TX/RX label exchange for one or several devices, with range selection and preview before application.
- Documented JSON and CSV formats for generic exchange and collaboration with external tools.
- Read [dLive MIDI Tools](https://github.com/togrupe/dlive-midi-tools) XLSX workbooks and directly export a bundled dLive or Avantis model, with optional explicit DMT ASCII/eight-character adaptation.
- The same label workflow on Windows and macOS through the shared XML engine.
- `Atomic Bomb` moved into a dedicated tab so it no longer dominates the main navigation.
- V3.1 installer cleanly replacing installed V3.07, V3.08 and V3.09 versions.
- New immutable `v3.1` tag; the `v3.08` and `v3.09` tags remain in Git history even after their Release pages are removed in V3.3.

## What's new in V3.09

- Shared Windows/macOS `Atomic Bomb` troubleshooting generator with three confirmations, Save As protection, and XML non-regression tests.
- Deliberate mixing of names, channels, subscriptions, network modes, Preferred Master states, latencies, sample rates, encodings, and primary IP settings.
- Dante technical identifiers, namespaces, DNS, gateways, and secondary interfaces remain protected.
- The V3.09 installer cleanly replaces legacy V3.07/V3.08 installations.

## Easy patch introduced in V3.08

- RX devices and channels are displayed on the left; TX sources are on the right.
- Previous/next controls make it quick to move between devices.
- `Ctrl` and `Shift` provide independent multi-selection in TX and RX lists.
- Equal TX/RX selections are paired one-to-one.
- One TX may feed multiple RX channels.
- Multiple TX sources cannot be assigned to one RX channel.
- Range patching uses a first TX, first RX and exact channel count.
- Oversized or ambiguous ranges are blocked atomically.
- Every preview joins one cumulative pending batch.
- The XML remains unchanged until the whole batch is applied.
- Existing subscriptions require an explicit replace, skip or cancel decision.
- The matrix uses compact cells and full TX names are available in tooltips.
- Horizontal gestures prepare consecutive TX/RX pairs.
- Vertical gestures feed one TX into several RX channels.
- Exact diagonals prepare one-to-one series.
- The final operation creates one undo step.

## Build from source

Requirements:

- Windows for the WPF application and installer
- .NET 8 SDK
- Inno Setup 6 for the Windows installer

Build the application:

```powershell
.\build.ps1
```

Build the self-contained Windows installer:

```powershell
.\installer\build_installer.ps1
```

Run all automated test suites:

```powershell
.\tests\run-tests.ps1
```

The macOS packaging process is documented in `MACOS_BUILD.md`.

## Validation and maintenance

- `TESTING.md`: automated results and validation history.
- `COMPATIBILITY_MATRIX.md`: evidence level for recognized XML structures.
- `MANUAL_DANTE_CONTROLLER_TESTS.md`: checklist for real imports.
- `ACCESSIBILITY.md`: completed and remaining accessibility checks.
- `KNOWN_LIMITATIONS.md`: technical and distribution limitations.
- `ARCHITECTURE_REFACTORING.md`: progressive architecture work.

## File safety

- Always work on a copy.
- Do not overwrite a production preset without testing.
- Keep automatically generated backups.
- Validate the final file in official Dante tools before deployment.
- The application checks generated XML consistency, but a real Dante Controller import remains the final validation.

## Public repository

https://github.com/Mamat79/DanteConfigEditorV3

## Credit

**By Mamat**<br>
<sub>et ses agents</sub>

Special thanks to **Charles Bouticourt** for the idea behind the `Atomic Bomb` feature.

### A very special thank you to Tobias Grupe

A very special thank you to **[Tobias Grupe (@togrupe)](https://github.com/togrupe)** for the considerable time he has spent testing Dante Config Editor, for his precise screenshots and feedback, and for his many improvement ideas. His suggestions concerning fixed TX/RX headers, 1:1 patching, the Flip button, keyboard navigation, matrix performance, and DMT label exchange have directly helped move V3.5 forward.

Tobias also develops **[dLive MIDI Tools](https://github.com/togrupe/dlive-midi-tools)**, the project used to design and test DCE's label import and export workflows.
