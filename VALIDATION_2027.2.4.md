# DCE 2027.2.4 validation

- Private source commit: `57ae942`; source tag: `v2027.2.4`. The public
  distribution tag points to the separate code-free commit `60cf986`.
- Windows: 720 shared tests, 46 Mac interface tests, and 5 licensing tests
  passed locally. The Release build completed with zero warnings or errors.
- Eight French/English PDF guides were regenerated; guide checks passed.
- The autonomous Windows installer was built, its SHA-256 verified, installed
  locally, and the installed executable reported version 2027.2.4 and started.
- [Codemagic build 6abc09acc101571699173599](https://codemagic.io/app/6a9ebddbfd08389ea4f59252/build/6abc09acc101571699173599)
  succeeded: shared and Mac tests, both DMG builds, Apple Silicon smoke test,
  Intel architecture check, and checksum verification.
- Both uploaded DMGs were downloaded again and their SHA-256 compared with
  their companion checksum files and GitHub asset digests.

This does not establish import into Dante Controller or operation on a live
Dante network. The Mac packages are not Apple Developer ID signed or
notarized. Runtime testing on a physical Intel Mac was not performed.

See `DOWNLOADS.json` and `SHA256SUMS.txt` for the published package sizes
and checksums.
