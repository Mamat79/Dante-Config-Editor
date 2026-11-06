# Dante Config Editor 2027.2.14 — macOS qualification

Apple Silicon and Intel DMGs were produced by one private manual Codemagic build from the same frozen product source as the qualified Windows installer. Each DMG contains 508 files, a self-contained .NET 10.0.12 runtime, the corrected banks and the matching French/English manuals and quick starts. The eight public Mac files are taken from these qualified packages; private QA and diagnostic receipts are not public assets.

New automated results: 1,004 Core tests, 88 Mac headless interface tests and 25 Python version/tooling checks passed, with no failures or skips (1,117 distinct automated checks). These include bank add/export/readback and empty-role regressions, Create-action behavior and patch-cell fill. They do not constitute installed desktop UI acceptance of those workflows.

On native Apple Silicon, the actual app from a read-only mounted DMG completed three nonvisual startup, synthetic XML and StageFlow launch scenes with graceful exit code 0. Five synthetic inputs remained unchanged. Native Keychain write, offline cross-process read/restart and scoped cleanup passed against the same Core component embedded in the app. HTTP activation, refusals and save failures were simulated in process; no commercial or customer activation was used.

Limits: Apple Silicon launch checks are nonvisual; no complete visual acceptance or actual installed bank-add/Create/export walkthrough is claimed. Intel was cross-built and inspected statically, without a physical Intel execution or Intel Keychain test. macOS 15 is the declared package minimum, without an execution matrix of old macOS versions. Strict ad-hoc signatures were checked on the Mac runner; packages have no Developer ID signature or notarization. The historical native detach cleanup suppresses errors, so a clean unmount was not independently established. No real-user-profile upgrade, live Dante network, physical device, audio, phone or LAN acceptance was performed. Work on copies and review exported XML in Dante Controller.

Apple Silicon DMG: 82,557,434 bytes; SHA-256 c7d740a7c75676af7b6cdafa332c09f6e010ccfe76b762ee23bc4d6bcf9c15b7.

Intel DMG: 84,354,778 bytes; SHA-256 4e3538d5d10d408a0b70ebdad39b785a2cdd8ef9ead2eb95c820136574a1d89a.

Les essais natifs Apple Silicon portent sur le démarrage et la fermeture normale, sans recette visuelle complète. Intel reste qualifié statiquement. Les contrôles Trousseau utilisent le composant réellement livré, avec échanges d’activation simulés. Aucune installation personnelle Mac ni validation sur matériel Dante n’est revendiquée.
