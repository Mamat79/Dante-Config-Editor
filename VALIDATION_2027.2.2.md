# DCE 2027.2.2 validation

- Private source: 292447e, tag v2027.2.2.
- Shared tests: 717 passed, including updater version and rate-limit regressions.
- macOS/Avalonia tests: 42 passed.
- Licensing worker: 5 passed; licensing behavior unchanged.
- Mac packaging metadata validator: 13 passed.
- Windows Release build: no errors or warnings. Self-contained Inno installer
  built from a clean detached checkout of the release commit.
- Windows installed successfully (installer exit 0); executable reports
  2027.2.2 and starts with a responsive native main window.
- Codemagic build 6ab1b34e50d35310b248a28a: success. Shared/Mac tests, both DMGs,
  Apple Silicon native smoke test, Intel architecture checks and checksums.
- Eight regenerated FR/EN PDF guides. Updated instruction pages rendered and
  visually checked; quick-start layout was checked in the preceding draft.
- Package hashes verified against their companion checksum files.

No project XML, matrix, multicast or license behavior changed in this fix.
Rate-limit behavior and retry persistence are covered by automated HTTP tests;
this does not remove GitHub service limits or guarantee service availability.
Automated tests are not a new claim of live Dante network or Controller import
validation. Mac packages are not Developer ID signed or notarized; physical
Intel runtime and a full manual Mac UI acceptance test were not performed.

See DOWNLOADS.json and SHA256SUMS.txt for final package sizes and checksums.
