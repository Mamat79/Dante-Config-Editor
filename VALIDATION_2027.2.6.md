# DCE 2027.2.6 Validation

Windows and both Mac packages were built from the same qualified source.
The public tag targets a separate distribution-only commit, without source.

- 755 shared tests, 50 Mac headless tests and 5 licensing tests passed.
- Windows Release build: no errors or warnings.
- Native hidden Windows tests checked the expired-trial countdown 5,4,3,2,1,0
  in French and English, plus active-trial and valid-license bypass.
- The exact Windows installer payload was extracted and startup checked.
- Eight FR/EN Windows/macOS PDF guides were regenerated and reviewed.
- Codemagic build `6ac2749537f0308a64ebbe2f` passed shared/Mac tests,
  Apple Silicon startup/XML/StageFlow checks and Intel architecture checks.
- All release assets were checked against local size and SHA-256 values.

Limits: no native Intel runtime, physical/manual GUI acceptance, dedicated
native Mac expired-license countdown test, or live Dante network acceptance.
Mac packages are not Apple Developer ID signed or notarized.
Local installation is a separate operation and is not claimed by publication.

See DOWNLOADS.json and SHA256SUMS-2027.2.6-all-platforms.txt.
