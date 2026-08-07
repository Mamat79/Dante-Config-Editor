# Dante Config Editor 2026.1.1

[Notes de version françaises](RELEASE_NOTES.md)

## Status

`2026.1.1` is the official release produced by the progressive rebuild of DCE
on top of the stabilized V3.6 baseline. V3.6.1 remains available as a
historical release; installer identities and local profiles are separate.

DCE remains an unofficial third-party tool and is not affiliated with
Audinate. It only works on offline files.

## Main changes

- clarified visual identity: `Dante Config Editor` remains the primary product
  name, while `SiLeMI/O`, `By Mamat` and the fader form a compact signature in
  the footer and About dialog;
- fixed contrast for menus, submenus and context menus in light and dark themes
  on Windows and macOS;
- complete 11 min 22 sec French and English visual guides, each with an
  introduction explaining DCE, an interface genuinely displayed in the stated
  language, and a selectable MKV subtitle track;
- short 1 min 20 sec French and English presentations for discovering the main
  workflows before opening the complete guides;
- automatic checks for newer Releases, with no visible error while offline,
  plus a manual check from the Help menu;
- download of the matching Windows or macOS installer, SHA-256 verification,
  and launch only after confirmation;
- direct GitHub updates for official banks stored in Documents, with a
  transactional backup and no change to the personal bank;
- batch insertion of 1 to 100 devices from the bank, with a name preview,
  whole-batch XML validation, one Undo action, and a bank window that stays open
  for consecutive insertions;
- fixed `FLIP TX/RX` in the matrix: DCE now selects two distinct devices when
  a valid swap is possible and explains why an identical or non-reversible pair
  cannot be swapped;

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
- global view of personal and bundled banks, deduplicated to `46` unique
  templates, with bundled templates protected as read-only;
- community bank expanded to `44` sanitized templates, with two separate Lake
  LM44 roles: `8 TX / 4 RX` and `RX only (0 TX / 4 RX)`;
- shared device selection across Machines, Patch, Easy Patch, and the
  inspector, preserved when switching views;
- direct access to the global 46-template view from Devices, with no redundant
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
- cross-navigation in the matrix: an Rx button locates and highlights its Tx
  source, while a Tx button lists every Rx destination and opens the selected
  one;
- fill handles remain next to channel labels while targeting arrows now border
  the grid, with a stable Tx header in both light and dark themes;
- a detachable Patch matrix in a large independent window retaining the Rx/Tx
  selectors, FLIP, Patch 1:1, and zoom;
- corrected XML merge when two presets contain the same
  `device_id` / `process_id` pair: explicit reuse of the existing role or
  creation of an independent generic role after rename, with no fake hardware
  identifier;
- second-XML subscriptions redirected to the reused existing role or the
  selected new name;
- reorganized 38-page French and English full guides following the actual
  workflow: general settings, per-device settings, patch, project composition,
  exports, validation, and advanced tools, with additional screenshots;
- isolated 2026.1 local profile.

## XML fidelity

The original document remains the source of truth. DCE performs targeted
mutations and preserves unknown nodes, attributes, namespaces, ordering, and
values. Saving uses a temporary file, reload, validation, backup, and safe
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

- 445 Core/Windows tests passed;
- 22 headless Avalonia/macOS tests passed;
- Windows Release build completed without warnings;
- synthetic corpus saved and semantically compared without loss;
- 11 local XML files checked read-only by integration tests without modifying
  their originals.

Detailed preparation checks remain recorded in the dated technical reports
under `docs/2026.1`.

## Installation

### Windows

`DanteConfigEditor2026_1_1_Installer.exe` includes the .NET 8 runtime and
bilingual guides. Its default folder is
`C:\Program Files\Dante Config Editor 2026.1\`.
All 46 bundled templates are installed in the application folder. Updated
official copies are stored in Documents without replacing the personal bank.

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
