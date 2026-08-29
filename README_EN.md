<p align="center">
  <img src="media/dce-app-icon.png" width="112" alt="Dante Config Editor icon">
</p>

<h1 align="center">Dante Config Editor</h1>

<p align="center">
  <strong>Prepare, review, edit, merge, and patch Dante configurations offline,
  without connecting the devices.</strong>
</p>

<p align="center">
  <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.7/DanteConfigEditor2026_7_Installer.exe"><strong>Download for Windows</strong></a>
  ·
  <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.7/Notice_DanteConfigEditorV3_EN.pdf"><strong>Read the professional manual</strong></a>
  ·
  <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/tag/v2026.7"><strong>Open the Release</strong></a>
</p>

<p align="center">
  <a href="README.md">Français</a> · English
</p>

---

## At a glance

**DCE 2026.7** is a Dante Controller XML project editor designed for offline
preparation, fast reviews, and repetitive changes that would otherwise require
opening many separate pages in Dante Controller.

DCE brings devices, Tx/Rx channels, subscriptions, latency, sample rates,
encoding, network modes, Preferred Masters, and IP settings into one workspace.
It edits the properties actually exposed by the XML and saves a new file for
use in Dante Controller.

[![Dante Config Editor overview](media/en/overview.png)](media/en/overview.png)

## Download, watch, and learn

