"""Genere les videos 2026.1 a partir des captures et SRT valides."""

from __future__ import annotations

import hashlib
import re
import shutil
import subprocess
import tempfile
from dataclasses import dataclass
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter, ImageFont


ROOT = Path(__file__).resolve().parent
MEDIA = ROOT / "media"
CAPTURES = MEDIA / "2026.1"
VERSION = "2026.1 Beta"

WIDTH = 1920
HEIGHT = 1080
FPS = 30
TRANSITION_SECONDS = 0.35

FONT_REGULAR = Path(r"C:\Windows\Fonts\segoeui.ttf")
FONT_BOLD = Path(r"C:\Windows\Fonts\segoeuib.ttf")

SRT_PATTERN = re.compile(
    r"(?ms)^\s*(\d+)\s*\n"
    r"(\d{2}:\d{2}:\d{2},\d{3})\s+-->\s+"
    r"(\d{2}:\d{2}:\d{2},\d{3})\s*\n"
    r"(.*?)(?=\n\s*\n|\Z)"
)


@dataclass(frozen=True)
class SubtitleCue:
    index: int
    start: float
    end: float
    lines: tuple[str, ...]


@dataclass(frozen=True)
class SceneSpec:
    key: str
    heading_fr: str
    heading_en: str
    image: str | None = None
    kind: str = "capture"

    def heading(self, language: str) -> str:
        return self.heading_fr if language == "fr" else self.heading_en


SCENES = (
    SceneSpec("intro", "Dante Config Editor 2026.1 Beta", "Dante Config Editor 2026.1 Beta", kind="intro"),
    SceneSpec("workspace", "Un espace de travail unifié", "One unified workspace", "overview.png"),
    SceneSpec("patch", "Patch immédiat", "Immediate patching", "patch.png"),
    SceneSpec("replacement", "Remplacement contrôlé", "Controlled replacement", "patch.png"),
    SceneSpec("renaming", "Renommages cohérents", "Consistent renaming", "devices.png"),
    SceneSpec("device-bank", "Banques de machines", "Device banks", "device-bank.png"),
    SceneSpec("projects", "Projets et profils", "Projects and profiles", "devices.png"),
    SceneSpec("validation", "Validation avant sauvegarde", "Validation before saving", "validation.png"),
    SceneSpec("synoptic", "Synoptique interactif", "Interactive synoptic", "synoptic.png"),
    SceneSpec("labels", "Import et export de labels", "Channel label exchange", "labels.png"),
    SceneSpec("platforms", "Windows et macOS", "Windows and macOS", kind="platforms"),
    SceneSpec("support", "Un projet gratuit", "A free project", "support.png"),
    SceneSpec("thanks", "Merci aux contributeurs", "Thanks to the contributors", kind="thanks"),
    SceneSpec("caution", "Un outil tiers prudent", "A cautious third-party tool", "validation.png"),
    SceneSpec("end", "Dante Config Editor 2026.1 Beta", "Dante Config Editor 2026.1 Beta", kind="end"),
)


def font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont:
    path = FONT_BOLD if bold else FONT_REGULAR
    if not path.exists():
        raise FileNotFoundError(f"Police requise introuvable : {path}")
    return ImageFont.truetype(str(path), size)


def parse_srt_time(value: str) -> float:
    hours, minutes, rest = value.split(":")
    seconds, milliseconds = rest.split(",")
    return (
        int(hours) * 3600
        + int(minutes) * 60
        + int(seconds)
        + int(milliseconds) / 1000
    )


