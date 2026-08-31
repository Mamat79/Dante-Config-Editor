# Dante Config Editor 2026.10

[Notes de version françaises](RELEASE_NOTES_2026.10.md)

## Unified SiLeMI/O interface

This 2026.10 refresh applies the shared SiLeMI/O visual system across the
Windows interface: a compact identity bar, 208 px navigation, a 280 px
contextual inspector, clearer surface hierarchy, and a persistent status bar.
Controls now share consistent heights, radii, spacing, and hover states;
values remain vertically centered and long inspector labels are no longer
split in the middle of a word.

Light and dark palettes were reviewed in both French and English on the
Project, Devices, Patch, and Atomic Bomb screens. This is a visual-only change:
the XML engine, targeted mutations, project formats, device banks, and
licensing system remain unchanged.

## New: StageFlow LIVE V1 following

When a `.stageflow` project is orchestrated by StageFlow Desktop, DCE now
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

When a StageFlow project does not contain a Dante domain yet, DCE now states
the exact workflow: open the StageFlow project, open the Dante XML file, then
choose **Save**. **Save as** remains dedicated to creating a new project and
intentionally refuses an existing `.stageflow` directory.

A contract test covers the shared Windows/macOS wording and the actual flow.
It also verifies that adding `dante/dante.json` and the `.dceproj` package
preserves `patch`, `CAD`, and `SMT` byte for byte. The XML engine, licensing,
and StageFlow format are unchanged.

## Native `.stageflow` project

DCE now defaults to a `.stageflow` project directory. The application can
create, open, and save it independently. StageFlow Desktop remains free and
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

- 512 Core/Windows tests passed, including 21 focused StageFlow tests and 3
  dedicated SiLeMI/O visual-system contracts;
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

This publication includes the Windows installer. The 2026.10 macOS DMGs will
be added to the same release after the GitHub runner is restored.
