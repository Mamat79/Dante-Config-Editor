# Dante Config Editor 2026.1 Beta

[Notes de version françaises](RELEASE_NOTES.md)

## Status

`2026.1.0-beta.1` is a progressive rebuild of DCE on top of the stabilized
V3.6 baseline. It does not replace V3.6: applications, shortcuts, installer
identities, and local profiles are separate.

DCE remains an unofficial third-party tool and is not affiliated with
Audinate. It only works on offline files.

## Main changes

- Domain, DanteXml, Application, and Infrastructure separation;
- central session, transactional commands, Undo/Redo, and history;
- versioned `.dceproj` for DCE layout, notes, and assets without polluting the
  Dante XML;
- 2026.1 Windows shell with side navigation and inspector;
- one Patch workspace with table, matrix, Easy Patch, selection, and 1:1 views;
- interactive synoptic synchronized with selection and subscriptions;
- searchable and exportable Validation Center;
- XML profiles that can restrict an unknown structure or open it read-only;
- format-2 device bank and verified copy-based V3.6 migration;
- global view of personal and bundled banks, deduplicated to `43` unique
  templates, with bundled templates protected as read-only;
- shared device selection across Machines, Patch, Easy Patch, and the
  inspector, preserved when switching views;
- direct access to the global 43-template view from Devices, with no redundant
  bank selector and a compact quick-list bar integrated below global actions;
- Global actions now matches the Device/Channels panel height and no longer
  needs an inner scrollbar in Network/audio on a standard display;
- a larger Device bank window automatically constrained to the available work
  area;
- device settings opened at startup and collapsible through a centered arrow
  that remains accessible in both states;
- the complete device list starts collapsed behind a titled bar and a
  persistent arrow so settings receive the available height;
- settings fit without a scrollbar at `1536 x 864`, with fallback scrolling
  retained for smaller displays;
- standard application menu on Windows and macOS with direct access to files,
  devices, views, tools, and guides;
- light theme on first launch, followed by restoration of the last selected
  theme and language;
- integrated Atomic Bomb page with a safety key that automatically opens the
  cover, then enables ARM, LOCK, and FIRE;
- technical settings are available only when their element already exists in
  that Dante role; DCE does not create `redundancy`, `preferred_master`,
  `samplerate`, `encoding`, `unicast_latency`, or `ipv4_address` to simulate
  an unsupported capability;
- unavailable controls are disabled with a bilingual explanation and tooltip;
- Windows and macOS tooltips were audited in French and English and now wrap
  onto multiple lines instead of clipping long explanations;
- a more compact Patch matrix, larger side-panel arrows, and Show in Patch
  opening directly on the selected device;
- isolated 2026.1 local profile.

## XML fidelity

The original document remains the source of truth. DCE performs targeted
mutations and preserves unknown nodes, attributes, namespaces, ordering, and
values. Saving uses a temporary file, reload, validation, backup, and atomic
replacement. A technical element missing from the original role is never
added to simulate an unproven capability.

Automated corpus coverage includes partial presets, TX-only and RX-only
devices, local `.` subscriptions, missing sources, missing channels, default
namespaces, Unicode, unknown extensions, multiple interfaces, and mixed audio
modes.

The maintainer successfully imported 2026.1 output into Dante Controller with
his validation files. The
`docs/2026.1/DANTE_CONTROLLER_MANUAL_VALIDATION.md` checklist remains
recommended for each XML structure and every production operation.

## Performance

For the synthetic 200-device preset with 64 TX and 64 RX per device:

- grouped edit: `317.410 ms` to `38.092 ms`;
- edit allocations: `390.759 MiB` to `29.358 MiB`;
- validation: `86.948 ms` to `36.062 ms`;
- XML save: `501.695 ms` to `363.457 ms`.

## Automated validation

- 419 Core/Windows tests passed;
- 22 headless Avalonia/macOS tests passed;
- Windows Release build completed without warnings;
- synthetic corpus saved and semantically compared without loss;
- 11 local XML files checked read-only by integration tests without modifying
  their originals.

Final delivery counts are recorded in the beta report.

## Installation

### Windows

`DanteConfigEditor2026_1_Beta_Installer.exe` includes the .NET 8 runtime and
bilingual guides. Its default folder is
`C:\Program Files\Dante Config Editor 2026.1 Beta\`.
All 43 bundled templates are installed in the application folder without
replacing the personal bank.

### macOS

Two self-contained DMGs are prepared for Apple Silicon and Intel. They are ad
hoc signed but not notarized.

## Limitations

- complete project creation remains experimental;
- duplication and bank insertion do not create a real hardware identity;
- a few controls retain different native rendering on Windows and macOS;
- Windows installer is not Authenticode signed;
- DMGs are not notarized;
- GitHub publication is performed manually after artifact validation.
