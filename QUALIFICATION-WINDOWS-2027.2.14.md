# Dante Config Editor 2027.2.14 — Windows qualification

Windows 11 x64 self-contained installer, built with the official installer/build_installer.ps1 recipe and .NET 10.0.12. This report distinguishes source tests, packaged content and observed installed behavior.

Installer: DanteConfigEditor2027_Installer.exe, 78,794,132 bytes.
SHA-256: 186766859f0458db68929bc5ddd022eaafc0bdc958614ba73c6e462dd18bb585

The installer completed normally with exit code 0. All 283 installed application files matched the qualified payload. The installed application was launched with a synthetic, non-production XML and closed normally with exit code 0. Existing application location, upgrade identity and user-data paths were retained. Backups and recovery copies were checked before installation; personal banks were not replaced. After the installed UI trial, no preexisting personal file was missing; the two changed runtime-state files were preserved separately and previous recent-file entries remained present.

Source qualification supplied by the product owner: 1,004 Core tests, 88 Mac interface tests and 5 worker checks passed. The cell-fill change has 217 WPF assertions and separate 24-case WPF and 24-case Avalonia pixel checks covering two themes, four densities and three zoom factors. The Windows build added 20 passing version-contract tests. These automated checks are separate from the following native observations.

In the actual installed Windows application, the detached patch matrix displayed full green cells and check marks at Fine 14, Dense 18, Airy 24 and Comfort 40 densities. Selection outlines remained visible; three existing patches were unchanged and no patch edit was applied. The synthetic XML stayed byte-identical. Density 18 was restored.

Limits: the main-window screenshot was white, while the detached matrix using the same installed view was readable. Ctrl+wheel zoom, held-pointer amber preview and Escape cancellation during that preview were not exercised natively because the available UI tool cannot retain those modifier/button states. Their source pixel/behavior checks are recorded separately above. The native trial covered the dark theme, not every language, Windows scaling value or hardware configuration. The new generic CL3, QL5, HY144-D and RU-LB4 models are structurally validated; no new physical device or live Dante network compatibility is claimed. No audio/network control is performed by DCE. The installer is unsigned.

Les observations natives portent sur la matrice détachée réellement installée et ses quatre densités. Le zoom et l'aperçu orange sont couverts par les contrôles source distincts, sans être présentés comme des essais natifs. Les banques personnelles et les données existantes sont conservées.
