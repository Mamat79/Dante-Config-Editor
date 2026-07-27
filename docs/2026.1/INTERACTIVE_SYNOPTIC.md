# Synoptique interactif 2026.1

## Source des données

Le synoptique Windows est construit depuis le même `DanteProject` que les
pages Machines, Patch et Centre de validation. Il ne possède pas de copie
métier du patch.

Un câble regroupe visuellement plusieurs subscriptions consécutives, mais
conserve désormais la liste exacte des subscriptions qui le composent :

- machine et canal TX ;
- machine et canal RX ;
- Dante Id TX et RX ;
- état normal, local, absent ou en conflit ;
- message de diagnostic.

L'identifiant d'une liaison repose sur les identités stables des machines et
non sur leurs noms affichés.

## Interactions

- clic sur une machine : sélection de la machine et synchronisation avec la
  sélection globale ;
- déplacement d'une machine : enregistrement de sa position dans la
  disposition DCE ;
- clic sur un câble ou sa légende : sélection et surbrillance de la liaison ;
- sélection d'une subscription dans Patch : surbrillance du câble
  correspondant ;
- bouton `Ouvrir dans Patch` : ouverture de la première subscription de la
  liaison dans la liste RX vers TX ;
- bouton `Centrer` : recentrage sur la machine ou la liaison sélectionnée ;
- `Ctrl` + molette : zoom ;
- bouton central de la souris, ou `Espace` + glissement gauche : déplacement
  panoramique ;
- filtre Emplacement : filtre temporaire de l'aperçu.

Le panneau inférieur affiche les extrémités, les plages TX/RX, le nombre de
subscriptions et les erreurs ou avertissements regroupés.

## Persistance et XML

Les emplacements, l'ordre, la visibilité et les coordonnées manuelles restent
hors du XML Dante. La V3.6 les conserve dans un fichier annexe
`.synoptic.json`; le format `.dceproj` 2026.1 prévoit les mêmes données dans
`workspace.json`.

Le filtre d'emplacement est uniquement visuel : il ne transforme jamais une
machine en machine masquée. Les exports SVG et PDF utilisent la visibilité
persistante, pas ce filtre temporaire.

La sélection, le zoom et la position des ascenseurs ne modifient ni le projet
ni le XML.

## Validation effectuée

Le 27 juillet 2026, avec `representative-preset.xml` :

- sélection de deux liaisons distinctes ;
- navigation d'une liaison vers la bonne ligne Patch ;
- sélection Patch vers surbrillance du câble ;
- sélection d'une machine depuis la carte et depuis la liste ;
- affichage des détails en français et en anglais ;
- thèmes sombre et clair ;
- tests unitaires sur le regroupement, les abonnements exacts et les
  identifiants stables ;
- tests complets Windows et macOS.

Le comportement interactif ajouté dans cette tranche concerne l'interface
Windows 2026.1. L'interface macOS conserve le synoptique V3.6, tout en
consommant le même modèle et les mêmes exports.
