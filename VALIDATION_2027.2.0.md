# DCE 2027.2.0 - Validation

Qualification: 2026-09-21. Source private commit `ac11d3a`, tag `v2027.2.0`.

| Platform | File | Bytes | SHA-256 |
| --- | --- | ---: | --- |
| Windows x64 | DanteConfigEditor2027_Installer.exe | 85440870 | f4bfd64c4d509113bf28967ee94c9a0314a9034b6c47ba6700025cf273128df4 |
| Apple Silicon | DanteConfigEditor2027_macOS_AppleSilicon.dmg | 79063524 | 04413a406f744f1dba8dfe94a8288e8deede52c98e9112b8cf3c4d04db12cfa8 |
| Intel | DanteConfigEditor2027_macOS_Intel.dmg | 80393742 | abde475b10c434a9b913e133e6ab9e0c9a17d60a6fd89e5dbac604651a5560b6 |

- 702 shared-engine tests passed, including portable filenames and multicast XML round trips.
- 42 Avalonia tests passed, including seven new project matrix tests: pointer preview pixels, cancellation, diagonal and fan-out, flip, inline rename and fill handles, multicast, and Easy Patch integration.
- 129 native Windows checks passed in a regular release build, including real transaction undo/redo and compact/high-DPI rendering.
- 5 license-worker tests passed. License formats, keys, price, and stable profile remain unchanged.
- Windows Release build completed without errors or warnings, from an isolated checkout of the committed source, without the test-profile flag.
- The Windows installer completed successfully. Installed executable version 2027.2.0.0 was launched and responded normally.
- Existing local installations and profiles were archived and all archive entries hash-verified before installation. Test-project recovery was copied into the stable profile.
- Windows FR/EN full manuals: 47 pages each; Mac FR/EN: 23 pages each. All four quick starts fit one page. New matrix and multicast pages were rendered and visually reviewed.
- Codemagic build `6ab0eb42dcbe25a2513d3815` completed successfully on 2026-09-21 at 08:34:57 UTC: shared and Mac tests, both builds, Apple Silicon native smoke, Intel architecture checks, DMG checksums, and draft-release upload.
- Both DMGs were downloaded again from GitHub and matched their published checksum files.

## Limits

DCE remains an offline, unofficial third-party editor. No live Dante control is claimed. Check final XML in Dante Controller and on the target hardware. Simple Dante audio multicast flows in preset 3.0.0 are supported; unknown formats remain read-only. An empty preset flow list does not guarantee removal of active network flows.

Mac packages are not Apple Developer ID signed or notarized. Intel qualification covers cross-compilation and architecture, not a physical Intel Mac runtime test. Automated Avalonia captures are not a physical Mac visual acceptance test.
