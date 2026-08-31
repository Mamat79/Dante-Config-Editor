# Dante Config Editor 2026.9

[Notes de version françaises](RELEASE_NOTES_2026.9.md)

## Map the StageFlow patch to Dante RX channels

DCE can now use a StageFlow patch group to name all or part of a Dante device's
RX channels.

The new **Import / Export > Labels > Map the StageFlow patch to RX channels**
workflow lets you choose:

- the StageFlow group;
- the naming mode: Source, Microphone, Source + microphone, or StageFlow label;
- the Dante device;
- the first patch channel, first Dante RX channel, and channel count.

A Before / After preview is shown before any change. Empty cells remain visible
but are ignored. DCE applies only populated names after confirmation.

## Groups and common pairs

DCE resolves common pairs inherited by a group, group-specific overrides, and
intentionally hidden pairs. Links to the StageFlow patch lines are kept the
next time the project is saved, without changing the patch owned by other
applications.

## DCE remains standalone

StageFlow Desktop remains free and optional. DCE can create, open, and save a
`.stageflow` project by itself while continuing to support Dante XML and legacy
DCE projects.

## Validation

- 498 Core/Windows tests passed;
- 22 headless Avalonia/macOS tests passed;
- Windows and Avalonia/macOS builds completed without warnings or errors;
- four French and English PDF guides were regenerated and reviewed;
- Windows installation and upgrade were verified without losing license state
  or creating a duplicate application.

DCE remains an unofficial third-party offline editor and is not affiliated
with Audinate. Always open and review the final XML in Dante Controller before
real-world use.
