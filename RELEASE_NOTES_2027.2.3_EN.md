# Dante Config Editor v2027

[Notes de version en français](RELEASE_NOTES_2027.2.3.md)

## Windows and macOS 2027.2.3

- Flip swaps the two devices at the selected intersection: TX stays at the top
  and RX on the left. A device with no TX displays a "0 TX" header; that
  intersection cannot be patched.
- Moving the pointer to Flip no longer changes the selected pair. Expanded
  channels stay open, and another Flip reverses the pair again.
- Two-finger trackpad scrolling moves vertically through the matrix. Pinch
  (or Ctrl + wheel) zooms around the pointer without changing assignments.
- XML formats and validation rules are unchanged. Always check the exported
  file in Dante Controller before using it on a network.

## Windows and macOS 2027.2.2

- Combines the version detection fix below with GitHub rate-limit handling.
- On temporary 403/429 rate limits, DCE respects the server deadline, even
  after restarting. Manual checks show an informational retry time; automatic
  startup checks remain silent.

### Version detection fix

- Fix the update prompt offering 2027.2.0 when it is already installed.
  Update checks use the running Windows or Mac application's version.

## Windows and macOS 2027.2.0

- The project-wide matrix starts with devices collapsed and separates device
  headers from channel headers.
- Drag gestures automatically preview a diagonal or column. Amber previews
  remain visible for imprecise diagonals. Release applies; Escape cancels.
  The complete gesture is one undoable transaction.
- Patching retains the matrix and avoids drawing a hidden synoptic. XML guards
  and transaction history remain enabled.
- Double-click channel labels to rename inline; drag the fill handle to extend
  a series. Batch rename, Flip, source/destination lookup, and device settings
  on double-click remain available.
- Multicast lists every project flow. Choose a device, check audio TX channels,
  and create, update, or delete a flow.
- On Windows, Synoptic stays under Tools. A gear beside Workspace customizes navigation.

Multicast editing supports simple Dante audio flows in 3.0.0 presets. Unknown
forms are preserved without editing. Removing every preset flow does not
guarantee network removal: verify in Dante Controller. DCE remains offline.
The same matrix is ported to the native Avalonia Mac interface, including
automatic gestures, previews, inline rename and fill handles, connection lookup,
device settings, and the multicast manager. Apple Silicon and Intel packages
are built from the same sources.

## Windows and macOS 2027.1.1

This release simplifies daily work and automatically adapts DCE to smaller
screens without shrinking text or adding a setting users need to manage.

- DCE selects a wide, compact, or narrow layout from the actual available
  space. The header compacts, Devices and Easy Patch panels reflow, and
  scrolling remains a fallback.
- Words are no longer split inside buttons and labels. Commands remain readable
  in French and English on a 1024×640 display.
- Primary navigation now reflects six clear intents: **Project**, **Overview**,
  **Devices**, **DCE Patch**, **Synoptic**, and **Tools**. Import/export,
  validation, history, banks, reports, and training remain available in
  **Tools**.
- **New DCE project** is now a two-step assistant. The first step defines the
  file, name, and description; the second prepares the first device and its
  configuration.
- Easy Patch separates device selection, **Source / destination**, and the
  **Recommended hybrid** mode. Existing multi-selection and drag-and-drop patch
  rules are unchanged.
- The same principles apply to Windows, Apple Silicon macOS, and Intel macOS.
  XML/DCE/StageFlow formats, banks, and DCEP1, DCEF1, and V2 licenses are
  unchanged.

## Windows and macOS 2027.1.0

This release improves compact-screen use and aligns the license reminder while
keeping the application fully usable.

- Devices, Patch, Easy Patch, and Synoptic reflow with the available width.
  Scrolling is a fallback; text is not shrunk or split inside words.
- Footer commands can wrap to another row. Compact navigation displays complete
  labels, including Validation center.
- Theme choices and disabled buttons remain legible in light and dark themes.
- **New DCE project** immediately opens an empty in-memory configuration,
  without asking for a name, path, XML file, or first device. Devices can then
  be added from the bank and the destination is selected when saving.
