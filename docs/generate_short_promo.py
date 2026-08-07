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
SCREEN_BOX = (36, 118, 1540, 956)
COPY_BOX = (1568, 118, 1884, 956)

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
        6.0,
        None,
        None,
        "Dante Config Editor",
        "Dante Config Editor",
        ("Preparez, controlez et modifiez", "vos configurations Dante hors ligne."),
        ("Prepare, inspect, and edit", "Dante configurations offline."),
    ),
    Scene(
        "project",
        8.0,
        None,
        None,
        "Composez votre projet",
        "Build your project",
        ("Ouvrez un preset existant, partez de zero", "ou fusionnez plusieurs fichiers XML."),
        ("Open an existing preset, start from scratch,", "or merge several XML files."),
    ),
    Scene(
        "overview",
        8.0,
        "overview.png",
        "overview.png",
        "Controle global instantane",
        "Instant global overview",
        ("Frequences, latences, modes reseau", "et alertes sont visibles au meme endroit."),
        ("Sample rates, latency, network modes,", "and warnings are visible in one place."),
    ),
    Scene(
        "devices",
        9.0,
        "machines.png",
        "devices.png",
        "Renommez sans perdre le patch",
        "Rename without losing the patch",
        ("Machines, canaux RX et TX sont editables.", "Les subscriptions associees restent coherentes."),
        ("Devices, Rx, and Tx channels are editable.", "Related subscriptions remain consistent."),
    ),
    Scene(
        "series",
        8.0,
        "machines.png",
        "devices.png",
        "Renommage en serie",
        "Series renaming",
        ("Deux noms termines par un numero suffisent.", "Etirez la serie : les numeros continuent."),
        ("Two names ending in a number are enough.", "Extend the series to continue numbering."),
    ),
    Scene(
        "patch",
        9.0,
        "patch-matrix.png",
        "patch.png",
        "Patchez comme vous preferez",
        "Patch the way you prefer",
        ("Matrice, Easy Patch, liste RX vers TX,", "Flip et patch 1:1 dans un espace unique."),
        ("Matrix, Easy Patch, Rx-to-Tx list,", "Flip, and 1:1 patching in one workspace."),
    ),
    Scene(
        "bank",
        8.0,
        "device-bank.png",
        "device-bank.png",
        "Banques de machines",
        "Device banks",
        ("Ajoutez plusieurs machines depuis des modeles", "partageables et actualisables depuis GitHub."),
        ("Add multiple devices from reusable models", "that can be shared and updated from GitHub."),
    ),
    Scene(
        "synoptic",
        8.0,
        "synoptic.png",
        "synoptic.png",
        "Documentez votre installation",
        "Document your installation",
        ("Generez un synoptique lisible, puis exportez", "labels, rapports, PDF et SVG."),
        ("Generate a clear synoptic, then export", "labels, reports, PDF, and SVG."),
    ),
    Scene(
        "validation",
        8.0,
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


def fit_capture(path: Path, language: str) -> Image.Image:
    source = Image.open(path).convert("RGB")

    # Certaines captures francaises affichent le chemin local du preset de
    # demonstration. On masque uniquement cette ligne sans recadrer l'interface.
    if language == "fr" and source.width >= 1800 and source.height >= 900:
        draw = ImageDraw.Draw(source)
        draw.rectangle((235, 98, 1525, 122), fill="#FFFFFF")
        draw.text(
            (245, 99),
            "Projet de demonstration",
            fill="#536273",
            font=face(15),
        )

    x1, y1, x2, y2 = SCREEN_BOX
    max_width = x2 - x1
    max_height = y2 - y1
    scale = min(max_width / source.width, max_height / source.height)
    target = source.resize(
        (round(source.width * scale), round(source.height * scale)),
        Image.Resampling.LANCZOS,
    )
    frame = Image.new("RGB", (max_width, max_height), "#111B2A")
    left = (max_width - target.width) // 2
    top = (max_height - target.height) // 2
    frame.paste(target, (left, top))
    return frame


def wrapped_lines(
    draw: ImageDraw.ImageDraw,
    text: str,
    font: ImageFont.FreeTypeFont,
    max_width: int,
) -> list[str]:
    lines: list[str] = []
    current = ""
    for word in text.split():
        candidate = f"{current} {word}".strip()
        if not current or draw.textlength(candidate, font=font) <= max_width:
            current = candidate
        else:
            lines.append(current)
            current = word
    if current:
        lines.append(current)
    return lines


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

    x1, y1, x2, y2 = COPY_BOX
    draw.rounded_rectangle((x1, y1, x2, y2), radius=10, fill="#111B2A", outline="#506682", width=2)
    draw.rectangle((x1, y1, x1 + 8, y2), fill="#2F8AF0")
    title_font = face(32, True)
    body_font = face(23)
    y = y1 + 42
    for line in wrapped_lines(draw, title, title_font, x2 - x1 - 54):
        draw.text((x1 + 28, y), line, fill="#FFFFFF", font=title_font)
        y += 43
    y += 26
    for paragraph in lines:
        for line in wrapped_lines(draw, paragraph, body_font, x2 - x1 - 54):
            draw.text((x1 + 28, y), line, fill="#D6E0EF", font=body_font)
            y += 34
        y += 16


def draw_series_example(canvas: Image.Image, language: str) -> None:
    draw = ImageDraw.Draw(canvas)
    x1, _, x2, _ = COPY_BOX
    labels = ("Mic 01", "Mic 02", "Mic 03", "Mic 04", "Mic 05")
    start_y = 520
    for index, label in enumerate(labels):
        y = start_y + index * 60
        fill = "#DCE9F7" if index < 2 else "#D8F3E3"
        outline = "#6B8BAE" if index < 2 else "#4DAA72"
        draw.rounded_rectangle((x1 + 40, y, x2 - 40, y + 46), radius=6, fill=fill, outline=outline, width=2)
        draw.text((x1 + 60, y + 9), label, fill="#132033", font=face(22, index >= 2))
    arrow = "ETIRER" if language == "fr" else "EXTEND"
    draw.text(((x1 + x2) // 2, 475), f"{arrow}  v", fill="#7EC5FF", font=face(20, True), anchor="ma")


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
    canvas = Image.new("RGB", (WIDTH, HEIGHT), "#0B1422")
    draw_brand(canvas)
    frame = fit_capture(capture_path, language)
    x1, y1, x2, y2 = SCREEN_BOX
    canvas.paste(frame, (x1, y1))
    draw = ImageDraw.Draw(canvas)
    draw.rounded_rectangle((x1 - 2, y1 - 2, x2 + 2, y2 + 2), radius=8, outline="#506682", width=2)
    draw_copy(canvas, title, lines)
    if scene.key == "series":
        draw_series_example(canvas, language)
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
            fade_out = max(0.0, scene.duration - FADE_SECONDS)
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
