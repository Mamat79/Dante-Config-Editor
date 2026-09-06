# DCE Windows 2027.0.2 - connection center qualification

Updated 2026-09-06. The Windows 2027.0.2 replacement was approved on the
combined automated, package, document and bounded native evidence below.
The uncompleted native center checks remain explicit limitations, not passes.

## Windows Package

- Installer: `DanteConfigEditor2027_Installer.exe`.
- Version: `2027.0.2`, executable file version `2027.0.2.0`.
- Size: 85,621,243 bytes.
- SHA-256: `987a9d80dc37741263d465d4958d4eb7bb25cb24b0f4e4c3a091c22f5b39aae6`.
- Extracted application SHA-256:
  `87c3d080ef331ab70d3490ee3452ba6e1d5d6d560f01f6b9fb2c3d8d27d48e6b`.
- Private source revision: `96f169f3531e1625166f12dea070ba899024a927`.

The previous package and all release assets were backed up and checked
before preparation. No previously released Mac package was rebuilt or renamed.

## Checks Performed

| Check | Result |
| --- | --- |
| Core / Windows automated tests | 643 passed |
| Avalonia headless tests | 34 passed on Windows; not native Mac acceptance |
| Licence Worker tests | 5 passed; no deployment or licensing change |
| Windows Release build | 0 warnings, 0 errors |
| WPF connection layout | 12 off-screen cases, FR/EN, light/dark, three sizes |
| WPF modeless lifecycle | 12 additional cases, synthetic callbacks only |
| Installer contents | Extracted without installing; all 266 adjacent files match the candidate |
| Single-file application | 453 embedded entries inspected, including .NET/WPF and application assemblies |
| Windows manuals | 48 pages, 49 bookmarks and 149 annotations in each language |
| Quick starts | One page per language |
| Supplied banks | ZIP files identical to the previous public hashes |

The lifecycle checks exercise the actual connection window handlers for success,
refusal, code error and closing during a pending operation. Separate checks call
the real MainWindow connection wrapper with a synthetic HTTP handler and verify
that editing returns to its original enabled or disabled state after failure
and cancellation. They do not show the application or contact a network host.

The XML editor, transport protocol, personal banks and licence formats are
unchanged by this lot. Licences are not reset and no payment service is deployed.
The four Windows PDFs were regenerated and rendered for review. Only the
connection page changed in the complete guides; all other text and embedded
images match the previous documents. No native application screenshot was fabricated.

## Remaining Acceptance

This exact candidate was launched without a project. One native main-window
capture is complete and nonblank in French/dark mode at 1920x1032 logical pixels.
The header is readable and the empty Project page has no central scrollbar.
The connection center was also observed in the accessibility tree. However,
input-geometry and overlapping-window errors prevented completion of its
native mobile/Return checks. The center itself was not captured. Full native
focus, light-theme, English and DPI acceptance is not claimed by these checks.

The owned candidate was closed normally. All 56 pre-existing profile files
remain; only the licence state's normal LastSeenAtUtc timestamp changed. No
project, personal bank, licence contents or user preferences were modified.
Installation on the author's PC is checked separately after publication. The
previous installation's results must not be presented as installation acceptance
of this new binary hash.

The accepted Mac Apple Silicon and Intel packages remain **2027.0**. This
Windows replacement claims no new native Mac build, Dante Controller trial,
physical-network test, signing certificate or Apple notarization.

Source code and development history remain private. Public delivery is limited
to installers, documentation, media and verified checksums.