- Before replacing modified work, DCE offers Save, Don't save, or Cancel.
  `Ctrl+N`, `Ctrl+S`, and `Ctrl+Shift+S` are aligned across Windows and macOS.
- For Yamaha console and I/O-rack templates, the bank suggests `Y001-`,
  `Y002-`, and subsequent device names. The sequence avoids collisions,
  supports multi-digit identifiers, and remains fully editable. Existing or
  imported equipment is never renamed.
- During the 30-day trial, the reminder can be dismissed immediately. Once the
  trial has expired, DCE waits 60 seconds at startup and then enables every
  feature; valid activation releases the interface immediately.
- The delay does not restart when another main window opens in the same process.
  Existing DCEP1, DCEF1, and V2 licenses remain compatible.
- A Codemagic retry accepts an already published macOS `DMG + SHA-256` pair
  when it is complete and consistent, even if a rebuilt DMG has different
  metadata. An incomplete pair is repaired without overwriting the DMG.

## Windows and macOS 2027.0.7

This corrective release makes StageFlow export reliable and extends sequential
patching without changing XML, project, bank, or license formats.

- **Export Dante XML** now works from a `.stageflow` project: the internal DCE
  package entry is no longer passed to the Windows Save As dialog.
- File dialogs propose a valid physical folder and file name, with a safe retry
  if Windows still rejects the initial path.
- In **Easy Patch**, several Tx channels dropped on the first Rx feed successive
  Rx channels. In the other direction, several Rx channels dropped on the first
  Tx receive that Tx and the following Tx channels in sequence.
- Each Rx keeps exactly one source. The sequence stops cleanly at the end of the
  device and DCE reports channels that could not be assigned.
- A reference to a device or Tx channel missing from a partial preset is now
  information. It remains preserved in XML and no longer requires warning
  acknowledgement.
- Guides and behavior are aligned in French and English on Windows and macOS.

## Windows and macOS 2027.0.6

This corrective release restores network controls that are genuinely described
by some presets and speeds up sequential patching without changing XML
compatibility rules, licenses, personal banks, or project formats.

- **Redundant** and **Daisychain** remain editable when a device contains a
  `redundancy` element, even when it has no `ipv4_address` element.
- The IP tab deliberately remains disabled when the role has no IPv4 section.
  A visible explanation states that DCE preserves the original document and
  never invents a missing technical capability.
- In **Easy Patch**, select one or more Tx channels with Ctrl or Shift, then
  drop them on the first Rx. The first Tx is assigned to that Rx and the
  remaining Tx channels to successive Rx channels, immediately.
- Replacing an already patched Rx keeps the user's warning preference. Several
  Tx channels can never feed the same Rx.
- The behavior and explanations are available in French and English on Windows
  and macOS.

## Windows and macOS 2027.0.5

This release clarifies DCE and StageFlow projects, makes synoptic persistence
reliable, and keeps validation focused on issues that matter before export.

- A standalone DCE project remains a Dante XML file: it can be created from
  scratch, opened, edited, and exported for Dante Controller without StageFlow.
- In a `.stageflow` project, **Save** now preserves synoptic locations, ordering,
  visibility, and manual positions inside the Dante domain. These workspace data
  are never added to exported Dante XML.
- **Import / Export** directly exposes Dante Controller XML import and export,
  in addition to the File menu.
- The validation center no longer reports unused Rx channels or legitimate
  devices with no Tx or no Rx. Blocking structure, identity, and reference checks
  remain active.
- Synoptic storage uses a portable identity to find each device again after a
  StageFlow project is closed and reopened.
- Labels, tests, guides, and version numbers are aligned across Windows, macOS
  Apple Silicon, and macOS Intel.

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
- Procedures distinguish Save, Save as, and Export to Dante Controller, preserving the
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
- **File > Export to Dante Controller** and **Import / Export > Export to Dante Controller** open
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

This release targets **2027.1.1**, binary version **2027.1.1.0**, with separate
packages for Windows, macOS Apple Silicon, and macOS Intel.

Avalonia headless tests run on Windows are not native Mac acceptance tests.
Installation checks and visual verification are reported separately from
automated tests. This update claims no new Dante Controller or physical hardware
trial. Commercial Windows signing and Apple notarization are outside this update.
