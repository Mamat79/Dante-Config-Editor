# Dante Config Editor 2026.9

[English release notes](RELEASE_NOTES_2026.9_EN.md)

## Le patch StageFlow arrive dans les RX Dante

DCE peut maintenant utiliser un groupe du patch StageFlow pour nommer tout ou
partie des canaux RX d'une machine Dante.

Le nouveau parcours **Import / Export > Labels > Associer le patch StageFlow
aux canaux RX** permet de choisir :

- le groupe StageFlow ;
- le mode de nommage : Source, Micro, Source + micro ou Libellé StageFlow ;
- la machine Dante ;
- le premier canal du patch, le premier RX Dante et le nombre de canaux.

Un aperçu Avant / Après est affiché avant toute modification. Les cellules
vides sont visibles mais ignorées. DCE applique uniquement les noms réellement
renseignés après validation.

## Groupes et paires communes

DCE tient compte des paires communes héritées par un groupe, des surcharges
propres à ce groupe et des paires volontairement masquées. Les liens avec les
lignes StageFlow sont conservés lors du prochain enregistrement du projet,
sans modifier le patch des autres applications.

## DCE reste autonome

StageFlow Desktop reste gratuit et facultatif. DCE sait créer, ouvrir et
enregistrer un projet `.stageflow` tout seul, tout en continuant à ouvrir les
XML Dante et les anciens projets DCE.

## Validation

- 498 tests Core/Windows réussis ;
- 22 tests Avalonia/macOS sans écran réussis ;
- builds Windows et Avalonia/macOS sans avertissement ni erreur ;
- quatre notices PDF françaises et anglaises régénérées et contrôlées ;
- installation et mise à niveau Windows vérifiées sans perte de licence ni
  doublon d'application.

DCE reste un éditeur hors ligne tiers, sans affiliation avec Audinate. Le XML
final doit toujours être ouvert et contrôlé dans Dante Controller avant une
exploitation réelle.
