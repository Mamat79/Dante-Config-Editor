# DCE v2027 - verification / vérification

## Versions publiques / Public versions

| Système / Platform | Version | Distribution |
|---|---|---|
| Windows x64 | 2027.0.1 | [Release Windows](https://github.com/Mamat79/Dante-Config-Editor/releases/tag/v2027.0.1) |
| macOS Apple Silicon | 2027.0 | [Release Mac](https://github.com/Mamat79/Dante-Config-Editor/releases/tag/v2027.0) |
| macOS Intel | 2027.0 | [Release Mac](https://github.com/Mamat79/Dante-Config-Editor/releases/tag/v2027.0) |

La release 2027.0.1 ne contient aucun DMG Mac. Le repère global Latest est
conservé pour les anciens gestionnaires de mises à jour. Les liens directs
du README et du site sont la référence par système.

Release 2027.0.1 contains no Mac DMG. The global Latest marker is retained for
historical updaters. The README and website provide explicit per-platform links.

## Installateur Windows / Windows installer

- Fichier / File: `DanteConfigEditor2027_Installer.exe`.
- Octets / Bytes: `85507311`.
- SHA-256: `6489c73d2ba2cb73f844a74d98556e853acdb05dbc5aa9a06dd177242330c3d4`.
- Source privée / Private source revision: `5feb6df5ded8d8fdac60d1b2a11a9ebbb527fa5d`.
- Construction et recette / Build and native QA run: `33934722105`, successful.
- 614 core/service tests, 33 Avalonia tests, 5 licence Worker tests; no failures or skips.
- Self-contained installer, six bundled PDF checksums, 17 original native Windows captures.
- Isolated QA processes exited normally; six input files remained byte-identical.
- This exact installer was installed locally; the installed executable reports `2027.0.1.0`.
- No rebuild, licence change or new CI run was performed for this publication.

La recette native Windows a utilisé un bureau 1024x768. Certaines captures
gauche/droite sont complémentaires et ne sont pas présentées comme une seule
image. À petite résolution, certains contrôles replient ou sont partiellement
coupés, le synoptique est petit et quelques libellés historiques restent
français dans la vue anglaise. La capture distante de la fenêtre principale
sur le PC local est restée incertaine, malgré une application réactive et une
boîte À propos lisible. Aucune ergonomie parfaite sur tous les écrans n'est revendiquée.

Native Windows QA used a 1024x768 desktop. Some left/right captures complement
each other; they are not presented as one image. Small-screen wrapping,
partially clipped controls, a small synoptic view and some historical French
labels in the English view remain limitations. Remote capture of the main
window on the local PC remained inconclusive, although the application responded
and its About dialog was readable. Perfect rendering on every display is not claimed.

## macOS

Les paquets 2027.0 publics restent inchangés. Leurs preuves et limites sont
dans le [rapport Mac de cette version](https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2027.0/MACOS_VALIDATION_2027.json).
Les candidats Mac 2027.0.1 restent privés : leur vérification native complète
n'est pas terminée et la reprise a été empêchée par le plafond GitHub Actions.
Ils ne sont ni distribués ni déclarés acceptés ici.

Public Mac 2027.0 packages are unchanged; their evidence and limits are in the
linked version-specific Mac report. Mac 2027.0.1 candidates remain private:
complete native verification is unfinished and a further check was prevented
by the GitHub Actions spending limit. They are not distributed or declared accepted here.

## Limites communes / Shared limits

No new physical Dante Controller/hardware test, exhaustive Mac model coverage
or two-physical-computer LAN test is claimed by this publication. Windows
packages are not commercially signed and Mac packages are not Apple-notarised.
The public repository contains distribution material only, never private source code.
