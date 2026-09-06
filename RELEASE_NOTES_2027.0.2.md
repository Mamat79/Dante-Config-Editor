# Dante Config Editor v2027

[English release notes](RELEASE_NOTES_2027.0.2_EN.md)

## Windows 2027.0.2

Cette mise à jour clarifie les commandes de préparation, de connexion à la suite
et d'export. DCE reste autonome et prépare les configurations Dante hors ligne.

### Un bandeau commun, toujours accessible

- L'icône et le nom de DCE restent visibles avec les menus métier.
- Les commandes suivent l'ordre Connexion StageFlow, Alertes, Thème, Langue,
  Guide de la suite et Aide de DCE. Elles passent sur une seconde ligne lorsque
  la largeur disponible ne suffit pas.
- Le thème indique le choix courant, Clair ou Sombre. Le thème et la langue
  sont mémorisés. Les commandes disposent de noms accessibles et d'infobulles.
- Les champs de thème et de langue ont le même centrage vertical que les
  boutons voisins, sans marge interne héritée qui décale leur fond.
- Annuler et Rétablir restent dans le bandeau. Atomic Bomb se trouve dans
  **Outils > Formation** et conserve sa séquence de sécurité.

### Connexion StageFlow et alertes

- **Connexion StageFlow** distingue le projet local de la session LIVE. L'état
  autonome, la connexion en cours, la perte de connexion et les erreurs sont
  indiqués explicitement. Le nom complet du projet reste consultable.
- Le centre permet d'ouvrir un dossier `.stageflow` local sans lancer StageFlow.
  Une session LIVE doit être déconnectée avant cette ouverture ; les changements
  non enregistrés font l'objet d'un avertissement.
- La découverte, le code à six chiffres et la déconnexion sont conservés.
  L'adresse IPv4 et les détails techniques se trouvent dans une zone dépliable.
- **Alertes** sépare la validation XML des changements StageFlow non acquittés.
  Consulter les alertes ne les acquitte pas et ne désactive pas leur réception.
- **Guide** ouvre le guide commun SiLeMI/O ; **Aide** ouvre la notice de DCE,
  dans la langue choisie.

### Préparer, vérifier, exporter

- La page Projet donne accès aux machines, au patch, à la validation et à l'export.
- Les compteurs tiennent sur une ligne en grande largeur. Les fichiers récents
  utilisent la hauteur restante et défilent dans leur propre liste ; la page
  conserve un défilement de secours dans une petite fenêtre, sans couper les commandes.
- **Fichier > Exporter le XML Dante** et **Import / Export > Exporter le XML
  Dante** ouvrent le contrôle avant export, puis enregistrent une copie XML.
- Cette copie conserve le projet ouvert, ses modifications et son historique
  d'annulation. Le fichier source n'est pas écrasé. Une destination existante
  reste protégée par la confirmation et les sauvegardes du moteur existant.
- L'export refuse les extensions autres que `.xml` et les chemins à l'intérieur
  du dossier StageFlow courant, y compris les jonctions et liens symboliques.
- **Enregistrer** conserve son rôle de sauvegarde du projet StageFlow. Une
  exportation XML ne remplace pas cette sauvegarde.

Le moteur de modification XML n'est pas réécrit. Les banques, le patch, le
renommage en série, la fusion XML, le synoptique et les licences sont préservés.
StageFlow est gratuit et facultatif. Une connexion LIVE à la suite ne constitue
pas un contrôle du réseau ou du matériel Dante.

## Documentation et mise à niveau

L'installateur Windows inclut les notices et démarrages rapides FR/EN ainsi que
la paire exacte des guides communs SiLeMI/O 2027.2 validés par leur propriétaire.

Depuis une version 2026.10 qui ne propose pas v2027, téléchargez manuellement
l'installateur Windows depuis le dépôt public. Les versions récentes choisissent
la mise à jour correspondant à leur plateforme et vérifient taille et SHA-256.

La mise à niveau conserve les profils, banques personnelles, projets et licences.
Un seul raccourci Bureau est utilisé : **Dante Config Editor v2027**. Un ancien
nom de dossier d'installation, tel que `Dante Config Editor 2026.3`, peut être
conservé ; la version effective figure dans À propos et dans l'exécutable.

## Windows et Mac

Cette corrective vise **Windows 2027.0.2**, version binaire **2027.0.2.0**.
Les derniers paquets Mac acceptés restent **2027.0**, distincts pour Apple
Silicon et Intel. Ils ne sont ni renommés ni présentés comme cette corrective.
Les liens publics et les numéros de version restent propres à chaque plateforme.

Les tests Avalonia sans écran exécutés sous Windows ne constituent pas une
recette native Mac. Les vérifications d'installation et les contrôles visuels
sont rapportés séparément des tests automatisés. Aucun nouvel essai Dante
Controller ou matériel physique n'est revendiqué par ce lot. La signature
commerciale Windows et la notarisation Apple ne font pas partie de cette mise à jour.
