# Dante Config Editor v2027

[Notes de version en français](RELEASE_NOTES_2027.0.4.md)

## Windows 2027.0.4

This Windows correction improves field legibility and access to the training
and synoptic commands. Existing project data, licenses, banks, and formats are
unchanged.

- Shared dropdowns and input fields regain consistent vertical centering,
  including after a theme change.
- The StageFlow connection title remains legible in the light theme.
- Atomic Bomb is reachable from navigation again; its key, cover, ARM, LOCK,
  FIRE sequence is unchanged. The key and open-cover label are no longer
  clipped in the standard layout.
- The synoptic view exposes explicit Detach and Reattach commands, preserves
  its layout, and applies the theme correctly in its separate window.

## Windows 2027.0.2

This update clarifies project preparation, suite connection, and export commands.
DCE remains a standalone application for preparing Dante configurations offline.

### A consistent, accessible header

- The DCE icon and product name remain visible alongside the application menus.
- Commands follow this order: StageFlow connection, Alerts, Theme, Language,
  DCE guide, and DCE help. They move onto a second row when space is limited.
- Theme shows the current Light or Dark choice. Theme and language preferences
  are saved. Commands have accessible names and tooltips.
- Theme and language fields align vertically with the adjacent buttons, without
  inherited inner margins shifting their backgrounds.
- Undo and Redo remain in the header. Atomic Bomb is under **Tools > Training**,
  with its safety sequence unchanged.

### Help and manuals

- **Guide** now opens DCE's full manual in the selected language. **Help**
  opens **Discover DCE**; the shared guide is available only through
  **Help > SiLeMI/O suite guide**, without an additional dedicated button.
- Product manuals in both languages have a compact, single-page contents
  section, preserving links and bookmarks. Captions explain the screens,
  user links lead to the SiLeMI/O website, and group-specific values are explained.
- Procedures distinguish Save, Save as, and Export Dante XML, preserving the
  working project while producing the preset intended for the installation.

### StageFlow connection and alerts

- The **StageFlow connection** button keeps its name; a separate line shows
  its state and the joined session when connected.
- The center uses the common **StageFlow LIVE / Dante Config Editor remote**
  side rail, current project, and **Back to project**. It remains non-modal;
  local editing is temporarily protected during connection transitions.
- The mobile section identifies the **StageFlow suite** as its target. Manage
  the QR, its state and permissions in StageFlow on the host: DCE does not invent
  a standalone mobile server, link, or activation button. If no session is found,
  open StageFlow and share the project on this PC or the local network.
- **StageFlow connection** distinguishes local projects from LIVE sessions.
  Standalone mode, connection progress, connection loss, and errors are explicit.
  The full project name remains available.
- The center can open a local `.stageflow` folder without running StageFlow.
  Disconnect an active LIVE session first; opening another project warns about
  unsaved changes.
- Discovery, six-digit pairing codes, and explicit disconnect are retained.
  The IPv4 address and technical details are in an expandable section.
- **Alerts** separates XML validation from unacknowledged StageFlow changes.
  Opening the list neither acknowledges changes nor disables future reception.

### Prepare, validate, export

- The Project page gives direct access to devices, patching, validation, and export.
- Counters share a single row on wider windows. Recent files use the remaining
  height and scroll within their own list; smaller windows retain a page-level
  scrolling fallback so commands are never cut off.
- **File > Export Dante XML** and **Import / Export > Export Dante XML** open
  the pre-export checks, then save a separate XML copy.
- The copy preserves the open project, pending edits, and undo history. It does
  not overwrite the source. Existing destinations retain the confirmation and
  backup protections of the existing writer.
- Export refuses extensions other than `.xml` and destinations inside the current
  StageFlow folder, including directory junctions and symbolic links.
- **Save** still saves the StageFlow project. Exporting an XML copy does not
  replace saving that project.

The XML mutation engine is not rewritten. Banks, patching, series renaming, XML
merging, synoptic views, and existing licenses are preserved. StageFlow is free
and optional. A LIVE suite connection does not control Dante hardware or the
physical Dante network.

## Documentation and upgrading

The Windows installer includes French/English manuals and quick starts, plus
the exact shared SiLeMI/O 2027.3 guide pair approved by its owner.

From a 2026.10 installation that does not offer v2027, download the Windows
installer manually from the public repository. Recent versions select updates
for their platform and check the download size and SHA-256.

Upgrading preserves profiles, personal banks, projects, and licenses. One desktop
shortcut is used: **Dante Config Editor v2027**. A historical installation folder
name such as `Dante Config Editor 2026.3` may remain; About and the executable
report the effective version.

## Windows and Mac

This correction targets **Windows 2027.0.4**, binary version **2027.0.4.0**.
The latest accepted Mac packages remain **2027.0**, with separate Apple Silicon
and Intel downloads. They are not renamed or presented as this correction.
Public download links and version numbers remain platform-specific.

Avalonia headless tests run on Windows are not native Mac acceptance tests.
Installation checks and visual verification are reported separately from
automated tests. This update claims no new Dante Controller or physical hardware
trial. Commercial Windows signing and Apple notarization are outside this update.
