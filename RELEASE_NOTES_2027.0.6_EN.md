# Dante Config Editor 2027.0.6

[Notes de version françaises](RELEASE_NOTES_2027.0.6.md)

This corrective release fully preserves existing licenses, projects, personal
banks, and XML formats.

## Network and XML

- **Redundant** and **Daisychain** remain available when a device contains a
  `redundancy` element, even without an IPv4 section.
- The IP tab clearly explains why it is unavailable when no `ipv4_address`
  element exists. DCE preserves the XML and does not invent that element.

## Easy Patch

- Select one or more Tx channels with Ctrl or Shift, then drop them on the
  first Rx channel to patch.
- The first Tx is assigned to the target Rx and the following Tx channels to
  successive Rx channels.
- Assignment is immediate, respects the selected replacement warning, and
  strictly keeps one source per Rx.

## Verification

- 651 Windows and shared XML-engine tests passed.
- 34 macOS/Avalonia tests passed.
- 13 macOS version and packaging tests passed.
- 7 guide tests passed, with visual review of the French and English renders.
- 5 license Worker tests passed.
- Windows Release build: 0 errors, 0 warnings.
- Real Windows installation and startup verified.

DCE remains an offline editor independent of Audinate. XML files produced by
this generation have already been imported successfully into Dante Controller
by the maintainer; this corrective release adds no missing technical element.
