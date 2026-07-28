from __future__ import annotations

from io import BytesIO
from pathlib import Path

from PIL import Image as PillowImage
from PIL import ImageDraw, ImageFont
from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import mm
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    Image as ReportLabImage,
    KeepTogether,
    PageBreak,
    Paragraph,
    SimpleDocTemplate,
    Spacer,
    Table,
    TableStyle,
)


ROOT = Path(__file__).resolve().parent
# Les quatre PDF sont générés depuis une source unique pour garder les versions
# française et anglaise synchronisées avec l'application et l'installateur.
PRODUCT = "Dante Config Editor 2026.1 Beta"
VERSION = "2026.1.0-beta.1"
GITHUB = "github.com/Mamat79/Dante-Config-Editor"

INK = colors.HexColor("#172033")
MUTED = colors.HexColor("#526070")
ACCENT = colors.HexColor("#1677D2")
LINE = colors.HexColor("#D7DEE8")
PALE_BLUE = colors.HexColor("#EEF6FF")
PALE_RED = colors.HexColor("#FFF1F1")
PALE_GREEN = colors.HexColor("#EDF8F2")


def register_fonts() -> tuple[str, str]:
    candidates = [
        (Path(r"C:\Windows\Fonts\segoeui.ttf"), Path(r"C:\Windows\Fonts\segoeuib.ttf")),
        (Path(r"C:\Windows\Fonts\arial.ttf"), Path(r"C:\Windows\Fonts\arialbd.ttf")),
    ]
    for regular, bold in candidates:
        if regular.exists() and bold.exists():
            pdfmetrics.registerFont(TTFont("GuideRegular", str(regular)))
            pdfmetrics.registerFont(TTFont("GuideBold", str(bold)))
            return "GuideRegular", "GuideBold"
    return "Helvetica", "Helvetica-Bold"


REGULAR, BOLD = register_fonts()
BASE = getSampleStyleSheet()
STYLES = {
    "title": ParagraphStyle(
        "GuideTitle",
        parent=BASE["Title"],
        fontName=BOLD,
        fontSize=21,
        leading=25,
        textColor=INK,
        alignment=TA_CENTER,
        spaceAfter=3 * mm,
    ),
    "subtitle": ParagraphStyle(
        "GuideSubtitle",
        parent=BASE["Normal"],
        fontName=REGULAR,
        fontSize=10.5,
        leading=14,
        textColor=MUTED,
        alignment=TA_CENTER,
        spaceAfter=4 * mm,
    ),
    "h1": ParagraphStyle(
        "GuideH1",
        parent=BASE["Heading1"],
        fontName=BOLD,
        fontSize=14,
        leading=17,
        textColor=INK,
        spaceBefore=2.5 * mm,
        spaceAfter=2 * mm,
    ),
    "h2": ParagraphStyle(
        "GuideH2",
        parent=BASE["Heading2"],
        fontName=BOLD,
        fontSize=10.5,
        leading=13,
        textColor=ACCENT,
        spaceBefore=2 * mm,
        spaceAfter=1.2 * mm,
    ),
    "body": ParagraphStyle(
        "GuideBody",
        parent=BASE["BodyText"],
        fontName=REGULAR,
        fontSize=9.1,
        leading=12.3,
        textColor=INK,
        spaceAfter=1.6 * mm,
    ),
    "small": ParagraphStyle(
        "GuideSmall",
        parent=BASE["BodyText"],
        fontName=REGULAR,
        fontSize=8.2,
        leading=10.8,
        textColor=MUTED,
    ),
    "bullet": ParagraphStyle(
        "GuideBullet",
        parent=BASE["BodyText"],
        fontName=REGULAR,
        fontSize=8.9,
        leading=12,
        leftIndent=4 * mm,
        firstLineIndent=-3 * mm,
        textColor=INK,
        spaceAfter=1.1 * mm,
    ),
    "table_header": ParagraphStyle(
        "GuideTableHeader",
        parent=BASE["BodyText"],
        fontName=BOLD,
        fontSize=8.3,
        leading=10.5,
        textColor=colors.white,
    ),
    "table": ParagraphStyle(
        "GuideTable",
        parent=BASE["BodyText"],
        fontName=REGULAR,
        fontSize=8.2,
        leading=10.5,
        textColor=INK,
    ),
    "caption": ParagraphStyle(
        "GuideCaption",
        parent=BASE["BodyText"],
        fontName=REGULAR,
        fontSize=7.8,
        leading=10.2,
        textColor=MUTED,
        alignment=TA_CENTER,
        spaceAfter=2 * mm,
    ),
    "eyebrow": ParagraphStyle(
        "GuideEyebrow",
        parent=BASE["BodyText"],
        fontName=BOLD,
        fontSize=8.5,
        leading=11,
        textColor=ACCENT,
        alignment=TA_CENTER,
        spaceAfter=2.5 * mm,
    ),
    "cover_lead": ParagraphStyle(
        "GuideCoverLead",
        parent=BASE["BodyText"],
        fontName=REGULAR,
        fontSize=11.2,
        leading=15.2,
        textColor=INK,
        alignment=TA_CENTER,
        leftIndent=10 * mm,
        rightIndent=10 * mm,
        spaceAfter=4 * mm,
    ),
}


def para(text: str, style: str = "body") -> Paragraph:
    return Paragraph(text, STYLES[style])


def bullets(items: list[str]) -> list[Paragraph]:
    return [para(f"- {item}", "bullet") for item in items]


def callout(text: str, background: colors.Color = PALE_RED) -> Table:
    table = Table([[para(text, "body")]], colWidths=[170 * mm])
    table.setStyle(
        TableStyle(
            [
                ("BACKGROUND", (0, 0), (-1, -1), background),
                ("BOX", (0, 0), (-1, -1), 0.6, LINE),
                ("LEFTPADDING", (0, 0), (-1, -1), 4 * mm),
                ("RIGHTPADDING", (0, 0), (-1, -1), 4 * mm),
                ("TOPPADDING", (0, 0), (-1, -1), 2.5 * mm),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 2.5 * mm),
            ]
        )
    )
    return table


def data_table(headers: list[str], rows: list[list[str]], widths: list[float]) -> Table:
    content = [[para(header, "table_header") for header in headers]]
    content.extend([[para(cell, "table") for cell in row] for row in rows])
    table = Table(content, colWidths=[width * mm for width in widths], repeatRows=1)
    table.setStyle(
        TableStyle(
            [
                ("BACKGROUND", (0, 0), (-1, 0), ACCENT),
                ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, PALE_BLUE]),
                ("GRID", (0, 0), (-1, -1), 0.45, LINE),
                ("VALIGN", (0, 0), (-1, -1), "TOP"),
                ("LEFTPADDING", (0, 0), (-1, -1), 2.4 * mm),
                ("RIGHTPADDING", (0, 0), (-1, -1), 2.4 * mm),
                ("TOPPADDING", (0, 0), (-1, -1), 1.8 * mm),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 1.8 * mm),
            ]
        )
    )
    return table


def feature_band(items: list[tuple[str, str]]) -> Table:
    cells = [para(f"<b>{heading}</b><br/>{body}", "small") for heading, body in items]
    width = 170 / len(cells)
    table = Table([cells], colWidths=[width * mm] * len(cells))
    table.setStyle(
        TableStyle(
            [
                ("BACKGROUND", (0, 0), (-1, -1), PALE_BLUE),
                ("GRID", (0, 0), (-1, -1), 0.45, LINE),
                ("VALIGN", (0, 0), (-1, -1), "TOP"),
                ("LEFTPADDING", (0, 0), (-1, -1), 3 * mm),
                ("RIGHTPADDING", (0, 0), (-1, -1), 3 * mm),
                ("TOPPADDING", (0, 0), (-1, -1), 2.5 * mm),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 2.5 * mm),
            ]
        )
    )
    return table


def screenshot(
    language: str,
    name: str,
    caption: str,
    crop: tuple[int, int, int, int] | None = None,
    markers: list[tuple[float, float, str]] | None = None,
    width: float = 170,
    maximum_height: float = 112,
) -> list:
    """Ajoute une capture recadrée sans déformer l'interface."""
    source = ROOT / "media" / "2026.1" / language.lower() / f"{name}.png"
    with PillowImage.open(source) as opened:
        image = opened.convert("RGB")
        if crop is not None:
            image = image.crop(crop)

        if markers:
            draw = ImageDraw.Draw(image)
            radius = max(17, round(min(image.width, image.height) * 0.035))
            try:
                font = ImageFont.truetype(r"C:\Windows\Fonts\segoeuib.ttf", radius)
            except OSError:
                font = ImageFont.load_default()

            for x_ratio, y_ratio, label in markers:
                x = round(image.width * x_ratio)
                y = round(image.height * y_ratio)
                draw.ellipse(
                    (x - radius, y - radius, x + radius, y + radius),
                    fill=(22, 119, 210),
                    outline=(255, 255, 255),
                    width=max(2, radius // 8),
                )
                bounds = draw.textbbox((0, 0), label, font=font)
                text_width = bounds[2] - bounds[0]
                text_height = bounds[3] - bounds[1]
                draw.text(
                    (x - text_width / 2, y - text_height / 2 - bounds[1]),
                    label,
                    font=font,
                    fill=(255, 255, 255),
                )

        stream = BytesIO()
        image.save(stream, format="JPEG", quality=88, optimize=True)
        stream.seek(0)
        ratio = image.height / image.width

    draw_width = width * mm
    draw_height = draw_width * ratio
    max_height = maximum_height * mm
    if draw_height > max_height:
        scale = max_height / draw_height
        draw_width *= scale
        draw_height = max_height

    figure = ReportLabImage(stream, width=draw_width, height=draw_height)
    frame = Table([[figure]], colWidths=[draw_width + 2 * mm])
    frame.setStyle(
        TableStyle(
            [
                ("BACKGROUND", (0, 0), (-1, -1), colors.white),
                ("BOX", (0, 0), (-1, -1), 0.7, LINE),
                ("LEFTPADDING", (0, 0), (-1, -1), 1 * mm),
                ("RIGHTPADDING", (0, 0), (-1, -1), 1 * mm),
                ("TOPPADDING", (0, 0), (-1, -1), 1 * mm),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 1 * mm),
                ("ALIGN", (0, 0), (-1, -1), "CENTER"),
            ]
        )
    )
    return [frame, Spacer(1, 1.5 * mm), para(caption, "caption")]


def keyboard_table(language: str) -> Table:
    if language == "FR":
        rows = [
            ["Entrée", "Valider le nom sans quitter la cellule courante."],
            ["Tab", "Valider puis ouvrir le canal suivant."],
            ["Maj + Tab", "Valider puis ouvrir le canal précédent."],
            ["Échap", "Annuler l'édition ou la recopie en cours."],
            ["Ctrl / Maj", "Étendre une sélection de canaux dans Easy patch."],
            ["Molette", "Faire défiler ; dans la grille et le synoptique, utiliser les commandes de zoom prévues."],
        ]
        return data_table(["Touche", "Action"], rows, [38, 132])

    rows = [
        ["Enter", "Validate the name without leaving the current cell."],
        ["Tab", "Validate and open the next channel."],
        ["Shift + Tab", "Validate and open the previous channel."],
        ["Escape", "Cancel the current edit or fill operation."],
        ["Ctrl / Shift", "Extend a channel selection in Easy patch."],
        ["Mouse wheel", "Scroll; use the dedicated zoom controls in the matrix and synoptic."],
    ]
    return data_table(["Key", "Action"], rows, [38, 132])


def cover_page(language: str) -> list:
    french = language == "FR"
    eyebrow = "GUIDE COMPLET - ÉDITION XML DANTE HORS LIGNE" if french else "FULL GUIDE - OFFLINE DANTE XML EDITING"
    lead = (
        "Cet outil est né d'une tentative de pallier ce qui me manquait dans Dante Controller : voir rapidement une configuration entière, corriger les écarts et préparer un preset hors ligne."
        if french
        else "This tool began as an attempt to provide what I personally was missing in Dante Controller: review an entire configuration quickly, correct discrepancies, and prepare a preset offline."
    )
    goals = (
        [
            ("Vue d'ensemble", "Latence, sample rate, réseau, IP, horloge et canaux sur un même écran."),
            ("Renommage cohérent", "Les références reconnues suivent les devices et canaux renommés."),
            ("Préparation hors ligne", "Contrôlez, fusionnez et modifiez avant la validation officielle."),
        ]
        if french
        else [
            ("One overview", "Latency, sample rate, network, IP, clock, and channels on one screen."),
            ("Consistent renaming", "Recognized references follow renamed devices and channels."),
            ("Offline preparation", "Review, merge, and edit before official validation."),
        ]
    )
    warning = (
        "<b>Outil tiers non officiel.</b> Il ne se connecte pas au réseau Dante. Conservez toujours l'original et validez le XML final dans Dante Controller."
        if french
        else "<b>Unofficial third-party tool.</b> It does not connect to the Dante network. Always keep the original and validate the final XML in Dante Controller."
    )
    return [
        Spacer(1, 5 * mm),
        para(eyebrow, "eyebrow"),
        para(PRODUCT, "title"),
        para("Notice complète" if french else "Full user guide", "subtitle"),
        para(lead, "cover_lead"),
        Spacer(1, 7 * mm),
        feature_band(goals),
        Spacer(1, 4 * mm),
        callout(warning, PALE_RED),
        Spacer(1, 2 * mm),
        para(("Projet public : " if french else "Public project: ") + GITHUB, "small"),
    ]


