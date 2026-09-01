<p align="center">
  <img src="media/dce-app-icon.png" width="112" alt="Dante Config Editor icon">
</p>

<h1 align="center">Dante Config Editor</h1>

<p align="center">
  <strong>Prepare and edit Dante configurations offline, from one clear view.</strong>
</p>

<p align="center">
  <a href="https://github.com/Mamat79/Dante-Config-Editor/releases/latest/download/DanteConfigEditor2026_10_Installer.exe"><strong>⬇ Windows</strong></a>
  ·
  <a href="manuals/Notice_DanteConfigEditorV3_EN.pdf">Full guide</a>
  ·
  <a href="manuals/SiLeMIO-Suite-Guide-EN.pdf">Suite guide</a>
  ·
  <a href="README.md">Français</a>
</p>

---

## What is Dante Config Editor for?

Dante Config Editor, or **DCE**, is an offline preparation tool for Dante audio
networks.

It opens Dante Controller XML files and provides a clear view of devices, TX/RX
channels and subscriptions. You can rename items, prepare the patch, merge
projects and add reusable device models without connecting the actual hardware.

DCE is useful for preparing an installation before arriving on site,
documenting an existing network, applying repetitive changes and checking a
file before opening it in Dante Controller.

[![Dante Config Editor overview](media/en/overview.png)](media/en/overview.png)

## A typical workflow

1. Open the reference Dante XML file.
2. Review devices, TX/RX channels and subscriptions.
3. Rename roles and channels individually or as a series.
4. Prepare routing in the patch matrix or with Easy Patch.
5. Add missing devices from the reusable catalogue.
6. Run the project checks.
7. Export the final XML and open it in Dante Controller before operation.

This keeps on-site time focused on the real system: cabling, clocking, network
status and audio checks.

## Main features

### Understand the whole installation

DCE brings together the devices, channels, subscriptions, audio formats,
latencies, sample rates, Preferred Masters and network information available
in the file.

### Rename quickly

- Rename devices and channels directly.
- Apply numbered series to a selection.
- Preserve leading zeroes.
- Continue stereo pairs such as <code>FX-1L</code>,
  <code>FX-1R</code>, <code>FX-2L</code>, <code>FX-2R</code>.
- Duplicate and reorganise roles without rebuilding the project.

[![Device settings](media/en/devices.png)](media/en/devices.png)

### Prepare routing

- Compact patch matrix.
- Easy Patch by selection or 1:1 range.
- Single, vertical and diagonal patch actions.
- Find the source connected to an RX channel.
- Display every destination of a TX channel.
- Flip the displayed RX/TX devices.
- Reset only the required part of a patch.

[![Patch matrix](media/en/patch.png)](media/en/patch.png)

### Merge projects

A second XML file can be added to the open project. DCE helps resolve naming
conflicts and reuse or rename imported roles.

### Work without the devices

The device catalogue provides reusable console, stagebox, interface, amplifier
and network-audio profiles for offline preparation.

[![Device catalogue](media/en/device-bank.png)](media/en/device-bank.png)

### Check and document

- Pre-export project checks.
- Errors and warnings grouped by severity.
- TXT and PDF reports.
- Patchbooks and before/after comparisons.
- Label exchange with Excel, CSV, JSON and ODS.
- Synoptic export to PDF or SVG.

[![Dante Config Editor synoptic](media/en/synoptic.png)](media/en/synoptic.png)

## One project with the SiLeMI/O suite

DCE uses the <code>.stageflow</code> folder as its native project. DCE can
create and open it on its own; StageFlow is optional.

The same project can later be used with Save My Time, StageMark, StageFlow,
StageMon and AutoCAD.

### Map a StageFlow patch to Dante RX channels and follow it LIVE

A StageFlow patch group can directly name all or part
of a Dante device's RX channels:

1. open the `.stageflow` project in DCE;
2. choose the group and naming mode: source, microphone, source + microphone,
   or StageFlow label;
3. choose the device, first RX channel, and number of channels;
4. review the Before / After preview, then apply.

DCE resolves common pairs, group overrides, and hidden pairs. Empty cells are
ignored, and nothing changes before confirmation. This workflow also works
when DCE is used on its own, without StageFlow.

When StageFlow is open on the same project, DCE 2026.10 recognizes its
local LIVE session and can follow linked RX changes through explicit UUIDs.
Connected, standalone, and conflict states remain visible. A missing rule,
unsaved local work, or hash conflict rejects the complete transaction and
retains the last valid state. DCE does not control the Dante network and only
updates its own offline domain.

[![One project, several tools](media/stageflow-suite-workflow.svg)](media/stageflow-suite-workflow.svg)

- [StageFlow — patch lists, groups, Excel and stage plan](https://github.com/Mamat79/SiLeMIO-StageFlow-Distribution/releases/latest)
- [Save My Time — transfer between consoles and software](https://github.com/Mamat79/Save-My-Time-SMT/releases/latest)
- [StageMark — placement and projection](https://github.com/Mamat79/StageMark/releases/latest)
- [StageMon — live monitoring matrix](https://github.com/Mamat79/StageMon/releases/latest)

## File formats

- **<code>.stageflow</code> project**: native project shared across the
  SiLeMI/O suite.
- **Dante XML**: exchange file intended for Dante Controller.
- **Legacy <code>.dceproj</code>**: previous DCE project format, still
  supported.
- **Device catalogue**: reusable models for offline preparation.

## Download and start

The current **2026.10** release is available for **Windows 11 x64**.

| Resource | Link |
|---|---|
| Windows installer | [Direct download](https://github.com/Mamat79/Dante-Config-Editor/releases/latest/download/DanteConfigEditor2026_10_Installer.exe) |
| Quick start | [English PDF](manuals/QuickStart_DanteConfigEditorV3_EN.pdf) |
| Full guide | [English PDF](manuals/Notice_DanteConfigEditorV3_EN.pdf) |
| Suite guide | [English PDF](manuals/SiLeMIO-Suite-Guide-EN.pdf) · [PDF français](manuals/Guide-Suite-SiLeMIO-FR.pdf) |
| Community device catalogue | [Download from the latest Release](https://github.com/Mamat79/Dante-Config-Editor/releases/latest) |

The macOS interface retains the same workflow and passes the automated tests,
but the 2026.10 DMGs will be added after the GitHub macOS runner is restored.

The **Discover DCE** screen provides direct access to opening an XML file,
creating a project, browsing the catalogue and reading the guide.

## Use and license

DCE starts with 30 reminder-free days. Afterwards, the application and all its
features remain usable; only a startup reminder is displayed.

A permanent **€29 tax-included** one-time license removes this reminder.

**[Buy a permanent DCE license](https://dce-license.mamat79-dce.workers.dev/buy)**

## Before using a configuration

DCE is an independent third-party tool and is not affiliated with Audinate. It
prepares files offline and does not directly control a Dante network.

Work on a copy, open the exported XML in Dante Controller and verify the
configuration on the real hardware before an important production.

---

<p align="right">
  <strong>SiLeMI/O</strong><br>
  By Mamat<br>
  <code>-------[]--</code>
</p>
