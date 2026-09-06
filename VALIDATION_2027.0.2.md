# DCE Windows 2027.0.2 - release verification

Updated: 2026-09-06. This report replaces earlier candidate notes that
contained obsolete packaging hashes and contradictory delivery statements.

## Published Windows package

- Stable release: [v2027.0.2](https://github.com/Mamat79/Dante-Config-Editor/releases/tag/v2027.0.2).
- Installer: `DanteConfigEditor2027_Installer.exe`.
- File version: `2027.0.2.0`.
- Size: 85,710,301 bytes.
- SHA-256: `65d692ba2f41057e13eafff589a820fab70485883c408561a3ac2d41a032dd20`.
- The local installer, public repository copy and GitHub release asset match.
- The executable asset is unchanged during this documentation correction.

## Checks

| Check | Evidence |
| --- | --- |
| Core / Windows tests | 638 passed, 0 failed, 0 skipped in the final delivery run |
| Avalonia tests | 33 headless tests passed in the final delivery run; this is not native Mac QA |
| Licence Worker tests | 5 passed; no Worker deployment or licensing change |
| Layout component probe | 80 cases, FR/EN and light/dark, including resize back to a large viewport |
| Large Home viewports | No outer scrolling at tested central viewports 1280x838 and 1120x740 logical units |
| Small Home viewports | Scrolling fallback keeps commands reachable at 900x630, 640x430 and 440x340 |
| Header alignment | Theme and Language control/chrome heights match at 36 logical units; no inherited internal margin |
| Windows manuals | 48 pages, 49 bookmarks and 149 link annotations in each language; no text outside page bounds |
| Quick starts | One page each, FR/EN |
| Shared guides | Exact approved suite bundle 2027.2, with matching manifest hashes |
| Installation on the author's PC | Final upgrade pending; Windows currently declares 2027.0.1 |

The XML export tests exercise preservation of project history and unsaved state,
invalid extensions, source protection, foreign StageFlow domains and Windows
junctions. XML is exported through the existing writer. The layout correction
does not change the XML mutation engine, personal banks or licence formats.

## Documentation provenance

The four downloadable Windows PDFs correspond to the final 2027.0.2 documents.
Earlier copies attached to the release were outdated and have been replaced.
Their hashes are listed in `SHA256SUMS.txt`.

The shared French guide is 405,754 bytes with SHA-256
`e438d52a1012e2c91f9aaf935dc9db16ee77e9989eda692a26a27ac9bf75fc49`.
The English guide is 363,318 bytes with SHA-256
`104d554c08f886032345eef7ec98cfbf2dddb283d6edee55e044cb3206953dbc`.
The approved suite manifest SHA-256 is
`40ae77c353922cb3bb2faf5be0f2aff86a0bb6c72e22cb41a6fc8c345e0f6bf9`.

## Boundaries

Earlier native checks opened the candidate, imported a synthetic XML fixture,
cancelled the export preflight and opened the StageFlow connection dialogue.
Automated captures of the main WPF client and preflight were white, while
the connection dialogue and the suite PDF rendered. Their cause was not
determined. Component renders in the manuals are explicitly labelled and
must not be mistaken for native installed-application screenshots. Full native
DPI and focus acceptance at every scale is not claimed by component tests.

The accepted macOS Apple Silicon and Intel packages remain **2027.0**, with
their existing platform-specific downloads. This Windows correction does not
relabel or rebuild those packages. No new physical Dante network or Dante
Controller trial, code-signing certificate or Apple notarization is claimed.

Private source code and development history remain in the private repository.
The public repository contains distribution files and public documentation.
