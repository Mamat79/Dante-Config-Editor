"""Build the English DCE visual guide from real English UI captures."""

from __future__ import annotations

import argparse
import hashlib
import shutil
import subprocess
import tempfile
from dataclasses import dataclass
from pathlib import Path

from generate_visual_guide import FPS, HEIGHT, WIDTH, make_title_card, run, verify_duration


ROOT = Path(__file__).resolve().parent
DEFAULT_SCREENS = ROOT / "media" / "2026.1" / "en"
DEFAULT_SUBTITLES = ROOT.parent.parent / "Video Drafts" / "2026.1.1" / "subtitles" / "dce-2026-1-guide-visual-en-final.ass"
DEFAULT_OUTPUT = ROOT / "media" / "dce-2026-1-guide-visual-en.mkv"


@dataclass(frozen=True)
class Scene:
    name: str
    source: str | None
    duration: float
    title: str | None = None
    subtitle: str | None = None


SCENES = (
    Scene("00-intro", None, 18, "Dante Config Editor 2026.1.1", "Offline editor for Dante Controller XML presets"),
    # Each scene boundary follows the eight-second subtitle groups. Keeping the
    # map explicit prevents a caption from describing the previous screen.
    Scene("01-interface", "devices-collapsed-sidebars.png", 32),  # cues 4-7
    Scene("02-project", "new-project.png", 8),                    # cue 8
    Scene("03-overview", "overview.png", 40),                    # cues 9-13
    Scene("04-layout", "devices-collapsed-sidebars.png", 8),     # cue 14
    Scene("05-devices", "devices.png", 104),                     # cues 15-27
    Scene("06-bank", "device-bank.png", 48),                     # cues 28-33
    Scene("07-atomic-intro", "atomic-bomb.png", 8),              # cue 34
    Scene("08-new-project", "new-project.png", 32),              # cues 35-38
    Scene("09-matrix", "patch.png", 104),                        # cues 39-51
    Scene("10-easy-patch", "easy-patch.png", 16),                # cues 52-53
    Scene("11-patch-list", "patch-list.png", 24),                # cues 54-56
    Scene("12-matrix-review", "patch.png", 8),                   # cue 57
    Scene("13-labels", "labels.png", 72),                        # cues 58-66
    Scene("14-synoptic", "synoptic.png", 32),                    # cues 67-70
    Scene("15-validation", "validation.png", 64),                # cues 71-78
    Scene("16-atomic", "atomic-bomb.png", 16),                   # cues 79-80
    Scene("17-help", "overview.png", 40),                        # cues 81-85
    Scene("17-outro", None, 8, "Dante Config Editor 2026.1.1", "By Mamat et ses agents  -------[]--"),
)


def render_scene(scene: Scene, screens: Path, output: Path, card: Path) -> None:
    if scene.source is None:
        if not scene.title or not scene.subtitle:
            raise ValueError(f"Incomplete title card: {scene.name}")
        make_title_card(card, scene.title, scene.subtitle, "en")
        source = card
        video_filter = f"fps={FPS},format=yuv420p"
    else:
        source = screens / scene.source
        if not source.exists():
            raise FileNotFoundError(f"Missing English capture: {source}")
        fade_out = max(0.0, scene.duration - 0.30)
        video_filter = (
            "scale=1760:880:force_original_aspect_ratio=decrease,"
            "pad=1920:1080:(ow-iw)/2:42:color=#0B1220,"
            f"fade=t=in:st=0:d=0.30,fade=t=out:st={fade_out:.2f}:d=0.30,"
            f"fps={FPS},format=yuv420p"
        )

    run([
        "ffmpeg", "-hide_banner", "-loglevel", "warning", "-y",
        "-loop", "1", "-i", str(source), "-t", str(scene.duration),
        "-vf", video_filter, "-an", "-c:v", "libx264", "-preset", "veryfast",
        "-crf", "18", "-pix_fmt", "yuv420p", str(output),
    ])


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--screens-dir", type=Path, default=DEFAULT_SCREENS)
    parser.add_argument("--subtitles", type=Path, default=DEFAULT_SUBTITLES)
    parser.add_argument("--output", type=Path, default=DEFAULT_OUTPUT)
    args = parser.parse_args()

    if not args.subtitles.exists():
        raise FileNotFoundError(f"Missing English subtitle track: {args.subtitles}")
    if not shutil.which("ffmpeg") or not shutil.which("ffprobe"):
        raise FileNotFoundError("ffmpeg and ffprobe must be available in PATH")
    args.output.parent.mkdir(parents=True, exist_ok=True)

    expected_duration = sum(scene.duration for scene in SCENES)
    with tempfile.TemporaryDirectory(prefix="dce-english-guide-") as temp_name:
        temp = Path(temp_name)
        rendered: list[Path] = []
        for index, scene in enumerate(SCENES):
            output = temp / f"{index:02d}-{scene.name}.mp4"
            render_scene(scene, args.screens_dir, output, temp / f"card-{index:02d}.png")
            rendered.append(output)

        concat_file = temp / "concat.txt"
        concat_file.write_text(
            "".join(f"file '{path.as_posix()}'\n" for path in rendered),
            encoding="utf-8",
        )
        silent = temp / "guide-silent.mp4"
        run([
            "ffmpeg", "-hide_banner", "-loglevel", "warning", "-y",
            "-f", "concat", "-safe", "0", "-i", str(concat_file),
            "-c", "copy", str(silent),
        ])
        run([
            "ffmpeg", "-hide_banner", "-loglevel", "warning", "-y",
            "-i", str(silent), "-i", str(args.subtitles),
            "-map", "0:v:0", "-map", "1:0", "-c:v", "copy", "-c:s", "copy",
            "-metadata:s:s:0", "language=eng", "-metadata:s:s:0", "title=English",
            "-disposition:s:0", "default", "-an", str(args.output),
        ])

    verify_duration(args.output, expected_duration)
    digest = hashlib.sha256(args.output.read_bytes()).hexdigest()
    args.output.with_suffix(args.output.suffix + ".sha256").write_text(
        f"{digest}  {args.output.name}\n",
        encoding="ascii",
    )
    print(f"English visual guide created: {args.output}")
    print(f"SHA-256: {digest}")


if __name__ == "__main__":
    main()
