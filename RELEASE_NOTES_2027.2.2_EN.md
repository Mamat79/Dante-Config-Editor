# Dante Config Editor 2027.2.2

- Fixes repeated update offers when the current version is already installed:
  Windows and Mac read the running application's version.
- Replaces technical 403 rate-limit errors with an informational retry time
  on manual checks. Automatic startup checks remain silent.
- Respects the server deadline and persists it across restarts. DCE stays
  usable offline and never claims to be up to date after a failed check.
- Adds version and rate-limit regression tests. Matrix, multicast, projects,
  preferences and licenses are preserved.

2027.2.1 remained a draft; 2027.2.2 delivers both fixes.
Mac packages have no Developer ID signature or notarization. Native Apple
Silicon smoke test on Codemagic; Intel architecture verified, not physical Intel runtime.
