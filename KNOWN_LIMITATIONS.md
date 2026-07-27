# Limites connues - V3.6 en développement

## Compatibilité Dante

- L'application édite uniquement des fichiers XML hors ligne. Elle ne découvre, ne surveille et ne pilote aucun appareil Dante en temps réel.
- Aucun SDK ou service Audinate n'est utilisé. Dante Config Editor ne remplace pas Dante Controller ni Dante Domain Manager.
- La compatibilité dépend de la structure exacte du preset. Seul un import réel dans une version identifiée de Dante Controller peut valider le résultat final.
- Aucun fabricant ni modèle matériel n'est déclaré compatible sans preuve enregistrée dans `COMPATIBILITY_MATRIX.md`.
- Un preset partiel peut référencer un device TX absent. Cette situation reste un avertissement et non une erreur systématiquement bloquante.
- Les chemins XML inconnus sont bloqués par défaut à la sauvegarde. Un nouveau format Dante peut donc nécessiter une mise à jour de l'application avant d'être éditable.
- L'application ne vérifie pas les capacités matérielles réelles : nombre de flows, sample rates ou encodages supportés par un appareil.

## Subscriptions et patch visuel

- Seuls les aliases de subscription documentés et testés sont modifiés. Une structure différente reste préservée ou bloque la sauvegarde selon son emplacement.
- `subscribed_device="."` est traité comme une source locale au device RX.
- Des noms TX dupliqués sur un même device rendent une subscription textuelle ambiguë. Le patch visuel refuse cette affectation ; renommer d'abord les TX concernés.
- La matrice affiche un couple de devices à la fois pour rester exploitable sur les gros presets.
- Une sélection multiple doit contenir autant de TX que de RX, sauf lorsqu'un seul TX alimente plusieurs RX. Plusieurs TX vers un RX sont refusés.
- Une plage est entièrement refusée si le nombre demandé dépasse les TX ou RX disponibles ; il n'existe pas d'application partielle silencieuse.
- Dans la grille Windows, un glissement horizontal représente une série TX/RX à partir du RX choisi, car un RX Dante ne peut pas recevoir plusieurs TX. Une diagonale doit avancer du même nombre de cases TX et RX ; les gestes ambigus sont refusés sans changement partiel.
- Un RX déjà patché demande un choix explicite : annuler, ignorer le conflit ou remplacer la subscription.
- Les changements du patch visuel restent en attente jusqu'à `Appliquer au projet`. Fermer sans appliquer les abandonne.

## Fichiers et récupération

- Le support XLSX/ODS DMT cible la feuille `Channels` des modèles observés avec DMT 2.13.0. Les exports JSON/CSV de DMT 2.14.0-RC1 sont testés séparément sur la branche source `feature/add-json-export` au commit `3c34052`.
- Les noms DMT sont limités à huit caractères ASCII. Dante Config Editor n'applique cette conversion que si l'utilisateur l'active explicitement ; JSON et CSV conservent les labels Unicode complets.
- L'échange DMT se fait par fichiers et plages de canaux. Il n'existe pas de connexion directe ou de synchronisation en temps réel entre les applications.
- Le CSV Allen & Heath cible les sections `[Channels]` et les lignes `Input` observées dans les exemples dLive et Avantis fournis. Une évolution du format console peut demander une adaptation.
- Le support Yamaha cible `InName.csv` dans les packages CL/QL observés. Les autres fichiers du ZIP sont conservés, mais une structure future différente peut être refusée.
- Les imports et exports de labels ne configurent pas directement une console : ils créent des fichiers à importer ensuite avec les outils du fabricant.
- Le fichier ouvert doit être sauvegardé sous un nouveau nom avant toute écriture. Le nouveau chemin devient ensuite la référence de session.
- Une sauvegarde atomique protège la destination contre les échecs testés, mais ne remplace pas une sauvegarde externe du projet.
- La récupération automatique est temporisée. Une coupure immédiate après une modification peut survenir avant l'écriture de la copie de récupération.
- La pile d'annulation conserve au maximum 10 états XML pour limiter l'usage mémoire.
- Les gros presets restent traités en mémoire ; les mesures synthétiques actuelles sont publiées dans `TESTING.md`.

## Duplication, banque et nouveau projet

- DCE duplique des rôles génériques de preset, pas des appareils physiques. `instance_id` et `device_id` sont retirés au lieu d'être copiés ou inventés.
- Le mainteneur a confirmé un import réel réussi de XML V3.6 modifiés par DCE dans Dante Controller. Cette validation couvre le parcours effectivement essayé, pas toutes les structures XML, versions de Dante Controller ou extensions constructeur possibles.
- Réseau, subscriptions, flows, Preferred Master et réglages sensibles sont exclus d'une duplication ou insertion par défaut. Leur conservation doit être choisie explicitement.
- Un modèle de banque doit provenir de la même version de preset et d'un namespace compatible avec le projet cible. Les conversions entre formats Dante ne sont pas inventées.
- Les images de modèles sont limitées à PNG, JPEG ou WebP et à 10 MiB. Elles servent uniquement à l'interface et ne sont jamais écrites dans le XML Dante.
- Une sauvegarde complète de banque ne peut être restaurée que vers un dossier vide ou nouveau afin d'éviter un écrasement partiel.
- Le projet minimal 3.0.0 créé par DCE est expérimental. Il doit être rouvert dans DCE, validé puis importé manuellement dans Dante Controller.
- DCE ne peut pas connaître les capacités réelles d'un modèle à partir de sa seule banque. Le nombre de flows, les fréquences et les options matérielles restent à vérifier.

## Réseau

- Les changements IPv4 ciblent l'interface principale reconnue. Une structure réseau non reconnue peut être refusée.
- DNS, passerelle et interface secondaire ne sont pas modifiés implicitement.
- La remise en IP automatique ne configure pas physiquement un appareil ; elle modifie uniquement le preset XML.

## Plateformes et distribution

- L'installateur Windows est autonome pour Windows x64 et inclut .NET 8. Les architectures Windows ARM64 et x86 ne sont pas distribuées.
- L'installateur Windows n'est pas signé avec un certificat Authenticode public. Vérifier le SHA-256 publié avant distribution ; Windows peut afficher un avertissement de réputation.
- La V3.6 de développement est produite pour Windows x64, macOS Apple Silicon et macOS Intel. Windows ARM64 et x86 ne sont pas distribués.
- Les DMG macOS sont signés ad hoc, sans certificat Apple Developer ID ni notarisation. Gatekeeper peut demander un clic droit puis `Ouvrir` au premier lancement.
- Le moteur XML est partagé, mais l'interface Mac n'offre pas encore le nouvel onglet Windows `Easy patch` à l'identique. Elle conserve l'atelier visuel Avalonia avec sélection multiple, glisser-déposer et matrice.
- Les tests Avalonia sans écran ne remplacent pas une validation manuelle sur plusieurs modèles de Mac, ni un contrôle VoiceOver réel.
- Le contraste élevé, les lecteurs d'écran et les échelles Windows 125 %, 150 % et 200 % nécessitent encore la validation manuelle décrite dans `ACCESSIBILITY.md`.

## Statut de la version

La V3.4.2 reste la version officielle courante de `main` pour Windows et macOS. La V3.6 est une branche de développement et ses paquets ne remplacent pas la release stable. Dante Config Editor reste un outil tiers non officiel Audinate. Toujours travailler sur une copie, lire le rapport avant/après et valider le fichier généré dans l'outil Dante officiel avant une utilisation terrain.
