# Dante Config Editor V3.6.1

[Notes de version françaises](RELEASE_NOTES.md)

## Status

V3.6.1 fixes the V3.6 support screen on Windows and macOS. Dante Config Editor remains an unofficial third-party tool, not affiliated with Audinate. Work on a copy and always validate generated XML by importing it into the actual Dante Controller version in use.

## XML safety and fidelity

- Targeted edits to the original XML preserve unknown nodes, attributes, namespaces, ordering, and values.
- Stronger validation of identities, channel references, subscriptions, network structures, and node additions.
- Atomic Save as with temporary-file reload, semantic comparison, and backup of an existing destination.
- Import/export/import cycle, default namespace, Unicode, unknown-value, and large-preset tests.
- Ten local XML files covering 176 devices and 5,004 labels were loaded, validated, saved, semantically compared, and reloaded without changing the originals.

## Duplication and machine bank

- Duplicate a device as an independent generic preset role.
- Hardware `instance_id` and `device_id` values are never copied or invented.
- Network data, subscriptions, flows, Preferred Master, and sensitive settings are excluded by default.
- Versioned, shareable bank with metadata, tags, editable labels, and an optional copied PNG/JPEG/WebP image.
- Search, filters, edit, duplicate, confirmed delete, ZIP import/export, and complete bank backup/restore.
- Bilingual GitHub catalog and verified `*.dce-bank.zip` archive for downloading or sharing a complete bank.
- Included `DCE Generic Roles 3.6` bank with generic 8x8 and 32x32 roles and no hardware identity, network data, or subscription.
- Optional illustrated and sanitized `DCE Community Devices 3.6` bank with Yamaha QL1 and Rio1608-D2, Fohhn DI4.1000, Lake LM 44, RME Digiface Dante, Glensound Divine, Beatrice D8 and AOIP22, and Allen & Heath SDante 64x64.
- Transactional insertion of an independent instance from a template.
- Experimental minimal 3.0.0 new project, empty or seeded from a template.

## Diagnostics and interface

- Daily technical logs available from the application.
- Equivalent Windows/macOS commands for duplication, bank administration, template insertion, and project creation.
- Existing patch, zoom, rename, Enter, Tab, and Shift+Tab behaviors remain covered.
- XML comparison, statuses, and results now follow the active language on both Windows and macOS.
- `Support DCE` is available from `Safety and log` in French and English, with the PayPal QR code and a PayPal.Me button for computers.
- The optional reminder appears after 20 launches, can be postponed or permanently disabled, and remains strictly local with no telemetry or startup network request.

## Automated validation

- 283 Core/Windows tests passed.
- 20 headless Avalonia/macOS tests passed.
- Windows and macOS Release builds completed without warnings.
- The NuGet audit command reported no vulnerable packages.

## Documentation

- Complete 26-page French and English guides with annotated screenshots.
- Detailed workflows for profiles, direct and series renaming, Patch, Easy
  Patch, FLIP, PATCH 1:1, merging a second XML file, device banks, label
  import/export, synoptic generation, file review, and final saving.

## Distribution

- Self-contained Windows x64 installer: `DanteConfigEditorV3_6_Installer.exe`, including .NET 8, FR/EN guides, and support documentation.
- Separate choice of active-bank and included-bank folders; the generic and community banks can be selected independently without replacing existing banks.
- macOS DMGs contain both bank archives in a `Machine Banks` folder.
- V3.6 upgrades the V3.5 development line and leaves stable V3.4.2 untouched.
- macOS packages are planned for Apple Silicon and Intel under the V3.6 name.

## Limitations

- No V3.6 output has yet been imported into Dante Controller: automated tests and structural comparisons are not a field guarantee.
- A generic preset role is not a physical Dante device identity.
- Complete project creation remains experimental.
- The Windows installer is not Authenticode signed.
- Mac DMGs are ad hoc signed without an Apple Developer ID certificate or notarization.
