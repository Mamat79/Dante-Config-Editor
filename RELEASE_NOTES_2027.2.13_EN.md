# Dante Config Editor 2027.2.13 — Windows

## Windows 2027.2.13 — configuration preparation, 28 October 2026

The configuration name stays above four actions: **Open a configuration**,
**Add configurations** (optional), **Edit** (optional), and **Export for Dante
Controller**. You can go directly from opening to exporting, without combining
files or saving a DCE project. In Devices and Patch, the bar becomes compact and
existing tools remain available, including the dense matrix, Flip, renaming and
Multicast.

Additional files appear in a list separate from the starting file. Resolve
duplicates by keeping existing devices or renaming imported ones, then add the
whole batch. DCE also checks conflicts between queued files. A format, version
or namespace error, a file changed since selection, or cancellation leaves the
current project untouched. One undo removes the complete applied batch. Global
settings from the starting file remain authoritative; combining files imports
devices and their compatible structures without controlling the Dante network.

**Save preparation… (.dceproj)** is separate and optional. The DCE project retains
applied additions, their provenance and source protections, without automatically
reading external XML files on reopening. The pending list belongs to the session:
a warning precedes closing it or saving/exporting without those additions. Export
creates an XML copy under another name and retains existing validation.

Windows WPF and Mac Avalonia sources are synchronized. Windows delivery goes
through the Workshop with a backup before installation. Mac builds are planned
after the free quota renewal on 1 November; public 2027.2.12 Mac packages remain
available until their replacements qualify.

[Qualification Windows FR/EN](QUALIFICATION-WINDOWS-2027.2.13.md). macOS reste / remains 2027.2.12 : https://github.com/Mamat79/Dante-Config-Editor/releases/tag/v2027.2.12.
