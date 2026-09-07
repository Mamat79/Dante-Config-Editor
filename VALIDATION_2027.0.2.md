# DCE Windows 2027.0.2 - guide and help update

Qualified on 2026-09-07. This same-number Windows replacement updates the
Guide / Help entry points and documentation. Mac packages are unchanged.

## Package

- Installer: `DanteConfigEditor2027_Installer.exe`, 85,619,660 bytes.
- SHA-256: `4cf63d543e9edd6bcc4cebb3ea1706bcdaef356827c463765ee425201faa11db`.
- Application: 73,445,970 bytes, file version `2027.0.2.0`.
- Application SHA-256: `50c8e42e56eb660ccfdf5e16d39e9c80ecf5f839b04997791c15921538e6e622`.
- Built from private source commit `5a6b3b1d6b1c6c90cf3f55472b59e6c10388dd2a`.
- Standalone Windows x64 installer; .NET is included.

The previous 29 release assets were backed up and checked before replacement.
Existing Git tags are preserved; this document identifies the exact new binary.
Source code and development history remain private. Only compiled packages,
public documentation, media and checksums are distributed publicly.

## Tests and Documents

| Check | Result |
| --- | --- |
| Core / Windows automated suite | 643 passed, none skipped |
| Targeted UI and localization tests | 57 passed |
| Licence Worker tests | 5 passed; no deployment or payment |
| PDF document tests | 7 passed |
| Windows Release build | 0 warnings, 0 errors |
| Extracted and installed payload | 268 of 268 files match |
| Complete Windows manuals | 46 pages per language; 49 destinations, 99 link annotations |
| Contents | One clickable contents page per complete manual |
| Quick starts | One page per language |
| Shared suite guide 2027.3 | 14 pages per language; byte-identical approved PDFs |

All 94 product-document pages were rendered and reviewed using contact sheets,
with full-size checks of contents and dense pages. Existing workflow screenshots
were retained; they are not claimed to be new native captures of every updated
screen. The shared guide owner reports page-by-page visual review; DCE verified
the copied bundle against the upstream manifest.

Initial document-tool and obsolete-link-test failures were corrected before the
successful final runs above. The XML engine, routing, network transport, banks
and licensing are not changed by this update.

## Installed Application

The installer completed its upgrade on the author's PC with exit code 0.
The existing historical installation directory remains
`C:\Program Files\Dante Config Editor 2026.3`; the executable reports 2027.0.2.0.
All 268 payload files, including the six PDFs and 79 machine templates, match.
One desktop shortcut targets the installed application.

The installed application was launched in French/dark mode without a project.
Its **Guide** button opened the French 46-page product manual in Adobe Acrobat.
The document and DCE were then closed normally. This proves the installed Guide
workflow, not every feature, language, theme or display scaling configuration.

All 846 existing Documents files remain byte-identical. The 56 profile files
were unchanged by installation; after normal startup, only the licence state's
`LastSeenAtUtc` property changed. No project, personal bank or licence was reset.
Pre-existing non-payload installation files were retained, not silently deleted.

## Limits

No new native Mac build or test, Dante Controller import or physical Dante
network test was performed for this Windows documentation/UI lot. Existing Mac
Apple Silicon and Intel packages remain 2027.0, with their original assets.
Windows signing and Apple notarization are outside this update.

Publication readback checks sizes and SHA-256 through real public downloads;
unchanged assets and the Mac release are compared against the saved inventories.