def draw_header_footer(canvas, doc) -> None:
    canvas.saveState()
    width, height = A4
    canvas.setStrokeColor(LINE)
    canvas.setLineWidth(0.5)
    canvas.line(18 * mm, height - 15 * mm, width - 18 * mm, height - 15 * mm)
    canvas.setFont(REGULAR, 7.4)
    canvas.setFillColor(MUTED)
    canvas.drawString(18 * mm, height - 11.5 * mm, f"{PRODUCT} - version {VERSION} - By Mamat et ses agents")
    canvas.drawRightString(width - 18 * mm, 10 * mm, f"Page {doc.page}")
    canvas.line(18 * mm, 14 * mm, width - 18 * mm, 14 * mm)
    canvas.restoreState()


def build_document(path: Path, story: list) -> None:
    document = SimpleDocTemplate(
        str(path),
        pagesize=A4,
        leftMargin=20 * mm,
        rightMargin=20 * mm,
        topMargin=21 * mm,
        bottomMargin=18 * mm,
        title=PRODUCT,
        author="Mamat et ses agents",
        subject="Offline Dante XML editor user guide",
    )
    document.build(story, onFirstPage=draw_header_footer, onLaterPages=draw_header_footer)


def quick_start(language: str) -> None:
    french = language == "FR"
    subtitle = (
        "Démarrage rapide - édition hors ligne de fichiers XML Dante"
        if french
        else "Quick start - offline editing of Dante XML files"
    )
    warning = (
        "<b>Outil tiers non officiel Audinate.</b> Cette version 2026.1 est une bêta et peut encore contenir des bugs. Travaillez sur une copie et validez toujours le XML final par un import dans l'outil Dante officiel adapté avant toute utilisation réelle."
        if french
        else "<b>Third-party tool, not an official Audinate product.</b> Version 2026.1 is a beta and may still contain bugs. Work on a copy and always validate the final XML by importing it into the appropriate official Dante tool before real use."
    )
    steps = (
        [
            ("Ouvrir XML", "Choisissez un export Dante. L'application travaille hors ligne et n'accède pas au réseau."),
            ("Contrôler les alertes", "Affichez les machines concernées et vérifiez chaque point signalé."),
            ("Modifier", "Utilisez Machines, Matrice, Easy patch, Liste RX vers TX ou les actions globales. Verrouillez les machines à exclure."),
            ("Vérifier", "Utilisez Modifiées uniquement puis Avant / après. Les changements techniques inconnus sont bloqués."),
            ("Enregistrer sous", "Choisissez un nouveau nom. La destination est remplacée atomiquement et sauvegardée si elle existait."),
            ("Tester l'import", "Importez le résultat dans Dante Controller sur une copie de travail avant toute intervention terrain."),
        ]
        if french
        else [
            ("Open XML", "Choose a Dante export. The application works offline and does not access the network."),
            ("Review alerts", "Show affected devices and verify every reported item."),
            ("Edit", "Use Devices, Matrix, Easy patch, Rx-to-Tx list, or global actions. Lock devices that must be excluded."),
            ("Review", "Use Modified only and Before / after. Unknown technical changes are blocked."),
            ("Save as", "Choose a new name. The destination is replaced atomically and backed up when it already exists."),
            ("Test the import", "Import the result into Dante Controller on a working copy before any field operation."),
        ]
    )
    features = (
        [
            ("Patch visuel", "Chaque clic ou glissement applique immédiatement le patch. L'alerte de remplacement d'un RX déjà patché reste activée par défaut et peut être désactivée."),
            ("Récupération", "Une copie est écrite en arrière-plan après un court délai. La nouvelle destination devient la référence après Enregistrer sous."),
            ("Import / Export", "Labels JSON/CSV, XLSX/ODS DMT, CSV A&H et ZIP Yamaha sont regroupés avec des modèles dLive, Avantis, CL et QL inclus."),
            ("Synoptique", "Regroupez les machines par emplacement, ouvrez un aperçu séparé aux proportions préservées et exportez un SVG ou PDF sans modifier le XML Dante."),
            ("Banque de machines", "Exportez ou importez une banque complète, ouvrez le catalogue GitHub et ajoutez au projet une instance indépendante."),
        ]
        if french
        else [
            ("Visual patch", "Every click or drag applies the patch immediately. Replacement warnings for already-patched Rx channels are enabled by default and can be cleared."),
            ("Recovery", "A copy is written in the background after a short delay. Save as makes the new destination the session reference."),
            ("Import / Export", "JSON/CSV, DMT XLSX/ODS, A&H CSV, and Yamaha ZIP labels are grouped with bundled dLive, Avantis, CL, and QL templates."),
            ("Synoptic", "Group devices by location, open a separate proportion-preserving preview, and export SVG or PDF without changing Dante XML."),
            ("Machine bank", "Export or import a complete bank, open the GitHub catalog, and add an independent instance to a project."),
        ]
    )

    story = [para(PRODUCT, "title"), para(subtitle, "subtitle"), callout(warning), Spacer(1, 3 * mm)]
    story.append(para("Le parcours recommandé" if french else "Recommended workflow", "h1"))
    step_rows = []
    for number, (heading, text) in enumerate(steps, start=1):
        badge = para(f"<font color='#1677D2'><b>{number}</b></font>", "h1")
        detail = para(f"<b>{heading}</b><br/>{text}", "small")
        step_rows.append([badge, detail])
    step_table = Table(step_rows, colWidths=[12 * mm, 158 * mm])
    step_table.setStyle(
        TableStyle(
            [
                ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
                ("LINEBELOW", (1, 0), (1, -2), 0.35, LINE),
                ("TOPPADDING", (0, 0), (-1, -1), 1.2 * mm),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 1.2 * mm),
            ]
        )
    )
    story.extend([step_table, Spacer(1, 2.5 * mm), para("Fonctions utiles" if french else "Useful features", "h1")])
    feature_cells = []
    for heading, text in features:
        feature_cells.append(para(f"<b>{heading}</b><br/>{text}", "small"))
    feature_rows = [feature_cells[index:index + 2] for index in range(0, len(feature_cells), 2)]
    if len(feature_rows[-1]) == 1:
        feature_rows[-1].append("")
    feature_table = Table(feature_rows, colWidths=[84 * mm, 84 * mm])
    feature_table.setStyle(
        TableStyle(
            [
                ("BACKGROUND", (0, 0), (-1, -1), PALE_BLUE),
                ("GRID", (0, 0), (-1, -1), 0.45, LINE),
                ("VALIGN", (0, 0), (-1, -1), "TOP"),
                ("LEFTPADDING", (0, 0), (-1, -1), 3 * mm),
                ("RIGHTPADDING", (0, 0), (-1, -1), 3 * mm),
                ("TOPPADDING", (0, 0), (-1, -1), 2 * mm),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 2 * mm),
            ]
        )
    )
    reminder = (
        "<b>À retenir :</b> aucun pilotage en temps réel, aucune API Audinate, et aucune garantie pour tous les formats XML. L'import dans les outils officiels reste indispensable."
        if french
        else "<b>Remember:</b> no real-time control, no Audinate API, and no guarantee for every XML format. Import validation in official tools remains mandatory."
    )
    atomic_note = (
        "<b>Exercice :</b> Atomic Bomb se trouve dans Outils avancés. Décochez les catégories à épargner, confirmez trois fois, puis utilisez Enregistrer sous pour créer le fichier destiné aux stagiaires. Les identifiants techniques restent protégés."
        if french
        else "<b>Exercise:</b> Atomic Bomb is located under Advanced tools. Clear categories you want to spare, confirm three times, then use Save as to create the trainee file. Technical identifiers remain protected."
    )
    labels_note = (
        "<b>Labels console :</b> choisissez A&H CSV natif - dLive/Avantis, Yamaha ZIP natif - CL/QL ou DMT XLSX/ODS. Les modèles sont inclus. Le CSV générique DCE n'est pas un fichier dLive Director."
        if french
        else "<b>Console labels:</b> choose Native A&H CSV - dLive/Avantis, Native Yamaha ZIP - CL/QL, or DMT XLSX/ODS. Templates are bundled. Generic DCE CSV is not a dLive Director file."
    )
    story.extend([feature_table, Spacer(1, 2.5 * mm), callout(labels_note), Spacer(1, 2 * mm), callout(atomic_note, PALE_RED), Spacer(1, 2 * mm), callout(reminder, PALE_GREEN), Spacer(1, 2 * mm), para(("Dépôt public : " if french else "Public repository: ") + GITHUB, "small")])
    build_document(ROOT / f"QuickStart_DanteConfigEditorV3_{language}.pdf", story)


