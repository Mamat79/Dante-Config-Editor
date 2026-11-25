# Dante Config Editor 2027.3.3 · qualification macOS

Source privée et recette : `286671583a2c4de5bae4a291bd4dfdb868d053e6`. Job Codemagic `6b06eb657caac6d48973e746` du 25 novembre 2026. Construction normale des deux architectures, sans réemploi d’un ancien DMG.

- 1 079 tests du moteur, 105 tests d’interface Mac et 25 contrôles de fabrication ont réussi, sans échec ni test ignoré.
- La croix de Réseau Dante conserve la session. Le retour hors ligne déconnecte explicitement. Les sauvegardes XML/DCE/StageFlow incluent la session et préservent la préparation hybride.
- Ajout direct des XML prêts, import 2.1/3.0, correspondances et remplacement avec les abonnements dépendants, saisie des canaux avec Entrée/Tab et annulation Échap.
- Les préréglages sélectionnent tout au départ et proposent un XML natif. La grille commence repliée et les tableaux signalent les informations non fournies.
- Apple Silicon : lancement natif depuis le DMG monté en lecture seule, stabilité XML/StageFlow, fermeture gracieuse, Trousseau natif isolé et moteur réseau/WebKit en simulation. Les quatre guides embarqués correspondent octet par octet aux PDF publiés.
- Intel : architecture x86_64 et contenu vérifiés statiquement ; exécution native Intel non effectuée.
- Limites : macOS 15 minimum annoncé, signature ad hoc sans Developer ID ni notarisation. Pas de recette visuelle complète, de matériel Dante physique ou de mise à niveau d’un profil utilisateur réel. Le démontage propre du DMG n’est pas attesté séparément.
- Cette livraison conserve les dix fichiers Windows 2027.3.3 existants. Son source Windows reste `7660e30e4882eb2740f9f826ffa745a6d122131c` ; aucun tag existant n’est déplacé.

## English

The normal Mac build passed 1,079 Core tests, 105 Mac interface tests and 25 packaging tests with no failures or skips. Apple Silicon native startup, XML/StageFlow process stability, graceful Quit, isolated native Keychain and simulated network/WebKit checks passed from the mounted read-only DMG. Intel was cross-built and inspected statically; native Intel execution, full visual acceptance, real Dante hardware and real-user upgrade were not tested. The package declares macOS 15 minimum and uses ad-hoc signing without Developer ID or notarization. Clean DMG detach is not independently attested. The existing Windows release assets and source tag remain unchanged.

| Paquet / Package | Octets / Bytes | SHA-256 |
| --- | ---: | --- |
| AppleSilicon | 142958789 | a1600f8827bf954f33c76d4dbe1d7796d747305e0c7477da4896cdc747a3ae3a |
| Intel | 150020439 | 85f1168aa5806ecae4840e35c45e1b698aba6684b223cb3845e42499359bfe27 |
