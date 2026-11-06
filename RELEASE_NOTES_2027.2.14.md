# Dante Config Editor 2027.2.14

## Ajustement visuel du 6 novembre 2026 — matrice

Les liaisons de patch remplissent désormais toute leur case en vert. L’aperçu
avant application remplit la case en orange. Le remplissage et la coche suivent
la densité de la grille et le zoom, sur Windows et macOS ; le survol et le contour
de sélection restent visibles. Les gestes de patch et les données XML conservent
leur fonctionnement. Version livrée : 2027.2.14.

## Correctif 2027.2.14 — création et banque de machines, 2 novembre 2026

**Créer une configuration** et **Ouvrir une configuration** sont désormais deux
choix de départ au même niveau dans la barre principale. Créer ouvre un brouillon
vide, sans fichier imposé : ajoutez les machines de la banque, modifiez-les si
nécessaire puis exportez le XML pour Dante Controller. Les confirmations protègent
le projet non enregistré et les configurations encore en attente.

L'ajout de CL3, QL5, HY144-D et RU-LB4 était refusé parce que leurs modèles fournis
avaient des canaux sans `mediaType`. Ces quatre entrées utilisent maintenant des
rôles audio génériques explicites, avec les nombres de canaux du catalogue et sans
identité technique matérielle. Leur statut est **validé structurellement** ; cette
correction ne revendique aucun nouveau test sur matériel. Les contrôles XML restent
actifs et la banque personnelle n'est pas remplacée. Un test couvre l'ajout,
l'export et la relecture de chacun des 85 modèles actuellement fournis.

Les contrôles Windows, Mac et les limites sont détaillés dans les documents de qualification de cette livraison.
