# Changelog V3

## 2026-07-26 - Version 3.6.1

- La fenêtre `Soutenir DCE` affiche directement le QR PayPal fourni par Mamat
  Leroy sur Windows et macOS.
- Un bouton ouvre également la page PayPal.Me `MamatLeroy` sur ordinateur.
- Suppression du bouton qui ouvrait une redirection PayPal inadaptée aux
  navigateurs sur ordinateur.
- La documentation de soutien GitHub explique le scan du QR sur téléphone.
- Aucun paiement ni aucune donnée financière ne transitent par DCE.
- Les notices complètes française et anglaise passent à 26 pages avec captures
  annotées et procédures détaillées pour Configuration, profils, renommage,
  Patch, Easy Patch, fusion XML, banque de machines, labels, synoptique, santé
  du fichier et Atomic Bomb.
- La banque communautaire V3.6 intègre quatre modèles assainis supplémentaires :
  Glensound Divine, Beatrice D8 et AOIP22, ainsi qu'Allen & Heath SDante 64x64.
  La V3.6.1 est remplacée sans changement de numéro.

## 2026-07-25 - Version 3.6 en développement

- Audit complet de l'architecture, de la fidélité XML, des performances, des dépendances, des traductions et des risques documenté dans `AUDIT_V3_6.md`.
- Validation renforcée des identifiants, références de canaux, subscriptions, interfaces réseau, namespaces et ajouts de nœuds.
- Comparaison XML sémantique et tests de cycle import/export/import avec conservation des éléments inconnus.
- Duplication d'une machine sous forme de rôle générique indépendant sans recopier ni inventer `instance_id` ou `device_id`.
- Banque de machines versionnée, partageable et transactionnelle avec métadonnées, labels, image facultative, recherche, filtres, import/export et sauvegarde/restauration.
- Ajout d'une instance indépendante depuis la banque et création expérimentale d'un projet minimal 3.0.0.
- Commandes Windows et macOS pour la duplication, la banque, l'ajout de modèle, le nouveau projet et l'ouverture des journaux.
- Journaux techniques quotidiens et messages de validation plus précis.
- Corpus local de dix XML, 176 machines et 5 004 labels validé sans modifier les originaux.
- Numéro d'application, installateur, paquets macOS, workflows et notices passés en V3.6.
- Easy Patch conserve le point de départ et le nombre demandé après un clic immédiat afin que `PATCH 1:1` reste utilisable.
- L'aperçu séparé du synoptique conserve désormais ses proportions à tous les niveaux de zoom.
- Les onglets des fenêtres machine gardent un contraste lisible dans les thèmes clair et sombre, avec RX présenté avant TX.
- Les notices FR/EN décrivent l'application immédiate d'Easy Patch et l'alerte optionnelle des RX déjà patchés.
- Les banques complètes utilisent désormais explicitement le format d'échange `*.dce-bank.zip`, avec catalogue GitHub, archive générique 8x8/32x32 vérifiée et accès direct depuis Windows/macOS.
- L'installateur Windows permet de choisir le dossier de banque actif et celui des banques fournies, sans écraser une banque existante.
- Les modèles Yamaha QL1 et Rio1608-D2, Fohhn DI4.1000, Lake LM 44, RME Digiface Dante, Glensound Divine, Beatrice D8 et AOIP22, et Allen & Heath SDante 64x64 sont distribués dans une banque communautaire assainie, proposée séparément par l'installateur Windows et incluse dans les DMG macOS.
- La comparaison XML et ses résultats suivent maintenant la langue active sur Windows et macOS ; les libellés anglais TX/RX ont également été harmonisés.
- Ajout d'une page bilingue `Soutenir DCE` dans `Sécurité et journal`, sans paiement intégré.
- Ajout d'un rappel local facultatif après 20 lancements, reportable ou désactivable définitivement, sans télémétrie ni connexion au démarrage.
- Ajout de la page de soutien dans GitHub Funding, le README français/anglais et les notices complètes.
- Suite portée à 283 tests Core/Windows et 20 tests Mac sans écran.
- Aucun import V3.6 réel dans Dante Controller n'est revendiqué ; la création de projet et les rôles génériques restent à valider manuellement.

## V3.5 - ajustements de renommage et d'ergonomie

- Réorganise les actions machine en deux lignes : Reset RX, Reset TX, Reset RX/TX et Supprimer.
- Rend les libellés TX verticaux de la matrice Easy patch éditables au clic avec Entrée, Tab et Maj+Tab.
- Affiche la poignée de recopie uniquement pour les noms terminés par un nombre.
- Conserve les zéros initiaux pendant la recopie, par exemple `Mic 04` vers `Mic 05`.
- Complète les notices françaises et anglaises avec les méthodes de renommage et des exemples.

## 2026-07-23 - Version 3.5 en développement