def read_srt(language: str) -> list[SubtitleCue]:
    path = MEDIA / f"dce-2026-1-presentation-{language}.srt"
    content = path.read_text(encoding="utf-8-sig").replace("\r\n", "\n")
    cues = [
        SubtitleCue(
            index=int(match.group(1)),
            start=parse_srt_time(match.group(2)),
            end=parse_srt_time(match.group(3)),
            lines=tuple(line.strip() for line in match.group(4).splitlines() if line.strip()),
        )
        for match in SRT_PATTERN.finditer(content)
    ]

    if len(cues) != len(SCENES):
        raise ValueError(f"{path.name}: {len(cues)} cues au lieu de {len(SCENES)}")
    if [cue.index for cue in cues] != list(range(1, len(cues) + 1)):
        raise ValueError(f"{path.name}: numerotation des cues invalide")
    if cues[0].start != 0:
        raise ValueError(f"{path.name}: le premier cue doit commencer a 00:00:00,000")

    for current, following in zip(cues, cues[1:], strict=False):
        if current.end > following.start:
            raise ValueError(f"{path.name}: chevauchement entre les cues {current.index} et {following.index}")
        if abs((following.start - current.end) - 0.001) > 0.002:
            raise ValueError(f"{path.name}: intervalle inattendu apres le cue {current.index}")
    if cues[-1].end <= cues[-1].start:
        raise ValueError(f"{path.name}: duree finale invalide")
    if any(not cue.lines for cue in cues):
        raise ValueError(f"{path.name}: un cue ne contient aucun texte")
    return cues


def cue_duration(cues: list[SubtitleCue], index: int) -> float:
    if index + 1 < len(cues):
        return cues[index + 1].start - cues[index].start
    return cues[index].end - cues[index].start


def wrap_text(
    draw: ImageDraw.ImageDraw,
    text: str,
    face: ImageFont.FreeTypeFont,
    max_width: int,
) -> list[str]:
    lines: list[str] = []
    current = ""
    for word in text.split():
        candidate = f"{current} {word}".strip()
        if not current or draw.textlength(candidate, font=face) <= max_width:
            current = candidate
        else:
            lines.append(current)
            current = word
    if current:
        lines.append(current)
    return lines


def fit_image(image: Image.Image, box: tuple[int, int, int, int]) -> tuple[Image.Image, int, int]:
    x, y, width, height = box
    fitted = image.copy()
    fitted.thumbnail((width, height), Image.Resampling.LANCZOS)
    return fitted, x + (width - fitted.width) // 2, y + (height - fitted.height) // 2


def paste_with_shadow(canvas: Image.Image, image: Image.Image, x: int, y: int) -> None:
    shadow = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    shadow_draw = ImageDraw.Draw(shadow)
    shadow_draw.rounded_rectangle(
        (x + 12, y + 14, x + image.width + 12, y + image.height + 14),
        radius=7,
        fill=(0, 0, 0, 150),
    )
    shadow = shadow.filter(ImageFilter.GaussianBlur(14))
    canvas.paste(shadow, (0, 0), shadow)
    canvas.paste(image, (x, y))
    ImageDraw.Draw(canvas).rounded_rectangle(
        (x, y, x + image.width, y + image.height),
        radius=6,
        outline="#516482",
        width=2,
    )


def sanitize_capture(path: Path, language: str, image_name: str) -> None:
    """Valide les captures générées depuis le corpus synthétique assaini."""
    del language
    with Image.open(path) as image:
        minimum_height = 580 if image_name == "support.png" else 650
        if image.width < 680 or image.height < minimum_height:
            raise ValueError(f"Capture inattendue {path}: {image.size}")


def sanitize_captures() -> None:
    for language in ("fr", "en"):
        folder = CAPTURES / language
        if not folder.exists():
            raise FileNotFoundError(f"Dossier de captures manquant : {folder}")
        for path in sorted(folder.glob("*.png")):
            sanitize_capture(path, language, path.name)


def load_capture(language: str, image_name: str) -> Image.Image:
    capture_path = CAPTURES / language / image_name
    if not capture_path.exists():
        raise FileNotFoundError(f"Capture manquante : {capture_path}")

    image = Image.open(capture_path).convert("RGB")
    minimum_height = 580 if image_name == "support.png" else 650
    if image.width < 680 or image.height < minimum_height:
        raise ValueError(f"Capture inattendue {capture_path}: {image.size}")
    return image


