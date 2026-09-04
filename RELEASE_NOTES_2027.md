# Dante Config Editor v2027 pour macOS

[English release notes](RELEASE_NOTES_2027_EN.md)

Cette publication stable fournit uniquement les applications **macOS Apple
Silicon et Intel**. Windows reste en **2026.10** ; sa Release conserve le
marquage GitHub Latest et ses fichiers ne sont pas remplacés.

## Un centre de connexion StageFlow LIVE unique

- Accès permanent depuis le bandeau supérieur, quelle que soit la page affichée.
- Centre en pleine fenêtre sous macOS, en français et en anglais.
- Liste des sessions disponibles, nom du projet, poste hôte, IPv4 et fonctions partagées.
- Adresse IPv4 privée et port utilisables lorsque la découverte locale ne fonctionne pas.
- Code à six chiffres : les zéros initiaux, le collage avec espaces et le tiret sont gérés.
- Erreur expliquée sans fermer la fenêtre : le code peut être corrigé et réessayé.
- Déconnexion explicite ; aucun retour automatique dans une session après une coupure.
- Le dernier état de travail local reste disponible si l'hôte disparaît.

La session sélectionnée est vérifiée de nouveau juste avant l'envoi du code.
Un hôte compatible reçoit aussi les identifiants attendus de projet et de session.
Une réponse heartbeat tardive de l'ancienne connexion ne peut ni la réactiver
ni remplacer une nouvelle connexion.

## Trois parcours, sans confusion

Les notifications de modifications disposent d'un bandeau orange présent sur
tous les écrans : élément, ancien/nouveau label, origine et heure. Acquitter
et Tout acquitter restent propres à ce poste et n'effacent pas les nouvelles
alertes arrivées après le clic. Une pause décidée par StageFlow est indiquée
sans interrompre LIVE et sans demander de rejoindre à nouveau la session.

Le projet DCE autonome, le dossier StageFlow local et la session LIVE temporaire
restent distincts. StageFlow est gratuit et facultatif. DCE reste un éditeur de
configurations Dante hors ligne, pas un contrôleur du matériel Dante en direct.
Cette liaison LAN est réservée à un réseau local de confiance.

Sur Mac, les dimensions initiales respectent la zone de travail et son échelle.
Les petits écrans disposent d'un défilement de secours ; une taille et une
position déjà adaptées à un grand écran sont conservées.

Les fonctions de création depuis zéro, banque de machines, fusion XML,
renommage en série, patch, synoptique et association du patch StageFlow aux RX
sont conservées. Le modèle XML, les domaines étrangers, les banques personnelles
et les licences existantes ne sont pas modifiés par cette évolution.

## Notices et installation

Les quatre notices et démarrages rapides FR/EN sont actualisés. La notice
complète comprend les captures du nouveau centre, les étapes de connexion et
un tableau de dépannage.

Les guides communs de la suite SiLeMI/O, en français et en anglais, sont
également inclus dans les applications Mac.

Les paquets macOS sont distincts pour Apple Silicon et Intel. Aucun
installateur Windows v2027 n'est livré ici. Les notices v2027 couvrent aussi
l'interface Windows en préparation ; la notice Windows 2026.10 reste
disponible dans sa Release.

**v2027** est le nom public Mac, **v2027.0** le tag technique, et
**2027.0.0.0** la version binaire. Téléchargez les DMG avec les liens directs
du README : tant que Latest reste v2026.10 pour Windows, un ancien gestionnaire
de mises à jour peut ne pas proposer cette publication Mac.

## Limites de vérification

Les deux paquets ont été construits et démarrés sur des environnements macOS
natifs distincts, Apple Silicon et Intel. Chaque architecture a réussi
**579 tests du moteur partagé et 29 tests de l'interface Mac**, sans échec.
Le lancement du programme depuis le DMG monté a été vérifié sans fichier,
avec un XML et avec un dossier StageFlow. Les cinq fichiers d'entrée de la
recette sont restés identiques octet par octet. Les deux guides communs
embarqués ont été comparés à leurs empreintes approuvées.

Les captures natives et le bandeau d'alertes FR/EN clair/sombre ont été relus.
À 1024 × 768, des commandes nécessitent le défilement de secours ; la
présentation Mac n'est pas identique à celle de Windows. Le démarrage Intel
a émis des avertissements de compilation graphique Skia/Metal, sans arrêt du
programme ; les captures finales étaient complètes et les ouvertures XML et
StageFlow suivantes n'ont émis aucun message d'erreur. Ce contrôle ne vaut
pas une recette exhaustive sur tous les modèles de Mac.

L'interopérabilité du client DCE avec un véritable hôte StageFlow a été testée
sur le même PC. Elle ne remplace pas une recette entre deux ordinateurs ni une
recette Dante sur matériel. Les preuves de build, de tests et les limites
propres à chaque plateforme sont indiquées dans le compte rendu de publication.
Les paquets Mac ne bénéficient pas encore d'une notarisation Apple.

La console locale StageFlow qui pilote les applications du même poste reste
disponible sous Windows uniquement. Sur Mac, DCE ouvre les projets locaux et
rejoint les sessions LAN depuis son centre de connexion.