- Matrice macOS mise à jour cellule par cellule sans reconstruction complète après chaque clic.
- Changements visuels conservés en attente jusqu'à `Appliquer au projet`, avec abandon et annulation explicites.
- En-têtes TX/RX fixes, défilement synchronisé et labels complets en infobulle.
- Patch 1:1 par plage et directement depuis la grille, échange des sélections TX/RX et zoom 50 à 200 % avec ajustement à la fenêtre.
- Bouton `FLIP TX ⇄ RX` rendu très visible dans Easy patch.
- Validation par Tab et retour par Maj+Tab pendant le renommage direct, y compris après le rafraîchissement de la grille.
- Audit bilingue et corrections de mise en page macOS, notamment dans les imports/exports de labels.
- Architecture d'import par adaptateurs séparés JSON, CSV, XLSX, ODS et ZIP console.
- Validation JSON stricte, rejet des versions CSV mélangées, canaux dupliqués et colonnes XLSX/ODS manquantes.
- Rapport d'import visible : listes, machines, canaux, lignes ignorées, labels vides, doublons et avertissements.
- Compatibilité automatisée avec la structure JSON/CSV de DMT 2.14.0-RC1 au commit `3c34052`.
- Création d'appareils génériques abandonnée à la demande de l'utilisateur. La suppression reste testée de bout en bout ; la duplication de rôles n'est pas proposée sans validation Dante Controller.
- Notices complète et rapide V3.5 régénérées en français et en anglais, avec une carte de tous les écrans et fonctions.
- Deux présentations V3.5 de 55 secondes, sans voix ni piste audio, avec texte intégré et captures synthétiques anonymisées.
- Installateurs de développement V3.5 autonomes pour Windows x64, macOS Apple Silicon et macOS Intel, avec identités séparées de la V3.4.2 stable.

## 2026-07-22 - Version 3.4 officielle - ergonomie Patch et synoptique directionnel

- Maintenance V3.4 : la grille Easy patch affiche désormais le numéro et le label complet de chaque TX dans un en-tête vertical, avec infobulle et renommage direct conservés.
- Maintenance V3.4 : pointes de flèche explicites et plus visibles dans l'aperçu du synoptique ainsi que dans les exports SVG et PDF, y compris aux deux extrémités des liaisons bidirectionnelles.
- Maintenance V3.4 : Easy patch conserve désormais l'onglet `Sélection et plage` après un renommage, une prévisualisation ou une application.
- Maintenance V3.4 : renommage RX/TX et extension des séries numériques disponibles directement dans la grille Easy patch, avec contraste corrigé dans le thème sombre.
- Maintenance V3.4 : vue Patch enrichie avec les colonnes TX Device, TX Dante ID et TX Channel, renommage direct et poignées de recopie RX/TX.
- Maintenance V3.4 : page Configuration réorganisée avec listes/actions globales à gauche et ensemble visuel commun machine/canaux.
- Renommage direct des machines et canaux dans Patch, et des canaux TX/RX dans Easy patch.
- Extension par glisser des séries de noms numériques.
- Grille Easy patch placée avant `Sélection et plage` et filtres Patch RX placés avant TX.
- Nouvelle action globale garantissant un seul Preferred Master choisi par l'utilisateur.
- Reset complet de la disposition du synoptique.
- Fusion des flux aller-retour entre deux machines en un câble unique avec une flèche à chaque extrémité ; export SVG et PDF cohérent.
- Réglages Configuration affichés par défaut au premier lancement et ajustements de taille pour l'échelle Windows 125 %.
- Agrandissement du panneau Atomic Bomb.
- Passage des applications, paquets Windows/macOS et installateur à la V3.4.

## 2026-07-22 - Version 3.3 officielle - DMT ODS et Atomic Bomb configurable

- Import direct des classeurs DMT `XLSX` et `ODS` depuis l'onglet Labels.
- Export direct vers quatre modèles intégrés : XLSX dLive, XLSX Avantis, ODS dLive et ODS Avantis.
- Préservation des feuilles, styles et réglages hors `Channels` dans les copies ODS générées ; seules les cellules `Enabled` et `Name` sont adaptées.
- Nouveau panneau `Générateur d'expérience horrible (mais pédagogique)` dans Atomic Bomb.
- Ajout de dix options indépendantes pour protéger les noms de machines, labels TX/RX, patchs, modes réseau, Preferred Master, latences, fréquences, bits et IP principales.
- Conservation du routage existant lorsque les patchs sont exclus mais que les machines ou TX sont renommés.
- Correction macOS des boutons d'échange de labels et de prévisualisation masqués en Full HD.
- Correction des traductions du bandeau, de la recherche et des filtres macOS.
- Explication visible lorsqu'un second import de labels identique ne contient aucun changement à appliquer.
- Passage de l'application, des paquets Windows/macOS, des notices et de l'installateur à la V3.3.
- Extension de la suite à 150 tests Core/Windows et 11 tests d'interface macOS sans écran.
- Conservation de la Release V3.2 ; retrait volontaire des pages de Releases V3.08 et V3.09, avec conservation de leurs tags et commits Git.

