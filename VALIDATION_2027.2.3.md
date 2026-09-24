# DCE 2027.2.3 validation

- Private source commit: eb876ff; source tag: v2027.2.3. The public
  distribution tag deliberately points to a separate code-free commit.
- Shared engine: 717 tests passed. macOS/Avalonia interface: 46 tests passed.
  Licensing worker: 5 tests passed. macOS version validator: 13 tests passed.
- Native Windows matrix probe: 136 checks passed, including selected-device
  Flip, RX-only devices, preserved XML, drag preview and trackpad zoom anchor.
- Windows Release build: zero warnings or errors. The autonomous executable
  built from the tagged source started from an isolated profile. The Inno
  installer was built and its SHA-256 verified; the final installer was not
  installed over the user's existing installation for this check.
- Codemagic build 6ab59687ad56b7e252505049: success on an Apple Silicon
  runner. The pipeline ran shared and Mac tests, built both DMGs, smoked the
  native Apple Silicon app, checked Intel architecture and verified checksums.
  The uploaded DMGs were downloaded again and checked against both GitHub's
  digests and their companion SHA-256 files.
- Eight regenerated French/English PDF guides passed document checks.
  Updated Flip pages were rendered and visually reviewed.

This does not establish operation on a live Dante network or import into Dante
Controller. The Mac packages are neither Apple Developer ID signed nor
notarized. Runtime testing on a physical Intel Mac was not performed.

See DOWNLOADS.json and SHA256SUMS.txt for package sizes and checksums.
