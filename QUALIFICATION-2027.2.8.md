# Dante Config Editor 2027.2.8 — contrôles et limites / checks and limits

La livraison Windows x64 inclut le runtime .NET 10. Source privée : `9b83b712fa4f89b989b0a9341c84ea4d58d461ba`. Le dépôt public contient uniquement la distribution et sa documentation.

L’installeur Windows standard a été exécuté pour une mise à jour en place depuis 2027.2.6, après sauvegardes vérifiées. Les 283 fichiers attendus correspondent au paquet qualifié. Les profils, activations, banque personnelle et états contrôlés ont été conservés. Le registre et le binaire indiquent 2027.2.8 ; le démarrage depuis l’installation personnelle et la fermeture normale ont été observés. Ce contrôle local ne constitue pas une matrice de toutes les versions de Windows ou de toutes les mises à niveau.

Une recette native a ouvert un XML synthétique avec namespace, Unicode, deux interfaces et une extension inconnue, l’a sauvegardé dans un nouveau fichier puis l’a rouvert. Structure, attributs, ordre des éléments et valeurs sont conservés. Le XML d’origine est inchangé. La validation DCE affichait zéro erreur bloquante et un avertissement attendu pour l’identifiant matériel volontairement fictif du test, conservé sans inventer d’identité réelle.

La fenêtre française sombre a été observée. La matrice complète langues, thèmes, focus et DPI 125/150/200 % n’a pas été exécutée. Aucun import dans Dante Controller, réseau Dante actif, matériel audio ni transfert de licence n’est qualifié par cette livraison. L’installeur Windows n’est pas signé Authenticode. La date locale de dernière utilisation de la licence évolue normalement au démarrage ; les autres champs et fichiers d’activation contrôlés sont conservés.

Les quatre PDF Windows sont les fichiers exacts du paquet source et de l’installation vérifiée. Ils n’ont pas été régénérés par l’atelier de publication ; aucune nouvelle recette visuelle de mise en page n’est revendiquée. Le fichier `SHA256SUMS-2027.2.8-Windows.txt` couvre les fichiers Windows joints. Les anciens téléchargements restent disponibles pour retour arrière.

Les quatre fichiers macOS 2027.2.8 déjà publiés restent strictement inchangés. Minimum macOS 15 déclaré ; applications autonomes .NET 10.0.12. Le reçu Mac rapporte 934 tests réussis, sans échec ni test ignoré, et trois scènes de démarrage/fermeture ARM depuis DMG monté, sans observation visuelle. Intel a été cross-compilé et inspecté, sans exécution native Intel. Signature ad hoc, sans Developer ID ni notarisation. Le détachement propre du DMG n’est pas établi indépendamment ; aucun import Dante Controller, profil personnel, matériel ou mise à niveau Mac n’est validé.

V1 reste le parcours de projet par défaut. V2 exige une nouvelle copie locale explicite, sans migration sur place. Les échanges V2 réseau/LIVE/AutoCAD ne sont pas qualifiés dans cette tranche. Conserver une copie V1 pour les applications anciennes.

---

The Windows x64 delivery includes .NET 10. Private source commit: `9b83b712fa4f89b989b0a9341c84ea4d58d461ba`. Only distribution files and documentation are published.

The standard Windows installer was used for an in-place upgrade from 2027.2.6 after verified backups. All 283 expected payload files match. Checked profiles, activation files, the personal device bank and stored states were preserved. The executable and uninstall registry report 2027.2.8; startup from the actual personal installation and normal shutdown were observed. This local check is not a complete Windows-version or upgrade matrix.

The native XML check loaded a synthetic fixture with namespaces, Unicode, two interfaces and an unknown extension, saved it to a new file and reopened it. Structure, attributes, element order and values were preserved, with the original unchanged. DCE reported no blocking errors and one expected warning for the deliberately fake test device identifier; the value was retained without inventing a real hardware identity.

French dark-mode windows were observed. The full language, theme, keyboard-focus and 125/150/200% DPI matrix was not executed. Actual Dante Controller import, live Dante networks, physical audio hardware and licence transfer remain unqualified. The Windows installer is unsigned. The local licence's last-seen time changes normally on startup; other checked licence fields and activation files were preserved.

The four Windows PDFs are byte-for-byte copies of the pinned source payload and verified installation. Publication did not regenerate them or perform new visual layout QA. `SHA256SUMS-2027.2.8-Windows.txt` covers the attached Windows files. Older downloads remain available for rollback.

The four previously published macOS files are unchanged. They declare macOS 15 minimum and bundle .NET 10.0.12. The Mac receipt reports 934 passing tests and three ARM startup/shutdown scenes from a mounted DMG without visual acceptance. Intel was cross-built and inspected, without native Intel execution. Ad-hoc signing is not Developer ID signing or Apple notarization. Clean DMG detachment is not independently proven; actual Dante Controller import, personal profiles, hardware and Mac upgrades remain unqualified.

V1 remains the default project workflow. V2 requires an explicit new local copy, without in-place migration. V2 network/LIVE/AutoCAD exchange is not qualified in this tranche; retain a V1 copy for older applications.
