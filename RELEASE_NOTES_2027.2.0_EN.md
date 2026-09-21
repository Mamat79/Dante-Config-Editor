# Dante Config Editor v2027

[Notes de version en français](RELEASE_NOTES.md)

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
