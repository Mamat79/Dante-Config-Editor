# Dante Config Editor 2027.1.0

[Notes de version en français](RELEASE_NOTES_2027.1.0.md)

This release improves compact-screen use while preserving existing projects,
personal banks, Dante XML files, and every previously issued license.

## Compact screens

- Devices, Patch, Easy Patch, and Synoptic reflow with the available width.
  Scrolling remains a fallback and words are no longer split.
- Footer commands wrap when needed and navigation labels remain complete.
- Buttons, fields, dialogs, and disabled states remain legible in both light
  and dark themes.

## New projects and banks

- **New DCE project** directly opens an empty in-memory configuration without
  requiring a name, path, XML file, or first device.
- Before replacing modified work, DCE offers Save, Don't save, or Cancel.
- Yamaha consoles and I/O racks receive editable, collision-free `Y001-`,
  `Y002-`, and subsequent suggestions. Imported devices and other brands are
  never renamed automatically.

## Licensing and macOS builds

- The 30-day trial reminder can be dismissed immediately. Once the trial has
  expired, every feature becomes available after 60 seconds; valid activation
  releases the interface immediately.
- DCEP1, DCEF1, V2 licenses, existing activations, and stable storage remain
  fully compatible.
- The Codemagic workflow now validates an already published `DMG + SHA-256`
  pair before rebuilding or publishing. A consistent retry succeeds without
  attempting to overwrite an existing DMG; an incomplete pair is repaired
  explicitly.

## Verification

- 686 Windows and shared-engine tests passed.
- 35 macOS/Avalonia tests passed.
- Windows and macOS Release builds: 0 errors, 0 warnings.
- Native Windows checks, local installation, and macOS artifacts are recorded
  in the validation receipt attached to this release.

DCE remains an offline editor independent of Audinate. This release does not
claim real-time Dante network control, commercial Windows signing, or Apple
notarization.
