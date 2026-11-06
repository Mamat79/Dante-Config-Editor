# Dante Config Editor 2027.2.14

## 6 November 2026 visual update — matrix

Patched connections now fill their entire cell in green. The preview fills the
cell in amber before committing. The fill and check mark follow grid density
and zoom on Windows and macOS; hover and selection outlines remain visible.
Patch gestures and XML data retain their existing behavior. Release version: 2027.2.14.

## 2027.2.14 fix — creation and device bank, 2 November 2026

**Create a configuration** and **Open a configuration** are now equally visible
starting choices in the main toolbar. Create opens an empty draft without requiring
a destination file: add devices from the bank, edit as needed, then export the XML
for Dante Controller. Existing confirmations protect unsaved work and pending
configuration additions.

Adding CL3, QL5, HY144-D or RU-LB4 was blocked because their supplied templates had
channels without `mediaType`. These four entries now use explicit generic audio
roles with catalog channel counts and no technical hardware identity. They are
marked **structurally validated**; this correction does not claim new hardware
testing. XML guards remain active and personal banks are preserved. A regression
test adds, exports and reloads each of the 85 currently supplied models.

Windows and Mac checks and their limits are documented separately in this release qualification reports.
