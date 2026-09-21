# Dante Config Editor

**DCE lets you prepare, review, edit, merge, patch, and document Dante
configurations offline, without having the devices connected.**

DCE exposes three explicit workflows: create or open a standalone **Dante Config
Editor project** in XML, open a **local StageFlow project** available on the
computer, or deliberately join a **StageFlow LIVE session** on the local network
with its six-digit code. StageFlow is free and optional. A StageFlow project with
no Dante configuration can be initialized directly as an empty configuration,
then populated with the required device-bank templates.

[Version française](README.md)

## Download, watch, learn

**Shared stable version: 2027.2.0 for Windows, macOS Apple Silicon, and macOS Intel.**

| Platform | Direct download |
| --- | --- |
| Windows 11 x64 | [Self-contained 2027.2.0 `.exe` installer](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.0/DanteConfigEditor2027_Installer.exe) |
| macOS Apple Silicon | [`.dmg` disk image](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.0/DanteConfigEditor2027_macOS_AppleSilicon.dmg) |
| macOS Intel | [`.dmg` disk image](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.0/DanteConfigEditor2027_macOS_Intel.dmg) |
| Download verification | [SHA-256 checksums](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.0/SHA256SUMS.txt) |
| Optional StageFlow project tool | [StageFlow, free](https://github.com/Mamat79/StageFlow) |

| Discover Dante Config Editor | English | Français |
| --- | --- | --- |
| Short presentation | [MP4 video](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.0/dante-config-editor-presentation-en.mp4) | [Vidéo MP4](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.0/dante-config-editor-presentation-fr.mp4) |
| Windows quick start | [English PDF](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.0/QuickStart_DanteConfigEditorV3_EN.pdf) | [PDF français](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.0/QuickStart_DanteConfigEditorV3_FR.pdf) |
| Windows full manual | [English PDF](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.0/Notice_DanteConfigEditorV3_EN.pdf) | [PDF français](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.0/Notice_DanteConfigEditorV3_FR.pdf) |
| macOS quick start | [English PDF](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.0/QuickStart_DanteConfigEditor_macOS_EN.pdf) | [PDF français](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.0/QuickStart_DanteConfigEditor_macOS_FR.pdf) |
| macOS full manual | [English PDF](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.0/Notice_DanteConfigEditor_macOS_EN.pdf) | [PDF français](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.2.0/Notice_DanteConfigEditor_macOS_FR.pdf) |
| Shared SiLeMI/O suite guide | [English PDF](docs/guides/SiLeMIO-Suite-Guide-EN.pdf) | [PDF français](docs/guides/Guide-Suite-SiLeMIO-FR.pdf) |

The videos introduce the software's principles. The 2027.2.0 manuals cover
standalone and StageFlow workflows, persistent synoptic layout, and Dante Controller export.
Windows and Mac offer the project-wide matrix: collapsed devices, automatic
diagonal and column previews, Flip, inline renaming and series fill handles,
source/destination lookup, and device settings on double-click. Multicast lists
project flows and creates, updates, or removes simple audio flows in 3.0.0
presets. Unknown formats remain intact; verify network flow removal in Dante Controller.

Version 2027.2.0 automatically adapts the interface to wide, compact, or narrow
windows without shrinking text. Primary navigation is focused on Project,
Overview, Devices, DCE Patch, and Tools. On Windows a gear customizes shortcuts
and Synoptic stays under Tools. New projects use a two-step
assistant, and Easy Patch clearly separates Source / destination from the
Recommended hybrid mode.

> DCE is an unofficial third-party tool and is not affiliated with Audinate.
> It does not control a live Dante network and uses no Audinate SDK or API.
> Work on a copy and review the final file in Dante Controller before using it
> on an installation.

The maintainer has successfully imported XML produced by this generation into
Dante Controller. Structural, semantic, and regression tests complement those
real-world imports.

## Getting started

### Updating Windows from 2026.10

Save projects, close DCE, and use the direct Windows download above. Older
2026.10 clients may not find this update automatically. The installer preserves
projects, banks, preferences, and licenses. Then check `Help > About`: the
historical Program Files folder may still say "2026.3" without indicating the
installed version. Windows and Mac packages have separate versions; GitHub's
Latest label is not a platform selector. The new updater chooses a suitable
stable package.

### Header and preparation

The common header exposes **StageFlow connection**, **Alerts**, **Light/Dark**,
**FR/EN**, the **DCE guide**, and **DCE help**. **Guide** opens the full software
manual in the selected language, without an Internet connection. **Help** opens
**Discover DCE** to choose a starting point. The shared document remains in
**Help > SiLeMI/O suite guide**. [Official manuals and resources](https://www.silemio.com/en/software/dante-config-editor#guides)
are also available on the SiLeMI/O website. Alerts distinguish XML checks
from StageFlow changes to acknowledge. Prepare **Devices**, then **Patch**, open
**Validation center**, and **Export to Dante Controller** to create the final XML.
**Save** preserves the Dante domain and the synoptic workspace data in StageFlow:
locations, order, visibility, and manual positions. **Atomic Bomb** remains in
the sidebar. `Tools > Training > Atomic Bomb` and the full
safety sequence remain unchanged. In **Import / Export > Synoptic**, use
**Detach synoptic** to open the separate view, then **Reattach** to return to
the main window while keeping device and zone positions without changing XML.

The **Project** page adapts its counters, controls, and recent-file list to the
available space. In a large window, only recent files need to scroll. In a
small window, page scrolling remains available to reach every command without
shrinking the text.

## Why use DCE?

DCE started from a field requirement: **review an entire Dante installation
quickly without opening every Dante Controller page in sequence**. It brings
devices, channels, subscriptions, latency, sample rates, encoding, network
modes, Preferred Masters, and IP configuration into one interface.

It also addresses two operations that are usually time-consuming:

- **renaming a device or its Tx channels on an already-patched network**: DCE
  updates recognized references so subscriptions remain valid;
- **merging multiple installations**: a second XML file can be added to the
  project, with duplicate detection and automatic or manual conflict renaming.

DCE can start from an existing backup or from an offline project populated with
roles from the device bank. It is useful for preparing a future installation,
as well as reviewing, correcting, documenting, or reorganizing an existing one.

## What DCE can do

### Prepare and organize a project

- create a standalone Dante Config Editor project and its XML from scratch;
- open a local StageFlow project without installing or starting StageFlow;
- explicitly join a StageFlow LIVE session with its six-digit code;
- automatically follow a valid StageFlow LIVE session, with a visible state,
  immediate fallback to standalone mode when the lease expires, and a local
  option to disable following;
- open Dante XML, reimport edited output, and compare before/after states;
- create a minimal offline project, then add roles from a device bank;
- add an XML file to the open project to merge two installations;
- resolve name and identity conflicts during a merge;
- cautiously duplicate, delete, or reset a device;
- migrate previous `.dceproj` workspaces without making them mandatory.

### Edit devices and channels

- rename devices, Rx channels, and Tx channels directly;
- extend numbered naming series while preserving leading zeros, including stereo pairs such as `1L`, `1R`, `2L`, `2R`;
- preserve recognized subscriptions when renaming a device or Tx channel;
- edit latency, sample rate, encoding, Preferred Master, redundancy/daisychain,
  and IP settings when the device XML exposes those capabilities;
- keep Redundant/Daisychain controls available when a `redundancy` element
  exists, even if that role has no IPv4 section;
- apply profiles or global actions to a selection or every unlocked device;
- clearly report unavailable properties instead of inventing technical tags.

### Patch quickly

- work through Matrix, Easy Patch, or the detailed Rx-to-Tx list;
- patch one channel, drag across a range, or build a 1:1 series;
- in Easy Patch, select one or more Tx channels and drop them on the first Rx;
  DCE immediately assigns them to successive Rx channels;
- or select one or more Rx channels and drop them on the first Tx; DCE
  immediately uses that Tx and the following Tx channels for each Rx;
- swap displayed roles with Rx/Tx FLIP;
- locate the source of an Rx or every destination of a Tx;
- detach and enlarge the matrix while retaining filters, FLIP, 1:1, and zoom;
- disconnect Rx, Tx, or every patch associated with a device.

### Review, export, and document

- surface warnings, inconsistencies, and audio/network format differences;
- review every save through a pre-export validation assistant: blocking errors,
  warnings that require acknowledgement, information, and direct navigation to
  the affected item;
- keep references to a Tx missing from a partial preset as information rather
  than treating them as a blocking XML compatibility problem;
- generate TXT/PDF reports, patchbooks, and before/after comparisons;
- export a color synoptic as PDF or SVG with locations and grouped links;
- exchange labels through JSON, CSV, XLSX, and ODS, including DMT,
  Allen & Heath dLive/Avantis, and Yamaha CL/QL workflows;
- use a StageFlow patch group directly to name all or part of a Dante device's
  RX channels, choosing Source, Microphone, Source + microphone, or StageFlow
  label with a Before / After preview;
- persist the patch-set UUID, entry UUID, naming mode, and target Rx channel
  for each LIVE association: automatic text matching is never used;
- manage personal, bundled, or shared banks and update them from GitHub;
- distinguish the provenance and verification level of every bank template;
  57 community templates were tested on real hardware by the maintainer and 20
  additional profiles were structurally validated from official manufacturer
  specifications;
- prepare a sanitized, shareable bank contribution without hardware identity,
  network addresses, subscriptions, or project data;
- recover a session, Undo/Redo operations, and save through validation, backup,
  and safe destination replacement.

On first launch, a guided welcome clearly separates **Dante Config Editor
project**, **Local StageFlow project**, and **StageFlow LIVE session**, then
provides access to the machine bank and the full manual. It can be opened
again from `Help > Discover DCE`. For easier support,
`Tools > Create support package` creates a sanitized diagnostic archive with a
manifest and SHA-256 digest, without Dante XML or license codes.

DCE provides French/English interfaces, light/dark themes, and Windows/macOS
builds. The deliberately separate **Atomic Bomb** tool creates a chaotic
troubleshooting project for training exercises.
On Mac, the initial window fits the available work area and display scale.
Scrolling keeps controls reachable on small displays.

## Technical documentation

- [2026.1 architecture](docs/2026.1/ARCHITECTURE_2026_1.md)
- [StageFlow integration and Dante domain](docs/STAGEFLOW_INTEGRATION.md)
- [SiLeMIO suite getting-started guide](docs/guides/SUITE-GUIDE-EN.md)
- [`.dceproj` format](docs/2026.1/DCEPROJECT_FORMAT.md)
- [Device-bank format](docs/2026.1/DEVICE_LIBRARY_FORMAT.md)
- [Migration from V3.6](docs/2026.1/MIGRATION_V3_6_TO_2026_1.md)
- [Performance report](docs/2026.1/PERFORMANCE_REPORT.md)
- [Dante Controller checklist](docs/2026.1/DANTE_CONTROLLER_MANUAL_VALIDATION.md)
- [Known limitations](KNOWN_LIMITATIONS.md)

## Four different file concepts

### Local StageFlow `.stageflow` project

A local StageFlow project is a `.stageflow` directory used by the suite applications. DCE
only changes `dante/dante.json` and packages stored under `dante/`. Domains
owned by StageFlow, StageDesk, StageMark, AutoCAD, or other tools are preserved.
Writes acquire the Dante lock before the shared manifest lock, reread both
documents, retain concurrent additions made by other tools, and atomically
replace only the Dante domain. StageFlow is free and optional.

The same project can also be opened by
[StageDesk](https://github.com/Mamat79/StageDesk), while the
historical `smt` technical-domain identifiers remain unchanged.

To add Dante to a `.stageflow` project that does not contain it yet, open it in
DCE. An immediate choice offers **Start from scratch**, **Open Dante XML**, or
**Later**:

- choose **Create Dante configuration** to immediately open an empty in-memory
  configuration, then add the required devices from the bank; no name, path,
  XML file, or first device is required;
- or open an existing Dante XML file, then choose **Save** to add it to the
  project.

In both cases, **Save** updates only the StageFlow project's Dante domain.
On Windows, **Save as** can export a new XML or create another project;
it refuses to overwrite an existing `.stageflow` directory. On Mac,
**File > Export to Dante Controller** produces a separate XML outside the StageFlow
folder, preserving the open project and history. **Save** and **Export** are
separate operations.

Once the Dante configuration exists, **Map the StageFlow patch to RX channels**
appears directly on the Project page. The same command remains available under
**Import / Export > Labels**. Select a group, naming mode, device, and range.
DCE resolves common pairs and group overrides, ignores empty cells, and changes
nothing before Preview and **Apply**.

When a valid StageFlow LIVE session is present, DCE clearly displays **LIVE
connected** and can follow patch changes. Only Rx channels already associated
with an explicit UUID and a saved naming rule are updated. Following is enabled
by default, can be cleared through **Follow StageFlow LIVE sessions**, and
returns to strictly manual behavior when the lease disappears or expires.
Unsaved local changes, a dangling rule, or a base-hash conflict blocks the
automation and retains the last valid state. DCE never creates the LIVE session
and never controls the real Dante network.

The **StageFlow connection** button keeps its name. A separate line shows
**Standalone**, **Session available**, **Connected to [session]**, or **Disconnected**.
The same center combines **StageFlow LIVE** and **Dante Config Editor remote**
in a side rail, with the current project and **Back to project**.
Under **StageFlow LIVE**, select a session discovered on this PC
or the local network, check its project and
host, then enter the six-digit code displayed by StageFlow and choose **Join**.
Leading zeros are preserved, including when pasting spaces or a hyphen.

If the list is empty, first check that StageFlow is open and sharing its
session, then refresh. **Find by address** accepts the **private IPv4 address and port** shown
by StageFlow, then choose **Find**. A rejected code keeps the window open so you
can correct it and retry. DCE never joins a session by itself. After a connection
loss or host shutdown, reconnect explicitly. **Back to project** closes the center
without leaving an active session; **Disconnect** leaves it and keeps local work.
No cloud account or subscription is required for the connection. Use this LAN
transport only on a trusted local network: it is not an encrypted remote link
or a remote control for Dante devices.

The **Dante Config Editor remote** section identifies the real target:
the **StageFlow suite**, not a standalone DCE remote. Manage the QR, activation,
permissions and revocation in StageFlow on the session host. DCE does not receive
that link or its active/stopped state and therefore does not display a fake QR.
With a loaded project and the required permissions, existing suite commands can
open Patch / RX or the validation center; they do not control Dante hardware or
replace patch editing in DCE.

This connection can open the project published by StageFlow. If it has no Dante domain yet,
DCE offers the same **Start from scratch / Open Dante XML / Later** workflow.
When saving, the `.dceproj` package is uploaded before `dante/dante.json`, then
accepted only when its path, size, and SHA-256 match the manifest. Patch, CAD,
StageDesk, StageMark, and future domains remain unchanged.

The network session's **label alerts** are enabled by default as soon as a
computer joins LIVE. Human TX/RX renames show the previous and current label,
affected item, origin, time, and a persistent counter in an orange banner on
every screen. **Acknowledge** handles the displayed change; **Acknowledge all**
handles only the alerts present at the click, never later arrivals. **Details**
opens the list. Each computer acknowledges the alerts it
receives and can disable them locally: DCE synchronizes that choice with
StageFlow, acknowledges that recipient's existing alerts, and accumulates no
alerts while reception is off. Only new alerts arrive after re-enabling. Other
clients are unaffected. The host can also pause change notifications: DCE
distinguishes this from reception disabled locally. LIVE synchronization stays
active, without reconnecting or replaying a backlog on resumption. Connection
and safety messages remain active. A server that refuses alerts never blocks the rename or save. This link
transports the preparation project, not commands to Dante devices.

On Windows, StageFlow can also discover the open Dante Config Editor instance and ask
it to show Patch / RX, open the validation center, or save its Dante domain.
This local console is optional: it uses a current-user-only pipe, a per-instance
nonce, and verifies the project and session UUIDs. It exposes no Dante network
command and replaces neither standalone Dante Config Editor work nor direct
`.stageflow` opening. If another project contains unsaved changes, **Save /
Discard / Cancel** remains a local Dante Config Editor choice before any project
switch.

![SiLeMIO suite architecture](docs/media/ecosystem/suite-architecture-en.svg)

![Recommended SiLeMIO suite workflow](docs/media/ecosystem/suite-workflow-en.svg)

### Dante XML

XML remains the file intended for Dante Controller. DCE performs targeted
changes to the original document to preserve unknown nodes, attributes,
namespaces, values, and extensions. Saving uses a temporary file, reload,
validation, backup, and safe destination replacement.

### Previous `.dceproj` project

A `.dceproj` file is the previous DCE workspace container. It remains readable
for migration and can hold the Dante XML,
project name, layout, annotations, history, bank references, and DCE assets.
Never import it directly into Dante Controller: export its Dante XML first.

### Device bank

A bank holds reusable and shareable templates. Insertion creates an independent
instance and does not bind a project to the source template. Hardware identity,
IP configuration, flows, and subscriptions are not copied by default.
`DCE Generic` and `DCE Community` are bundled with the application. Together
they provide 79 templates, including 77 illustrated, sanitized community
templates. Of those, 57 were tested on real hardware and 20 are based on
manufacturer specifications with offline structural validation. Bundled
folders remain versioned, but DCE displays one logical bank
per family and automatically selects its latest generation. Coverage includes
Yamaha CL/QL/DM/TF/RIVAGE, Allen & Heath dLive/Avantis/SQ, DiGiCo, Focusrite
RedNet, and Neutrik through documented Dante devices, cards, or interfaces.
Yamaha DM7 and DM7 Compact 144 Tx / 144 Rx roles are included.
The bank includes two distinct LM44 roles:
`8 Tx / 4 Rx` and `0 Tx / 4 Rx`. The Devices page lets users select a bank, add
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
For Yamaha templates categorized as consoles or I/O racks, DCE suggests a
`Y001-` device-name prefix and then the next free identifier. This helper follows
the convention documented by Yamaha for some identification and control
workflows between compatible devices; it is not a universal Dante rule. The
name remains editable, the sequence avoids collisions, and no existing or
imported device is renamed.
Manufacturer references: [Yamaha CL/QL system guide](https://download.yamaha.com/files/tcm%3A39-1251310)
and [Yamaha Rio-D3 reference manual](https://usa.yamaha.com/files/download/other_assets/7/2345707/Rio3224-D3_reference_manual_En_A0.pdf).

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
- independently create, open, and save `.stageflow` projects;
- synchronize only the Dante domain with locking, base hashes, and external
  change recovery;
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
- fully offline Atomic Bomb training tool with a safety key, a manually opened
  cover, then ARM, LOCK, and FIRE.

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

## Install Dante Config Editor on Windows

The [2027.2.0 Release](https://github.com/Mamat79/Dante-Config-Editor/releases/tag/v2027.2.0)
contains the Windows installer, both Mac packages, guides, and bundled banks.
Intermediate build artifacts are not a distribution channel.

### Windows 11 x64

Artifact: `DCE-2027-Windows-Installer`

File: `DanteConfigEditor2027_Installer.exe`

### macOS 2027

- `DanteConfigEditor2027_macOS_AppleSilicon.dmg`
- `DanteConfigEditor2027_macOS_Intel.dmg`

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

## License and trial

Dante Config Editor includes a 30-day trial. Its startup reminder can be
dismissed immediately during the trial. Once the trial has expired, a 60-second
countdown runs once per process launch, after which all features remain accessible.
Valid activation releases the interface immediately. A commercial or
complimentary license, verified locally through a cryptographic signature,
removes both the reminder and the wait. The complete behavior and privacy boundaries are documented in
[docs/LICENSING_DCE_EN.md](docs/LICENSING_DCE_EN.md).

## Acknowledgements

Thanks to [Tobi / @togrupe](https://github.com/togrupe), author of
[dLive MIDI Tools](https://github.com/togrupe/dlive-midi-tools), for his
feedback, patch-workflow ideas, and help with DMT label exchange.

Thanks to Charles Bouticourt for the `Atomic Bomb` training-function idea.

---

**By Mamat**<br>
*et ses agents*<br>
`-------[]--`
