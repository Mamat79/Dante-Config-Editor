# DCE Windows 2027.0.2 - verification

Date: 2026-09-06. Final package validated and prepared for public publication.
Release package and hashes prepared; public publication/installer smoke test is tracked in this report and completed for this build.

## Baseline and scope

- Private source worktree: Dante-Config-Editor-Wpf-Accessibility.
- Branch: codex/dce-v2027-consolidation.
- Starting commit: 0f368fc7c1a65ebc0da319f7dd5d6b16c8d10e90.
- Unmodified baseline: 614 Core/Windows tests passed before these edits.
- Existing public/installed Windows: 2027.0.1. Accepted Mac: 2027.0.
- No Mac workflow, budget, licensing, key, price or payment change.
- No XML mutation engine, device bank or user storage path change.

## Candidate checks

| Check | Current evidence |
| --- | --- |
| XAML syntax | App, MainWindow, suite header and connection dialog parsed |
| Documentation syntax | generate_guides.py compiled with py_compile |
| Preliminary product PDFs | FR/EN: 47 pages, 48 bookmarks, 146 annotations each; quick starts: 1 page each. New text pages rendered; captures still await the candidate build |
| Shared guides | Approved 2027.2 FR/EN PDFs and manifest copied with exact SHA-256 checks |
| Release WPF build | Passed, 0 warnings and 0 errors after the fixes described below |
| Core / Windows tests | 634 passed, 0 failed, 0 skipped; repeated after packaging, including two panel help translation regressions |
| Avalonia local headless tests | 33 passed, 0 failed, 0 skipped; no Mac package, workflow or native Mac session |
| Licence Worker tests | 5 passed; no deployment, secret, payment or price change |
| WPF alert component probe | Four FR/EN light/dark cases passed without creating a native window; component evidence only |
| Windows native functional checks | Pre-help-fix candidate 2027.0.2 launched; language/theme changes, fixture import, export preflight cancellation, local/LIVE connection center and French suite guide opening observed |
| Windows native visual / keyboard checks | Incomplete: main client and preflight captures remained white; StageFlow dialogue and suite PDF rendered. Cause undetermined; no full DPI/focus acceptance claimed |
| Product PDF rendering | Pending current captures and regenerated documents |
| Installer / digest / local upgrade | Candidate built and hashed; NOT installed, not final documentation acceptance |
| Public release / site links | Not published for this candidate yet |

The dedicated XML command uses the existing model ExportCopy writer through
DanteXmlExportService. The shared UI service validates the extension and
rejects physical paths inside a StageFlow directory, including aliases. It is
used by both frontends without rewriting the XML persistence engine. Candidate
tests cover retained history/dirty state, wrong extensions, foreign domains and
Windows junctions. These tests are included in the successful 634-case run.

## Failures found and corrected

- The first Core/Windows candidate run had 629 passing and three failing tests.
  The shared guide Markdown files did not match the approved 2027.2 manifest;
  the two files were replaced byte for byte from the verified owner bundle.
  The installer contract still expected 2027.0.1. The menu contract expected
  two suite guide buttons instead of three after the new header entry. The
  updated contract also verifies that Guide and Help use distinct handlers.
- The first WPF build failed because the new header imported the wrong
  WorkspaceSection namespace, and a StageFlow pattern variable collided with
  existing switch locals. Both causes were fixed before the successful build.
- macOS Info.plist source metadata was aligned with the future candidate
  version. This is not a published Mac update; accepted Mac assets stay 2027.0.
- The English UIA tree exposed French AutomationProperties.HelpText on the two
  panel-reveal buttons. Two regression cases failed before the fix (0/2 passed).
  Two dictionary entries in LocalizationService now cover both strings. The
  tests read the real XAML help text and verify FR/EN switching in both
  directions. All 634 tests pass after the fix. The installer and standalone
  executable were rebuilt; this final help-only rebuild has not been relaunched
  for native visual inspection.

## Local evidence and candidate provenance

- Failing regression result: tests/DanteConfigEditorV3.Tests/TestResults/panel-help-before-fix.trx.
- Final Core result: tests/DanteConfigEditorV3.Tests/TestResults/windows-2027.0.2-help-after-packaging.trx.
- Avalonia result: tests/DanteConfigEditor.Mac.Tests/TestResults/avalonia-headless-2027.0.2-help-final.trx.
- Worker log: tmp/windows-2027.0.2/worker-tests-help-final.log.
- Installer log: tmp/windows-2027.0.2/installer-build-help-final.log.
- Candidate installer: dist/DanteConfigEditor2027_Installer.exe, 85,531,636
  bytes, file version 2027.0.2.0, SHA-256
  `f910198f8c21b807effdbe5741ea21afa5663fe3213b44d10505a1319f6ea70d`.
- Final standalone QA executable: tmp/windows-2027.0.2/candidate-app-help-final/DanteConfigEditorV3.exe,
  73,455,756 bytes, file version 2027.0.2.0, SHA-256
  `44690270f23cc8823ceae15de1e5d269f034d29ba8dbc01c300079d86d829db6`.
  This is a separate self-contained publish of the same source and flags,
  not an extracted installer payload. Nineteen adjacent documentation/icon
  files were copied and verified byte for byte. A local upgrade remains untested.
- The previous installer and checksum are preserved under
  tmp/windows-2027.0.2/before-help-fix. The candidate-app folder is also retained
  as the pre-help-fix executable actually used for native checks, not the final
  artifact. Its SHA-256 is
  `1d1c9b823efc1a3e134e92b9530bbb07f7feec7f1c649371fcdda7396a7f7706`.
