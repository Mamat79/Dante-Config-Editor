"""Monte la notice visuelle française depuis des captures réelles de DCE."""

from __future__ import annotations

import argparse
import hashlib
import shutil
import subprocess
import tempfile
from dataclasses import dataclass
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parent
MEDIA = ROOT / "media"
SRT = MEDIA / "dce-2026-1-guide-visuel-fr-subtitles.srt"
DEFAULT_RAW = ROOT.parent.parent / "Video Drafts" / "2026.1.1" / "raw"
DEFAULT_OUTPUT = MEDIA / "dce-2026-1-guide-visuel-fr.mp4"

WIDTH = 1920
HEIGHT = 1080
FPS = 30
FONT_REGULAR = Path(r"C:\Windows\Fonts\segoeui.ttf")
FONT_BOLD = Path(r"C:\Windows\Fonts\segoeuib.ttf")


@dataclass(frozen=True)
class Segment:
    name: str
    source: str | None
    start: float
    source_duration: float
    target_duration: float
    hold_last_frame: bool = False
    title: str | None = None
    subtitle: str | None = None


SEGMENTS = (
    Segment("00-intro", None, 0, 0, 8, title="Dante Config Editor 2026.1.1", subtitle="Notice visuelle complète"),
    Segment("01-interface", "10-interface.mp4", 0, 32, 32),
    Segment("02-open-overview", "01-open-project.mp4", 0, 14, 56, hold_last_frame=True),
    Segment("03-machine-settings", "02-machines.mp4", 0, 45, 56),
    Segment("04-machine-list", "11-renaming.mp4", 0, 40, 40),
    Segment("05-bank", "03-bank.mp4", 0, 45, 56),
    Segment("06-project", "09-project-help.mp4", 0, 40, 40),
    Segment("07-matrix", "04-patch-matrix.mp4", 0, 65, 96),
    Segment("08-easy-list", "05-easy-list.mp4", 0, 48, 48),
    Segment("09-labels-reports", "06-import-export.mp4", 0, 40, 56),
    Segment("10-synoptic", "06-import-export.mp4", 38, 32, 48),
    Segment("11-validation-history", "07-validation-history.mp4", 0, 50, 56),
    Segment("12-atomic", "08-atomic.mp4", 12, 32, 32),
    Segment("13-interface-end", "10-interface.mp4", 0, 12, 12),
    Segment("14-help", "09-project-help.mp4", 42, 28, 28),
    Segment("15-outro", None, 0, 0, 8, title="Dante Config Editor 2026.1.1", subtitle="By Mamat et ses agents  -------[]--"),
)


def run(command: list[str]) -> None:
    print(" ".join(command))
    subprocess.run(command, check=True)


def font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont:
    path = FONT_BOLD if bold else FONT_REGULAR
    if not path.exists():
        raise FileNotFoundError(f"Police requise introuvable : {path}")
    return ImageFont.truetype(str(path), size)