| Resource | Direct link |
| --- | --- |
| **Windows 11 x64 installer** | [DanteConfigEditor2026_7_Installer.exe](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.7/DanteConfigEditor2026_7_Installer.exe) |
| Quick start, 1 page | [English PDF](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.7/QuickStart_DanteConfigEditorV3_EN.pdf) |
| **Complete professional manual, 40 pages** | [Download the English PDF](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.7/Notice_DanteConfigEditorV3_EN.pdf) · [View in the repository](manuals/Notice_DanteConfigEditorV3_EN.pdf) |
| Download verification | [SHA-256 checksums](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.7/SHA256SUMS.txt) |
| Community device bank | [77 DCE Community templates](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.7/DCE_Community_Devices_2026_3.dce-bank.zip) |
| Every file in this version | [DCE 2026.7 Release](https://github.com/Mamat79/Dante-Config-Editor/releases/tag/v2026.7) |

The Windows installer includes both quick starts and both complete manuals. The
complete manual includes a SiLeMI/O cover, clickable contents, PDF
bookmarks, a contents link on every page, and annotated screenshots. It follows
the workflow in order: startup, project, global settings, devices, renaming,
patching, merging, banks, import/export, synoptic, validation, recovery,
licensing, and Atomic Bomb.

DCE has also been tested and validated on real Mac hardware by the maintainer.
The 2026.7 macOS packages are temporarily unavailable: they must be rebuilt on
an Apple runner before publication. No unverified macOS package is presented as
a working download.

## Why use DCE?

### Review an entire installation quickly

DCE provides a concise project overview and flags items that require attention:
mixed audio formats or network modes, multiple Preferred Masters, fixed IP
addresses, unpatched devices, and inconsistent references.

### Rename without rebuilding the patch

Devices, Tx, and Rx channels can be renamed directly or as a series. For
recognized references, DCE updates the related subscriptions so the existing
patch is preserved. Numeric series retain leading zeros and can continue stereo
pairs such as `FX-1L`, `FX-1R`, `FX-2L`, `FX-2R`.

### Merge multiple installations

A second XML file can be added to the open project. DCE detects name and
identity conflicts, lets the user reuse an existing role or rename an imported
role, and redirects recognized patch references.

### Prepare a project without the hardware

DCE can start from an existing XML file or create an offline project. Sanitized
roles can be added from the device bank without copying hardware identity, IP
settings, flows, or subscriptions from another production network.

## Main features

### Devices and global actions

- safely rename, duplicate, remove, or reset a role;
- rename Tx and Rx channels individually or as a series;
- edit supported audio, network, clock, latency, and Preferred Master settings
  that are actually present in the XML;
- apply a profile or one setting to multiple devices;
- clearly report when a property is unavailable for a role.

[![Device configuration](media/en/devices.png)](media/en/devices.png)

### Fast, readable patching

- compact patch matrix with zoom and a detachable window;
- Easy Patch for selection-based or 1:1 range workflows;
- immediate point, vertical, or diagonal patch operations when the resulting
  subscriptions are valid;
- Rx/Tx FLIP between the two displayed devices;
- locate the source of an Rx channel;
- list every destination of a Tx and navigate to the selected destination;
- reset Rx, Tx, or every Rx/Tx patch associated with a device.

[![Patch matrix](media/en/patch.png)](media/en/patch.png)

### Validation and safe saving

- a real pre-export assistant with blocking errors, acknowledged warnings, and
  information grouped by severity;
- direct navigation to the Validation Center or the affected item;
- export of a control report before handing the XML to Dante Controller;
- safeguards for missing or duplicated identities;
- detection of inconsistent patch references;
- preservation of namespaces, attributes, values, and unknown elements;
- Undo, Redo, and automatic recovery;
- temporary write, reload, validation, backup of the previous destination, and
  safe replacement.

DCE does not silently create missing technical elements. An unsupported action
is disabled or rejected with an explanation.

### Device banks

- simplified catalog with three sources: `My bank`, `DCE Community`, and
  `DCE Generic`;
- automatic selection of the newest official generation, without exposing
  older copies or duplicates;
- personal and shared banks are always preserved;
- filters by manufacturer, category, and Tx/Rx capacity;
- batch insertion of multiple roles;
- independent project instances after insertion;
- public-bank updates with SHA-256 verification;
- quality, provenance, and compatibility status displayed for every template;
- **77 physical models**: 57 profiles tested on real hardware and 20 new
  profiles structurally validated from manufacturer specifications, plus two
  generic roles for preparation and training;
- sanitized contribution archives that exclude identity, IP settings,
  subscriptions, and source-project data;
- personal banks are always preserved during updates.

Direct downloads: [DCE Community bank](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.7/DCE_Community_Devices_2026_3.dce-bank.zip) · [DCE Generic roles](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.7/DCE_Generic_Roles_2026_3.dce-bank.zip).

[![Device bank](media/en/device-bank.png)](media/en/device-bank.png)

### Reports, labels, and synoptic

- TXT/PDF reports, patchbooks, and before/after comparisons;
- JSON, CSV, XLSX, and ODS label import/export;
- label workflows for DMT, Allen & Heath dLive/Avantis, and Yamaha CL/QL;
- color synoptic with locations, devices, and grouped links;
- PDF or SVG synoptic export for further vector editing.

[![Dante Config Editor synoptic](media/en/synoptic.png)](media/en/synoptic.png)

### First launch and support

- a first-launch discovery screen offering four useful paths: open an XML,
  create a project, discover the device bank, or read the manual;
- the same screen remains available from `Help > Discover DCE`;
- contextual help and tooltips without hiding advanced functions;
- one-click creation of a privacy-safe support package containing diagnostics,
  useful logs, validation results, and SHA-256 fingerprints, but **never the
  project XML or a license code**.

## Three file types to distinguish

- **Dante XML**: the file intended for Dante Controller. DCE applies targeted
  changes to the loaded document.
- **`.dceproj` project**: a DCE workspace that can retain layout, history, and
  DCE-specific information. Export its XML before using it in Dante Controller.
- **Device bank**: reusable and shareable role templates without production
  hardware identity.

## Permanent license

- **30 days with no reminder** after first launch;
- after 30 days, **DCE and every feature remain usable**;
- a non-blocking reminder simply appears at startup;
- a **€29 permanent license** removes that reminder;
- new licenses allow 3 activations by default; DCE shows the used and allowed
  counts;
- **Deactivate this computer** releases a seat for another computer;
- after initial activation, the signed receipt is checked locally and remains
  valid offline after updates;
- every legacy license remains valid without conversion.

[Buy a permanent license with Stripe](https://dce-license.mamat79-dce.workers.dev/buy)

Payment is handled by Stripe. DCE receives no banking information, and license
verification works offline.

## Compatibility and limitations

XML produced by this generation has been successfully imported into Dante
Controller by the maintainer. Automated tests also cover open/save/reopen
cycles, cross-references, namespaces, unknown values, and transactional
operations. DCE has also been tested and validated on macOS by the maintainer.

> DCE is an unofficial third-party tool and is not affiliated with Audinate. It
> does not control a live Dante network and uses no Audinate SDK or API. Work on
> a copy and review the final XML in Dante Controller before using it on an
> important installation.

## Acknowledgements

Thanks to [Tobi / @togrupe](https://github.com/togrupe), author of
[dLive MIDI Tools](https://github.com/togrupe/dlive-midi-tools), for his
feedback and ideas about patch workflows and label exchange.

Thanks to Charles Bouticourt for the **Atomic Bomb** training-function idea.

---

<p align="right">
  <strong>SiLeMI/O</strong><br>
  By Mamat<br>
  <code>-------[]--</code>
</p>
