from __future__ import annotations

from pathlib import Path

from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import mm
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
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
PRODUCT = "Dante Config Editor V3.6"
VERSION = "3.6"
GITHUB = "github.com/Mamat79/DanteConfigEditorV3"

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
        "<b>Outil tiers non officiel Audinate.</b> Cette V3.6 est une version de développement et peut encore contenir des bugs. Travaillez sur une copie et validez toujours le XML final par un import dans l'outil Dante officiel adapté avant toute utilisation réelle."
        if french
        else "<b>Third-party tool, not an official Audinate product.</b> V3.6 is a development version and may still contain bugs. Work on a copy and always validate the final XML by importing it into the appropriate official Dante tool before real use."
    )
    steps = (
        [
            ("Ouvrir XML", "Choisissez un export Dante. L'application travaille hors ligne et n'accède pas au réseau."),
            ("Contrôler les alertes", "Affichez les machines concernées et vérifiez chaque point signalé."),
            ("Modifier", "Utilisez Détail machine, Easy patch sous Windows, l'atelier visuel sur Mac ou les actions globales. Verrouillez les machines à exclure."),
            ("Vérifier", "Utilisez Modifiées uniquement puis Avant / après. Les changements techniques inconnus sont bloqués."),
            ("Enregistrer sous", "Choisissez un nouveau nom. La destination est remplacée atomiquement et sauvegardée si elle existait."),
            ("Tester l'import", "Importez le résultat dans Dante Controller sur une copie de travail avant toute intervention terrain."),
        ]
        if french
        else [
            ("Open XML", "Choose a Dante export. The application works offline and does not access the network."),
            ("Review alerts", "Show affected devices and verify every reported item."),
            ("Edit", "Use Device details, Easy patch on Windows, the visual patch workshop on Mac, or global actions. Lock devices that must be excluded."),
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
        "<b>Exercice :</b> Atomic Bomb dispose de son propre onglet après Sécurité et journal. Décochez les catégories à épargner, confirmez trois fois, puis utilisez Enregistrer sous pour créer le fichier destiné aux stagiaires. Les identifiants techniques restent protégés."
        if french
        else "<b>Exercise:</b> Atomic Bomb has its own tab after Safety and log. Clear categories you want to spare, confirm three times, then use Save as to create the trainee file. Technical identifiers remain protected."
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
            callout("<b>Important :</b> cette application est un outil tiers non officiel Audinate. La V3.6 est une version de développement et peut encore contenir des bugs. Elle édite des XML hors ligne, sans connexion au réseau Dante ni API Audinate. Conservez l'original et validez le fichier généré dans Dante Controller avant toute utilisation en production."),
            para("L'installateur Windows x64 contient l'application et le runtime .NET 8 nécessaire. Il n'est normalement pas nécessaire d'installer .NET séparément."),
            *bullets([
                "L'installation proposée par défaut se trouve dans Program Files et crée des raccourcis dans le menu Démarrer et sur le Bureau.",
                "Une installation V3.6 neuve utilise son propre dossier et ses propres raccourcis afin de pouvoir cohabiter avec la V3.4.2 stable.",
                "La V3.6 remplace la ligne de développement V3.5 lorsqu'elle est déjà installée et conserve les données locales de travail.",
                "L'assistant propose le dossier de banque actif et le dossier des banques fournies. La banque générique 8x8/32x32 est facultative et aucun dossier existant n'est remplacé.",
                "Deux DMG V3.6 autonomes sont prévus pour Apple Silicon et Intel. Le bundle V3.6 distinct peut cohabiter avec la V3.4.2.",
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
        page2 = [
            para("4. Page Configuration", "h1"),
            para("La page Configuration rassemble la machine sélectionnée, ses canaux, les actions globales et le tableau général."),
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
            ]),
            para("Recherche, filtres et actions globales", "h2"),
            *bullets([
                "La recherche trouve les machines, canaux et références de patch après au moins deux caractères.",
                "Les listes rapides filtrent modes réseau, latences, sample rates, bits, IP fixes et preferred masters.",
                "Modifiées uniquement affiche les machines touchées ; Avant / après détaille chaque différence.",
                "Choisissez toutes les machines non verrouillées, la sélection ou le filtre affiché. Une prévisualisation précède l'application.",
            ]),
        ]
        page3 = [
            para("5. Alertes navigables", "h1"),
            para("Le bandeau Points à vérifier signale les mélanges redondant/daisychain, IP fixes, sample rates multiples et encodages multiples."),
            *bullets([
                "Cliquez sur Voir les machines, choisissez l'alerte puis examinez les devices filtrés.",
                "Après correction, vérifiez que l'alerte disparaît et consultez Santé du fichier.",
            ]),
            para("6. Profils rapides", "h1"),
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
            para("7. Récupération automatique", "h1"),
            para("Après une modification, l'application attend brièvement puis écrit la récupération en arrière-plan, sans bloquer l'interface ni remplacer le XML source."),
            *bullets([
                "À la prochaine ouverture du même XML, choisissez de restaurer ou d'abandonner la session.",
                "Après Enregistrer sous, le nouveau fichier devient la référence des modifications et récupérations suivantes.",
                "La copie disparaît après sauvegarde ou retour à l'original ; celles de plus de 30 jours sont nettoyées.",
            ]),
        ]
        page4 = [
            para("8. Canaux et patchs", "h1"),
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
            para("9. Ajouter un XML au projet", "h1"),
            *bullets([
                "Les machines dont le nom est unique sont toujours importées.",
                "Seuls les doublons sont proposés au renommage automatique ou manuel.",
                "Les patchs importés suivent les nouveaux noms des machines renommées.",
            ]),
            para("10. IP et formats audio", "h1"),
            *bullets([
                "L'IP automatique ou fixe est réglable machine par machine ou globalement.",
                "Seule l'interface IPv4 principale, network=0 si elle existe, est ciblée. Une interface secondaire n'est pas modifiée.",
                "Le DNS n'est pas réécrit implicitement. La passerelle ne change que lorsqu'une valeur est fournie par l'action.",
                "Sample rate et bits sont modifiables par machine, globalement ou via un profil.",
            ]),
            callout("Un mauvais réglage peut rendre une machine injoignable ou incompatible. Contrôlez les capacités réelles du matériel.", PALE_RED),
            para("11. Santé, comparaison et Import / Export", "h1"),
            *bullets([
                "Santé du fichier regroupe statistiques, erreurs, warnings, patchs libres/locaux et compatibilité.",
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
            para("12. Atomic Bomb : créer un exercice", "h1"),
            *bullets([
                "Ouvrez l'onglet Atomic Bomb placé après Sécurité et journal. Décochez les catégories à épargner ; toutes sont sélectionnées par défaut. Trois confirmations détaillent ensuite les conséquences avant toute modification.",
                "La copie en mémoire reçoit des noms uniques mythologiques, audio ou humoristiques, ainsi qu'un mélange de patchs, modes réseau, Preferred Master, latences, sample rates, encodages et IP principales.",
                "Les identifiants techniques, namespaces, DNS, passerelles et interfaces secondaires restent protégés.",
                "Le résumé indique la graine du scénario. L'ensemble s'annule en une seule action et le fichier source n'est jamais écrasé.",
                "Utilisez Enregistrer sous pour remettre le preset aux stagiaires, puis vérifiez son import dans l'outil Dante officiel approprié.",
            ]),
            callout("Ce mode sert uniquement à la formation hors ligne. Il ne dérègle aucun appareil et ne communique pas avec le réseau Dante.", PALE_RED),
            para("13. Sauvegarde et validation finale", "h1"),
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
            para("14. Tests de non-régression", "h1"),
            para("La suite V3.6 exécute 272 tests Core/Windows et 20 tests Mac sans écran. Ils couvrent notamment les garde-fous XML, la sauvegarde et la récupération, les interfaces IPv4, les subscriptions, les gros presets, la duplication, la banque de machines, la création expérimentale de projet, les formats DMT, les rapports d'import, le synoptique, Atomic Bomb, Easy patch et la cohérence des traductions."),
            para("15. Limites connues", "h1"),
            *bullets([
                "Aucun pilotage en temps réel et aucune communication avec les appareils.",
                "Aucun SDK/API Audinate et aucun contournement de protocole propriétaire.",
                "La compatibilité dépend de la structure du XML ; seul l'import officiel confirme le fichier final.",
                "L'historique d'annulation conserve au maximum 10 états pour limiter la mémoire.",
                "La matrice affiche uniquement les deux machines choisies pour préserver les performances sur les gros presets.",
                "Les DMG Mac sont signés ad hoc mais non notariés ; le premier lancement peut nécessiter un clic droit puis Ouvrir.",
                "L'onglet Windows Easy patch n'est pas reproduit à l'identique sur Mac, qui conserve l'atelier visuel Avalonia.",
                "Des noms TX dupliqués sont ambigus dans les subscriptions Dante et doivent être renommés avant Easy patch.",
                "Les classeurs natifs correspondent aux modèles DMT 2.13.0 observés et aux exemples dLive, Avantis, CL5 et QL5 fournis ; JSON/CSV DMT 2.14.0-RC1 est testé séparément.",
                "Les rôles génériques dupliqués ou ajoutés depuis la banque n'emportent aucun identifiant matériel instance_id/device_id ; seule une importation réelle dans Dante Controller peut confirmer leur utilisation avec une version donnée.",
                "Nouveau projet produit une structure minimale expérimentale au format 3.0.0. Elle doit impérativement être importée et contrôlée dans Dante Controller avant tout usage.",
            ]),
            para("16. Aide et informations", "h1"),
            para(
                f"Quick start et Notice complète ouvrent automatiquement le PDF français ou anglais selon la langue active. "
                f"Projet public : {GITHUB} - Crédit : By Mamat et ses agents.",
                "small",
            ),
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
                "Exporter la banque crée une archive vérifiée *.dce-bank.zip. Importer une banque exige un dossier neuf ou vide et ne remplace jamais l'existant.",
                "Banques GitHub ouvre le catalogue public V3.6. La banque DCE Generic Roles 3.6 fournit deux rôles d'essai 8x8 et 32x32 sans identité matérielle, réseau ni abonnement.",
                "L'administration permet recherche, filtres, modification, duplication, suppression confirmée et import/export d'un modèle ZIP.",
            ]),
            para("Ajouter un modèle au projet", "h2"),
            *bullets([
                "Choisissez Ajouter depuis la banque puis configurez le nouveau nom, les labels et les options explicitement souhaitées.",
                "L'instance ajoutée est indépendante du modèle. Modifier l'une ne modifie jamais l'autre.",
                "DCE vérifie la version du modèle, son empreinte, le nombre de canaux, le namespace et la version de preset avant une insertion transactionnelle.",
            ]),
            para("Nouveau projet expérimental", "h2"),
            *bullets([
                "Nouveau projet crée une structure minimale 3.0.0 vide ou contenant un premier rôle issu de la banque.",
                "Le fichier existant n'est jamais écrasé silencieusement. L'écriture passe par un temporaire puis par validation et remplacement atomique.",
                "Réouvrez le XML dans DCE, consultez Santé du fichier, puis effectuez obligatoirement un import manuel dans Dante Controller.",
            ]),
            callout("Les journaux techniques sont accessibles depuis Sécurité et journal. Ils expliquent les échecs d'import, de validation, de banque et d'export sans modifier le projet.", PALE_GREEN),
        ]
        screen_map = [
            para("Repère des écrans", "h1"),
            para("La barre supérieure ouvre, fusionne, sauvegarde, annule et restaure le projet. La colonne Projet reste visible pour les compteurs, alertes et recherches."),
            data_table(
                ["Écran", "Utilité principale"],
                [
                    ["Configuration", "Vue d'ensemble, machine sélectionnée, canaux, listes rapides, actions globales et tableau des machines."],
                    ["Patch", "Lecture et modification tabulaire des subscriptions RX vers TX, avec filtres et renommage direct."],
                    ["Easy patch", "Grille visuelle, sélection/plage, application immédiate et alerte de remplacement optionnelle."],
                    ["Import / Export > Labels", "Échange JSON/CSV, DMT XLSX/ODS, A&H et Yamaha, avec rapport d'import."],
                    ["Import / Export > Rapports", "Rapports TXT/PDF, patchbooks TXT/CSV et topologie textuelle simple."],
                    ["Import / Export > Synoptique", "Emplacements, ordre, visibilité, zoom, reset et exports SVG/PDF."],
                    ["Santé du fichier", "Erreurs, avertissements, informations de patch et filtres de contrôle."],
                    ["Sécurité et journal", "Validation, rapport final, compatibilité, historique, comparaison XML et notices."],
                    ["Atomic Bomb", "Création hors ligne d'un exercice de dépannage configurable et annulable."],
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
        ]
        visual_device = [
            para("Modifier une machine sans changer de page", "h1"),
            para("Détail machine regroupe les paramètres essentiels et permet de passer directement à une autre machine depuis le menu supérieur."),
            feature_band([
                ("Identité", "Nom de machine, mode réseau et Preferred Master."),
                ("Audio", "Latence, sample rate et bits par échantillon."),
                ("Réseau", "IP principale en automatique ou fixe, sans toucher aux interfaces secondaires."),
            ]),
            *bullets([
                "Les onglets RX puis TX permettent de renommer les canaux individuellement.",
                "Patch RX permet de contrôler ou déconnecter les subscriptions reçues par la machine.",
                "Appliquer valide l'ensemble en une seule opération groupée ; Annuler ne modifie pas le XML.",
            ]),
        ]
        visual_patch = [
            para("Deux façons de travailler sur le patch", "h1"),
            para("Patch reste l'éditeur tabulaire précis. Easy patch ajoute une sélection visuelle, les plages et une application immédiate."),
            feature_band([
                ("Patch", "Filtrer les TX/RX, rechercher une source, appliquer ou retirer une subscription."),
                ("Easy patch", "RX à gauche, TX à droite, sélection multiple, plages et application immédiate."),
                ("Contrôle", "Alerte activée par défaut avant le remplacement d'un RX déjà patché."),
            ]),
        ]
        visual_health = [
            para("Contrôler avant d'enregistrer", "h1"),
            para("Les deux dernières pages servent à comprendre les anomalies, produire les rapports et vérifier que seules les modifications attendues seront conservées."),
            feature_band([
                ("Santé du fichier", "Erreurs, warnings, formats audio mélangés, IP fixes et patchs locaux."),
                ("Sécurité et journal", "Résumé avant sauvegarde, compatibilité XML, rapports, historique et notices."),
                ("Atomic Bomb", "Onglet séparé, catégories configurables et trois confirmations obligatoires."),
            ]),
        ]
    else:
        page1 = [
            para("1. Installation and startup", "h1"),
            callout("<b>Important:</b> this is a third-party tool, not an official Audinate product. V3.6 is a development version and may still contain bugs. It edits XML files offline without connecting to a Dante network or using an Audinate API. Keep the original and validate the generated file in Dante Controller before production use."),
            para("The Windows x64 installer includes the application and the required .NET 8 runtime. A separate .NET installation is normally not required."),
            *bullets([
                "The default location is Program Files, with Start menu and desktop shortcuts.",
                "A fresh V3.6 installation uses its own folder and shortcuts so it can coexist with stable V3.4.2.",
                "V3.6 upgrades the V3.5 development line when it is already installed and preserves local working data.",
                "The wizard offers separate active-bank and included-bank folders. The generic 8x8/32x32 bank is optional and no existing folder is replaced.",
                "Two self-contained V3.6 DMGs are planned for Apple Silicon and Intel. The separate V3.6 bundle can coexist with V3.4.2.",
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
        page2 = [
            para("4. Configuration page", "h1"),
            para("The Configuration page combines the selected device, its channels, global actions, and the device table."),
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
            ]),
            para("Search, filters, and global actions", "h2"),
            *bullets([
                "Search finds devices, channels, and subscription references after at least two characters.",
                "Quick lists filter network modes, latencies, sample rates, bits, static IPs, and Preferred Masters.",
                "Modified only shows changed devices; Before / after lists every difference.",
                "Choose all unlocked, selected unlocked, or visible unlocked devices. A preview is shown before application.",
            ]),
        ]
        page3 = [
            para("5. Navigable alerts", "h1"),
            para("The Items to check banner reports mixed redundant/daisychain modes, static IPs, multiple sample rates, and multiple bit depths."),
            *bullets([
                "Click Show devices, choose an alert, then review the filtered devices.",
                "After correcting an item, verify that the alert disappears and review File health.",
            ]),
            para("6. Quick profiles", "h1"),
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
            para("7. Automatic recovery", "h1"),
            para("After a change, the application waits briefly and writes recovery data in the background without blocking the interface or replacing the source XML."),
            *bullets([
                "When reopening the same XML, choose whether to restore or discard the previous session.",
                "After Save as, the new file becomes the reference for later edits and recovery data.",
                "The copy is deleted after saving or reverting; copies older than 30 days are cleaned automatically.",
            ]),
        ]
        page4 = [
            para("8. Channels and subscriptions", "h1"),
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
            para("9. Add XML to project", "h1"),
            *bullets([
                "Devices with unique names are always imported.",
                "Only conflicting names are offered for automatic or manual rename.",
                "Imported subscriptions follow renamed imported devices.",
            ]),
            para("10. IP and audio formats", "h1"),
            *bullets([
                "Automatic or static IP is editable per device or through a global action.",
                "Only the primary IPv4 interface, network=0 when available, is targeted. A secondary interface is not changed.",
                "DNS is not rewritten implicitly. Gateway changes only when the action provides a value.",
                "Sample rate and bits per sample are editable per device, globally, or through a profile.",
            ]),
            callout("Incorrect settings can make a device unreachable or incompatible. Verify actual hardware capabilities.", PALE_RED),
            para("11. File health, comparison, and Import / Export", "h1"),
            *bullets([
                "File health combines statistics, errors, warnings, free/local subscriptions, and compatibility checks.",
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
            para("12. Atomic Bomb: create an exercise", "h1"),
            *bullets([
                "Open the Atomic Bomb tab after Safety and log. Clear the categories you want to spare; all are selected by default. Three confirmations then describe the consequences before any change.",
                "The in-memory copy receives unique mythological, audio-themed, or playful names plus a mixture of subscriptions, network modes, Preferred Master states, latencies, sample rates, encodings, and primary IP settings.",
                "Technical identifiers, namespaces, DNS, gateways, and secondary interfaces remain protected.",
                "The summary displays the scenario seed. The entire operation is one undo step and the source file is never overwritten.",
                "Use Save as to provide the trainee preset, then verify its import in the appropriate official Dante tool.",
            ]),
            callout("This mode is only for offline training. It does not alter any device or communicate with the Dante network.", PALE_RED),
            para("13. Save and final validation", "h1"),
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
            para("14. Regression tests", "h1"),
            para("The V3.6 suite runs 272 Core/Windows tests and 20 headless Mac tests. Coverage includes XML guards, save and recovery, IPv4 interfaces, subscriptions, large presets, duplication, the machine bank, experimental project creation, DMT formats, import reports, synoptic export, Atomic Bomb, Easy patch, and translation consistency."),
            para("15. Known limitations", "h1"),
            *bullets([
                "No real-time Dante control and no communication with devices.",
                "No Audinate SDK/API and no proprietary protocol bypass.",
                "Compatibility depends on the XML structure; only an official import confirms the final file.",
                "Undo keeps at most 10 states to limit memory use.",
                "The matrix displays only the two selected devices to preserve performance on large presets.",
                "Mac DMGs are ad hoc signed but not notarized; first launch may require right-clicking the application and choosing Open.",
                "The Windows Easy patch tab is not reproduced identically on Mac, which keeps the Avalonia visual patch workshop.",
                "Duplicate Tx names are ambiguous in Dante subscriptions and must be renamed before using Easy patch.",
                "Native workbooks match observed DMT 2.13.0 templates and the supplied dLive, Avantis, CL5, and QL5 examples; DMT 2.14.0-RC1 JSON/CSV is tested separately.",
                "Generic roles duplicated or added from the bank carry no hardware instance_id/device_id. Only an actual Dante Controller import can confirm their use with a given version.",
                "New project writes an experimental minimal 3.0.0 structure. It must be imported and reviewed in Dante Controller before use.",
            ]),
            para("16. Help and information", "h1"),
            para(
                f"Quick start and Full guide automatically open the French or English PDF for the active language. "
                f"Public project: {GITHUB} - Credit: By Mamat et ses agents.",
                "small",
            ),
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
                "Export bank creates a verified *.dce-bank.zip archive. Import bank requires a new or empty folder and never replaces existing data.",
                "GitHub banks opens the public V3.6 catalog. The DCE Generic Roles 3.6 bank provides generic 8x8 and 32x32 test roles with no hardware identity, network data, or subscription.",
                "Administration supports search, filters, edit, duplicate, confirmed delete, and model ZIP import/export.",
            ]),
            para("Add a template to the project", "h2"),
            *bullets([
                "Choose Add from bank, then configure the new name, labels, and only the options you explicitly need.",
                "The inserted instance is independent from the template. Editing either one never changes the other.",
                "DCE checks model version, checksum, channel counts, namespace, and preset version before a transactional insertion.",
            ]),
            para("Experimental new project", "h2"),
            *bullets([
                "New project writes a minimal 3.0.0 structure, either empty or with one initial role from the bank.",
                "An existing file is never overwritten silently. Writing uses a temporary file followed by validation and atomic replacement.",
                "Reopen the XML in DCE, review File health, then perform a mandatory manual import into Dante Controller.",
            ]),
            callout("Technical logs are available from Safety and log. They explain import, validation, bank, and export failures without changing the project.", PALE_GREEN),
        ]
        screen_map = [
            para("Screen map", "h1"),
            para("The top bar opens, merges, saves, undoes, and restores the project. The Project column stays visible for counters, alerts, and search."),
            data_table(
                ["Screen", "Main purpose"],
                [
                    ["Configuration", "Overview, selected device, channels, quick lists, global actions, and device table."],
                    ["Patch", "Tabular review and editing of Rx-to-Tx subscriptions, with filters and direct renaming."],
                    ["Easy patch", "Visual matrix, selection/range tools, immediate apply, and optional replacement warning."],
                    ["Import / Export > Labels", "JSON/CSV, DMT XLSX/ODS, A&H, and Yamaha exchange with an import report."],
                    ["Import / Export > Reports", "TXT/PDF reports, TXT/CSV patchbooks, and a simple text topology."],
                    ["Import / Export > Synoptic", "Locations, order, visibility, zoom, reset, and SVG/PDF exports."],
                    ["File health", "Errors, warnings, subscription information, and review filters."],
                    ["Safety and log", "Validation, final report, compatibility, history, XML comparison, and user guides."],
                    ["Atomic Bomb", "Configurable, undoable, offline troubleshooting exercise generation."],
                ],
                [54, 116],
            ),
        ]
        visual_overview = [
            para("The essentials on one screen", "h1"),
            para("The Configuration page addresses the software's original need: review an entire preset quickly without opening each Dante Controller page in turn."),
            feature_band([
                ("Spot issues", "Colored rows and the side banner highlight important discrepancies."),
                ("Target safely", "Filters, multiple selection, and locks define exactly which devices are affected."),
                ("Review", "Before / after lets you inspect every change before saving."),
            ]),
        ]
        visual_device = [
            para("Edit a device without leaving the workflow", "h1"),
            para("Device details combines the essential settings and lets you move directly to another device from the top menu."),
            feature_band([
                ("Identity", "Device name, network mode, and Preferred Master."),
                ("Audio", "Latency, sample rate, and bits per sample."),
                ("Network", "Automatic or static primary IP without changing secondary interfaces."),
            ]),
            *bullets([
                "The RX then TX tabs rename individual channels.",
                "Rx patch reviews or disconnects subscriptions received by the device.",
                "Apply validates the complete edit as one grouped operation; Cancel leaves the XML unchanged.",
            ]),
        ]
        visual_patch = [
            para("Two patching workflows", "h1"),
            para("Patch remains the precise tabular editor. Easy patch adds visual selection, ranges, and immediate apply."),
            feature_band([
                ("Patch", "Filter TX/RX devices, find a source, and apply or remove a subscription."),
                ("Easy patch", "RX on the left, TX on the right, multiple selection, ranges, and immediate apply."),
                ("Control", "Warning enabled by default before replacing an already-patched Rx channel."),
            ]),
        ]
        visual_health = [
            para("Review before saving", "h1"),
            para("The final two pages explain anomalies, produce reports, and help verify that only the intended changes will be kept."),
            feature_band([
                ("File health", "Errors, warnings, mixed audio formats, static IPs, and local subscriptions."),
                ("Safety and log", "Pre-save summary, XML compatibility, reports, history, and user guides."),
                ("Atomic Bomb", "Separate tab, configurable categories, and three required confirmations."),
            ]),
        ]

    story: list = []
    pages = [
        cover_page(language),
        page1,
        screen_map,
        visual_overview,
        page2,
        visual_device,
        page3,
        visual_patch,
        page4,
        label_page,
        bank_page,
        visual_health,
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