def make_title_card(path: Path, title: str, subtitle: str) -> None:
    image = Image.new("RGB", (WIDTH, HEIGHT), "#111827")
    draw = ImageDraw.Draw(image)
    draw.rectangle((0, 0, WIDTH, 12), fill="#2F8AF0")
    draw.text((WIDTH // 2, 425), title, fill="#F8FAFC", font=font(68, True), anchor="mm")
    draw.text((WIDTH // 2, 520), subtitle, fill="#AFC7E8", font=font(34), anchor="mm")
    draw.text((WIDTH // 2, 915), "DCE travaille hors ligne sur des fichiers XML Dante.", fill="#7F93B2", font=font(24), anchor="mm")
    image.save(path)


def video_filter(segment: Segment) -> str:
    common = (
        f"scale={WIDTH}:{HEIGHT}:force_original_aspect_ratio=decrease,"
        f"pad={WIDTH}:{HEIGHT}:(ow-iw)/2:(oh-ih)/2:color=black,"
        f"fps={FPS},format=yuv420p"
    )
    if segment.hold_last_frame:
        hold = max(0.0, segment.target_duration - segment.source_duration)
        return f"{common},tpad=stop_mode=clone:stop_duration={hold:.3f}"
    ratio = segment.target_duration / segment.source_duration
    return f"setpts={ratio:.8f}*PTS,{common}"


def render_segment(segment: Segment, raw_dir: Path, output: Path, card: Path) -> None:
    if segment.source is None:
        if not segment.title or not segment.subtitle:
            raise ValueError(f"Carte incomplète : {segment.name}")
        make_title_card(card, segment.title, segment.subtitle)
        run([
            "ffmpeg", "-hide_banner", "-loglevel", "warning", "-y",
            "-loop", "1", "-i", str(card), "-t", str(segment.target_duration),
            "-vf", f"fps={FPS},format=yuv420p", "-an", "-c:v", "libx264",
            "-preset", "veryfast", "-crf", "18", "-pix_fmt", "yuv420p", str(output),
        ])
        return

    source = raw_dir / segment.source
    if not source.exists():
        raise FileNotFoundError(f"Séquence brute manquante : {source}")
    run([
        "ffmpeg", "-hide_banner", "-loglevel", "warning", "-y",
        "-ss", str(segment.start), "-t", str(segment.source_duration), "-i", str(source),
        "-vf", video_filter(segment), "-t", str(segment.target_duration), "-an",
        "-c:v", "libx264", "-preset", "veryfast", "-crf", "20",
        "-pix_fmt", "yuv420p", str(output),
    ])


def escape_subtitle_path(path: Path) -> str:
    return path.resolve().as_posix().replace(":", r"\:").replace("'", r"\'")


def verify_duration(path: Path, expected: float) -> None:
    result = subprocess.run(
        ["ffprobe", "-v", "error", "-show_entries", "format=duration", "-of", "default=nw=1:nk=1", str(path)],
        check=True,
        capture_output=True,
        text=True,
    )
    duration = float(result.stdout.strip())
    if abs(duration - expected) > 0.15:
        raise RuntimeError(f"Durée inattendue : {duration:.3f}s au lieu de {expected:.3f}s")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--raw-dir", type=Path, default=DEFAULT_RAW)
    parser.add_argument("--output", type=Path, default=DEFAULT_OUTPUT)
    args = parser.parse_args()

    if not SRT.exists():
        raise FileNotFoundError(f"Sous-titres manquants : {SRT}")
    args.output.parent.mkdir(parents=True, exist_ok=True)
    ffmpeg = shutil.which("ffmpeg")
    ffprobe = shutil.which("ffprobe")
    if not ffmpeg or not ffprobe:
        raise FileNotFoundError("ffmpeg et ffprobe doivent être disponibles dans PATH")

    expected_duration = sum(segment.target_duration for segment in SEGMENTS)
    with tempfile.TemporaryDirectory(prefix="dce-visual-guide-") as temp_name:
        temp = Path(temp_name)
        rendered: list[Path] = []
        for index, segment in enumerate(SEGMENTS):
            output = temp / f"{index:02d}-{segment.name}.mp4"
            render_segment(segment, args.raw_dir, output, temp / f"card-{index:02d}.png")
            rendered.append(output)

        concat_file = temp / "concat.txt"
        concat_file.write_text(
            "".join(f"file '{path.as_posix()}'\n" for path in rendered),
            encoding="utf-8",
        )
        silent = temp / "guide-silent.mp4"
        run([
            ffmpeg, "-hide_banner", "-loglevel", "warning", "-y",
            "-f", "concat", "-safe", "0", "-i", str(concat_file),
            "-c", "copy", str(silent),
        ])

        subtitle_filter = (
            f"subtitles=filename='{escape_subtitle_path(SRT)}':"
            "force_style='FontName=Segoe UI,FontSize=9,PrimaryColour=&H00FFFFFF,"
            "OutlineColour=&H00101828,BorderStyle=1,Outline=1,Shadow=0,"
            "Alignment=2,MarginL=24,MarginR=24,MarginV=22'"
        )
        run([
            ffmpeg, "-hide_banner", "-loglevel", "warning", "-y", "-i", str(silent),
            "-vf", subtitle_filter, "-an", "-c:v", "libx264", "-preset", "veryfast",
            "-crf", "20", "-pix_fmt", "yuv420p", "-movflags", "+faststart", str(args.output),
        ])

    verify_duration(args.output, expected_duration)
    digest = hashlib.sha256(args.output.read_bytes()).hexdigest()
    args.output.with_suffix(args.output.suffix + ".sha256").write_text(
        f"{digest}  {args.output.name}\n",
        encoding="ascii",
    )
    print(f"Notice visuelle créée : {args.output}")
    print(f"SHA-256 : {digest}")


if __name__ == "__main__":
    main()
