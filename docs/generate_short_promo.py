"""Genere les videos courtes de presentation FR et EN de DCE 2026.1.1."""

from __future__ import annotations

import hashlib
import shutil
import subprocess
import tempfile
from dataclasses import dataclass
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter, ImageFont


ROOT = Path(__file__).resolve().parent
MEDIA = ROOT / "media"
CAPTURES = MEDIA / "2026.1"
BRAND_LOGO = ROOT.parent / "Resources" / "Branding" / "silemio-logo.png"

WIDTH = 1920
HEIGHT = 1080
FPS = 30
FADE_SECONDS = 0.28

FONT_REGULAR = Path(r"C:\Windows\Fonts\segoeui.ttf")
FONT_BOLD = Path(r"C:\Windows\Fonts\segoeuib.ttf")


@dataclass(frozen=True)
class Scene:
    key: str
    duration: float
    capture_fr: str | None
    capture_en: str | None
    title_fr: str
    title_en: str
    text_fr: tuple[str, ...]
    text_en: tuple[str, ...]


SCENES = (
    Scene(
        "intro",
        7.0,
        None,
        None,
        "Dante Config Editor",
        "Dante Config Editor",
        ("Preparez, controlez et modifiez", "vos configurations Dante hors ligne."),
        ("Prepare, inspect, and edit", "Dante configurations offline."),
    ),
    Scene(
        "project",
        9.0,
        None,
        None,
        "Composez votre projet",
        "Build your project",
        ("Ouvrez un preset existant, partez de zero", "ou fusionnez plusieurs fichiers XML."),
        ("Open an existing preset, start from scratch,", "or merge several XML files."),
    ),
    Scene(
        "overview",
        9.0,
        "overview.png",
        "overview.png",
        "Controle global instantane",
        "Instant global overview",
        ("Frequences, latences, modes reseau", "et alertes sont visibles au meme endroit."),
        ("Sample rates, latency, network modes,", "and warnings are visible in one place."),
    ),
    Scene(
        "devices",
        10.0,
        "machines.png",
        "devices.png",
        "Renommez sans perdre le patch",
        "Rename without losing the patch",
        ("Machines, canaux RX et TX sont editables.", "Les subscriptions associees restent coherentes."),
        ("Devices, Rx, and Tx channels are editable.", "Related subscriptions remain consistent."),
    ),
    Scene(
        "patch",
        10.0,
        "patch-matrix.png",
        "patch.png",
        "Patchez comme vous preferez",
        "Patch the way you prefer",
        ("Matrice, Easy Patch, liste RX vers TX,", "Flip et patch 1:1 dans un espace unique."),
        ("Matrix, Easy Patch, Rx-to-Tx list,", "Flip, and 1:1 patching in one workspace."),
    ),
    Scene(
        "bank",
        9.0,
        "device-bank.png",
        "device-bank.png",
        "Banques de machines",
        "Device banks",
        ("Ajoutez plusieurs machines depuis des modeles", "partageables et actualisables depuis GitHub."),
        ("Add multiple devices from reusable models", "that can be shared and updated from GitHub."),
    ),
    Scene(
        "synoptic",
        9.0,
        "synoptic.png",
        "synoptic.png",
        "Documentez votre installation",
        "Document your installation",
        ("Generez un synoptique lisible, puis exportez", "labels, rapports, PDF et SVG."),
        ("Generate a clear synoptic, then export", "labels, reports, PDF, and SVG."),
    ),
    Scene(
        "validation",
        9.0,
        "validation.png",
        "validation.png",
        "Validez avant d'enregistrer",
        "Validate before saving",
        ("DCE signale les incoherences et modifie", "de facon ciblee le document XML d'origine."),
        ("DCE reports inconsistencies and applies", "targeted changes to the original XML document."),
    ),
    Scene(
        "end",
        8.0,
        None,
        None,
        "Dante Config Editor 2026.1.1",
        "Dante Config Editor 2026.1.1",
        ("Gratuit - Windows et macOS", "github.com/Mamat79/Dante-Config-Editor"),
        ("Free - Windows and macOS", "github.com/Mamat79/Dante-Config-Editor"),
    ),
)


def face(size: int, bold: bool = False) -> ImageFont.FreeTypeFont:
    path = FONT_BOLD if bold else FONT_REGULAR
    return ImageFont.truetype(str(path), size)


