# Dante Config Editor V3.6 - development

[Notes de version françaises](RELEASE_NOTES.md)

## Status

V3.6 is a Windows and macOS development version based on V3.5. Dante Config Editor remains an unofficial third-party tool, not affiliated with Audinate. Work on a copy and always validate generated XML by importing it into the actual Dante Controller version in use.

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
- Transactional insertion of an independent instance from a template.
- Experimental minimal 3.0.0 new project, empty or seeded from a template.

## Diagnostics and interface

- Daily technical logs available from the application.
- Equivalent Windows/macOS commands for duplication, bank administration, template insertion, and project creation.
- Existing patch, zoom, rename, Enter, Tab, and Shift+Tab behaviors remain covered.

## Automated validation

- 258 Core/Windows tests passed.
- 16 headless Avalonia/macOS tests passed.
- Windows and macOS Release builds completed without warnings.
- The NuGet audit command reported no vulnerable packages.

## Distribution

- Self-contained Windows x64 installer: `DanteConfigEditorV3_6_Installer.exe`, including .NET 8 and FR/EN guides.
- V3.6 upgrades the V3.5 development line and leaves stable V3.4.2 untouched.
- macOS packages are planned for Apple Silicon and Intel under the V3.6 name.

## Limitations

- No V3.6 output has yet been imported into Dante Controller: automated tests and structural comparisons are not a field guarantee.
- A generic preset role is not a physical Dante device identity.
- Complete project creation remains experimental.
- The Windows installer is not Authenticode signed.
- Mac DMGs are ad hoc signed without an Apple Developer ID certificate or notarization.
