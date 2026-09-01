# Dante Config Editor 2026.8

[Notes de version françaises](RELEASE_NOTES_2026.8.md)

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
`patch.json`, `cad/cad.json`, and `cad/plan.svg` remained strictly identical,
byte for byte.

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

- 494 Core/Windows tests passed;
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
