# Dante Config Editor 2027.2.13 — Windows

## Windows 2027.2.13 — préparation de configurations, 28 octobre 2026

Le nom de la configuration reste affiché au-dessus de quatre actions :
**Ouvrir une configuration**, **Ajouter des configurations** (facultatif),
**Modifier** (facultatif), **Exporter pour Dante Controller**. Vous pouvez passer
directement de l'ouverture à l'export, sans fusionner ni enregistrer un projet DCE.
Dans Machines et Patch, la barre devient compacte et les outils actuels restent
accessibles, notamment la grille dense, Flip, le renommage et Multicast.

Les fichiers à ajouter apparaissent dans une liste distincte du fichier de départ.
Résolvez les doublons en conservant les appareils existants ou en renommant ceux
de l'import, puis ajoutez tout le lot. DCE vérifie aussi les conflits entre les
fichiers de la liste. Une erreur de format, de version ou de namespace, un fichier
modifié depuis sa sélection ou une annulation laisse le projet courant intact.
Une seule annulation retire tout le lot appliqué. Les réglages globaux du fichier
de départ restent la référence ; la fusion porte sur les appareils et leurs
structures compatibles, sans commander le réseau Dante.

**Enregistrer la préparation… (.dceproj)** est une action séparée et facultative.
Le projet DCE conserve les ajouts appliqués, leur provenance et les protections
des sources ; il ne relit pas automatiquement les XML externes à la réouverture.
La liste encore en attente appartient à la session : un avertissement précède
sa fermeture ou une sauvegarde/export qui l'exclut. L'export produit une copie
XML sous un autre nom et conserve les validations existantes.

Sources Windows WPF et Mac Avalonia synchronisées. La livraison Windows passe par
l'Atelier avec sauvegarde avant installation. La construction Mac est prévue
après renouvellement gratuit du 1er novembre ; les Mac publics 2027.2.12 restent
disponibles jusqu'à qualification de leurs remplaçants.

[Qualification Windows FR/EN](QUALIFICATION-WINDOWS-2027.2.13.md). macOS reste / remains 2027.2.12 : https://github.com/Mamat79/Dante-Config-Editor/releases/tag/v2027.2.12.
