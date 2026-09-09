# Dante Config Editor 2027.0.7

[Notes de version en français](RELEASE_NOTES_2027.0.7.md)

This maintenance release fully preserves existing licences, projects, personal
banks and Dante XML fidelity.

## Export from StageFlow

- **Export Dante XML** now works from a `.stageflow` project.
- DCE no longer passes its internal package path to the Windows **Save As**
  dialog.
- Suggested names and folders are always valid physical paths, with a safe
  fallback if Windows rejects the initial folder.

## Easy Patch

- Select one or more TX channels with Ctrl or Shift, then drop them on the first
  RX: the TX channels are assigned to consecutive RX channels.
- The reverse workflow is also available: select one or more RX channels and
  drop them on the first TX. The RX channels receive that TX and the following
  TX channels in sequence.
- Each RX keeps exactly one source. The sequence stops cleanly at the end of the
  device and DCE reports channels that could not be assigned.

## Validation

- A reference to a device or TX channel missing from a partial preset is now
  classified as **information**, not as a warning requiring acknowledgement.
- The reference remains in the XML so planned subscriptions to devices absent
  from the current preset are preserved.

## Verification

- 657 Windows and XML-engine tests passed.
- 34 macOS/Avalonia tests passed.
- 13 macOS version and packaging tests passed.
- 5 licence Worker tests passed.
- Windows and macOS Release builds: 0 errors, 0 warnings.
- Real Windows installation and launch verified.
- No-change XML cycle verified byte for byte, with automated rename, patch,
  network and clock scenarios.

DCE remains an offline editor independent from Audinate. The engine preserves
unknown nodes and references; this maintenance release does not invent technical
capabilities absent from the preset.