def draw_brand(draw: ImageDraw.ImageDraw) -> None:
    draw.text((64, 30), f"DCE {VERSION}", fill="#5CB3FF", font=font(24, True))
    draw.text((1792, 31), "By Mamat", fill="#E7EDF6", font=font(19, True), anchor="ra")
    draw.text((1792, 58), "et ses agents  -------[]--", fill="#91A5C1", font=font(14), anchor="ra")


def subtitle_layout(
    draw: ImageDraw.ImageDraw,
    source_lines: tuple[str, ...],
    max_width: int,
    max_lines: int,
) -> tuple[ImageFont.FreeTypeFont, list[str]]:
    for size in range(36, 23, -1):
        face = font(size, True)
        lines: list[str] = []
        for source in source_lines:
            lines.extend(wrap_text(draw, source, face, max_width))
        if len(lines) <= max_lines and all(draw.textlength(line, font=face) <= max_width for line in lines):
            return face, lines
    raise ValueError(f"Sous-titre trop long : {source_lines}")


def draw_subtitle_panel(draw: ImageDraw.ImageDraw, cue: SubtitleCue) -> None:
    panel = (48, 838, 1872, 1042)
    draw.rounded_rectangle(panel, radius=7, fill="#151E2D", outline="#3A4B67", width=2)
    draw.rectangle((48, 838, 58, 1042), fill="#2F8AF0")

    face, lines = subtitle_layout(draw, cue.lines, 1705, 4)
    line_height = int(face.size * 1.28)
    total_height = len(lines) * line_height
    start_y = 940 - total_height // 2
    for index, line in enumerate(lines):
        draw.text((82, start_y + index * line_height), line, fill="#F4F7FB", font=face)


def make_capture_slide(language: str, spec: SceneSpec, cue: SubtitleCue) -> Image.Image:
    if spec.image is None:
        raise ValueError(f"Image manquante pour la scene {spec.key}")

    canvas = Image.new("RGB", (WIDTH, HEIGHT), "#0C121D")
    draw = ImageDraw.Draw(canvas)
    draw_brand(draw)
    draw.text((64, 72), spec.heading(language), fill="#F7FAFF", font=font(40, True))

    source = load_capture(language, spec.image)
    fitted, x, y = fit_image(source, (54, 130, 1812, 670))
    paste_with_shadow(canvas, fitted, x, y)
    draw_subtitle_panel(draw, cue)
    return canvas