def find_ffmpeg() -> str:
    executable = shutil.which("ffmpeg")
    if not executable:
        raise SystemExit("FFmpeg est requis pour generer les videos.")
    return executable


def draw_brand(canvas: Image.Image) -> None:
    logo = Image.open(BRAND_LOGO).convert("RGBA")
    logo.thumbnail((84, 84), Image.Resampling.LANCZOS)
    canvas.paste(logo, (66, 42), logo)
    draw = ImageDraw.Draw(canvas)
    draw.text((166, 52), "SiLeMI/O", fill="#F5F8FC", font=face(28, True))
    draw.text((166, 90), "By Mamat  -------[]--", fill="#9FB1CA", font=face(18))


def cover_capture(path: Path) -> Image.Image:
    source = Image.open(path).convert("RGB")
    # Les captures de documentation contiennent la barre de titre et le chemin
    # du preset de test. La presentation courte cadre uniquement la zone utile.
    top_cut = min(175, source.height // 5)
    source = source.crop((0, top_cut, source.width, source.height))
    target_ratio = WIDTH / HEIGHT
    source_ratio = source.width / source.height
    if source_ratio > target_ratio:
        crop_width = int(source.height * target_ratio)
        left = (source.width - crop_width) // 2
        source = source.crop((left, 0, left + crop_width, source.height))
    else:
        crop_height = int(source.width / target_ratio)
        top = (source.height - crop_height) // 2
        source = source.crop((0, top, source.width, top + crop_height))
    return source.resize((WIDTH, HEIGHT), Image.Resampling.LANCZOS)


def draw_copy(
    canvas: Image.Image,
    title: str,
    lines: tuple[str, ...],
    *,
    centered: bool = False,
) -> None:
    draw = ImageDraw.Draw(canvas)
    if centered:
        draw.text((WIDTH // 2, 392), title, fill="#F7FAFF", font=face(70, True), anchor="ma")
        y = 515
        for line in lines:
            draw.text((WIDTH // 2, y), line, fill="#C8D5E8", font=face(38), anchor="ma")
            y += 54
        return

    panel = (80, 720, 1840, 1010)
    draw.rounded_rectangle(panel, radius=12, fill=(10, 17, 29, 232), outline="#506682", width=2)
    draw.rectangle((80, 720, 92, 1010), fill="#2F8AF0")
    draw.text((126, 758), title, fill="#FFFFFF", font=face(48, True))
    y = 838
    for line in lines:
        draw.text((128, y), line, fill="#D6E0EF", font=face(31))
        y += 47


def draw_project_choices(canvas: Image.Image, language: str) -> None:
    draw = ImageDraw.Draw(canvas)
    if language == "fr":
        choices = (
            ("OUVRIR", "Charger un preset Dante existant"),
            ("CREER", "Partir d'un projet hors ligne"),
            ("FUSIONNER", "Ajouter un XML au projet ouvert"),
        )
    else:
        choices = (
            ("OPEN", "Load an existing Dante preset"),
            ("CREATE", "Start with an offline project"),
            ("MERGE", "Add an XML file to the open project"),
        )

    card_width = 520
    gap = 42
    start_x = (WIDTH - (card_width * 3 + gap * 2)) // 2
    for index, (heading, detail) in enumerate(choices):
        x1 = start_x + index * (card_width + gap)
        box = (x1, 350, x1 + card_width, 650)
        draw.rounded_rectangle(box, radius=12, fill="#151F2F", outline="#476382", width=2)
        draw.rectangle((x1, 350, x1 + card_width, 360), fill="#2F8AF0")
        draw.text((x1 + 34, 410), heading, fill="#FFFFFF", font=face(34, True))
        words = detail.split()
        lines: list[str] = []
        current = ""
        for word in words:
            candidate = f"{current} {word}".strip()
            if draw.textlength(candidate, font=face(24)) <= card_width - 68:
                current = candidate
            else:
                lines.append(current)
                current = word
        if current:
            lines.append(current)
        for line_index, line in enumerate(lines):
            draw.text((x1 + 34, 485 + line_index * 38), line, fill="#BDCAE0", font=face(24))


def make_slide(language: str, scene: Scene) -> Image.Image:
    title = scene.title_fr if language == "fr" else scene.title_en
    lines = scene.text_fr if language == "fr" else scene.text_en
    capture_name = scene.capture_fr if language == "fr" else scene.capture_en

    if capture_name is None:
        canvas = Image.new("RGB", (WIDTH, HEIGHT), "#0B1422")
        glow = Image.new("RGBA", (WIDTH, HEIGHT), (0, 0, 0, 0))
        glow_draw = ImageDraw.Draw(glow)
        glow_draw.ellipse((180, -260, 1500, 1060), fill=(31, 101, 181, 105))
        glow = glow.filter(ImageFilter.GaussianBlur(150))
        canvas.paste(glow, (0, 0), glow)
        draw_brand(canvas)
        if scene.key == "project":
            draw = ImageDraw.Draw(canvas)
            draw.text((WIDTH // 2, 205), title, fill="#F7FAFF", font=face(60, True), anchor="ma")
            draw_project_choices(canvas, language)
            draw.text((WIDTH // 2, 760), lines[0], fill="#D3DEED", font=face(31), anchor="ma")
            draw.text((WIDTH // 2, 808), lines[1], fill="#D3DEED", font=face(31), anchor="ma")
        else:
            draw_copy(canvas, title, lines, centered=True)
        if scene.key == "end":
            draw = ImageDraw.Draw(canvas)
            disclaimer = "Outil tiers non officiel" if language == "fr" else "Unofficial third-party tool"
            draw.text((WIDTH // 2, 690), disclaimer, fill="#8EA2BF", font=face(24), anchor="ma")
        return canvas

    capture_path = CAPTURES / language / capture_name
    if not capture_path.exists():
        raise FileNotFoundError(capture_path)
    canvas = cover_capture(capture_path)
    veil = Image.new("RGBA", canvas.size, (4, 10, 20, 22))
    canvas = Image.alpha_composite(canvas.convert("RGBA"), veil).convert("RGB")
    draw_copy(canvas, title, lines)
    return canvas


def write_checksum(path: Path) -> None:
    digest = hashlib.sha256(path.read_bytes()).hexdigest()
    path.with_suffix(path.suffix + ".sha256").write_text(
        f"{digest}  {path.name}\n", encoding="ascii"
    )


def build(language: str) -> Path:
    ffmpeg = find_ffmpeg()
    output = MEDIA / f"dce-2026-1-presentation-courte-{language}.mp4"
    with tempfile.TemporaryDirectory(prefix=f"dce-short-{language}-") as temp_name:
        temp = Path(temp_name)
        segments: list[Path] = []
        for index, scene in enumerate(SCENES, start=1):
            slide = temp / f"slide-{index:02}.png"
            make_slide(language, scene).save(slide, optimize=True)
            segment = temp / f"segment-{index:02}.mp4"
            frames = int(scene.duration * FPS)
            fade_out = max(0.0, scene.duration - FADE_SECONDS)
            zoom_direction = "min(zoom+0.00020,1.025)" if index % 2 else "if(eq(on,1),1.025,max(zoom-0.00020,1.0))"
            command = [
                ffmpeg,
                "-y",
                "-loop",
                "1",
                "-framerate",
                str(FPS),
                "-i",
                str(slide),
                "-vf",
                (
                    f"zoompan=z='{zoom_direction}':x='iw/2-(iw/zoom/2)':"
                    f"y='ih/2-(ih/zoom/2)':d={frames}:s={WIDTH}x{HEIGHT}:fps={FPS},"
                    f"fade=t=in:st=0:d={FADE_SECONDS},"
                    f"fade=t=out:st={fade_out:.3f}:d={FADE_SECONDS},format=yuv420p"
                ),
                "-t",
                f"{scene.duration:.3f}",
                "-an",
                "-c:v",
                "libx264",
                "-preset",
                "medium",
                "-crf",
                "20",
                "-pix_fmt",
                "yuv420p",
                str(segment),
            ]
            subprocess.run(command, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            segments.append(segment)

        concat_file = temp / "segments.txt"
        concat_file.write_text(
            "\n".join(f"file '{segment.as_posix()}'" for segment in segments),
            encoding="utf-8",
        )
        subprocess.run(
            [
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
            ],
            check=True,
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
        )

    write_checksum(output)
    return output


def main() -> None:
    MEDIA.mkdir(parents=True, exist_ok=True)
    for language in ("fr", "en"):
        output = build(language)
        print(f"{output.name}: {output.stat().st_size} octets")


if __name__ == "__main__":
    main()
