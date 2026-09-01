# Dante Config Editor - version 2026.10

[Notes de version françaises](RELEASE_NOTES_2026.10.md)

## Unified SiLeMIO interface

Dante Config Editor now follows the SiLeMIO Visual System v1: a compact
identity bar, permanently available Undo/Redo commands, language and theme
controls consistently ordered on the right as a theme icon, compact language
selector, then an outlined `? Help` button, calmer navigation, and panels
without unnecessary decorative effects. Light and dark modes now use the
Atelier Hybrid and Studio Graphite palettes.

Fields, lists, buttons, and dialogs share consistent heights, radii, and
vertical alignment. This is strictly a visual update: the XML engine, device
libraries, StageFlow integration, licensing, and project formats are unchanged.

## Create Dante from an empty StageFlow project

After opening a `.stageflow` project with no Dante domain, DCE directly offers
**Start from scratch**, **Open Dante XML**, or **Later**. The first choice opens
the first-device wizard for a custom device or a device-bank role. DCE adds only
`dante/dante.json` and its package; Patch, CAD, SMT, and StageMark domains remain
unchanged.

The previous workflow remains available: open the empty StageFlow project,
open an existing Dante XML file, then choose **Save**. Once an Rx device is
available, **Map the StageFlow patch to RX channels** is available from Project
and from **Import / Export > Labels**. It applies Source, Microphone, Source +
microphone, or StageFlow label names from the selected group with a Before /
After preview.

## Local StageFlow console on Windows

StageFlow can discover DCE, open the current project in the existing
single instance, show Patch / RX or the validation center, and request a save
of the Dante domain. Presence uses a short lease and atomic writes; the pipe is
restricted to the current user. DCE verifies the instance nonce plus the LIVE
project and session UUIDs.

This console advertises no Dante network-control capability. Opening or
reloading remains an offline project operation and never triggers hardware. If
the current project has unsaved changes, DCE presents **Save / Discard /
Cancel** before switching. Interactive opening may wait for up to five minutes;
quick commands remain limited to two seconds and no duplicate instance is
created.

For a shared save, DCE acquires `dante.lock` before `project.lock`, rereads both
documents while locked, and adds its reference to the latest manifest. Concurrent
additions made by other tools are retained; a collision on the Dante domain is
explicitly rejected.

The bundled French/English suite guide and its architecture and workflow
diagrams distinguish standalone work in each application, shared `.stageflow`
projects that do not require StageFlow, and the optional central
StageFlow console.

The **Help** menu and validation centre also expose the **SiLeMI/O suite
guide**. DCE opens the local English or French PDF for the active language
without replacing DCE's own quick start or full guide.

## New: StageFlow LIVE V1 following

When a `.stageflow` project is orchestrated by StageFlow, DCE now
recognizes its short LIVE lease and displays an explicit state: connected,
available with following disabled, standalone, or conflict. Following is
enabled by default, stored locally, and can be disabled from the Labels page.

An Rx association now stores the patch-set UUID, entry UUID, naming mode,
target device, and Rx `DanteId`. In LIVE mode, only those channels are
recomputed. DCE never falls back to text matching. A missing rule, concurrent
hash, or unsaved local work blocks the complete transaction and retains the
last valid state.

The lease is validated before use: protocol and version, `projectId`, clocks,
maximum duration, event capability, size, and absence of symbolic links on
`.live`. When the lease expires, DCE automatically returns to standalone mode.
DCE does not create sessions and never controls the real Dante network: it
updates only its offline project and its own domain.

## New: StageFlow patch to RX channels

From an open `.stageflow` project, DCE can now use a patch group directly to
name all or part of a Dante device's RX channels. The command is under
**Import / Export > Labels > Map the StageFlow patch to RX channels**.

The dialog provides the StageFlow group, four naming modes - Source,
Microphone, Source + microphone, or StageFlow label -, the target device, first
patch channel, first Dante RX, and channel count. A Before / After preview is
required. Common pairs, group overrides, and hidden pairs are resolved for the
selected group; empty cells are shown and then ignored without blocking
populated rows.

DCE remains standalone and offline. No XML changes before **Apply**, TX channels
are unavailable in this workflow, and `patch.json` is never rewritten. Linked
UUIDs are retained in the Dante domain when the project is saved.

Windows and macOS expose the same workflow in French and English.

## 2026.8.1 corrective release

When a StageFlow project does not contain a Dante domain yet, DCE can either
create its configuration from scratch with **New project**, or open a Dante XML
file and then choose **Save**. **Save as** remains dedicated to creating
another project and intentionally refuses an existing `.stageflow` directory.

A contract test covers the shared Windows/macOS wording and the actual flow.
It also verifies that adding `dante/dante.json` and the `.dceproj` package
preserves `patch`, `CAD`, and `SMT` byte for byte. The XML engine, licensing,
and StageFlow format are unchanged.

## Native `.stageflow` project

DCE now defaults to a `.stageflow` project directory. The application can
create, open, and save it independently. StageFlow remains free and
optional; it is not required to use DCE.

Dante XML import and export remain available. Previous XML files and `.dceproj`
packages remain readable. This evolution does not replace the XML engine, its
targeted mutations, or its compatibility guards.

## Conflict-safe shared project

- DCE only owns `dante/dante.json` and packages stored under `dante/`;
- `patch.json` is read through explicit UUIDs, never label matching;
- CAD, SMT, StageMark, and unknown domains are never rewritten or removed;
- saving uses a domain lock, base hash, temporary write, and atomic replacement;
- a debounced watcher reloads valid external changes and retains the last valid
  state while another write is incomplete;
- concurrent conflicts block saving with a clear explanation instead of
  overwriting another application's work.

The shared QA project was copied and extended with the Dante domain.
`patch.json`, `cad/cad.json`, `cad/plan.svg`, and `smt/smt.json` remained
strictly identical, byte for byte.

## Interface and documentation

- New project and Save as default to `.stageflow`;
- an empty StageFlow project can receive Dante XML through Open XML and Save;
- recent items, startup arguments, and reopening support `.stageflow`
  directories;
- first-run guidance explains the native format while retaining direct access
  to legacy XML, machine banks, and the manual;
- Windows and macOS expose the same workflow;
- French and English guides include the “One project, multiple tools” diagram
  and document locking, UUIDs, hashes, and ownership boundaries.

## License and compatibility

The `DCEP1` and `DCEF1` formats, V2 licenses, signed `DCE` product, public keys,
and stable local storage remain unchanged. Every issued license remains valid.
The public price remains a one-time EUR 29 purchase including French VAT via
Stripe after a non-blocking 30-day free period.

DCE remains an unofficial third-party offline editor, is not affiliated with
Audinate, and does not control a live Dante network.

## Validation

- 540 Core/Windows tests passed, including focused StageFlow workflows;
- 22 headless Avalonia/macOS tests passed;
- Windows and macOS/Avalonia Release builds completed without errors;
- dedicated StageFlow round-trip, cross-domain integrity, UUID, and base-hash
  conflict tests;
- dedicated tests for Windows junctions, links escaping the project, and
  foreign envelopes with an invalid hash or `projectId`;
- Windows visual review before publication;
- four regenerated and reviewed bilingual PDF guides.

The Windows installer is not Authenticode-signed. macOS packages remain
unnotarized. The final exported XML must still be reviewed in Dante Controller
before deployment.