def make_intro(language: str, cue: SubtitleCue) -> Image.Image:
    canvas = Image.new("RGB", (WIDTH, HEIGHT), "#0C121D")
    draw = ImageDraw.Draw(canvas)
    source = load_capture(language, "overview.png")
    fitted, x, y = fit_image(source, (90, 185, 1740, 570))
    fitted = fitted.filter(ImageFilter.GaussianBlur(0.4))
    fitted = Image.blend(fitted, Image.new("RGB", fitted.size, "#0C121D"), 0.28)
    paste_with_shadow(canvas, fitted, x, y)
    draw_brand(draw)
    draw.text(
        (WIDTH // 2, 105),
        "Dante Config Editor 2026.1 Beta",
        fill="#F7FAFF",
        font=font(53, True),
        anchor="ma",
    )
    draw_subtitle_panel(draw, cue)
    return canvas


def draw_info_card(
    draw: ImageDraw.ImageDraw,
    box: tuple[int, int, int, int],
    title: str,
    lines: tuple[str, ...],
    accent: str,
) -> None:
    x1, y1, x2, y2 = box
    draw.rounded_rectangle(box, radius=7, fill="#151E2D", outline="#3A4B67", width=2)
    draw.rectangle((x1, y1, x1 + 9, y2), fill=accent)
    draw.text((x1 + 36, y1 + 34), title, fill="#F7FAFF", font=font(31, True))
    y = y1 + 105
    for line in lines:
        wrapped = wrap_text(draw, line, font(24), x2 - x1 - 76)
        for wrapped_line in wrapped:
            draw.text((x1 + 36, y), wrapped_line, fill="#C6D2E3", font=font(24))
            y += 36
        y += 9


def make_platforms(language: str, cue: SubtitleCue) -> Image.Image:
    canvas = Image.new("RGB", (WIDTH, HEIGHT), "#0C121D")
    draw = ImageDraw.Draw(canvas)
    draw_brand(draw)
    heading = "Installateurs autonomes" if language == "fr" else "Self-contained installers"
    draw.text((64, 85), heading, fill="#F7FAFF", font=font(43, True))

    if language == "fr":
        windows = ("Windows 64 bits", ".NET 8 inclus", "Notices FR et EN", "Installation dans Program Files")
        mac = ("Apple Silicon et Intel", ".NET 8 inclus", "Notices FR et EN", "Images DMG séparées")
    else:
        windows = ("64-bit Windows", ".NET 8 included", "French and English guides", "Program Files installation")
        mac = ("Apple Silicon and Intel", ".NET 8 included", "French and English guides", "Separate DMG images")
    draw_info_card(draw, (105, 210, 925, 755), "Windows", windows, "#2F8AF0")
    draw_info_card(draw, (995, 210, 1815, 755), "macOS", mac, "#44B78B")
    draw_subtitle_panel(draw, cue)
    return canvas


def make_thanks(language: str, cue: SubtitleCue) -> Image.Image:
    canvas = Image.new("RGB", (WIDTH, HEIGHT), "#0C121D")
    draw = ImageDraw.Draw(canvas)
    draw_brand(draw)
    heading = "Merci" if language == "fr" else "Thank you"
    draw.text((WIDTH // 2, 115), heading, fill="#F7FAFF", font=font(56, True), anchor="ma")

    if language == "fr":
        tobi = (
            "Tobi / @togrupe",
            "Idées, retours et compatibilité DMT",
            "github.com/togrupe/dlive-midi-tools",
        )
        charles = (
            "Charles Bouticourt",
            "Idée de la fonction Atomic Bomb",
            "Un outil pédagogique hors ligne",
        )
    else:
        tobi = (
            "Tobi / @togrupe",
            "Ideas, feedback, and DMT compatibility",
            "github.com/togrupe/dlive-midi-tools",
        )
        charles = (
            "Charles Bouticourt",
            "Atomic Bomb feature idea",
            "An offline training tool",
        )
    draw_info_card(draw, (165, 270, 930, 740), tobi[0], tobi[1:], "#2F8AF0")
    draw_info_card(draw, (990, 270, 1755, 740), charles[0], charles[1:], "#F39C12")
    draw_subtitle_panel(draw, cue)
    return canvas


def make_end(language: str, cue: SubtitleCue) -> Image.Image:
    canvas = Image.new("RGB", (WIDTH, HEIGHT), "#0C121D")
    draw = ImageDraw.Draw(canvas)
    draw_brand(draw)
    draw.text(
        (WIDTH // 2, 230),
        "Dante Config Editor 2026.1 Beta",
        fill="#F7FAFF",
        font=font(60, True),
        anchor="ma",
    )
    draw.text(
        (WIDTH // 2, 335),
        "github.com/Mamat79/DanteConfigEditorV3",
        fill="#5CB3FF",
        font=font(31, True),
        anchor="ma",
    )
    draw.rounded_rectangle((530, 440, 1390, 730), radius=8, fill="#151E2D", outline="#3A4B67", width=2)
    draw.text((WIDTH // 2, 515), "By Mamat", fill="#F7FAFF", font=font(40, True), anchor="ma")
    draw.text((WIDTH // 2, 585), "et ses agents", fill="#AFC0D8", font=font(27), anchor="ma")
    draw.text((WIDTH // 2, 655), "-------[]--", fill="#72BDFF", font=font(29, True), anchor="ma")
    draw_subtitle_panel(draw, cue)
    return canvas


def make_slide(language: str, spec: SceneSpec, cue: SubtitleCue) -> Image.Image:
    if spec.kind == "intro":
        return make_intro(language, cue)
    if spec.kind == "platforms":
        return make_platforms(language, cue)
    if spec.kind == "thanks":
        return make_thanks(language, cue)
    if spec.kind == "end":
        return make_end(language, cue)
    return make_capture_slide(language, spec, cue)


def write_checksum(path: Path) -> None:
    digest = hashlib.sha256(path.read_bytes()).hexdigest()
    checksum_path = path.with_suffix(path.suffix + ".sha256")
    checksum_path.write_bytes(f"{digest}  {path.name}\n".encode("ascii"))


def find_ffmpeg() -> str:
    executable = shutil.which("ffmpeg")
    if executable:
        return executable

    try:
        from imageio_ffmpeg import get_ffmpeg_exe
    except ImportError as exc:
        raise SystemExit("FFmpeg ou imageio-ffmpeg est requis pour generer les videos.") from exc
    return get_ffmpeg_exe()


def build_video(language: str) -> None:
    ffmpeg = find_ffmpeg()
    cues = read_srt(language)
    output = MEDIA / f"dce-2026-1-presentation-{language}.mp4"
    with tempfile.TemporaryDirectory(prefix=f"dce-2026-1-{language}-") as temporary:
        folder = Path(temporary)
        slides: list[Path] = []
        for index, (spec, cue) in enumerate(zip(SCENES, cues, strict=True), start=1):
            slide_path = folder / f"slide-{index:02}.png"
            make_slide(language, spec, cue).save(slide_path, optimize=True)
            slides.append(slide_path)

        segments: list[Path] = []
        for index, (slide_path, cue) in enumerate(zip(slides, cues, strict=True), start=1):
            duration = cue_duration(cues, index - 1)
            fade_out_start = max(0.0, duration - TRANSITION_SECONDS)
            segment_path = folder / f"segment-{index:02}.mp4"
            segment_command = [
                ffmpeg,
                "-y",
                "-framerate",
                str(FPS),
                "-loop",
                "1",
                "-t",
                f"{duration:.3f}",
                "-i",
                str(slide_path),
                "-vf",
                (
                    f"fade=t=in:st=0:d={TRANSITION_SECONDS:.3f},"
                    f"fade=t=out:st={fade_out_start:.3f}:d={TRANSITION_SECONDS:.3f},"
                    "format=yuv420p"
                ),
                "-r",
                str(FPS),
                "-c:v",
                "libx264",
                "-preset",
                "medium",
                "-tune",
                "stillimage",
                "-crf",
                "20",
                "-pix_fmt",
                "yuv420p",
                str(segment_path),
            ]
            subprocess.run(segment_command, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            segments.append(segment_path)

        concat_file = folder / "segments.txt"
        concat_file.write_text(
            "\n".join(f"file '{segment.as_posix()}'" for segment in segments),
            encoding="utf-8",
        )
        concat_command = [
            ffmpeg,
            "-y",
            "-f",
            "concat",
            "-safe",
            "0",
            "-i",
            str(concat_file),
            "-c",
            "copy",
            "-movflags",
            "+faststart",
            str(output),
        ]
        subprocess.run(concat_command, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

    write_checksum(output)
    print(f"{output.name}: {cues[-1].end:.3f} s")


def main() -> None:
    MEDIA.mkdir(parents=True, exist_ok=True)
    sanitize_captures()
    for language in ("fr", "en"):
        build_video(language)


if __name__ == "__main__":
    main()
