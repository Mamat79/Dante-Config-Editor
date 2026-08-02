"""Construit le guide visuel francais dans un ordre de travail coherent."""

from __future__ import annotations

import argparse
import hashlib
import re
import shutil
import tempfile
from dataclasses import dataclass
from pathlib import Path

from PIL import Image, ImageDraw

from generate_visual_guide import FPS, HEIGHT, WIDTH, make_title_card, run, verify_duration


ROOT = Path(__file__).resolve().parent
DEFAULT_SCREENS = ROOT / "media" / "2026.1" / "fr"
DEFAULT_SRT = (
    ROOT.parent.parent
    / "Video Drafts"
    / "2026.1.1"
    / "subtitles"
    / "dce-2026-1-guide-visuel-fr-user-corrected.srt"
)
DEFAULT_OUTPUT = ROOT / "media" / "dce-2026-1-guide-visuel-fr.mkv"


@dataclass(frozen=True)
class Scene:
    name: str
    source: str | None
    slots: int
    click: tuple[int, int] | None = None
    title: str | None = None
    subtitle: str | None = None

    @property
    def duration(self) -> float:
        return 18.0 if self.name == "00-intro" else self.slots * 8.0


# Apres l'introduction, chaque slot de huit secondes correspond exactement a
# un sous-titre. L'ordre suit le parcours naturel d'un projet dans la barre de
# navigation au lieu de revenir sur des ecrans deja presentes.
SCENES = (
    Scene("00-intro", None, 0, title="Dante Config Editor 2026.1.1", subtitle="Guide visuel complet"),
    Scene("01-menu", "help-menu.png", 1, (342, 45)),
    Scene("02-theme", "project-loaded.png", 1, (1835, 100)),
    Scene("03-project", "project-loaded.png", 1, (48, 263)),
    Scene("04-new-project", "new-project.png", 1, (492, 316)),
    Scene("05-merge", "project-loaded.png", 3, (834, 316)),
    Scene("06-overview", "overview.png", 6, (72, 300)),
    Scene("07-machines", "machines.png", 6, (58, 339)),
    Scene("08-device-details", "device-details.png", 3, (1066, 510)),
    Scene("09-machine-list", "machines.png", 3, (830, 410)),
    Scene("10-channel-renaming", "device-details.png", 2, (608, 666)),
    Scene("11-machine-actions", "machines.png", 1, (1025, 620)),
    Scene("12-bank", "device-bank.png", 6, (298, 915)),
    Scene("13-matrix", "patch-matrix.png", 13, (45, 377)),
    Scene("14-easy-patch", "easy-patch.png", 2, (398, 246)),
    Scene("15-patch-list", "patch-list.png", 3, (502, 246)),
    Scene("16-matrix-review", "patch-matrix.png", 1, (317, 246)),
    Scene("17-import-export", "import-export.png", 9, (71, 415)),
    Scene("18-synoptic", "synoptic.png", 4, (515, 233)),
    Scene("19-validation", "validation.png", 3, (85, 453)),
    Scene("20-history", "history.png", 5, (58, 490)),
    Scene("21-advanced", "advanced.png", 3, (73, 528)),
    Scene("22-help", "help-menu.png", 1, (342, 45)),
    Scene("23-project-end", "project-loaded.png", 3, (48, 263)),
    Scene("24-updates", "help-menu.png", 1, (434, 163)),
    Scene("25-outro", None, 1, title="Dante Config Editor 2026.1.1", subtitle="By Mamat et ses agents  -------[]--"),
)


# Indices 1-based du SRT fourni par l'utilisateur. Les trois premiers restent
# l'introduction et le dernier reste la conclusion.
CAPTION_ORDER = (
    1, 2, 3, 4, 5,
    8, 35, 36, 37, 38,
    9, 10, 11, 12, 13, 14,
    6, 7, 15, 16, 17, 18,
    19, 20, 21,
    22, 23, 24,
    25, 26,
    27,
    28, 29, 30, 31, 32, 33,
    39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51,
    52, 53,
    54, 55, 56,
    57,
    58, 59, 60, 61, 62, 63, 64, 65, 66,
    67, 68, 69, 70,
    71, 72, 73,
    74, 75, 76, 77, 78,
    34, 79, 80,
    81,
    82, 83, 84,
    85,
    86,
)


def parse_time(value: str) -> float:
    hours, minutes, rest = value.split(":")
    seconds, milliseconds = rest.split(",")
    return int(hours) * 3600 + int(minutes) * 60 + int(seconds) + int(milliseconds) / 1000