## 2026-07-21 - Version 3.2 officielle - Import / Export et synoptique visuel

- Maintenance V3.2 : ajout de l'export PDF vectoriel du synoptique, en complément du SVG.
- Maintenance V3.2 : répartition automatique des nombreuses liaisons sur des ports distincts afin d'éviter la superposition des flèches.
- Maintenance V3.2 : cases `Voir` activables en un clic et liste réutilisable des emplacements déjà saisis.
- Maintenance V3.2 : correction du dialogue d'import de labels dont le bouton de prévisualisation pouvait être masqué ; association par nom désormais disponible avec une seule liste et plage proposée limitée à la capacité de la machine cible.
- Maintenance V3.2 : intégration des modèles natifs DMT dLive/Avantis, CSV dLive/Avantis et ZIP Yamaha CL/QL ; l'export ne demande plus de choisir un fichier modèle existant.
- Maintenance V3.2 : distinction explicite entre CSV générique DCE et CSV natif console, bascule automatique TX vers RX pour les machines sans TX et blocage des machines sans canal dans le sens choisi.
- Maintenance V3.2 : correction de l'ouverture de l'export pour une machine sans TX ; le choix RX est appliqué avant l'aperçu et une plage vide ne déclenche plus d'erreur interne.
- Maintenance V3.2 : écriture atomique des exports natifs afin de conserver une destination existante si la génération échoue.
- Regroupement logique des fonctions de fichiers dans un onglet principal `Import / Export`.
- Ajout des sous-onglets `Labels`, `Rapports et patchbook` et `Synoptique` sur Windows et macOS.
- Déplacement des exports TXT/PDF, patchbooks et topologie simple hors de `Sécurité et journal`.
- Ajout d'un synoptique visuel en couleur construit depuis les subscriptions du projet.
- Affectation d'emplacements physiques, affichage/masquage et réorganisation des machines.
- Regroupement des séries TX/RX consécutives en câbles numérotés avec légende séparée.
- Export du synoptique au format SVG autonome.
- Stockage des emplacements et préférences de présentation dans un fichier local séparé, sans ajout de balise au XML Dante.
- Ajout de tests de non-régression pour le regroupement des câbles, l'identité stable après renommage, les fichiers annexes corrompus et l'absence de mutation XML.
- Ajout de l'import/export natif des labels Allen & Heath dLive/Avantis au format CSV console.
- Ajout de l'import/export natif des labels Yamaha CL/QL depuis un package ZIP ou un fichier `InName.csv`, sans modifier les autres fichiers du package.
- Conservation de l'échange XLSX avec dLive MIDI Tools (DMT), avec lecture des modèles dLive et Avantis observés en version 2.13.0.
- Amélioration du synoptique avec routes orthogonales, troncs partagés et légende sur deux colonnes pour les projets denses.
- Passage de l'application, des paquets Windows/macOS, des notices et de l'installateur à la V3.2.
- L'installateur V3.2 remplace les anciennes installations V3 détectées et conserve les données locales de travail.
- Promotion de la V3.2 sur `main` avec un tag immuable `v3.2` ; les Releases historiques `v3.08` et `v3.09` étaient encore publiées à cette date, avant leur retrait en V3.3.

## 2026-07-21 - Maintenance de main après V3.1

- Nouvel onglet générique `Labels` sur Windows et macOS pour importer ou exporter en JSON, CSV ou XLSX, avec compatibilité DMT dLive / Avantis clairement indiquée au niveau du format.

## 2026-07-21 - Version 3.1 - échange de labels et collaboration DMT