def full_guide(language: str) -> None:
    french = language == "FR"
    if french:
        page1 = [
            para("1. Installation et démarrage", "h1"),
            callout("<b>Important :</b> cette application est un outil tiers non officiel Audinate. La 2026.1 est une version bêta et peut encore contenir des bugs. Elle édite des XML hors ligne, sans connexion au réseau Dante ni API Audinate. Conservez l'original et validez le fichier généré dans Dante Controller avant toute utilisation en production."),
            para("L'installateur Windows x64 contient l'application et le runtime .NET 8 nécessaire. Il n'est normalement pas nécessaire d'installer .NET séparément."),
            *bullets([
                "L'installation proposée par défaut se trouve dans Program Files et crée des raccourcis dans le menu Démarrer et sur le Bureau.",
                "Une installation 2026.1 Beta utilise son propre AppId, son dossier Program Files, ses raccourcis et son profil local afin de cohabiter avec la V3.6.",
                "La 2026.1 peut copier les réglages V3.6 vers son profil, mais ne modifie jamais le profil V3.6 en place.",
                "Les 43 modèles de DCE Generic Roles 2026.1 et DCE Community Devices 2026.1 sont intégrés à l'application. L'assistant peut aussi en créer des copies partageables dans un dossier choisi, sans remplacer la banque personnelle.",
                "Deux DMG 2026.1 Beta autonomes sont construits sur macOS pour Apple Silicon et Intel. Ils contiennent les banques 2026.1 dans le dossier Machine Banks ; leur bundle distinct peut cohabiter avec la V3.6.",
                "Les quatre notices PDF françaises et anglaises sont installées et restent accessibles depuis l'application.",
            ]),
            para("2. Principes de sécurité", "h1"),
            *bullets([
                "Travaillez sur une copie du XML exporté et utilisez Enregistrer sous.",
                "Le garde-fou suit les machines par identité technique stable, bloque les chemins inconnus et protège les Dante Id, mediaType et instance_id.",
                "La destination est remplacée atomiquement. Le fichier source et toute destination existante reçoivent une copie dans DanteConfigEditor_Backups.",
                "L'import réussi dans Dante Controller constitue la validation finale avant exploitation.",
            ]),
            para("3. Ouvrir un projet", "h1"),
            para("Cliquez sur Ouvrir XML, sélectionnez le fichier, puis contrôlez les compteurs de machines, canaux TX/RX et patchs actifs. Les XML avec namespace par défaut sont pris en charge. La langue et le thème restent modifiables à tout moment."),
        ]
        beta_page = [
            para("4. Espace de travail 2026.1", "h1"),
            para("Windows et macOS organisent le travail par intention : Projet, Vue d'ensemble, Machines, Patch, Import / Export, Centre de validation, Historique et Outils avancés. La banque est accessible directement depuis Machines."),
            *bullets([
                "La barre supérieure conserve le fichier actif, Enregistrer sous, Annuler et Rétablir.",
                "L'inspecteur de droite suit la dernière machine sélectionnée dans Machines, Patch ou Easy Patch et conserve ce contexte au changement de page.",
                "Navigation, réglages et inspecteur démarrent ouverts ; une flèche toujours visible masque ou rouvre chaque zone.",
                "Matrice, Easy patch, Liste RX vers TX et Synoptique partagent les mêmes identités stables et la même session.",
                "Le Centre de validation sépare contrôles internes DCE et validation manuelle Dante Controller.",
                "Un XML Dante reste le fichier d'échange officiel ; un .dceproj conserve en plus la disposition, les notes, les ressources et l'historique DCE.",
            ]),
            callout("Toutes les captures de cette notice sont générées avec l'interface 2026.1, un preset synthétique anonymisé et la banque publique assainie. Aucun XML de production n'est utilisé.", PALE_BLUE),
        ]
        page2 = [
            para("5. Page Machines", "h1"),
            para("La page Machines rassemble la banque de modèles, les listes rapides, les actions globales, la machine sélectionnée, ses canaux et le tableau général."),
            para("Machine sélectionnée", "h2"),
            *bullets([
                "Modifiez ensemble le nom, le mode réseau, la latence et le preferred master avec Appliquer les paramètres.",
                "Double-cliquez une ligne ou utilisez Détail machine pour régler l'IP, la sample rate, les bits, les canaux TX/RX et le patch de ses entrées RX.",
                "Les changements d'une fiche machine sont appliqués en groupe avec une seule reconstruction du modèle.",
                "Les resets peuvent déconnecter les RX, retirer les patchs utilisant les TX, ou effectuer les deux opérations.",
                "La suppression d'une machine retire aussi les points de patch qui la référencent.",
            ]),
            para("Tableau des machines", "h2"),
            *bullets([
                "La sélection multiple définit la cible Sélection non verrouillée. La colonne Lock protège les machines des actions globales.",
                "Le preferred master peut être coché directement. Réduire les réglages agrandit le tableau.",
                "Source de modèles filtre la banque personnelle ou une banque fournie. Gérer la banque ouvre par défaut une liste globale dédupliquée de 43 modèles avec leur origine ; Ajouter depuis la banque crée une instance indépendante.",
            ]),
            para("Recherche, filtres et actions globales", "h2"),
            *bullets([
                "La recherche trouve les machines, canaux et références de patch après au moins deux caractères.",
                "Les listes rapides filtrent modes réseau, latences, sample rates, bits, IP fixes et preferred masters.",
                "Modifiées uniquement affiche les machines touchées ; Avant / après détaille chaque différence.",
                "Choisissez toutes les machines non verrouillées, la sélection ou le filtre affiché. La cible reste visible avant l'application.",
            ]),
        ]
        page3 = [
            para("6. Alertes navigables", "h1"),
            para("Le bandeau Points à vérifier signale les mélanges redondant/daisychain, IP fixes, sample rates multiples et encodages multiples."),
            *bullets([
                "Cliquez sur Voir les machines, choisissez l'alerte puis examinez les devices filtrés.",
                "Après correction, vérifiez que l'alerte disparaît et consultez le Centre de validation.",
            ]),
            para("7. Profils rapides", "h1"),
            data_table(
                ["Profil", "Réglages appliqués"],
                [
                    ["48 kHz / 24 bit / 1 ms", "IP automatique"],
                    ["48 kHz / 24 bit / 2 ms", "IP automatique"],
                    ["96 kHz / 24 bit / 1 ms", "IP automatique"],
                    ["96 kHz / 24 bit / 2 ms", "IP automatique"],
                    ["48 kHz / 24 bit / 1 ms / Redondant", "Mode redondant et IP automatique"],
                    ["48 kHz / 24 bit / 1 ms / Daisychain", "Mode daisychain et IP automatique"],
                ],
                [75, 95],
            ),
            Spacer(1, 2 * mm),
            callout("Vérifiez que chaque matériel accepte la sample rate, les bits, la latence et le mode demandés.", PALE_RED),
            para("8. Récupération automatique", "h1"),
            para("Après une modification, l'application attend brièvement puis écrit la récupération en arrière-plan, sans bloquer l'interface ni remplacer le XML source."),
            *bullets([
                "À la prochaine ouverture du même XML, choisissez de restaurer ou d'abandonner la session.",
                "Après Enregistrer sous, le nouveau fichier devient la référence des modifications et récupérations suivantes.",
                "La copie disparaît après sauvegarde ou retour à l'original ; celles de plus de 30 jours sont nettoyées.",
            ]),
        ]
        page4 = [
            para("9. Canaux et patchs", "h1"),
            *bullets([
                "Les canaux TX/RX peuvent être renommés individuellement ou par plage avec {00}, {000}, {n} et {device}.",
                "Le renommage d'un TX met à jour tous les alias de subscription reconnus dans le projet.",
                "Dans les listes RX/TX, cliquez dans le nom puis utilisez Entrée pour valider, Tab pour valider et passer au canal suivant, Maj+Tab pour revenir au précédent, ou Échap pour annuler l'édition.",
                "Dans la matrice Easy patch, cliquez sur un libellé TX vertical pour le renommer. Entrée, Tab, Maj+Tab et Échap suivent le même fonctionnement que dans les listes.",
                "La poignée de recopie apparaît uniquement quand le nom se termine par un nombre. Mic 4 et Mic 04 sont prolongeables ; Mic, Mic gauche et Mic 4 principal ne le sont pas.",
                "La recopie conserve le texte avant le nombre et les zéros initiaux : Mic 04 devient Mic 05, Mic 06, etc. Aucun canal n'est modifié si le glissement est annulé.",
                "Les Dante Id ne sont pas renumérotés. Le marqueur local subscribed_device=\".\" est conservé.",
                "L'onglet Easy patch affiche les RX à gauche et les TX à droite. Les menus et flèches permettent de changer rapidement de machine.",
                "Sélectionnez autant de TX que de RX pour un appariement un-à-un, ou un seul TX pour alimenter plusieurs RX.",
                "Plusieurs TX vers un RX et les sélections multiples de tailles différentes sont refusés.",
                "Le patch par plage demande un premier TX, un premier RX et une quantité exacte ; une plage incomplète est entièrement bloquée.",
                "Un clic ou un glissement applique immédiatement les points de patch concernés, sans étape de prévisualisation.",
                "Les clics et glissements mettent uniquement à jour les cellules concernées : la matrice entière n'est plus reconstruite après chaque action.",
                "Les sélections, les plages et PATCH 1:1 sont également appliqués immédiatement.",
                "L'option M'avertir si le RX est déjà patché est cochée par défaut. Décochez-la uniquement pour remplacer une subscription sans afficher cette alerte.",
                "Dans la grille compacte, les RX sont en lignes et les TX en colonnes. Cliquez pour une affectation ou maintenez et glissez horizontalement, verticalement ou en diagonale pour une série sûre.",
                "Chaque opération immédiate reste annulable avec Annuler action.",
                "Dans Détail machine, le menu supérieur passe à une autre machine et protège les modifications non appliquées.",
            ]),
            Spacer(1, 2 * mm),
            data_table(
                ["Départ", "Poignée", "Résultat"],
                [
                    ["Mic 4  [tirer]", "Visible", "Mic 5, Mic 6, Mic 7..."],
                    ["Mic 04  [tirer]", "Visible", "Mic 05, Mic 06, Mic 07..."],
                    ["Mic gauche", "Masquée", "Aucune recopie proposée"],
                ],
                [50, 35, 85],
            ),
            para("10. Ajouter un XML au projet", "h1"),
            *bullets([
                "Les machines dont le nom est unique sont toujours importées.",
                "Seuls les doublons sont proposés au renommage automatique ou manuel.",
                "Les patchs importés suivent les nouveaux noms des machines renommées.",
            ]),
            para("11. IP et formats audio", "h1"),
            *bullets([
                "L'IP automatique ou fixe est réglable machine par machine ou globalement.",
                "Seule l'interface IPv4 principale, network=0 si elle existe, est ciblée. Une interface secondaire n'est pas modifiée.",
                "Le DNS n'est pas réécrit implicitement. La passerelle ne change que lorsqu'une valeur est fournie par l'action.",
                "Sample rate et bits sont modifiables par machine, globalement ou via un profil.",
            ]),
            callout("Un mauvais réglage peut rendre une machine injoignable ou incompatible. Contrôlez les capacités réelles du matériel.", PALE_RED),
            para("12. Validation, comparaison et Import / Export", "h1"),
            *bullets([
                "Le Centre de validation regroupe statistiques, erreurs, avertissements, patchs libres/locaux et compatibilité.",
                "La comparaison XML affiche les différences dans un tableau.",
                "Les exports TXT/PDF portent la version du logiciel et la signature By Mamat et ses agents.",
                "Import / Export regroupe Labels, Rapports et patchbook et Synoptique. Le synoptique mémorise les emplacements, affiche ou masque les machines, propose un aperçu séparé dont le zoom conserve les proportions et exporte un SVG ou un PDF ; sa mise en page locale ne modifie jamais le XML Dante.",
            ]),
        ]
        label_page = [
            para("Échanger les labels sans modèle externe", "h1"),
            callout("Les modèles dLive, Avantis, Yamaha CL/QL et DMT sont inclus dans Dante Config Editor. Un export natif demande seulement le nom et le dossier du nouveau fichier."),
            para("Choisir le bon format", "h1"),
            data_table(
                ["Format", "Destination", "Contenu"],
                [
                    ["JSON / CSV générique", "DCE ou outil tiers", "Unicode complet. Ne pas importer dans dLive Director."],
                    ["DMT XLSX/ODS dLive / Avantis", "dLive MIDI Tools", "Classeur DMT direct ; lignes hors sélection désactivées."],
                    ["A&H CSV natif dLive", "dLive Director", "Structure [Version]/[Channels] dLive et noms Input."],
                    ["A&H CSV natif Avantis", "Avantis Director", "Structure [Version]/[Channels] Avantis et noms Input."],
                    ["Yamaha ZIP natif CL / QL", "CL/QL Editor", "Paquet complet de neuf CSV ; seul InName.csv reçoit les labels."],
                ],
                [42, 44, 84],
            ),
            para("Procédure", "h1"),
            *bullets([
                "Dans Import / Export > Labels, cliquez sur Exporter des labels.",
                "Choisissez TX ou RX, les machines, le premier canal et le nombre. Une machine sans TX mais avec des RX bascule automatiquement sur RX.",
                "Choisissez le format natif correspondant au modèle réel. Les machines sans canal dans le sens choisi ne peuvent pas être cochées.",
                "Contrôlez l'aperçu. Activez l'adaptation ASCII/8 caractères uniquement si la destination l'exige, puis cliquez sur Exporter.",
                "DCE ouvre directement Enregistrer sous. La destination est écrite atomiquement et un échec ne détruit pas un fichier existant.",
            ]),
            callout("À l'import, DCE affiche le format détecté, la version source, les listes, machines, canaux, lignes ignorées, labels vides, doublons et avertissements. Appliquer exige au moins un changement sans erreur. Après un second chargement identique, le bouton reste volontairement désactivé et DCE indique que les labels correspondent déjà."),
            callout("Les exports JSON/CSV de DMT 2.14.0-RC1 sont vérifiés sur des fixtures produites avec les exporteurs DMT au commit 3c34052. Les classeurs XLSX/ODS restent fondés sur la feuille Channels des modèles DMT observés."),
            callout("Avant utilisation, ouvrez toujours le fichier généré dans DMT, dLive Director, Avantis Director ou Yamaha CL/QL Editor et vérifiez les labels et le modèle ciblé.", PALE_RED),
            para("Les classeurs DMT inclus proviennent du projet MIT dLive MIDI Tools de Tobias Grupe. Le fichier DMT_LICENSE.txt est fourni avec l'application.", "small"),
        ]
        page5 = [
            para("13. Atomic Bomb : créer un exercice", "h1"),
            *bullets([
                "Ouvrez Outils avancés puis Atomic Bomb. Décochez les catégories à épargner ; toutes sont sélectionnées par défaut. Trois confirmations détaillent ensuite les conséquences avant toute modification.",
                "La copie en mémoire reçoit des noms uniques mythologiques, audio ou humoristiques, ainsi qu'un mélange de patchs, modes réseau, Preferred Master, latences, sample rates, encodages et IP principales.",
                "Les identifiants techniques, namespaces, DNS, passerelles et interfaces secondaires restent protégés.",
                "Le résumé indique la graine du scénario. L'ensemble s'annule en une seule action et le fichier source n'est jamais écrasé.",
                "Utilisez Enregistrer sous pour remettre le preset aux stagiaires, puis vérifiez son import dans l'outil Dante officiel approprié.",
            ]),
            callout("Ce mode sert uniquement à la formation hors ligne. Il ne dérègle aucun appareil et ne communique pas avec le réseau Dante.", PALE_RED),
            para("14. Sauvegarde et validation finale", "h1"),
            para("Utilisez Enregistrer sous. Le XML temporaire est relu, le garde-fou vérifie les changements, puis la destination est remplacée atomiquement. Une erreur avant le remplacement laisse l'ancienne destination intacte."),
            data_table(
                ["Contrôle", "Action recommandée"],
                [
                    ["Points à vérifier", "Ouvrir les machines concernées et justifier ou corriger chaque écart."],
                    ["Modifiées uniquement", "Vérifier que seules les machines attendues apparaissent."],
                    ["Avant / après", "Relire les paramètres, canaux et patchs touchés."],
                    ["Dante Controller", "Importer le fichier sur une copie de travail avant toute intervention terrain."],
                ],
                [48, 122],
            ),
            para("15. Tests de non-régression", "h1"),
            para("La suite 2026.1 exécute 395 tests Core/Windows et 22 tests Mac sans écran. Ils couvrent notamment les garde-fous XML, la sauvegarde et la récupération, les interfaces IPv4, les subscriptions, les gros presets, la duplication, la banque de machines, la création de projet, le format .dceproj, les profils XML, les commandes, les formats DMT, les rapports d'import, le synoptique, Atomic Bomb, Easy patch, le soutien facultatif et la cohérence des traductions."),
            para("16. Limites connues", "h1"),
            *bullets([
                "Aucun pilotage en temps réel et aucune communication avec les appareils.",
                "Aucun SDK/API Audinate et aucun contournement de protocole propriétaire.",
                "La compatibilité dépend de la structure du XML ; seul l'import officiel confirme le fichier final.",
                "L'historique d'annulation conserve au maximum 10 états pour limiter la mémoire.",
                "La matrice affiche uniquement les deux machines choisies pour préserver les performances sur les gros presets.",
                "Les DMG Mac sont signés ad hoc mais non notariés ; le premier lancement peut nécessiter un clic droit puis Ouvrir.",
                "Windows et macOS proposent les mêmes parcours principaux ; certains contrôles gardent toutefois un rendu natif légèrement différent.",
                "Des noms TX dupliqués sont ambigus dans les subscriptions Dante et doivent être renommés avant Easy patch.",
                "Les classeurs natifs correspondent aux modèles DMT 2.13.0 observés et aux exemples dLive, Avantis, CL5 et QL5 fournis ; JSON/CSV DMT 2.14.0-RC1 est testé séparément.",
                "Les rôles génériques dupliqués ou ajoutés depuis la banque n'emportent aucun identifiant matériel instance_id/device_id ; seule une importation réelle dans Dante Controller peut confirmer leur utilisation avec une version donnée.",
                "Nouveau projet produit une structure minimale au format 3.0.0. Contrôlez toujours le fichier final dans Dante Controller avant exploitation.",
            ]),
            para("17. Aide et informations", "h1"),
            para(
                f"Quick start et Notice complète ouvrent automatiquement le PDF français ou anglais selon la langue active. "
                f"Projet public : {GITHUB} - Crédit : By Mamat et ses agents.",
                "small",
            ),
            para("18. Soutenir DCE", "h1"),
            para("Dante Config Editor reste entièrement gratuit et toutes ses fonctions sont disponibles sans contribution."),
            *bullets([
                "Le bouton Soutenir DCE se trouve dans le bandeau supérieur et les informations d'aide restent accessibles depuis Historique.",
                "Le bouton Soutenir DCE affiche le QR PayPal à scanner avec l'application du téléphone et un bouton PayPal.Me pour ordinateur ; aucun paiement n'est intégré à DCE et aucune connexion n'est effectuée au démarrage.",
                "Le rappel local n'apparaît pas au premier lancement. Il peut être reporté de 20 lancements ou désactivé définitivement.",
                "Une étoile sur GitHub ou un retour utilisateur aide aussi gratuitement. Et si vous êtes vraiment fous, vous pouvez même faire les deux !",
            ]),
        ]
        bank_page = [
            para("Banque de machines et rôles génériques", "h1"),
            callout("Ces fonctions manipulent des rôles de preset hors ligne, pas des appareils réels. DCE supprime les identifiants matériels de l'instance source et ne prétend pas créer l'identité d'un équipement Dante."),
            para("Dupliquer une machine", "h2"),
            *bullets([
                "Sélectionnez une machine puis choisissez Dupliquer. Donnez un nom de rôle unique.",
                "Les labels TX/RX peuvent être conservés. Réseau, réglages, flows, Preferred Master et subscriptions sont désactivés par défaut.",
                "L'original reste inchangé. La copie est ajoutée en une opération annulable et reçoit une identité de session propre à DCE, jamais écrite dans le XML.",
            ]),
            para("Enregistrer et partager un modèle", "h2"),
            *bullets([
                "Choisissez Enregistrer dans la banque, renseignez fabricant, modèle, catégorie, description, tags et labels génériques.",
                "Une image PNG, JPEG ou WebP facultative est copiée dans le dossier du modèle ; aucun chemin externe fragile n'est conservé.",
                "La banque se trouve par défaut dans Documents/Dante Config Editor/Machine Bank. Son emplacement peut être choisi, ouvert, copié ou placé dans un dossier synchronisé.",
                "La fenêtre affiche toutes les banques installées dans une liste dédupliquée de 43 modèles. Le sélecteur isole une banque et la colonne Banque indique l'origine ; une copie personnelle est prioritaire sur un doublon fourni.",
                "Exporter la banque crée une archive vérifiée *.dce-bank.zip. Importer une banque exige un dossier neuf ou vide et ne remplace jamais l'existant.",
                "Banques GitHub ouvre le catalogue public. DCE Generic Roles 2026.1 fournit deux rôles génériques d'essai. DCE Community Devices 2026.1 fournit 41 modèles illustrés et assainis issus de fabricants variés. Les 43 modèles sont intégrés aux installateurs sans modifier la banque personnelle et ne contiennent ni identité matérielle, ni donnée réseau, ni flow, ni subscription.",
                "L'administration permet recherche, filtres, modification, duplication, suppression confirmée et import/export d'un modèle ZIP.",
            ]),
            para("Ajouter un modèle au projet", "h2"),
            *bullets([
                "Choisissez Ajouter depuis la banque puis configurez le nouveau nom, les labels et les options explicitement souhaitées.",
                "L'instance ajoutée est indépendante du modèle. Modifier l'une ne modifie jamais l'autre.",
                "DCE vérifie la version du modèle, son empreinte, le nombre de canaux, le namespace et la version de preset avant une insertion transactionnelle.",
            ]),
            para("Nouveau projet hors ligne", "h2"),
            *bullets([
                "Nouveau projet crée une structure minimale 3.0.0 vide ou contenant un premier rôle issu de la banque.",
                "Le fichier existant n'est jamais écrasé silencieusement. L'écriture passe par un temporaire puis par validation et remplacement atomique.",
                "Réouvrez le XML dans DCE, consultez le Centre de validation, puis effectuez un import de contrôle dans Dante Controller.",
            ]),
            callout("Les journaux techniques sont accessibles depuis Historique. Ils expliquent les échecs d'import, de validation, de banque et d'export sans modifier le projet.", PALE_GREEN),
        ]
        screen_map = [
            para("Repère des écrans", "h1"),
            para("La barre supérieure ouvre, fusionne, sauvegarde, annule et restaure le projet. La colonne Projet reste visible pour les compteurs, alertes et recherches."),
            data_table(
                ["Écran", "Utilité principale"],
                [
                    ["Vue d'ensemble", "Compteurs, formats audio, réseau, horloge, alertes et dernières modifications."],
                    ["Machines", "Machine sélectionnée, canaux, listes rapides, actions globales et tableau des machines."],
                    ["Patch > Matrice", "Grille visuelle RX/TX, glissement, PATCH 1:1, zoom et renommage direct."],
                    ["Patch > Easy patch", "Sélection et plage 1:1 avec application immédiate et alerte de remplacement optionnelle."],
                    ["Patch > Liste RX vers TX", "Lecture et modification tabulaire des subscriptions, avec filtres et état détaillé."],
                    ["Import / Export > Labels", "Échange JSON/CSV, DMT XLSX/ODS, A&H et Yamaha, avec rapport d'import."],
                    ["Import / Export > Rapports", "Rapports TXT/PDF, patchbooks TXT/CSV et topologie textuelle simple."],
                    ["Import / Export > Synoptique", "Emplacements, ordre, visibilité, zoom, reset et exports SVG/PDF."],
                    ["Centre de validation", "Erreurs, avertissements, informations, chemins XML, navigation et export du rapport."],
                    ["Historique", "Modifications, comparaison XML, notices, journaux et diagnostic."],
                    ["Outils avancés", "Atomic Bomb : création hors ligne d'un exercice configurable et annulable."],
                ],
                [54, 116],
            ),
        ]
        visual_overview = [
            para("L'essentiel en un écran", "h1"),
            para("La page Configuration répond au besoin d'origine du logiciel : survoler rapidement tout le preset sans ouvrir successivement chaque page de Dante Controller."),
            feature_band([
                ("Repérer", "Les lignes colorées et le bandeau latéral signalent les écarts importants."),
                ("Cibler", "Filtres, sélection multiple et verrouillage définissent précisément les machines touchées."),
                ("Vérifier", "Avant / après permet de relire les changements avant la sauvegarde."),
            ]),
            Spacer(1, 3 * mm),
            *screenshot(
                language,
                "overview",
                "Vue d'ensemble : compteurs, formats audio, réseau, horloge et dernières modifications dans un seul écran.",
                crop=(220, 195, 1605, 760),
                maximum_height=102,
            ),
        ]
        visual_device = [
            para("Modifier une machine sans changer de page", "h1"),
            para("Détail machine regroupe les paramètres essentiels et permet de passer directement à une autre machine depuis le menu supérieur."),
            feature_band([
                ("Identité", "Nom de machine, mode réseau et Preferred Master."),
                ("Audio", "Latence, sample rate et bits par échantillon."),
                ("Réseau", "IP principale en automatique ou fixe, sans toucher aux interfaces secondaires."),
            ]),
            Spacer(1, 3 * mm),
            *screenshot(
                language,
                "devices",
                "<b>1</b> paramètres de la machine ; <b>2</b> canal TX/RX choisi ; <b>3</b> renommage par plage.",
                crop=(700, 195, 1600, 700),
                markers=[(0.25, 0.34, "1"), (0.70, 0.22, "2"), (0.75, 0.66, "3")],
                maximum_height=86,
            ),
            *bullets([
                "Les onglets RX puis TX permettent de renommer les canaux individuellement.",
                "Patch RX permet de contrôler ou déconnecter les subscriptions reçues par la machine.",
                "Appliquer valide l'ensemble en une seule opération groupée ; Annuler ne modifie pas le XML.",
            ]),
        ]
        profiles_detailed = [
            para("Profils rapides et actions globales", "h1"),
            para("Une action globale s'applique uniquement à la cible affichée : toutes les machines non verrouillées, la sélection non verrouillée ou le filtre courant. Vérifiez toujours la cible avant de confirmer."),
            *screenshot(
                language,
                "devices",
                "<b>1</b> listes rapides ; <b>2</b> onglets Réseau/audio, IP, Profils et Horloge ; <b>3</b> cible des actions et verrouillage.",
                crop=(230, 210, 690, 820),
                markers=[(0.22, 0.10, "1"), (0.45, 0.45, "2"), (0.55, 0.91, "3")],
                width=104,
                maximum_height=118,
            ),
            para("Utiliser un profil", "h2"),
            *bullets([
                "Ouvrez Actions globales > Profils, choisissez le profil, puis cliquez sur Appliquer le profil.",
                "Avant / après permet de relire l'état initial et l'état obtenu pour chaque machine réellement concernée.",
                "Les profils règlent sample rate, bits, latence et IP automatique ; les deux derniers règlent aussi Redondant ou Daisychain.",
                "Une machine verrouillée est toujours exclue. Annuler action restaure l'état précédent en une seule opération.",
            ]),
            callout("Un profil ne vérifie pas les capacités physiques du matériel. Contrôlez les fréquences, latences et modes réellement supportés.", PALE_RED),
        ]
        renaming_detailed = [
            para("Renommer rapidement les RX et les TX", "h1"),
            para("Le renommage direct fonctionne dans Configuration, Détail machine, Patch et Easy patch. Pour un TX, DCE met également à jour toutes les subscriptions reconnues qui utilisent ce nom."),
            *screenshot(
                language,
                "devices",
                "<b>1</b> choisir RX ou TX et le canal ; <b>2</b> saisir le nom ; <b>3</b> choisir la plage ; <b>4</b> définir le préfixe et le numéro de départ.",
                crop=(1150, 210, 1595, 610),
                markers=[(0.18, 0.13, "1"), (0.24, 0.36, "2"), (0.19, 0.69, "3"), (0.73, 0.86, "4")],
                width=155,
                maximum_height=90,
            ),
            para("Raccourcis pendant l'édition", "h2"),
            keyboard_table(language),
        ]
        series_detailed = [
            para("Prolonger une série comme dans un tableur", "h1"),
            para("La poignée de recopie apparaît uniquement lorsque le dernier caractère du label appartient à un nombre final. Tirez-la jusqu'au dernier canal souhaité ; un aperçu montre la série avant le relâchement."),
            data_table(
                ["Nom de départ", "Poignée", "Résultat"],
                [
                    ["Mic 4", "Visible", "Mic 5, Mic 6, Mic 7..."],
                    ["Mic 04", "Visible", "Mic 05, Mic 06, Mic 07..."],
                    ["Micro HF 12", "Visible", "Micro HF 13, Micro HF 14..."],
                    ["Mic", "Masquée", "Aucune série numérique détectée"],
                    ["Mic 4 principal", "Masquée", "Le nom ne se termine pas par un nombre"],
                ],
                [48, 34, 88],
            ),
            Spacer(1, 3 * mm),
            *bullets([
                "Le texte placé avant le nombre est conservé exactement.",
                "Le nombre peut comporter plusieurs chiffres et les zéros initiaux sont préservés.",
                "La recopie fonctionne dans les listes RX/TX et dans la grille Easy patch.",
                "Échap annule l'aperçu. Aucun label n'est modifié tant que la poignée n'a pas été déposée sur une cible valide.",
                "Une seule opération Annuler action restaure toute la série.",
            ]),
            callout("<b>Important :</b> le nombre final est un suffixe de label. DCE ne renumérote jamais les Dante Id techniques.", PALE_GREEN),
        ]
        visual_patch = [
            para("Patch : lire et corriger précisément une subscription", "h1"),
            para("Chaque ligne représente un RX et sa source TX. Patch est la vue la plus précise pour filtrer, vérifier une source locale ou externe, remplacer une subscription ou la supprimer."),
            *screenshot(
                language,
                "patch-list",
                "Filtres RX/TX à gauche, choix de source en haut et résultat complet RX vers TX dans le tableau.",
                crop=(230, 215, 1595, 940),
                maximum_height=96,
            ),
            feature_band([
                ("Simple", "Affiche RX device/Id/canal, TX device/Id/canal et l'état."),
                ("Expert", "Ajoute source brute, source résolue, type, actif, modifié et source complète."),
                ("Local", "La source « . » désigne la machine RX elle-même et reste préservée."),
            ]),
            Spacer(1, 2 * mm),
            *bullets([
                "Filtre récepteur RX et filtre émetteur TX réduisent le tableau sans modifier le XML.",
                "Sélectionnez une ligne RX, choisissez la machine TX et son canal, puis Appliquer.",
                "Supprimer déconnecte uniquement la ligne RX sélectionnée.",
                "Un patch vers une machine absente peut être normal dans un preset partiel : l'alerte doit être comprise avant remplacement.",
                "Le renommage direct des colonnes RX et TX accepte Entrée, Tab, Maj+Tab et la recopie en série.",
            ]),
        ]
        easy_patch_detailed = [
            para("Easy patch : travailler visuellement et immédiatement", "h1"),
            para("Easy patch affiche toujours les RX de la machine réceptrice à gauche et les TX de la machine émettrice à droite. Les flèches et menus passent rapidement d'une machine à l'autre."),
            *screenshot(
                language,
                "easy-patch",
                "<b>1</b> machine RX ; <b>2</b> FLIP échange seulement les rôles affichés ; <b>3</b> machine TX ; <b>4</b> sélection, plage et Patch 1:1.",
                crop=(230, 215, 1595, 900),
                markers=[(0.20, 0.30, "1"), (0.50, 0.30, "2"), (0.80, 0.30, "3"), (0.50, 0.67, "4")],
                maximum_height=105,
            ),
            callout("<b>FLIP ne retourne aucun patch.</b> Il échange uniquement les deux machines sélectionnées afin d'observer ou de créer les liaisons dans l'autre sens.", PALE_GREEN),
        ]
        easy_patch_workflows = [
            para("Easy patch : clic, glissement, plage et PATCH 1:1", "h1"),
            data_table(
                ["Geste", "Résultat"],
                [
                    ["Cliquer sur une case", "Applique immédiatement ce TX à ce RX."],
                    ["Glisser horizontalement", "Un TX de départ progresse sur plusieurs colonnes d'une même ligne."],
                    ["Glisser verticalement", "Une source TX alimente plusieurs RX successifs."],
                    ["Glisser en diagonale", "Crée une série un-à-un TX1>RX1, TX2>RX2, etc."],
                    ["Sélection RX/TX", "Même quantité : appariement 1:1. Un TX : distribution vers plusieurs RX."],
                    ["PATCH 1:1", "Cliquer le premier croisement, choisir le nombre, puis appliquer la série."],
                ],
                [52, 118],
            ),
            Spacer(1, 3 * mm),
            *screenshot(
                language,
                "patch",
                "Matrice compacte : les RX sont en lignes, les TX en colonnes, et chaque clic ou glissement s'applique immédiatement.",
                crop=(230, 215, 1595, 930),
                maximum_height=92,
            ),
            *bullets([
                "Chaque clic ou glissement modifie immédiatement le projet ; il n'existe plus de lot de prévisualisation à valider.",
                "M'avertir si le RX est déjà patché est activé par défaut. Décochez cette option uniquement si vous acceptez les remplacements sans confirmation.",
                "Une sélection avec plusieurs TX et plusieurs RX doit contenir le même nombre de canaux.",
                "Plusieurs TX vers un seul RX ou deux sélections multiples de tailles différentes sont refusés.",
                "Les labels RX et TX sont éditables directement. Tab, Maj+Tab, Échap et la poignée de série restent disponibles.",
                "La molette et les boutons -, 100 %, + et Ajuster contrôlent le zoom de la matrice.",
                "Annuler action restaure la dernière opération immédiate.",
            ]),
        ]
        merge_detailed = [
            para("Ajouter un autre XML au projet ouvert", "h1"),
            para("Cette commande fusionne des machines déjà décrites dans un second preset. Elle est différente d'un ajout depuis la banque : le second XML conserve sa structure de machine et ses patchs internes compatibles."),
            data_table(
                ["Étape", "Ce que fait DCE"],
                [
                    ["1. Ouvrir le projet principal", "Ce XML reste la base de la session et de la sauvegarde."],
                    ["2. Ajouter XML au projet", "DCE charge et valide le second fichier sans modifier son original."],
                    ["3. Vérifier le format", "Version de preset et namespace doivent être identiques."],
                    ["4. Gérer les doublons", "Seuls les noms déjà utilisés sont proposés : ignorer, suffixe automatique personnalisé ou noms manuels."],
                    ["5. Adapter les références", "Les subscriptions importées suivent les machines du second XML qui ont été renommées."],
                    ["6. Contrôler", "Le résultat indique machines ajoutées, renommées et doublons ignorés."],
                ],
                [50, 120],
            ),
            Spacer(1, 3 * mm),
            callout("Les machines dont le nom ne crée aucun conflit sont toujours ajoutées. Le suffixe automatique est normalisé sans parenthèses.", PALE_GREEN),
            *bullets([
                "Un XML invalide, une version différente ou un namespace différent bloque toute la fusion.",
                "Un nom final déjà utilisé bloque l'opération au lieu de produire un doublon ambigu.",
                "La fusion est annulable. Utilisez ensuite le Centre de validation et Avant / après avant Enregistrer sous.",
            ]),
        ]
        bank_concept_page = [
            para("Comprendre la banque de machines", "h1"),
            callout("Une banque contient des <b>modèles réutilisables de rôles hors ligne</b>. Ce n'est ni un inventaire du réseau réel, ni un catalogue d'identités Dante prêtes à être déployées."),
            data_table(
                ["Action", "Source", "Résultat"],
                [
                    ["Dupliquer", "Machine du projet courant", "Nouvelle machine indépendante dans le même projet."],
                    ["Enregistrer dans la banque", "Machine du projet courant", "Modèle assaini, versionné et réutilisable."],
                    ["Ajouter depuis la banque", "Modèle de banque", "Nouvelle instance indépendante dans le projet ouvert."],
                    ["Ajouter XML au projet", "Second preset XML", "Machines compatibles et leurs références ajoutées au projet courant."],
                    ["Nouveau projet", "Structure minimale et banque", "XML 3.0.0 à contrôler avant exploitation."],
                ],
                [43, 51, 76],
            ),
            Spacer(1, 3 * mm),
            para("Ce qui est retiré d'un modèle", "h2"),
            *bullets([
                "instance_id, device_id et autres identités matérielles de l'instance source ;",
                "adresses et interfaces réseau du projet source ;",
                "subscriptions, patchs et flows liés aux autres machines ;",
                "Preferred Master et valeurs spécifiques qui n'ont pas été explicitement choisies.",
            ]),
            para("Ce qui peut rester", "h2"),
            *bullets([
                "fabricant, modèle, catégorie, description, tags et image ;",
                "structure compatible du rôle et nombre de canaux TX/RX ;",
                "labels TX/RX génériques modifiables avant insertion.",
            ]),
        ]
        bank_workflow_page = [
            para("Créer, partager et réutiliser une banque", "h1"),
            data_table(
                ["Besoin", "Procédure"],
                [
                    ["Créer un modèle", "Sélectionner la machine > Enregistrer dans la banque > remplacer les labels propres au projet par des labels génériques > renseigner les métadonnées > Enregistrer."],
                    ["Ajouter une image", "Choisir PNG, JPEG ou WebP. L'image est copiée dans le dossier du modèle ; le fichier original peut ensuite être déplacé."],
                    ["Parcourir les banques", "Machines > Gérer la banque ouvre les 43 modèles sans doublon. Le sélecteur isole la banque personnelle ou une banque fournie et la colonne Banque affiche l'origine."],
                    ["Partager toute la banque", "Exporter la banque crée une archive *.dce-bank.zip vérifiée."],
                    ["Installer une banque", "Importer une banque puis choisir un dossier neuf ou vide. DCE ne remplace jamais silencieusement une banque existante."],
                    ["Ajouter au projet", "Sélectionner le modèle > Ajouter au projet > choisir un nom unique et les labels > confirmer."],
                ],
                [48, 122],
            ),
            Spacer(1, 3 * mm),
            *bullets([
                "Modifier une machine ajoutée ne modifie jamais son modèle de banque.",
                "Modifier le modèle ne change jamais les instances déjà ajoutées aux projets.",
                "La version, l'empreinte, le namespace, le nombre de canaux et la version de preset sont vérifiés avant insertion.",
                "Les 43 modèles GitHub fournis sont assainis et installés avec l'application ; une banque personnelle n'est jamais remplacée.",
            ]),
            callout("Après ajout ou création de projet, réouvrez le XML, vérifiez le Centre de validation puis importez une copie dans Dante Controller. Un rôle générique sans identité matérielle doit être contrôlé avant exploitation.", PALE_RED),
        ]
        visual_labels = [
            para("Import / Export de labels", "h1"),
            para("L'onglet Labels centralise les échanges génériques, DMT, Allen & Heath et Yamaha. Les formats natifs sont créés depuis les modèles inclus ; DCE ne demande pas de choisir un fichier modèle externe."),
            *screenshot(
                language,
                "labels",
                "Import à gauche, export à droite. Le bouton DMT ouvre le projet dLive MIDI Tools associé.",
                crop=(230, 215, 1595, 930),
                maximum_height=96,
            ),
            *bullets([
                "Import : choisir le fichier, vérifier le rapport détecté, associer les listes aux machines, puis appliquer uniquement les changements valides.",
                "Export : choisir RX/TX, machines, plage, format et adaptation éventuelle des noms, puis donner le nom du nouveau fichier.",
                "DMT utilise XLSX/ODS ou JSON/CSV ; dLive et Avantis utilisent leur CSV natif ; CL/QL utilisent le ZIP natif complet.",
                "Un second import identique n'active pas Appliquer : les labels correspondent déjà.",
            ]),
        ]
        visual_synoptic = [
            para("Construire et exporter le synoptique", "h1"),
            *screenshot(
                language,
                "synoptic",
                "<b>1</b> emplacement ; <b>2</b> ordre et visibilité ; <b>3</b> aperçu zoomable ; <b>4</b> reset et exports SVG/PDF.",
                crop=(230, 215, 1595, 940),
                markers=[(0.15, 0.22, "1"), (0.15, 0.50, "2"), (0.65, 0.47, "3"), (0.75, 0.94, "4")],
                maximum_height=105,
            ),
            *bullets([
                "Sélectionnez une ou plusieurs machines, saisissez ou choisissez un emplacement, puis Affecter.",
                "À emplacement identique, le plus petit numéro d'ordre place la machine plus haut.",
                "Décochez Voir pour masquer une machine sans la supprimer du projet.",
                "Dans l'éditeur visuel, déplacez une machine à la souris ; les câbles suivent. Reset reconstruit une disposition automatique propre.",
                "Les patchs consécutifs sont regroupés sur un câble libellé par plage. Une liaison dans les deux sens utilise une seule ligne avec une flèche à chaque extrémité.",
                "Le zoom, Ajuster et la fenêtre d'aperçu séparée facilitent les grands projets.",
                "SVG et PDF sont des exports de présentation. Emplacements et positions sont conservés à côté du projet et ne modifient jamais le XML Dante.",
            ]),
        ]
        visual_health = [
            para("Contrôler avant d'enregistrer", "h1"),
            para("Le Centre de validation sépare les erreurs bloquantes, les avertissements et les informations. Lisez la ligne complète : elle précise la catégorie, l'élément concerné, la cause et le chemin XML."),
            *screenshot(
                language,
                "validation",
                "Exemple anonymisé : formats audio mélangés, mode réseau mixte, IP fixe, patch local et RX libre.",
                crop=(230, 215, 1595, 930),
                maximum_height=96,
            ),
            feature_band([
                ("Centre de validation", "Erreurs, avertissements, formats audio, IP fixes, patchs locaux et chemins XML."),
                ("Historique", "Modifications, comparaison XML, journaux, diagnostic et notices."),
                ("Outils avancés", "Atomic Bomb, catégories configurables et trois confirmations obligatoires."),
            ]),
            Spacer(1, 3 * mm),
            *bullets([
                "Une erreur bloque la sauvegarde ; un warning exige une vérification mais peut décrire une situation volontaire.",
                "Points à vérifier > Voir les machines applique un filtre pour retrouver rapidement les appareils concernés.",
                "Modifiées uniquement puis Avant / après permettent de relire précisément le périmètre des changements.",
                "Historique regroupe modifications, comparaison XML, journaux, diagnostic et accès aux notices.",
            ]),
        ]
        visual_atomic = [
            para("Atomic Bomb : préparer un exercice, jamais un réseau réel", "h1"),
            *screenshot(
                language,
                "atomic-bomb",
                "Décochez les catégories à préserver. Trois confirmations sont exigées avant de modifier la copie en mémoire.",
                crop=(450, 300, 1375, 840),
                maximum_height=100,
            ),
            *bullets([
                "Les catégories couvrent noms, labels, patchs, réseau, Preferred Master, latences, sample rates, bits et IP principales.",
                "Les identifiants techniques, namespaces, DNS, passerelles et interfaces secondaires restent protégés.",
                "Toute l'opération tient dans une seule étape Annuler action.",
                "Le XML source n'est jamais écrasé. Utilisez Enregistrer sous pour produire le fichier d'exercice.",
            ]),
            callout("Atomic Bomb ne communique avec aucun appareil. Le fichier d'exercice doit malgré tout être contrôlé dans Dante Controller avant d'être remis aux stagiaires.", PALE_RED),
        ]
    else:
        page1 = [
            para("1. Installation and startup", "h1"),
            callout("<b>Important:</b> this is a third-party tool, not an official Audinate product. Version 2026.1 is a beta and may still contain bugs. It edits XML files offline without connecting to a Dante network or using an Audinate API. Keep the original and validate the generated file in Dante Controller before production use."),
            para("The Windows x64 installer includes the application and the required .NET 8 runtime. A separate .NET installation is normally not required."),
            *bullets([
                "The default location is Program Files, with Start menu and desktop shortcuts.",
                "A 2026.1 Beta installation uses its own AppId, Program Files folder, shortcuts, and local profile so it can coexist with V3.6.",
                "2026.1 may copy V3.6 settings into its own profile but never modifies the V3.6 profile in place.",
                "The 43 templates from DCE Generic Roles 2026.1 and DCE Community Devices 2026.1 are bundled with the application. The wizard may also create shareable copies in a chosen folder without replacing the personal bank.",
                "Two self-contained 2026.1 Beta DMGs are built on macOS for Apple Silicon and Intel. They include the 2026.1 banks in the Machine Banks folder, and their separate bundle can coexist with V3.6.",
                "All four French and English PDFs are installed and remain available from the application.",
            ]),
            para("2. Safety principles", "h1"),
            *bullets([
                "Work on a copy of the exported XML and use Save as.",
                "The guard tracks devices by stable technical identity, blocks unknown paths, and protects Dante IDs, mediaType, and instance_id.",
                "The destination is replaced atomically. The source and any existing destination receive a copy in DanteConfigEditor_Backups.",
                "A successful import into Dante Controller is the final validation before operation.",
            ]),
            para("3. Open a project", "h1"),
            para("Click Open XML, choose the file, then review device, TX/RX channel, and active subscription counts. XML files with a default namespace are supported. Language and theme can be changed at any time."),
        ]
        beta_page = [
            para("4. 2026.1 workspace", "h1"),
            para("Windows and macOS organize work by intent: Project, Overview, Devices, Patch, Import / Export, Validation center, History, and Advanced tools. The bank is available directly from Devices."),
            *bullets([
                "The top bar keeps the active file, Save as, Undo, and Redo available.",
                "The right inspector follows the last device selected in Devices, Patch, or Easy patch and keeps that context when changing pages.",
                "Navigation, settings, and inspector start expanded; one persistent arrow hides or reopens each area.",
                "Matrix, Easy patch, Rx-to-Tx list, and Synoptic share stable identities and one session.",
                "The Validation Center separates internal DCE checks from manual Dante Controller validation.",
                "Dante XML remains the official exchange file; a .dceproj additionally stores DCE layout, notes, assets, and history.",
            ]),
            callout("Every screenshot in this guide is generated with the 2026.1 interface, an anonymized synthetic preset, and the sanitized public bank. No production XML is used.", PALE_BLUE),
        ]
        page2 = [
            para("5. Devices page", "h1"),
            para("The Devices page combines the template bank, quick lists, global actions, the selected device, its channels, and the complete device table."),
            para("Selected device", "h2"),
            *bullets([
                "Apply the name, network mode, latency, and Preferred Master state together with Apply settings.",
                "Double-click a row or use Device details to edit IP settings, sample rate, bits per sample, TX/RX names, and subscriptions for its Rx inputs.",
                "A complete Device details change is grouped into one model rebuild.",
                "Clear actions can disconnect Rx inputs, remove subscriptions using Tx channels, or do both.",
                "Deleting a device also removes subscription points that reference it.",
            ]),
            para("Device table", "h2"),
            *bullets([
                "Multiple selection defines the Selected unlocked target. The Lock column protects devices from global actions.",
                "Preferred Master can be toggled directly. Hide settings enlarges the table.",
                "Template source filters the personal or a bundled bank. Manage bank opens a global deduplicated list of 43 templates with their source; Add from bank creates an independent instance.",
            ]),
            para("Search, filters, and global actions", "h2"),
            *bullets([
                "Search finds devices, channels, and subscription references after at least two characters.",
                "Quick lists filter network modes, latencies, sample rates, bits, static IPs, and Preferred Masters.",
                "Modified only shows changed devices; Before / after lists every difference.",
                "Choose all unlocked, selected unlocked, or visible unlocked devices. The target remains visible before application.",
            ]),
        ]
        page3 = [
            para("6. Navigable alerts", "h1"),
            para("The Items to check banner reports mixed redundant/daisychain modes, static IPs, multiple sample rates, and multiple bit depths."),
            *bullets([
                "Click Show devices, choose an alert, then review the filtered devices.",
                "After correcting an item, verify that the alert disappears and review the Validation center.",
            ]),
            para("7. Quick profiles", "h1"),
            data_table(
                ["Profile", "Applied settings"],
                [
                    ["48 kHz / 24 bit / 1 ms", "Automatic IP"],
                    ["48 kHz / 24 bit / 2 ms", "Automatic IP"],
                    ["96 kHz / 24 bit / 1 ms", "Automatic IP"],
                    ["96 kHz / 24 bit / 2 ms", "Automatic IP"],
                    ["48 kHz / 24 bit / 1 ms / Redundant", "Redundant mode and automatic IP"],
                    ["48 kHz / 24 bit / 1 ms / Daisychain", "Daisychain mode and automatic IP"],
                ],
                [75, 95],
            ),
            Spacer(1, 2 * mm),
            callout("Verify that every device supports the requested sample rate, bit depth, latency, and network mode.", PALE_RED),
            para("8. Automatic recovery", "h1"),
            para("After a change, the application waits briefly and writes recovery data in the background without blocking the interface or replacing the source XML."),
            *bullets([
                "When reopening the same XML, choose whether to restore or discard the previous session.",
                "After Save as, the new file becomes the reference for later edits and recovery data.",
                "The copy is deleted after saving or reverting; copies older than 30 days are cleaned automatically.",
            ]),
        ]
        page4 = [
            para("9. Channels and subscriptions", "h1"),
            *bullets([
                "TX/RX channels can be renamed individually or by range with {00}, {000}, {n}, and {device}.",
                "Renaming a Tx channel updates every recognized subscription alias in the project.",
                "In Rx/Tx lists, click the name, then press Enter to validate, Tab to validate and move forward, Shift+Tab to move backward, or Escape to cancel the edit.",
                "In the Easy patch matrix, click a vertical Tx label to rename it. Enter, Tab, Shift+Tab, and Escape behave exactly as they do in the lists.",
                "The fill handle appears only when the name ends with a number. Mic 4 and Mic 04 can be extended; Mic, Mic left, and Mic 4 main cannot.",
                "Fill keeps the text before the number and any leading zeroes: Mic 04 becomes Mic 05, Mic 06, and so on. Cancelling the drag changes no channel.",
                "Dante IDs are not renumbered. The local subscribed_device=\".\" marker is preserved.",
                "The Easy patch tab shows Rx channels on the left and Tx channels on the right. Menus and arrows switch devices quickly.",
                "Select equal Tx and Rx counts for one-to-one mapping, or one Tx to feed several Rx channels.",
                "Several Tx channels to one Rx and unequal multiple selections are blocked.",
                "Range patching requires a first Tx, first Rx, and exact count; an incomplete range is blocked as a whole.",
                "A click or drag immediately applies the affected crosspoints, with no preview step.",
                "Clicks and drags update only the affected cells: the entire matrix is no longer rebuilt after every action.",
                "Selections, ranges, and PATCH 1:1 are also applied immediately.",
                "Warn me when the Rx channel is already patched is selected by default. Clear it only to replace a subscription without that warning.",
                "In the compact matrix, Rx channels are rows and Tx channels are columns. Click for one assignment, or hold and drag horizontally, vertically, or diagonally for a safe range.",
                "Each immediate operation remains reversible with Undo.",
                "In Device details, the top menu switches devices and protects unapplied changes.",
            ]),
            Spacer(1, 2 * mm),
            data_table(
                ["Starting name", "Handle", "Result"],
                [
                    ["Mic 4  [drag]", "Visible", "Mic 5, Mic 6, Mic 7..."],
                    ["Mic 04  [drag]", "Visible", "Mic 05, Mic 06, Mic 07..."],
                    ["Mic left", "Hidden", "No fill action offered"],
                ],
                [50, 35, 85],
            ),
            para("10. Add XML to project", "h1"),
            *bullets([
                "Devices with unique names are always imported.",
                "Only conflicting names are offered for automatic or manual rename.",
                "Imported subscriptions follow renamed imported devices.",
            ]),
            para("11. IP and audio formats", "h1"),
            *bullets([
                "Automatic or static IP is editable per device or through a global action.",
                "Only the primary IPv4 interface, network=0 when available, is targeted. A secondary interface is not changed.",
                "DNS is not rewritten implicitly. Gateway changes only when the action provides a value.",
                "Sample rate and bits per sample are editable per device, globally, or through a profile.",
            ]),
            callout("Incorrect settings can make a device unreachable or incompatible. Verify actual hardware capabilities.", PALE_RED),
            para("12. Validation, comparison, and Import / Export", "h1"),
            *bullets([
                "The Validation center combines statistics, errors, warnings, free/local subscriptions, and compatibility checks.",
                "XML comparison displays differences in a table.",
                "TXT/PDF exports include the application version and the By Mamat et ses agents signature.",
                "Import / Export groups Labels, Reports and patchbook, and Synoptic. The synoptic remembers locations, shows or hides devices, provides a separate preview whose zoom preserves proportions, and exports SVG or PDF; its local layout sidecar never changes Dante XML.",
            ]),
        ]
        label_page = [
            para("Exchange labels without an external template", "h1"),
            callout("dLive, Avantis, Yamaha CL/QL, and DMT templates are bundled with Dante Config Editor. Native export only asks for the new file name and folder."),
            para("Choose the correct format", "h1"),
            data_table(
                ["Format", "Destination", "Content"],
                [
                    ["Generic JSON / CSV", "DCE or third-party tool", "Full Unicode. Do not import into dLive Director."],
                    ["DMT XLSX/ODS dLive / Avantis", "dLive MIDI Tools", "Direct DMT workbook; rows outside the selection are disabled."],
                    ["Native A&H CSV dLive", "dLive Director", "dLive [Version]/[Channels] structure and Input names."],
                    ["Native A&H CSV Avantis", "Avantis Director", "Avantis [Version]/[Channels] structure and Input names."],
                    ["Native Yamaha ZIP CL / QL", "CL/QL Editor", "Complete nine-CSV package; only InName.csv receives labels."],
                ],
                [42, 44, 84],
            ),
            para("Workflow", "h1"),
            *bullets([
                "Under Import / Export > Labels, choose Export labels.",
                "Choose TX or RX, devices, first channel, and count. A device with RX but no TX automatically switches to RX.",
                "Choose the native format matching the real model. Devices without channels in the selected direction cannot be checked.",
                "Review the preview. Enable ASCII/eight-character adaptation only when required, then choose Export.",
                "DCE opens Save as directly. Output is written atomically, so a failed export does not destroy an existing file.",
            ]),
            callout("During import, DCE reports the detected format, source version, lists, devices, channels, ignored rows, empty labels, duplicates, and warnings. Apply requires at least one error-free change. After loading the same labels again, the button intentionally remains disabled and DCE states that the labels already match."),
            callout("DMT 2.14.0-RC1 JSON/CSV exports are checked with fixtures generated by the DMT exporters at commit 3c34052. XLSX/ODS support continues to target the Channels sheet from observed DMT workbooks."),
            callout("Before use, always open the generated file in DMT, dLive Director, Avantis Director, or Yamaha CL/QL Editor and verify labels and the selected model.", PALE_RED),
            para("Bundled DMT workbooks come from Tobias Grupe's MIT-licensed dLive MIDI Tools project. DMT_LICENSE.txt is included with the application.", "small"),
        ]
        page5 = [
            para("13. Atomic Bomb: create an exercise", "h1"),
            *bullets([
                "Open Advanced tools, then Atomic Bomb. Clear the categories you want to spare; all are selected by default. Three confirmations describe the consequences before any change.",
                "The in-memory copy receives unique mythological, audio-themed, or playful names plus a mixture of subscriptions, network modes, Preferred Master states, latencies, sample rates, encodings, and primary IP settings.",
                "Technical identifiers, namespaces, DNS, gateways, and secondary interfaces remain protected.",
                "The summary displays the scenario seed. The entire operation is one undo step and the source file is never overwritten.",
                "Use Save as to provide the trainee preset, then verify its import in the appropriate official Dante tool.",
            ]),
            callout("This mode is only for offline training. It does not alter any device or communicate with the Dante network.", PALE_RED),
            para("14. Save and final validation", "h1"),
            para("Use Save as. The temporary XML is reloaded, protected changes are checked, and the destination is replaced atomically. A failure before replacement leaves the previous destination intact."),
            data_table(
                ["Check", "Recommended action"],
                [
                    ["Items to check", "Open affected devices and explain or correct every unexpected difference."],
                    ["Modified only", "Verify that only the intended devices appear."],
                    ["Before / after", "Review every changed setting, channel, and subscription."],
                    ["Dante Controller", "Import the file into a working copy before any field operation."],
                ],
                [48, 122],
            ),
            para("15. Regression tests", "h1"),
            para("The 2026.1 suite runs 395 Core/Windows tests and 22 headless Mac tests. Coverage includes XML guards, save and recovery, IPv4 interfaces, subscriptions, large presets, duplication, the device bank, project creation, .dceproj packages, XML profiles, commands, DMT formats, import reports, synoptic export, Atomic Bomb, Easy patch, optional support, and translation consistency."),
            para("16. Known limitations", "h1"),
            *bullets([
                "No real-time Dante control and no communication with devices.",
                "No Audinate SDK/API and no proprietary protocol bypass.",
                "Compatibility depends on the XML structure; only an official import confirms the final file.",
                "Undo keeps at most 10 states to limit memory use.",
                "The matrix displays only the two selected devices to preserve performance on large presets.",
                "Mac DMGs are ad hoc signed but not notarized; first launch may require right-clicking the application and choosing Open.",
                "Windows and macOS expose the same main workflows, although some controls retain a slightly different native rendering.",
                "Duplicate Tx names are ambiguous in Dante subscriptions and must be renamed before using Easy patch.",
                "Native workbooks match observed DMT 2.13.0 templates and the supplied dLive, Avantis, CL5, and QL5 examples; DMT 2.14.0-RC1 JSON/CSV is tested separately.",
                "Generic roles duplicated or added from the bank carry no hardware instance_id/device_id. Only an actual Dante Controller import can confirm their use with a given version.",
                "New project writes a minimal 3.0.0 structure. Always review the final file in Dante Controller before operation.",
            ]),
            para("17. Help and information", "h1"),
            para(
                f"Quick start and Full guide automatically open the French or English PDF for the active language. "
                f"Public project: {GITHUB} - Credit: By Mamat et ses agents.",
                "small",
            ),
            para("18. Support DCE", "h1"),
            para("Dante Config Editor remains completely free, and every feature is available without contributing."),
            *bullets([
                "The Support DCE button is available in the top banner, while help information remains accessible from History.",
                "The Support DCE button displays the PayPal QR code for the phone app and a PayPal.Me button for computers; DCE contains no payment system and performs no network request at startup.",
                "The local reminder does not appear on first launch. It can be postponed for 20 launches or disabled permanently.",
                "Starring the GitHub project or sharing feedback also helps for free. And if you are truly crazy, you can even do both!",
            ]),
        ]
        bank_page = [
            para("Machine bank and generic roles", "h1"),
            callout("These functions edit offline preset roles, not real devices. DCE removes hardware identifiers from the source instance and does not claim to create a Dante device identity."),
            para("Duplicate a device", "h2"),
            *bullets([
                "Select a device, choose Duplicate, and provide a unique role name.",
                "TX/RX labels may be retained. Network data, settings, flows, Preferred Master, and subscriptions are disabled by default.",
                "The source is unchanged. The copy is added as one undoable operation and receives a DCE session identity that is never serialized.",
            ]),
            para("Save and share a template", "h2"),
            *bullets([
                "Choose Save to machine bank and enter manufacturer, model, category, description, tags, and generic labels.",
                "An optional PNG, JPEG, or WebP image is copied into the model folder; no fragile external path is kept.",
                "The default bank is Documents/Dante Config Editor/Machine Bank. You may choose, open, copy, or place it in a synchronized folder.",
                "The window combines all installed banks into one deduplicated list of 43 templates. The selector isolates one bank and the Bank column identifies the source; a personal copy takes priority over a bundled duplicate.",
                "Export bank creates a verified *.dce-bank.zip archive. Import bank requires a new or empty folder and never replaces existing data.",
                "GitHub banks opens the public catalog. DCE Generic Roles 2026.1 provides two generic test roles. DCE Community Devices 2026.1 provides 41 sanitized illustrated templates from several manufacturers. These banks contain no hardware identity, network data, flow, or subscription.",
                "Administration supports search, filters, edit, duplicate, confirmed delete, and model ZIP import/export.",
            ]),
            para("Add a template to the project", "h2"),
            *bullets([
                "Choose Add from bank, then configure the new name, labels, and only the options you explicitly need.",
                "The inserted instance is independent from the template. Editing either one never changes the other.",
                "DCE checks model version, checksum, channel counts, namespace, and preset version before a transactional insertion.",
            ]),
            para("New offline project", "h2"),
            *bullets([
                "New project writes a minimal 3.0.0 structure, either empty or with one initial role from the bank.",
                "An existing file is never overwritten silently. Writing uses a temporary file followed by validation and atomic replacement.",
                "Reopen the XML in DCE, review the Validation center, then perform a control import into Dante Controller.",
            ]),
            callout("Technical logs are available from History. They explain import, validation, bank, and export failures without changing the project.", PALE_GREEN),
        ]
        screen_map = [
            para("Screen map", "h1"),
            para("The top bar opens, merges, saves, undoes, and restores the project. The Project column stays visible for counters, alerts, and search."),
            data_table(
                ["Screen", "Main purpose"],
                [
                    ["Overview", "Counters, audio formats, network, clock, alerts, and latest changes."],
                    ["Devices", "Selected device, channels, quick lists, global actions, and device table."],
                    ["Patch > Matrix", "Visual Rx/Tx matrix, drag gestures, PATCH 1:1, zoom, and direct rename."],
                    ["Patch > Easy patch", "Selection and 1:1 range tools with immediate apply and optional replacement warning."],
                    ["Patch > Rx-to-Tx list", "Tabular subscription review and editing with filters and detailed state."],
                    ["Import / Export > Labels", "JSON/CSV, DMT XLSX/ODS, A&H, and Yamaha exchange with an import report."],
                    ["Import / Export > Reports", "TXT/PDF reports, TXT/CSV patchbooks, and a simple text topology."],
                    ["Import / Export > Synoptic", "Locations, order, visibility, zoom, reset, and SVG/PDF exports."],
                    ["Validation center", "Errors, warnings, information, XML paths, navigation, and report export."],
                    ["History", "Changes, XML comparison, user guides, logs, and diagnostics."],
                    ["Advanced tools", "Atomic Bomb: configurable, undoable, offline troubleshooting exercises."],
                ],
                [54, 116],
            ),
        ]
        visual_overview = [
            para("The essentials on one screen", "h1"),
            para("The Overview page addresses the software's original need: review an entire preset quickly without opening each Dante Controller page in turn."),
            feature_band([
                ("Spot issues", "Colored rows and the side banner highlight important discrepancies."),
                ("Target safely", "Filters, multiple selection, and locks define exactly which devices are affected."),
                ("Review", "Before / after lets you inspect every change before saving."),
            ]),
            Spacer(1, 3 * mm),
            *screenshot(
                language,
                "overview",
                "Overview: counters, audio formats, network, clock, and latest changes on one screen.",
                crop=(220, 195, 1605, 760),
                maximum_height=102,
            ),
        ]
        visual_device = [
            para("Edit a device without leaving the workflow", "h1"),
            para("Device details combines the essential settings and lets you move directly to another device from the top menu."),
            feature_band([
                ("Identity", "Device name, network mode, and Preferred Master."),
                ("Audio", "Latency, sample rate, and bits per sample."),
                ("Network", "Automatic or static primary IP without changing secondary interfaces."),
            ]),
            Spacer(1, 3 * mm),
            *screenshot(
                language,
                "devices",
                "<b>1</b> device settings; <b>2</b> selected Tx/Rx channel; <b>3</b> range rename.",
                crop=(700, 195, 1600, 700),
                markers=[(0.25, 0.34, "1"), (0.70, 0.22, "2"), (0.75, 0.66, "3")],
                maximum_height=86,
            ),
            *bullets([
                "The RX then TX tabs rename individual channels.",
                "Rx patch reviews or disconnects subscriptions received by the device.",
                "Apply validates the complete edit as one grouped operation; Cancel leaves the XML unchanged.",
            ]),
        ]
        profiles_detailed = [
            para("Quick profiles and global actions", "h1"),
            para("A global action affects only the displayed target: all unlocked devices, the unlocked selection, or the current filter. Always review the target before confirming."),
            *screenshot(
                language,
                "devices",
                "<b>1</b> quick lists; <b>2</b> Network/audio, IP, Profiles, and Clock tabs; <b>3</b> target and locking controls.",
                crop=(230, 210, 690, 820),
                markers=[(0.22, 0.10, "1"), (0.45, 0.45, "2"), (0.55, 0.91, "3")],
                width=104,
                maximum_height=118,
            ),
            para("Apply a profile", "h2"),
            *bullets([
                "Open Global actions > Profiles, choose a profile, then select Apply profile.",
                "Before / after reviews the initial and resulting states for every device that was actually affected.",
                "Profiles set sample rate, bit depth, latency, and automatic IP; the last two also set Redundant or Daisy-chain.",
                "A locked device is always excluded. Undo restores the previous state as one operation.",
            ]),
            callout("A profile cannot verify physical hardware capabilities. Check the supported rates, latencies, and network modes.", PALE_RED),
        ]
        renaming_detailed = [
            para("Rename Rx and Tx channels quickly", "h1"),
            para("Direct rename works in Configuration, Device details, Patch, and Easy patch. For a Tx channel, DCE also updates every recognized subscription that uses the old name."),
            *screenshot(
                language,
                "devices",
                "<b>1</b> choose Rx or Tx and a channel; <b>2</b> enter the name; <b>3</b> choose the range; <b>4</b> set prefix and starting number.",
                crop=(1150, 210, 1595, 610),
                markers=[(0.18, 0.13, "1"), (0.24, 0.36, "2"), (0.19, 0.69, "3"), (0.73, 0.86, "4")],
                width=155,
                maximum_height=90,
            ),
            para("Editing shortcuts", "h2"),
            keyboard_table(language),
        ]
        series_detailed = [
            para("Extend a numbered series like a spreadsheet", "h1"),
            para("The fill handle appears only when the final characters form a number at the end of the label. Drag it to the last required channel; a preview displays the resulting series before release."),
            data_table(
                ["Starting name", "Handle", "Result"],
                [
                    ["Mic 4", "Visible", "Mic 5, Mic 6, Mic 7..."],
                    ["Mic 04", "Visible", "Mic 05, Mic 06, Mic 07..."],
                    ["Wireless 12", "Visible", "Wireless 13, Wireless 14..."],
                    ["Mic", "Hidden", "No trailing number found"],
                    ["Mic 4 main", "Hidden", "The name does not end with a number"],
                ],
                [48, 34, 88],
            ),
            Spacer(1, 3 * mm),
            *bullets([
                "All text before the final number is preserved exactly.",
                "The number may contain several digits and leading zeroes are retained.",
                "Fill works in Rx/Tx lists and in the Easy patch matrix.",
                "Escape cancels the preview. No label changes until the handle is dropped on a valid target.",
                "One Undo operation restores the complete series.",
            ]),
            callout("<b>Important:</b> the trailing number belongs to the label. DCE never renumbers technical Dante IDs.", PALE_GREEN),
        ]
        visual_patch = [
            para("Patch: inspect and correct a subscription precisely", "h1"),
            para("Every row represents one Rx channel and its Tx source. Patch is the most precise view for filtering, reviewing local or external sources, replacing a subscription, or removing it."),
            *screenshot(
                language,
                "patch-list",
                "Rx/Tx filters on the left, source selection at the top, and the complete Rx-to-Tx result in the table.",
                crop=(230, 215, 1595, 940),
                maximum_height=96,
            ),
            feature_band([
                ("Simple", "Shows Rx device/ID/channel, Tx device/ID/channel, and state."),
                ("Expert", "Adds raw source, resolved source, type, active, modified, and complete source."),
                ("Local", "The “.” source means the Rx device itself and is preserved."),
            ]),
            Spacer(1, 2 * mm),
            *bullets([
                "Rx receiver and Tx transmitter filters reduce the table without changing XML.",
                "Select an Rx row, choose its Tx device and channel, then select Apply.",
                "Remove disconnects only the selected Rx row.",
                "A source device missing from a partial preset may be intentional: understand the warning before replacing it.",
                "Direct Rx and Tx column rename supports Enter, Tab, Shift+Tab, and series fill.",
            ]),
        ]
        easy_patch_detailed = [
            para("Easy patch: visual and immediate patching", "h1"),
            para("Easy patch always shows Rx channels for the receiving device on the left and Tx channels for the transmitting device on the right. Menus and arrows move quickly between devices."),
            *screenshot(
                language,
                "easy-patch",
                "<b>1</b> Rx device; <b>2</b> FLIP exchanges only the displayed roles; <b>3</b> Tx device; <b>4</b> selection, range, and Patch 1:1.",
                crop=(230, 215, 1595, 900),
                markers=[(0.20, 0.30, "1"), (0.50, 0.30, "2"), (0.80, 0.30, "3"), (0.50, 0.67, "4")],
                maximum_height=105,
            ),
            callout("<b>FLIP does not reverse subscriptions.</b> It only exchanges the two selected devices so you can inspect or create subscriptions in the opposite direction.", PALE_GREEN),
        ]
        easy_patch_workflows = [
            para("Easy patch: click, drag, range, and PATCH 1:1", "h1"),
            data_table(
                ["Gesture", "Result"],
                [
                    ["Click one cell", "Immediately applies that Tx source to that Rx channel."],
                    ["Drag horizontally", "Advances from the starting Tx across columns on one Rx row."],
                    ["Drag vertically", "Feeds several consecutive Rx channels from one Tx source."],
                    ["Drag diagonally", "Creates Tx1>Rx1, Tx2>Rx2, and so on."],
                    ["Rx/Tx selection", "Equal counts: one-to-one. One Tx: distribution to several Rx channels."],
                    ["PATCH 1:1", "Click the first intersection, select the count, then apply the series."],
                ],
                [52, 118],
            ),
            Spacer(1, 3 * mm),
            *screenshot(
                language,
                "patch",
                "Compact matrix: Rx channels are rows, Tx channels are columns, and every click or drag is applied immediately.",
                crop=(230, 215, 1595, 930),
                maximum_height=92,
            ),
            *bullets([
                "Every click or drag changes the project immediately; there is no preview batch waiting to be applied.",
                "Warn me when the Rx channel is already patched is enabled by default. Clear it only when you accept replacement without confirmation.",
                "A selection containing several Tx and Rx channels must contain equal counts.",
                "Several Tx channels into one Rx or unequal multiple selections are rejected.",
                "Rx and Tx labels are directly editable. Tab, Shift+Tab, Escape, and the series handle remain available.",
                "The wheel and -, 100%, +, and Fit controls adjust matrix zoom.",
                "Undo restores the last immediate operation.",
            ]),
        ]
        merge_detailed = [
            para("Add another XML file to the open project", "h1"),
            para("This command merges devices already described by a second preset. It differs from adding from the bank: the second XML retains its compatible device structure and internal subscriptions."),
            data_table(
                ["Step", "What DCE does"],
                [
                    ["1. Open the main project", "This XML remains the session and save baseline."],
                    ["2. Add XML to project", "DCE loads and validates the second file without modifying its original."],
                    ["3. Verify format", "Preset version and namespace must match."],
                    ["4. Resolve duplicates", "Only used names are offered: skip, custom automatic suffix, or manual names."],
                    ["5. Adapt references", "Imported subscriptions follow renamed devices from the second XML."],
                    ["6. Review", "The result reports added, renamed, and skipped duplicate devices."],
                ],
                [50, 120],
            ),
            Spacer(1, 3 * mm),
            callout("Devices whose names do not conflict are always added. The automatic suffix is normalized without parentheses.", PALE_GREEN),
            *bullets([
                "Invalid XML, a different preset version, or a different namespace blocks the complete merge.",
                "A final name that is already used blocks the operation instead of producing an ambiguous duplicate.",
                "The merge can be undone. Review the Validation center and Before / after before Save as.",
            ]),
        ]
        bank_concept_page = [
            para("Understand the device bank", "h1"),
            callout("A bank contains <b>reusable offline role templates</b>. It is not a live network inventory and does not contain deployable Dante hardware identities."),
            data_table(
                ["Action", "Source", "Result"],
                [
                    ["Duplicate", "Device in the open project", "Independent new device in the same project."],
                    ["Save to device bank", "Device in the open project", "Sanitized, versioned, reusable template."],
                    ["Add from device bank", "Bank template", "Independent new instance in the open project."],
                    ["Add XML to project", "Second XML preset", "Compatible devices and references added to the open project."],
                    ["New project", "Minimal structure and bank", "3.0.0 XML to review before operation."],
                ],
                [43, 51, 76],
            ),
            Spacer(1, 3 * mm),
            para("Removed from a template", "h2"),
            *bullets([
                "instance_id, device_id, and other hardware identities from the source instance;",
                "network addresses and interfaces from the source project;",
                "subscriptions, patches, and flows tied to other devices;",
                "Preferred Master and project-specific values not explicitly selected.",
            ]),
            para("Retained when appropriate", "h2"),
            *bullets([
                "manufacturer, model, category, description, tags, and image;",
                "compatible role structure and Tx/Rx channel counts;",
                "generic Tx/Rx labels that remain editable before insertion.",
            ]),
        ]
        bank_workflow_page = [
            para("Create, share, and reuse a device bank", "h1"),
            data_table(
                ["Need", "Procedure"],
                [
                    ["Create a template", "Select the device > Save to device bank > replace project-specific labels with generic labels > enter metadata > Save."],
                    ["Add an image", "Choose PNG, JPEG, or WebP. The image is copied into the template folder, so the source file can later be moved."],
                    ["Browse banks", "Devices > Manage bank opens all 43 templates without duplicates. The selector isolates the personal or a bundled bank, while the Bank column shows its source."],
                    ["Share the bank", "Export bank creates a verified *.dce-bank.zip archive."],
                    ["Install a bank", "Import bank and choose a new or empty folder. DCE never silently replaces an existing bank."],
                    ["Add to project", "Select the template > Add to project > choose a unique name and labels > confirm."],
                ],
                [48, 122],
            ),
            Spacer(1, 3 * mm),
            *bullets([
                "Editing an inserted device never changes its bank template.",
                "Editing the template never changes devices already inserted into projects.",
                "Version, checksum, namespace, channel counts, and preset version are checked before insertion.",
                "The 43 bundled GitHub templates are sanitized and installed with the application; a personal bank is never replaced.",
            ]),
            callout("After insertion or project creation, reopen the XML, review the Validation center, and import a copy into Dante Controller. A generic role without hardware identity must be reviewed before operation.", PALE_RED),
        ]
        visual_labels = [
            para("Label Import / Export", "h1"),
            para("The Labels tab centralizes generic, DMT, Allen & Heath, and Yamaha exchange. Native files are created from bundled templates; DCE does not ask for an external template file."),
            *screenshot(
                language,
                "labels",
                "Import is on the left and export on the right. The DMT button opens the associated dLive MIDI Tools project.",
                crop=(230, 215, 1595, 930),
                maximum_height=96,
            ),
            *bullets([
                "Import: choose the file, review the detection report, associate source lists with devices, and apply only valid changes.",
                "Export: choose Rx/Tx, devices, range, format, and any required name adaptation, then provide the new output name.",
                "DMT uses XLSX/ODS or JSON/CSV; dLive and Avantis use their native CSV; CL/QL use the complete native ZIP.",
                "A second identical import does not enable Apply because labels already match.",
            ]),
        ]
        visual_synoptic = [
            para("Build and export a synoptic", "h1"),
            *screenshot(
                language,
                "synoptic",
                "<b>1</b> location; <b>2</b> order and visibility; <b>3</b> zoomable preview; <b>4</b> reset and SVG/PDF exports.",
                crop=(230, 215, 1595, 940),
                markers=[(0.15, 0.22, "1"), (0.15, 0.50, "2"), (0.65, 0.47, "3"), (0.75, 0.94, "4")],
                maximum_height=105,
            ),
            *bullets([
                "Select one or more devices, enter or choose a location, then select Assign.",
                "Within one location, the smallest order number places the device higher.",
                "Clear Show to hide a device without removing it from the project.",
                "In the visual editor, drag a device; cables follow. Reset rebuilds a clean automatic layout.",
                "Consecutive subscriptions are grouped on one range-labelled cable. A link in both directions uses one line with an arrow at each end.",
                "Zoom, Fit, and the separate preview window help with large projects.",
                "SVG and PDF are presentation exports. Locations and positions are stored next to the project and never modify Dante XML.",
            ]),
        ]
        visual_health = [
            para("Review before saving", "h1"),
            para("The Validation center separates blocking errors, warnings, and information. Read the complete row: it identifies the category, affected item, cause, and XML path."),
            *screenshot(
                language,
                "validation",
                "Anonymized example: mixed audio formats, mixed network mode, static IP, local subscription, and free Rx.",
                crop=(230, 215, 1595, 930),
                maximum_height=96,
            ),
            feature_band([
                ("Validation center", "Errors, warnings, audio formats, static IPs, local subscriptions, and XML paths."),
                ("History", "Changes, XML comparison, logs, diagnostics, and user guides."),
                ("Advanced tools", "Atomic Bomb, configurable categories, and three required confirmations."),
            ]),
            Spacer(1, 3 * mm),
            *bullets([
                "An error blocks saving; a warning requires review but may describe an intentional condition.",
                "Items to check > Show devices applies a filter to find affected devices quickly.",
                "Modified only followed by Before / after reviews the exact scope of the changes.",
                "History combines changes, XML comparison, logs, diagnostics, and user guides.",
            ]),
        ]
        visual_atomic = [
            para("Atomic Bomb: prepare an exercise, never a live network", "h1"),
            *screenshot(
                language,
                "atomic-bomb",
                "Clear categories that must be preserved. Three confirmations are required before changing the in-memory copy.",
                crop=(450, 300, 1375, 840),
                maximum_height=100,
            ),
            *bullets([
                "Categories cover names, labels, subscriptions, network modes, Preferred Master, latencies, sample rates, bit depth, and primary IP.",
                "Technical identifiers, namespaces, DNS, gateways, and secondary interfaces remain protected.",
                "The complete operation is one Undo step.",
                "The source XML is never overwritten. Use Save as to create the exercise file.",
            ]),
            callout("Atomic Bomb does not communicate with any device. The exercise file must still be reviewed in Dante Controller before distribution to trainees.", PALE_RED),
        ]

    story: list = []
    pages = [
        cover_page(language),
        page1,
        beta_page,
        screen_map,
        visual_overview,
        page2,
        visual_device,
        profiles_detailed,
        page3,
        renaming_detailed,
        series_detailed,
        visual_patch,
        easy_patch_detailed,
        easy_patch_workflows,
        page4,
        merge_detailed,
        visual_labels,
        label_page,
        bank_concept_page,
        bank_workflow_page,
        bank_page,
        visual_synoptic,
        visual_health,
        visual_atomic,
        page5,
    ]
    for index, page in enumerate(pages):
        if index:
            story.append(PageBreak())
        story.extend(page)
    build_document(ROOT / f"Notice_DanteConfigEditorV3_{language}.pdf", story)


if __name__ == "__main__":
    for language_code in ("FR", "EN"):
        quick_start(language_code)
        full_guide(language_code)