def parse_srt(path: Path) -> list[tuple[float, float, tuple[str, ...]]]:
    cues: list[tuple[float, float, tuple[str, ...]]] = []
    text = path.read_text(encoding="utf-8-sig").strip()
    for block in re.split(r"\n\s*\n", text):
        lines = block.splitlines()
        if len(lines) < 3 or " --> " not in lines[1]:
            raise ValueError(f"Bloc SRT invalide : {block!r}")
        start_text, end_text = lines[1].split(" --> ")
        cues.append((parse_time(start_text), parse_time(end_text), tuple(lines[2:])))
    return cues


def ass_time(seconds: float) -> str:
    centiseconds = round(seconds * 100)
    hours, remainder = divmod(centiseconds, 360000)
    minutes, remainder = divmod(remainder, 6000)
    whole_seconds, centiseconds = divmod(remainder, 100)
    return f"{hours}:{minutes:02d}:{whole_seconds:02d}.{centiseconds:02d}"


def prepare_captions(source: Path) -> list[tuple[float, float, tuple[str, ...]]]:
    original = parse_srt(source)
    if len(original) != 86:
        raise ValueError(f"86 sous-titres attendus, {len(original)} trouves")
    if sorted(CAPTION_ORDER) != list(range(1, 87)):
        raise RuntimeError("L'ordre des sous-titres doit contenir chaque index une fois")

    revised = list(original)
    # Le texte fourni reste la reference. Seuls ces passages sont renforces
    # pour expliquer les deux besoins mis en avant par l'auteur du logiciel.
    revised[2] = (
        original[2][0],
        original[2][1],
        (
            "Renommez machines et canaux sans refaire le patch,",
            "puis fusionnez, validez et exportez vos projets.",
        ),
    )
    revised[22] = (
        original[22][0],
        original[22][1],
        (
            "Renommez une machine directement dans sa cellule.",
            "DCE met à jour les références de ses patchs.",
        ),
    )
    revised[24] = (
        original[24][0],
        original[24][1],
        (
            "Renommez aussi les canaux RX et TX hors ligne.",
            "Les points de patch existants restent valides.",
        ),
    )

    ordered_text = [revised[index - 1][2] for index in CAPTION_ORDER]
    result: list[tuple[float, float, tuple[str, ...]]] = []
    for slot, lines in zip(original, ordered_text, strict=True):
        cleaned = tuple(line.replace("Con troller", "Controller") for line in lines)
        if len(cleaned) > 2 or any(len(line) > 76 for line in cleaned):
            raise ValueError(f"Sous-titre trop long : {cleaned}")
        result.append((slot[0], slot[1], cleaned))
    return result


def write_ass(path: Path, cues: list[tuple[float, float, tuple[str, ...]]]) -> None:
    header = """[Script Info]
Title: DCE 2026.1.1 - Francais
ScriptType: v4.00+
PlayResX: 1920
PlayResY: 1080
WrapStyle: 2
ScaledBorderAndShadow: yes

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Segoe UI,31,&H00FFFFFF,&H000000FF,&H00101828,&H96000000,0,0,0,0,100,100,0,0,1,2,0,2,48,48,32,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
"""
    events = []
    for start, end, lines in cues:
        text = r"\N".join(lines).replace("{", r"\{").replace("}", r"\}")
        events.append(f"Dialogue: 0,{ass_time(start)},{ass_time(end)},Default,,0,0,0,,{text}")
    path.write_text(header + "\n".join(events) + "\n", encoding="utf-8-sig")


