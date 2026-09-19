# Dante Config Editor 2027.1.1

[Notes de version en français](RELEASE_NOTES_2027.1.1.md)

This release automatically adapts Dante Config Editor to wide, compact, or
narrow screens without shrinking text.

## Adaptive interface

- The header, margins, and panels reflow with the available space. Words are
  no longer split inside buttons.
- Devices and Easy Patch stack their work areas on smaller displays; scrolling
  remains available when height is limited.
- Primary navigation is focused on Project, Overview, Devices, DCE Patch,
  Synoptic, and Tools. Advanced functions remain available under Tools.

## Simplified workflows

- New DCE project is now a two-step assistant: project information, followed
  by the first device and configuration.
- Easy Patch clearly separates Source / destination from Recommended hybrid.
  Existing multi-selection and drag-and-drop rules remain unchanged.
- Windows and macOS use the same layout thresholds and matching French/English
  labels.

## Preserved compatibility

- XML, DCE, and StageFlow formats are unchanged.
- Foreign StageFlow domains, unknown XML nodes, personal banks, preferences,
  and storage locations are preserved.
- Existing DCEP1, DCEF1, and V2 licenses remain compatible.
- DCE remains an offline editor independent of Audinate; it does not control a
  live Dante network.

Windows checks, automated macOS checks, local installation, and artifacts are
recorded in `VALIDATION_2027.1.1.md`.
