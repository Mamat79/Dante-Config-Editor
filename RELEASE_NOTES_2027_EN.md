# Dante Config Editor v2027 for macOS

[Notes de version en français](RELEASE_NOTES_2027.md)

This stable release provides **macOS Apple Silicon and Intel** applications
only. Windows remains on **2026.10**; its Release keeps the GitHub Latest
designation and none of its files are replaced.

## One StageFlow LIVE connection center

- Always accessible from the top bar, regardless of the current page.
- A full-window connection center on macOS, in English and French.
- Available sessions with project, host computer, IPv4 address, and shared functions.
- Manual private IPv4 address and port when local discovery is unavailable.
- Six-digit codes preserve leading zeros and support pasting spaces or a hyphen.
- Errors keep the window open so the code can be corrected and retried.
- Explicit disconnect; no automatic rejoin after a connection loss.
- The last local working state remains available if the host disappears.

The selected session is rechecked immediately before sending the code.
Compatible hosts also receive the expected project and session identifiers.
A late heartbeat response from an old connection cannot revive it or replace a new one.

## Three distinct workflows

Change notifications use an orange banner on every screen: affected item,
previous/current label, origin, and time. Acknowledge and Acknowledge all are
local to this computer and leave later arrivals pending. A pause requested
by StageFlow is shown without interrupting LIVE or asking the user to rejoin.

Standalone DCE projects, local StageFlow folders, and temporary LIVE sessions
remain separate. StageFlow is free and optional. DCE remains an offline Dante
configuration editor, not a live Dante hardware controller. Use this LAN
connection only on a trusted local network.

On Mac, the initial window respects the available work area and display scale.
Small displays have fallback scrolling; valid window dimensions and positions
on a larger screen remain unchanged.

Starting from scratch, device banks, XML merging, series renaming, patching,
synoptic views, and StageFlow-patch-to-RX mapping are retained. This update
does not change the XML model, other applications' domains, personal banks,
or existing licenses.

## Guides and installation

All four French/English guides and quick starts are updated. The complete guide
includes screenshots of the new center, the connection steps, and troubleshooting.

The shared SiLeMI/O suite guides, in English and French, are also included in
the Mac applications.

Separate macOS packages are provided for Apple Silicon and Intel. No Windows
v2027 installer is included here. The v2027 guides also cover the Windows
interface in preparation; the Windows 2026.10 guide remains in its Release.

**v2027** is the public Mac name, **v2027.0** the technical tag, and
**2027.0.0.0** the binary version. Use the README's direct DMG links: while
Latest remains v2026.10 for Windows, an older updater may not offer this
Mac release.

## Verification limits

Both packages were built and launched on separate native macOS environments,
Apple Silicon and Intel. Each architecture passed **579 shared-engine tests
and 29 Mac UI tests**, with no failures. The packaged application was launched
from the mounted DMG with no file, an XML file, and a StageFlow folder. All
five test input files remained byte-identical. Both embedded shared guides
were checked against their approved hashes.

Native screenshots and the French/English light/dark alert banner were
reviewed. At 1024 × 768, some controls require fallback scrolling; the Mac
layout is not identical to Windows. Intel startup emitted Skia/Metal shader
compilation warnings without terminating the application; the final captures
were complete and subsequent XML/StageFlow launches emitted no errors.
This is not exhaustive testing on every Mac model.

The DCE client was tested against a real StageFlow host on the same PC. This
does not replace a two-computer test or physical Dante hardware validation.
Build/test evidence and platform-specific limits are listed in the publication
report. The Mac packages are not yet notarized by Apple.

The local StageFlow console that controls applications on the same computer
remains Windows-only. On Mac, DCE opens local projects and joins LAN sessions
through its connection center.