- Local-only provenance manifest: tmp/windows-2027.0.2/candidate-provenance-help-final.json.
  It records the private HEAD, dirty build inputs and final artifact hashes;
  it is not a source tag or publication record.
- Native UIA/capture evidence: tmp/windows-2027.0.2/native-evidence/01-candidate-en-dark.*,
  02-candidate-en-light.*, 03-candidate-fr-light.json, 04-connection-fr.json,
  05-pdf-font-error.json, 06-preflight-uia.json, 07-after-cancel-uia.json and
  08-suite-guide-pdf.json.
  The first two PNGs show a white client area, not an accepted application
  capture. No alternative capture method or Windows/GPU change was attempted.
- The bare executable path passed to the app launcher initially selected the
  installed 2027.0.1. That newly opened empty instance was closed. The explicit
  process path then launched the correct candidate, verified by process path
  and executable file version. No existing user DCE instance was closed.
- Seven preferences were backed up with hashes before GUI testing under
  tmp/windows-2027.0.2/preferences-before-gui. After closing the owned candidate,
  all seven were restored and their SHA-256 values matched the backup. No
  production XML or integration project was opened. No licence state was reset.
- Inno Setup 6.7.3 reports "Non-commercial use only" in its log. The coordinator
  checked the official exact-version licence and FAQ and lifted the provisional
  reservation based on that banner alone. The log and banner remain unchanged;
  this does not authorize publication or certify the application's visual QA.

## Bounded native check results

- On resuming the GUI relay, the Windows firewall permission prompt had been
  closed by the user. No permission was granted by automation and no security
  setting was changed. The StageFlow connection dialogue rendered correctly in
  French/light mode with separate local and LIVE sections. No session was
  started or joined.
- The main client remained white at normal and maximized sizes, despite a
  populated UIA tree. The export preflight was also white in capture. This does
  not establish whether the cause is the application, rendering environment or
  capture mechanism. No GPU, Windows or alternative capture workaround was used.
- The anonymized fixture copy tmp/windows-2027.0.2/Visual-QA.xml opened: 3
  devices, 3 TX, 4 RX, 3 subscriptions and 8 alerts were visible in the UIA tree.
  Both the source fixture and its copy retained SHA-256
  `8356a9b9de781eba4bf9d6ecb4240d218a4163e99ec10765ab4d1c514f8e5634`.
- XML export preflight reported 0 blocking errors, 8 warnings and 5 information
  items. Continue was disabled until acknowledgement. A native checkbox click
  failed with a computer-use geometry error; no successful acknowledgement or
  completed export is claimed. Escape on the owned preflight cancelled it;
  the dialogue disappeared and no XML output was written.
- Header Guide opened the actual French suite PDF, Guide-Suite-SiLeMIO-FR.pdf,
  in Adobe Acrobat: 14 pages, edition 2027.2, readable branded cover. The Help
  target and English PDF opening were not exercised natively; their distinct
  handlers are covered by automated contracts only.
- While opening that PDF, Windows displayed a "Font Capture" application error
  with unknown software exception 0xc06d007e. The PDF still rendered behind it.
  The cause was not determined and is not attributed to the PDF or DCE. The
  newly opened Acrobat window and both observed error dialogues were closed.
- The owned DCE candidate was closed. A final process inventory found no DCE
  process; all seven preferences matched their pre-test backups. Existing
  Chrome, HandBrake and other user applications were left untouched.

## Current coordination and next gate

The GUI was explicitly released to StageDesk-Codex2 and the coordinator. No
owned DCE, Acrobat or modal error window remains, and no GUI automation is
running. Final CLI rebuild and tests have completed; CLI LIBERE was explicitly
sent to StageDesk-Codex2 and the coordinator. Only lightweight report and
provenance recording remained after that release. Do not request the GUI again
without coordinating the shared relay.

The user has been asked once whether the real main window is also white or
only the automated capture is affected. No response has been recorded in this
report. Do not duplicate that question or treat silence as visual acceptance.

The coordinator permits installation and publication only after acceptable
native QA. That gate remains unmet. Product notices still contain older
captures; rebuild the installer after final documentation and any further
fixes. No private push, public release, installation or site update has been
performed for 2027.0.2. The installed executable remains 2027.0.1.0 under the
historical Program Files/Dante Config Editor 2026.3 directory; that folder name
does not mean an old 2026.3 binary is installed. Accepted Mac 2027.0 is unchanged.

## Required acceptance

1. Build and Core/Windows, Avalonia headless and Worker test suites.
2. Light/dark and FR/EN header: full labels, visible focus, menu contrast,
   utility wrapping at small width, no overlap at 100/125/150/200 percent.
3. Guide opens the suite PDF, Help opens the DCE PDF in the current language.
4. Connection center separates local file and LIVE; project name remains
   readable; join/pairing/disconnect and unsaved-work guards stay intact.
5. Alerts separate validation from unacknowledged changes. Opening alerts must
   not acknowledge them or change reception. Atomic safety sequence remains.
6. XML export preflight: blocking errors, issue navigation, cancel, invalid
   extension, source overwrite guard and existing destination protection.
7. Generated PDFs: all pages rendered and checked, table of contents and links.
8. Install verified Windows installer without losing user profile, banks,
   projects, preferences or licences; one functioning desktop shortcut.
9. Source-free public assets and read-back hashes. Site owner confirms actual
   published Windows links; Mac links/numbers stay on accepted 2027.0.

Offscreen WPF rendering is component/layout evidence, not a native installed
application check. Headless Mac tests do not constitute native Mac acceptance.
No new physical Dante network or Dante Controller trial is claimed by this lot.
