# Dante Config Editor 2027.0.5

[Notes de version en français](RELEASE_NOTES_2027.0.5.md)

This shared Windows and macOS release clarifies DCE and StageFlow projects,
makes synoptic persistence reliable, and keeps validation focused on issues that
matter before export.

## What's new

- A standalone DCE project remains a Dante XML file: create it from scratch,
  open it, edit it, and export it for Dante Controller without StageFlow.
- In a `.stageflow` project, **Save** now preserves synoptic locations, ordering,
  visibility, and manual positions inside the Dante domain. These workspace data
  are never added to exported Dante XML.
- **Import / Export** gives direct access to opening a Dante Controller XML file
  and exporting to Dante Controller, in addition to the File menu.
- The validation center no longer reports unused RX channels or legitimate
  devices with no TX or no RX. Blocking structure, identity, and reference checks
  remain active.
- Synoptic storage uses a portable identity to find every device again after a
  StageFlow project is closed and reopened.
- Labels, manuals, and version numbers are aligned across Windows, macOS Apple
  Silicon, and macOS Intel.

## Compatibility preserved

- `DCEP1`, `DCEF1`, and V2 licence formats remain compatible.
- Personal banks, preferences, XML projects, and legacy DCE projects remain
  preserved during upgrades.
- The targeted XML mutation engine, merge workflow, patching, series renaming,
  device banks, and PDF/SVG exports are not replaced.
- StageFlow remains free and optional. DCE prepares configurations offline and
  does not directly control a Dante network or Dante hardware.

The maintainer has successfully imported XML produced by DCE into Dante Controller.
No new physical Dante-hardware test is claimed for this workflow and synoptic
persistence update.