- Import et export des labels TX/RX pour une ou plusieurs machines, avec choix des plages et prévisualisation avant application.
- Ajout d'un format JSON versionné et d'un CSV lisible par les tableurs.
- Lecture de la feuille `Channels` des classeurs [dLive MIDI Tools](https://github.com/togrupe/dlive-midi-tools).
- Export vers une copie d'un modèle XLSX DMT ; le modèle original n'est jamais modifié.
- Adaptation DMT ASCII et huit caractères visible et activée uniquement sur demande explicite.
- Mise à jour groupée des labels et des subscriptions TX reconnues avec une seule étape d'annulation.
- Interface d'échange disponible sur Windows et macOS.
- Déplacement d'`Atomic Bomb` dans un onglet dédié après `Sécurité et journal` sur les deux plateformes.
- Passage de l'application, des installateurs, des données locales et des paquets macOS en V3.1.
- Publication initiale des Releases immuables `v3.08` et `v3.09` avec leurs propres artefacts ; leurs tags restent conservés après le retrait ultérieur des pages de Release en V3.3.

## 2026-07-20 - Version 3.09 - exercice de dépannage atomique

- Ajout d'`Atomic Bomb` dans la colonne Projet et dans `Sécurité et journal` sur Windows et macOS.
- Génération d'un preset de formation volontairement incohérent : noms, canaux, patchs, modes réseau, Preferred Master, latences, sample rates, encodages et IP principales sont mélangés.
- Les machines reçoivent des noms uniques tirés d'un catalogue mythologique, audio et humoristique, par exemple `ATHENA`, `RAVENNA`, `PYRAMIX`, `INFERNO` ou `PATCHOS`, au lieu d'un préfixe uniforme.
- Garantie d'un mélange de valeurs sur les presets comportant plusieurs machines, avec une graine de scénario affichée.
- Trois confirmations avant exécution, fichier original protégé, `Enregistrer sous` obligatoire et une seule étape d'annulation.
- Conservation des identifiants techniques Dante, namespaces, DNS, passerelles et interfaces secondaires.
- Extension de la suite à 100 tests Core/contrats Windows et 9 tests Avalonia sans écran.

## 2026-07-17 - Version 3.08 - crédit et présentation bilingue

- Conservation de `By Mamat` comme signature principale, avec `et ses agents` sur une seconde ligne plus discrète dans les interfaces Windows/macOS et l'installateur ; les exports et notices conservent le crédit complet sur une ligne.
- Présentation de l'origine personnelle du projet : pallier ce qui me manquait dans Dante Controller pour contrôler rapidement un preset, renommer sans perdre les références reconnues et préparer une configuration hors ligne.
- Ajout d'un README anglais complet et de notes de version anglaises, reliés aux documents français.
- Remplacement de la démonstration Easy patch par deux présentations générales V3.08, en français et en anglais, avec sous-titres intégrés et fichiers SRT séparés.
- Refonte des notices complètes française et anglaise en guides illustrés de l'ensemble du logiciel, accompagnés de démarrages rapides bilingues.
- Conservation stricte du numéro V3.08 et du fonctionnement XML existant.

## 2026-07-13 - Version 3.08 - mise à jour Easy patch

- Chaque commande `Prévisualiser` ajoute désormais immédiatement l'opération au lot temporaire, sans modifier le XML.
- Le lot prévisualisé reste visible et se cumule lors des changements de machines RX/TX, des sélections et des plages successives.
- `Appliquer tout le lot` conserve une seule mutation XML et une seule étape d'annulation pour l'ensemble des opérations.
- Réduction des cellules de la grille à `28 × 22` pixels et des colonnes TX à 32 pixels ; le nom complet reste disponible au survol.
- Ajout du glissement horizontal pour une série TX/RX, vertical pour un TX vers plusieurs RX et diagonal pour une série un-à-un.
- Blocage atomique des diagonales ambiguës et des séries dépassant les canaux disponibles.
- Extension de la suite à 95 tests Core et contrats Windows, plus 8 tests Avalonia sans écran.

## 2026-07-12 - Version 3.08 officielle Windows et macOS

- Retrait du suffixe `Beta` dans les interfaces, métadonnées, exports, notices et installateur.
- Publication d'un installateur autonome Windows x64 qui met à niveau une précédente V3.08 sans toucher à la V3.07.
- Passage de l'interface Avalonia et des métadonnées macOS à la V3.08.
- Production de deux DMG autonomes : Apple Silicon et Intel, avec runtime .NET 8 et notices FR/EN.
- Réactivation du workflow GitHub Actions macOS sur les changements de `main`, en plus du lancement manuel.
- Vérification automatisée des versions officielles Windows/macOS et des contrats de packaging.
- Conservation documentée de la différence d'interface : `Easy patch` reste propre à Windows ; macOS conserve l'atelier visuel Avalonia.
- Publication de sommes SHA-256 pour l'installateur et les deux DMG.
- Les paquets Mac restent signés ad hoc et non notariés faute de certificat Apple Developer.

## 2026-07-12 - Version 3.08 Beta Windows

- Promotion de la V3.08 Beta dans la branche `main` en remplacement de la V3.07 pour le développement Windows courant ; le tag et la Release V3.07 restent conservés.
- Conservation du workflow macOS en lancement manuel uniquement tant que la V3.08 n'est pas déclarée Release officielle.
- Création de la branche `3.08-beta` depuis la V3.07 officielle, sans workflow ni compilation macOS.
- Nouvel AppId V3.08, dossier `Program Files`, groupe Menu Démarrer et stockage local distincts pour permettre la coexistence avec la V3.07.
- Ajout d'un moteur central de patch par sélection avec identités RX stables basées sur le device et le Dante Id.
- Appariement un-à-un pour des sélections TX/RX de même taille et diffusion d'un TX vers plusieurs RX.
- Blocage de plusieurs TX vers un RX, des tailles multiples incohérentes et des plages incomplètes.
- Prévisualisation des créations, remplacements et lignes inchangées avant préparation du lot.
- Suppression de l'ascenseur général d'Easy patch : les machines RX/TX restent visibles et les listes défilent dans leur propre zone.
- Prévisualisation compacte, masquée lorsqu'elle est vide, avec colonnes RX, source actuelle, nouvelle source et action toujours lisibles.
- Ajout de deux parcours : `Appliquer` directement une sélection ou une plage, ou `Prévisualiser` puis `Ajouter au lot` / `Appliquer ces changements`.
- Résolution explicite des conflits : annuler le lot, ignorer les RX déjà patchés ou remplacer leurs subscriptions.
- Déconnexion groupée de plusieurs RX, matrice unitaire conservée et suppression de la dépendance au glisser-déposer.
- Ajout d'un patch par plage avec premier TX, premier RX et quantité exacte.
- Ajout d'un véritable onglet principal `Easy patch` à côté de l'onglet classique `Patch`.
- Réorganisation de l'outil avec RX à gauche, TX à droite et navigation précédent/suivant entre machines.
- Ajout d'un sélecteur de machine en haut de `Détail machine`, avec protection des modifications non appliquées.
- Intégration de l'atelier dans `Détail machine`, avec RX verrouillé sur la machine ouverte et application avant les renommages.
- Application finale des patchs en un seul lot annulable et persistance contrôlée après sauvegarde/rechargement.
- Extension de la suite Windows à 88 tests ; build WPF sans warning.
- Génération automatique d'une somme SHA-256 fraîche après chaque construction de l'installateur.

## 2026-07-11 - Version 3.07 officielle

- Promotion de la V3.07 en version stable officielle du projet, sans prétendre à une publication officielle Audinate.
- Retrait du suffixe `Beta` dans les interfaces Windows/macOS, les métadonnées, les notices et l'installateur.
- Correction du mode d'affichage Expert de la page Patch : les colonnes TX détaillées apparaissent désormais réellement.
- Extension de la suite à 67 tests Core et maintien des 7 tests d'interface Mac headless.
- Conservation du même AppId : l'installateur V3.07 remplace la V3.07 Beta au lieu de créer une copie parallèle.
- Les nouvelles fonctions de patch par sélection et par série sont réservées à la branche `3.08-beta`.

## 2026-07-11 - Version 3.07 Beta

- Extraction d'un projet `DanteConfigEditor.Core` partagé par les interfaces Windows et macOS.
- Ajout d'une interface macOS Avalonia reprenant les workflows de configuration, patch, santé du fichier, sécurité, exports et notices.
- Ajout de deux tests Avalonia headless pour la structure du rail latéral et l'affichage réel des alertes.
- Placement des alertes importantes dans la colonne latérale sur macOS et Windows.
- Ajout de bundles `.app` et de DMG autonomes Apple Silicon / Intel, avec runtime .NET 8 et documentation FR/EN inclus.
- Ajout d'une signature ad hoc pour vérifier l'intégrité locale ; la notarisation Apple reste une limite connue de cette bêta.
- Promotion de l'unique version Windows/macOS en V3.07 Beta, sans branche de version parallèle.
- Ajout d'un atelier de patch visuel Windows/macOS avec filtres par device, sélection TX multiple, affectation séquentielle, glisser-déposer et matrice TX vers RX.
- Les changements du patch visuel sont préparés sans toucher au projet, puis appliqués en un lot unique et une seule étape d'annulation.
- Refus des affectations visuelles ambiguës quand plusieurs TX d'un même device portent le même nom.
- Réorganisation compacte de l'en-tête et des panneaux ; le reset global des canaux est déplacé sous la latence globale.
- Extension de la suite à 63 tests Core et 7 tests d'interface Mac headless.
- Ajout du pack de validation non destructif, de la matrice de compatibilité et des documents de validation, accessibilité, architecture et limites connues.

## 2026-07-10 - Version 3.06 Beta

- Remplacement de l'identité XML basée sur le nom par une association stable utilisant en priorité `instance_id/device_id`, puis les identifiants techniques de repli.
- Blocage par défaut des chemins XML inconnus et maintien du blocage des attributs techniques `danteId`, `mediaType` et identifiants de machine.
- Comparaison des balises par contenu et identité plutôt que par position, afin de tolérer leur simple réordonnancement.
- Prise en charge des XML utilisant un namespace par défaut, sans création de balises hors namespace pendant les modifications.
- `SaveAs` rendu atomique avec fichier temporaire unique, backup de la destination remplacée et injection d'erreur testée avant validation finale.
- La destination sauvegardée devient la nouvelle référence de la session et de la récupération automatique.
- Ciblage exclusif de l'interface IPv4 principale ; conservation du DNS, de la passerelle non demandée et des interfaces secondaires.
- Ajout d'une API de mutations groupées avec un seul `ReloadModel` pour les réglages machine et les actions globales.
- Récupération automatique déplacée hors du thread UI après une temporisation de 750 ms.
- Limitation de la pile d'annulation à 10 snapshots XML.
- Ajout d'un workflow GitHub Actions `windows-latest` et vérification stricte des codes retour dans `build.ps1`.
- Passage de l'application, des exports et de l'installateur en V3.06 Beta.
- Suppression de l'option d'installation parallèle : l'installateur V3.06 remplace la version existante avec le même AppId et le même dossier par défaut.
- Extension de la suite à 38 tests, incluant les garde-fous, l'atomicité, la récupération, les alias de subscription, les namespaces, les deux interfaces IPv4 et les presets 10/50/200 en 64x64.

## 2026-07-10 - Version 3.05 - mise à jour

- Passage du statut public et des métadonnées de `dev` à `beta`, sans changer le numéro V3.05.
- Ajout des notices rapide et complète en français et en anglais ; l'application ouvre automatiquement la langue active.
- Ajout d'alertes navigables qui filtrent les machines concernées depuis `Points à vérifier`.
- Ajout du filtre `Modifiées uniquement` et d'une comparaison détaillée `Avant / après` des machines, canaux et patchs.
- Traduction complète en français/anglais des alertes dynamiques et des lignes de comparaison avant/après.
- Ajout d'une récupération automatique des modifications non sauvegardées, avec détection d'un XML original modifié depuis la copie temporaire.
- Ajout de six profils rapides combinant sample rate, bits, latence, IP automatique et mode réseau optionnel.
- Les profils rapides utilisent la cible d'actions globales et respectent le verrouillage des machines.
- Ajout d'une suite de tests de non-régression sur des fixtures XML anonymisées et d'un contrôle facultatif des vrais exports locaux.
- Retrait des binaires V2 du dépôt et des anciennes Releases V3.03/V3.04 ; la V3.05 reste l'unique publication téléchargeable.
- Remplacement des boutons séparés de nom, mode réseau, latence et preferred master par un bouton unique `Appliquer les paramètres`.
- Conservation de la configuration IP automatique/fixe dans la fenêtre `Détail machine`.
- Regroupement sur une seule ligne des resets patch RX/TX, RX, TX et de la suppression de machine.
- Transformation du bandeau `Points à vérifier` en alerte compacte sur une ligne, avec accès au message complet par `Détails`.
- Ajout d'un bouton `Réduire les réglages` / `Afficher les réglages` pour donner plus de hauteur au tableau des machines.
- Suppression du résumé de fichier dupliqué au-dessus de la page Configuration.
- Vérification visuelle à 1920 x 1080 : plusieurs lignes du tableau restent visibles avec une alerte de sample rates mélangées.
- Vérification fonctionnelle du bouton unique et de l'annulation groupée des changements.

## 2026-07-09 - Version 3.05

- Première publication des textes visibles et métadonnées V3.05.
- Correction de l'import d'un second XML : les machines uniques sont importées même si d'autres machines du fichier sont en doublon.
- Ajout d'une fenêtre de résolution des doublons à l'import, avec choix entre import des uniques seulement, renommage automatique et renommage manuel.
- Mise à jour des subscriptions/patchs importés quand une machine importée est renommée.
- Ajout des réglages sample rate et bits par échantillon machine par machine.
- Ajout des actions globales sample rate et bits par échantillon.
- Ajout d'une fonction pour remettre les adresses IPv4 en automatique, par machine ou globalement quand le XML expose ces champs.
- Ajout des listes rapides Sample rates, Bits et IP fixes.
- Ajout d'alertes visibles si plusieurs sample rates ou plusieurs encodages coexistent dans le preset.
- Adaptation de la validation de sauvegarde aux imports et suppressions de machines.
- Amélioration visuelle de la page Configuration : zone de réglages défilable, table des machines toujours visible et recherche globale plus lisible quand aucun résultat n'est affiché.
- Ouverture de l'application en plein écran pour que la page Configuration soit lisible dès le départ.
- Ajout d'une fenêtre de détail machine au double-clic, avec édition du nom, formats audio, IP et canaux TX/RX.
- Preferred master cochable directement depuis la table des machines.
- Ajout de l'IP fixe manuelle par machine et d'une action globale pour fixer une plage IP en série.
- Ajout d'un reset patch RX/TX par machine : déconnexion des entrées RX et suppression des patchs qui utilisent ses TX.
- Retouche de la page Configuration : bloc machine plus compact, bouton détail machine visible, actions globales séparées en onglets.
- Retouche de la fenêtre détail machine : les champs IP fixes sont alignés avec le mode `Fixe` et désactivés en mode automatique.
- Correction du style des onglets internes en thème sombre, pour éviter le fond blanc dans `Actions globales`.
- Retrait de l'action globale `Preferred master sécurisé`, trop ambiguë dans l'interface ; le réglage reste disponible machine par machine.
- Ajout d'un filtre machines, d'une sélection multiple et d'une colonne `Lock` dans la table des machines.
- Ajout d'une cible pour les actions globales : toutes les machines non verrouillées, sélection non verrouillée ou filtre affiché non verrouillé.
- Ajout des boutons `Sélectionner visibles`, `Effacer sélection`, `Verrouiller sélection` et `Déverrouiller sélection`.
- Ajout des resets séparés `Reset patch RX` et `Reset patch TX`, en plus du reset RX/TX complet.
- Ajout de modèles de renommage en série : `{00}`, `{000}`, `{n}` et `{device}`.
- Ajout d'un rapport final avant import Dante et d'un affichage de l'historique des actions.
- Ajout d'une fenêtre de comparaison XML en tableau.
- Ajout d'une notice rapide PDF, de raccourcis d'ouverture des notices dans l'application et de propositions d'ouverture en fin d'installation.
- Ajout d'info-bulles sur les commandes principales.
- Ajout d'une détection de version déjà installée dans l'installateur, avec choix entre remplacement/mise à jour et installation supplémentaire dans un autre dossier.

## 2026-07-07 - Version 3.04

- Passage des textes visibles et métadonnées en V3.04 dev.
- Ajout du garde-fou de modifications XML avant sauvegarde.
- Ajout du rapport compatibilité Dante Controller.
- Correction des libellés utilisateur `Dante Id` avec espace, sans renommer l'attribut XML `danteId`.
- Amélioration de l'affichage des latences Dante en ms, avec conservation des valeurs XML brutes.
- Ajout du mode Lecture seule / Édition.
- Prévisualisation des actions globales existantes.
- Sécurisation du preferred master global.
- Amélioration de la page Patch : mode simple/expert, colonne Source complète et choix TX avec Dante Id.
- Amélioration du Patchbook TXT et ajout d'un export CSV lecture seule.
- Amélioration de la page Santé du fichier.
- Ajout d'une vue Topologie simple.
- Amélioration du script installateur pour détecter Inno Setup dans plusieurs emplacements.
- Ajout d'un script d'archive source propre.
- Correction des accents dans l'export PDF et ajout de la signature/version dans les exports TXT/PDF.
- Correction du mode lecture seule : les boutons d'action restent utilisables après ouverture du XML, avec sauvegarde obligatoire sous un autre nom.
- Ajout d'un sélecteur Français / Anglais dans l'interface, mémorisé entre deux lancements.
- Ajout de la suppression d'une machine, avec nettoyage des subscriptions qui pointent vers cette machine.
- Ajout de l'import d'un second XML dans le projet ouvert, avec refus des noms de machines déjà présents.

## 2026-07-07 - Version 3.03 - mise à jour

- Ajout de la page `Santé du fichier` avec synthèse du preset, statistiques TX/RX, patchs actifs/libres/locaux, preferred masters, modes réseau, IP fixes et tableau filtrable des points à vérifier.
- Ajout d'un service de compatibilité XML Dante Controller avant sauvegarde : contrôle de la racine `<preset>`, de la version, des devices, des canaux `txchannel` / `rxchannel`, des attributs XML `danteId` / `mediaType` et des balises techniques importantes.
- Gestion correcte de `subscribed_device="."` comme source locale : affichage distinct, pas de faux conflit, conservation du `.` à la sauvegarde quand le fichier source l'utilise.
- Utilisation du Dante Id comme identifiant métier principal des canaux, avec conservation des attributs XML existants.
- Amélioration de la page `Patch` : colonnes TX brut/résolu/affiché, type de patch, warnings et filtres par état.
- Ajout d'un export `Patchbook TXT` organisé par device RX.
- Amélioration de la comparaison XML pour les canaux et patchs en s'appuyant sur les Dante Id.

## 2026-07-05 - Version 3.03

- Correction de la lisibilité des listes et menus déroulants : fond blanc et texte noir, y compris en thème sombre.
- Correction de la réinitialisation des canaux TX : les patchs RX reconnus sont maintenant mis à jour vers les nouveaux noms `1`, `2`, `3`, etc.
- Ajout du choix canal début / canal fin dans le renommage en série, pour ne renommer qu'une plage de canaux.
- Renforcement de la mise à jour des patchs TX pendant les renommages en série : les abonnements reconnus sont mis à jour en une seule passe depuis les anciens noms.
- Ajout d'un bandeau `Points à vérifier` pour les réseaux mélangeant redondant/daisychain et pour les machines détectées en IP fixe.
- Ajout d'une exclusion explicite du dossier `tmp` dans le projet `.NET` pour éviter que les fichiers temporaires de test soient compilés.

## 2026-07-05 - Version 3.02

- Ajout de l'annulation de la dernière action.
- Ajout d'un résumé avant sauvegarde plus lisible, avec tableau des modifications.
- Ajout d'une recherche globale machines / canaux / patchs.
- Ajout de l'export de rapport en TXT et PDF.
- Mise en évidence visuelle des conflits et lignes modifiées dans la table Patch.
- Ajout du renommage de canaux en série.
- Ajout de la liste des fichiers récents.
- Ajout d'un mode comparaison avec un autre XML.
- Renforcement de la sécurité XML : sauvegarde via fichier temporaire relu avant remplacement.
- Renforcement de la compatibilité XML : les renommages écrivent dans le champ de nom d'origine quand il existe.

## 2026-07-05 - Version 3.01

- Réduction de la colonne `Projet` à un résumé compact.
- Suppression des boutons d'onglets dupliqués dans la colonne de gauche.
- Conservation de la navigation uniquement par les onglets du haut.
- Réorganisation horizontale de la page `Configuration`.
- Ajout du filtre émetteur TX effectif dans la page `Patch`.
- Ajout des choix `Tous les émetteurs` et `Tous les récepteurs`.
- Clarification de la zone d'application de patch : `Source TX à appliquer` et `Canal TX à appliquer`.
- Séparation des champs de renommage `RX sélectionné` et `TX source`.
- Passage de la livraison en version `3.01`.

## 2026-07-04

- Création du dossier `V3` sans modification de `V2`.
- Ajout d'un projet source WPF `.NET 8`.
- Reprise de l'icône existante.
- Ajout d'une interface modernisée :
  - navigation latérale ;
  - thème sombre ;
  - thème clair ;
  - barre de statut ;
  - journal d'actions ;
  - affichage du fichier ouvert ;
  - état modifié / non modifié.
- Ajout d'une couche métier séparée pour les fichiers Dante.
- Conservation des fonctions V2 :
  - ouverture XML ;
  - renommage des devices ;
  - modification redondance / daisychain ;
  - modification latence ;
  - reset des noms de canaux ;
  - preferred master ;
  - listes rapides ;
  - sauvegarde sous un autre nom.
- Ajout du renommage individuel des canaux TX/RX.
- Ajout de la mise à jour automatique des références RX quand un canal TX renommé est utilisé par un patch reconnu.
- Ajout d'une première vue Patch hors ligne.
- Ajout de la détection de conflits simples :
  - device TX introuvable ;
  - canal TX introuvable.
- Ajout de confirmations avant actions globales ou risquées.
- Ajout d'une validation avant sauvegarde.
- Ajout d'un résumé des modifications avant sauvegarde.
- Ajout d'un backup automatique du fichier original.
- Ajout de `ANALYSE_V2.md`.
- Ajout de `README_V3.md`.
- Ajout de `build.ps1` et `run.ps1`.
- Installation du SDK .NET 8 en mode utilisateur pour permettre la compilation locale.
- Ajout d'un installateur autonome `dist\DanteConfigEditorV3_Setup.exe`.
- Ajout d'une version portable `dist\portable\DanteConfigEditorV3.exe`.
- Ajout d'un script reproductible `installer\build_installer.ps1`.
- Correction du démarrage autonome : l'icône reste au niveau de l'exécutable et des raccourcis, sans chargement XAML fragile au lancement.
- Ajout d'une signature discrète `By Mamat` dans la barre de statut.
- Correction d'un crash après ouverture XML causé par des colonnes booléennes en lecture seule.
- Ajout d'une interception globale des erreurs UI avec journal dans `%LOCALAPPDATA%\DanteConfigEditorV3\Logs`.
- Ajout de l'ouverture directe d'un XML passé en argument de ligne de commande pour faciliter les tests et les raccourcis.
- Correction de la lisibilité des en-têtes de colonnes en thème sombre.
- Ajout du renommage TX/RX directement depuis la page Patch.
- Renforcement de la mise à jour des patchs quand un canal TX est renommé : tous les champs d'abonnement reconnus du fichier sont parcourus.
- Ajout d'une copie archive de la V2 dans `legacy/V2`.
- Ajout d'un installateur Windows professionnel `dist\DanteConfigEditorV3_Installer.exe` avec choix du dossier, raccourcis et désinstallation.
- Installation par défaut dans Program Files avec demande de droits administrateur.
- Ajout de la signature `By Mamat` et du lien GitHub public dans l'assistant d'installation.
- Ajout des release notes et de la notice PDF dans l'installateur, avec proposition d'ouverture en fin d'installation.
