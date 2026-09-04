# The SiLeMIO suite — Getting started

**From the first patch list to live operation, with one application or the whole suite.**

v2027 edition - user guide. Each application is delivered separately: install
only a release published for your platform and check its release notes.
Windows and macOS limitations are explained below.

StageFlow is free and optional. Every SiLeMIO application remains usable on its
own with its native project. When the same show must move between several
tools, every application can also create, open and save a StageFlow
`.stageflow` project, even when StageFlow is not installed.

Discover StageFlow and its published versions: [official StageFlow page](https://github.com/Mamat79/StageFlow).

## Windows and macOS availability

| Application | Windows | macOS |
|---|---|---|
| StageFlow | Native application* | Port in progress - not available |
| StageDesk | Native application* | Native Intel / Apple Silicon* |
| StageMark | Native application* | Native Intel / Apple Silicon* |
| StageMon | Native application* | Port in progress - not available |
| Dante Config Editor | Native application* | Native Intel / Apple Silicon* |
| AutoCAD | StageFlow 2026 connector for Windows* | StageFlow connector not available |

*A v2027 installer on the application's Releases page confirms availability
for that platform. A built native application or a Windows edition does not
guarantee a published Mac release. This guide does not replace release notes
or acceptance tests on your hardware.*

The StageFlow host currently runs on **Windows**. Compatible Mac editions of
StageDesk, StageMark and Dante Config Editor can join it over the local network
with its six-digit code, without installing StageFlow.

The launch cards and console shortcuts described here apply to the Windows
workstation. A control channel **on the same Mac** is implemented in StageDesk,
but not in StageMark or Dante Config Editor. Both can still join a
**StageFlow LIVE session** over the network. Only commands actually offered by
the connected application are enabled. StageFlow and StageMon for Mac remain
work in progress and are **not available**.

![SiLeMIO suite architecture](../media/ecosystem/suite-architecture-en.svg)

## Choose your workflow

The same three workflows use the same names throughout the suite:

- **<Application> project**: create, open and save the application's native
  project in standalone mode;
- **Local StageFlow project**: open a `.stageflow` project stored on this
  workstation; the StageFlow application is not required;
- **StageFlow LIVE session**: discover the show and host computer on the local
  network, then join explicitly with its six-digit code.

### I use one application

Create and save its native project as usual. You need neither StageFlow
nor the other applications. If you install another suite tool later, you can
create or open a `.stageflow` project from the application you already own.

### I want to share a show between applications

Create a `.stageflow` project from StageFlow, StageDesk, StageMark, StageMon
or Dante Config Editor. Each application writes only its own
part and preserves the work produced by the others.

### I want a central workstation

Open the project in StageFlow. Its console shows applications that are
installed, open, connected to the project or in LIVE mode. It can open them
directly on the current show and expose a few essential controls. Unused cards
can be hidden in **Settings** without changing the project.

## Recommended workflow

![Recommended workflow from preparation to live operation](../media/ecosystem/suite-workflow-en.svg)

Every step is optional: you may go directly from the patch to StageMark, use
StageMon alone, or prepare Dante without using StageDesk.

## 1. Start the patch

Three entry points are available.

### From StageFlow

1. Click **New**.
2. Name the show.
3. Freely choose the number of pairs and groups.
4. Fill **Common to all groups** with reusable sources.
5. Open each group and add its specific content.
6. Save the `ShowName.stageflow` folder.

Data entry follows Excel habits: **Enter** moves down, **Tab** moves to the next
cell, `Ctrl+C` and `Ctrl+V` copy and paste, and the fill handle extends a list
downwards.

### From Excel

1. In any compatible application, choose **Excel template** or **Create workbook**.
2. Enter the number of pairs and groups: the workbook creates one central sheet
   and one sheet per group.
3. Fill the workbook in Excel.
4. Open or import it in StageFlow, StageDesk or StageMon.
5. Save a new `.stageflow` project if you want to share it.

Every group sheet is configured to fit one landscape A4 page. The workbook
keeps invisible, stable identifiers so applications can recognize groups and
pairs after an edit.

### From another SiLeMIO application

Choose **New StageFlow project** in that application. It creates a valid StageFlow
project with an empty or existing patch, then adds its own domain. StageFlow
does not need to be running.

## 2. Organize common and group-specific content

**Common to all groups** contains reusable source, microphone and comment
values. In each group, the **Common** column decides pair by pair whether the
common value appears.

- **Show all** enables every common pair in the current group.
- **Hide all** removes them from this group without deleting the common patch.
- A local value replaces the common value only in that group.
- A hidden common pair with no local value remains blank on the group page and
  in its Excel/A4 output.

## 3. Prepare the other disciplines

### StageDesk

Use StageDesk to retrieve a console patch, edit labels, colours and useful
settings, then send it to another console or application. It can work on its
native project or on its own part of the `.stageflow` project.

### Dante Config Editor

Use Dante Config Editor to prepare Dante devices, TX/RX channels, patching and
validation offline. From a StageFlow project, patch lines can be associated
with a device's RX channels and reused as names. Dante Config Editor never
controls the live Dante network.

### AutoCAD and the stage plan

A simple stage plan can be created directly in StageFlow. For a more advanced
technical drawing, the AutoCAD connector opens the same project, places patch
sources and publishes geometry back into the CAD domain. StageFlow remains
fully useful without AutoCAD.

The connector distinguishes a standalone drawing, a **Local StageFlow
project** and a **StageFlow LIVE session**. For network LIVE, select the host
and enter its six-digit code. The received patch stays in memory; refresh is
explicit and only the CAD part is published. Acceptance in real AutoCAD is
still required for this version before live operation.

## 4. Move to live operation

### StageMark

Prepare layout, plans, marks and cues, then use **SHOW** and **BLACKOUT**. Local
StageMark safety always takes priority, including when the application was
opened from StageFlow.

### StageMon

Prepare assignments and inputs, then operate the default A/B monitor mixes.
StageMon may offer two to six circuits: C to F only appear after configuration.
StageFlow console shortcuts may clear a cue. A monitor muted locally
inside StageMon can never be remotely unmuted by StageFlow.

## 5. Use the StageFlow console

The lower section of StageFlow displays one card per application:

- **greyed out**: application is absent, with access to its download;
- **installed · closed**: ready to launch;
- **open**: detected but not yet linked to the project;
- **standalone · offline** (grey): there is no LIVE link;
- **available** (orange): a StageFlow LIVE session can be joined;
- **connected · synchronized** (green): same project and same LIVE session;
- **conflict** or **connection lost** (red): action is required before
  continuing.

On Windows, select several cards and click **Open selection** to launch the
tools you need. With a compatible local connector, an application that is
already open receives the project in its existing window. Mac limits are in
the availability matrix. Use **Settings** to hide cards
you do not use. **Compact console** reduces StageFlow to the suite controls;
return to the full window when you need to edit the patch.

## 6. Control the workstation with one QR code

1. Save the StageFlow project, then open the **StageFlow LIVE session** centre with
   the large status button at the top.
2. Under **Phones and tablets**, select the Wi-Fi or Ethernet interface on
   the same network as the phone.
3. Click **Create QR code**: StageFlow enables LIVE and displays the QR code.
4. Scan it with the phone or tablet.

The **StageFlow** tab can change groups, edit sources, microphones and comments,
set common-pair visibility, add, rename or delete a group, and save. Deletion
always requires confirmation and the common group is protected.

The **StageMark** and **StageMon** tabs open their normal remote interfaces,
not reduced copies. They become available when the application is open on the
same project and LIVE session and its local remote is ready. `BLACKOUT`,
projection, mute and level rules remain enforced by the owning application.
Click **Stop** in StageFlow to revoke the portal immediately. On a phone or
tablet, or when the target application has a different network address, its
remote opens as a full page. Use the browser **Back** button to return to StageFlow.

StageDesk and Dante Config Editor do not have their own QR remote: they join
the session to work on the project. StageMark and StageMon also keep their own
QR access when used without StageFlow.

Several phones and tablets can use the same QR code at the same time. Each
controller has its own short lease and alert acknowledgements; StageFlow limits
the session to 24 controllers.

## 7. One connection centre, three clear workflows

![Application and mobile connections](../media/ecosystem/suite-connections-en.svg)

A **Local StageFlow project** folder is not a network connection. Opening the
same folder on a computer does not mean joining a **StageFlow LIVE session**.
Native projects, local folders and LIVE sessions remain separate choices.

### Manual mode

Each application saves and reloads when you decide. This is the simplest mode
for preparing a project alone or working without automatic synchronization.

### LIVE mode

Open the **StageFlow LIVE session** when several applications must follow the same show
during preparation or operation. Valid changes are published and reloaded
quickly. An application with unsaved local changes does not overwrite them: it
reports a conflict and asks for a choice.

The large status button remains visible. The **StageFlow LIVE session** centre
combines general activation, other workstations and the QR code. It separates
**computers / applications / mobiles**: several applications on one computer
do not count as several computers. Status text remains understandable without
relying on colour.

### Host or join

1. On the main computer, open the project in StageFlow and its **StageFlow LIVE
   session** centre. Select the network, then **Allow and broadcast**.
2. In another application, choose **StageFlow LIVE session**. Select the
   discovered show and host computer, or enter the host's displayed address
   when network discovery is unavailable.
3. Check the show and host, then copy all **six digits** of the code, including
   leading zeros. No remote folder path is required.
4. Wait for **connected · synchronized** before working as a team. StageFlow
   lists connected applications; it hosts the session and does not join itself.
5. To detach, choose **Disconnect** in your application. After a network loss,
   the status becomes **connection lost**: rejoining is always voluntary.

The code stays valid for the session. Stopping and restarting the session
creates a new code. An incorrect code can be corrected without restarting the
application; if the session changed, select it again. Share the code only with
your team on a trusted local network, never publicly on the Internet.

LIVE never bypasses discipline-specific safety. BLACKOUT, mutes, audio stop and
other critical states remain controlled by their application. Turn LIVE off at
any time to return to manual operation.

Several computers on the same local network can join the StageFlow LIVE
session. Every workstation clearly shows whether it is connected to the
StageFlow project or working standalone, and can detach without blocking the
other applications. Leaving the session immediately returns the application
to standalone mode and never triggers automatic reconnection.

### Label alerts

The **Label alerts** button remains directly accessible on the main page.
The host chooses when to emit notifications; each application receives them
**by default** when it joins. The persistent banner shows **old → new**, origin,
time, a counter and the list of changes.

**Local acknowledgement**, individual or bulk, affects only alerts present at
the click. It never clears another recipient's alerts or a new alert received
afterwards. Each recipient can turn reception off locally without affecting
others; ignored alerts are not replayed when reception is enabled again.

The host can **Pause change notifications**, then enable them again **without
stopping LIVE**. Changes made during this pause are not replayed. This is not
an acknowledgement for the whole team. Only source labels trigger alerts:
never microphones, comments, groups, sound-check notes or other settings.

## 8. Control with OSC, MIDI or Stream Deck

In **StageFlow on Windows**, open **Tools > OSC · MIDI · Stream Deck**.
For standalone controls, check the application's manual for features supported
on your platform. By default StageFlow listens locally
on `127.0.0.1:18040`. Explicitly choose a network interface only when another
trusted workstation must send commands. The traffic log shows received,
accepted and refused messages, and state feedback can be selected per command.

For MIDI, choose an input, click **Learn next message**, then press the control
to map. An optional MIDI output can return state. The included Stream Deck
plugin installs from the same screen: place the SiLeMIO action on a key, then
choose the command.

The shared command catalogue controls StageFlow, StageMark and StageMon only
while the owning application has joined the LIVE project. These controls remain
optional and every application keeps its local controls.

## 9. Backups and conflicts

A `.stageflow` project is a folder. Do not move one JSON file on its own: move
or back up the complete folder.
For exchange or archiving as one file, create a `.stageflowpack`, then import it
to reconstruct the complete folder.

Every application saves its own part atomically and checks that it has not
changed in the meantime. Independent changes, such as two different pairs or
fields, are merged automatically and then shown in the editor. If both sides
changed the same value differently:

1. do not force an overwrite;
2. read the reported domain and application name;
3. compare or reload the latest version;
4. reapply your change if required;
5. save again.

## 10. Which applications do I actually need?

| Need | Recommended application |
|---|---|
| Patch, groups, A4 Excel workbook, simple plan | StageFlow |
| Conversion between consoles and audio applications | StageDesk |
| Layout, cues and projection | StageMark |
| A/B monitoring, up to six configured circuits | StageMon |
| Offline Dante preparation and validation | Dante Config Editor |
| Technical DWG drawing | AutoCAD with the StageFlow connector |

Install only what you need. StageFlow becomes useful when you want to
see and control several tools from one screen, but it is never required to use
the other applications or the `.stageflow` format.

---

**SiLeMIO / By Mamat----[]---**