def make_click_halo(path: Path) -> None:
    image = Image.new("RGBA", (120, 120), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    draw.ellipse((7, 7, 113, 113), outline=(47, 138, 240, 95), width=5)
    draw.ellipse((27, 27, 93, 93), outline=(47, 138, 240, 190), width=6)
    draw.ellipse((52, 52, 68, 68), fill=(47, 138, 240, 235))
    image.save(path)


def render_scene(scene: Scene, screens: Path, output: Path, card: Path, halo: Path) -> None:
    if scene.source is None:
        if not scene.title or not scene.subtitle:
            raise ValueError(f"Carte incomplete : {scene.name}")
        make_title_card(card, scene.title, scene.subtitle, "fr")
        run([
            "ffmpeg", "-hide_banner", "-loglevel", "warning", "-y",
            "-loop", "1", "-i", str(card), "-t", str(scene.duration),
            "-vf", f"fps={FPS},format=yuv420p", "-an", "-c:v", "libx264",
            "-preset", "veryfast", "-crf", "18", "-pix_fmt", "yuv420p", str(output),
        ])
        return

    source = screens / scene.source
    if not source.exists():
        raise FileNotFoundError(f"Capture francaise manquante : {source}")
    fade_out = max(0.0, scene.duration - 0.30)
    base = (
        f"scale={WIDTH}:{HEIGHT}:force_original_aspect_ratio=decrease,"
        f"pad={WIDTH}:{HEIGHT}:(ow-iw)/2:(oh-ih)/2:color=#0B1220,"
        f"fade=t=in:st=0:d=0.30,fade=t=out:st={fade_out:.2f}:d=0.30,"
        f"fps={FPS},format=yuv420p"
    )
    if scene.click is None:
        run([
            "ffmpeg", "-hide_banner", "-loglevel", "warning", "-y",
            "-loop", "1", "-i", str(source), "-t", str(scene.duration),
            "-vf", base, "-an", "-c:v", "libx264", "-preset", "veryfast",
            "-crf", "18", "-pix_fmt", "yuv420p", str(output),
        ])
        return

    click_x, click_y = scene.click
    # Les captures font 1920 x 1032 et sont centrees dans une image 1080p.
    overlay_x = click_x - 60
    overlay_y = click_y + 24 - 60
    filter_complex = (
        f"[0:v]{base}[base];"
        "[1:v]format=rgba,fade=t=in:st=0.05:d=0.18:alpha=1,"
        "fade=t=out:st=0.80:d=0.35:alpha=1[halo];"
        f"[base][halo]overlay={overlay_x}:{overlay_y}:enable='between(t,0.05,1.20)',"
        "format=yuv420p[out]"
    )
    run([
        "ffmpeg", "-hide_banner", "-loglevel", "warning", "-y",
        "-loop", "1", "-i", str(source), "-loop", "1", "-i", str(halo),
        "-t", str(scene.duration), "-filter_complex", filter_complex,
        "-map", "[out]", "-an", "-c:v", "libx264", "-preset", "veryfast",
        "-crf", "18", "-pix_fmt", "yuv420p", str(output),
    ])


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--screens-dir", type=Path, default=DEFAULT_SCREENS)
    parser.add_argument("--srt", type=Path, default=DEFAULT_SRT)
    parser.add_argument("--output", type=Path, default=DEFAULT_OUTPUT)
    args = parser.parse_args()

    if not args.srt.exists():
        raise FileNotFoundError(f"Sous-titres manquants : {args.srt}")
    if not shutil.which("ffmpeg") or not shutil.which("ffprobe"):
        raise FileNotFoundError("ffmpeg et ffprobe doivent etre disponibles dans PATH")
    if sum(scene.slots for scene in SCENES if scene.source is not None) != 82:
        # 83 slots apres l'introduction, dont le dernier est la carte de fin.
        raise RuntimeError("La chronologie des scenes doit contenir 82 slots d'ecran")

    args.output.parent.mkdir(parents=True, exist_ok=True)
    captions = prepare_captions(args.srt)
    expected_duration = sum(scene.duration for scene in SCENES)
    if abs(expected_duration - 682.0) > 0.01:
        raise RuntimeError(f"Duree de montage inattendue : {expected_duration}")

    with tempfile.TemporaryDirectory(prefix="dce-french-guide-") as temp_name:
        temp = Path(temp_name)
        ass = temp / "subtitles-fr.ass"
        halo = temp / "click-halo.png"
        write_ass(ass, captions)
        make_click_halo(halo)

        rendered: list[Path] = []
        for index, scene in enumerate(SCENES):
            output = temp / f"{index:02d}-{scene.name}.mp4"
            render_scene(scene, args.screens_dir, output, temp / f"card-{index:02d}.png", halo)
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
            "-i", str(silent), "-i", str(ass),
            "-map", "0:v:0", "-map", "1:0", "-c:v", "copy", "-c:s", "copy",
            "-metadata:s:s:0", "language=fra", "-metadata:s:s:0", "title=Francais",
            "-disposition:s:0", "default", "-an", str(args.output),
        ])

    verify_duration(args.output, expected_duration)
    digest = hashlib.sha256(args.output.read_bytes()).hexdigest()
    args.output.with_suffix(args.output.suffix + ".sha256").write_text(
        f"{digest}  {args.output.name}\n",
        encoding="ascii",
    )
    print(f"Guide visuel francais cree : {args.output}")
    print(f"SHA-256 : {digest}")


if __name__ == "__main__":
    main()
