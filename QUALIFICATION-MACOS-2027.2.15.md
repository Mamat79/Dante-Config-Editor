# Dante Config Editor 2027.2.15 — contrôles macOS

Les DMG Apple Silicon et Intel proviennent d’une compilation privée Codemagic sur le même commit produit que Windows. Les deux paquets et les notices FR/EN ont été comparés à la livraison qualifiée. Les contrôles automatisés et les essais de démarrage du DMG monté sur Apple Silicon ont réussi. Les vérifications du Trousseau ont utilisé le composant livré avec des échanges d’activation simulés.

Les essais de démarrage sur Apple Silicon sont non visuels. Le paquet Intel a été compilé et contrôlé statiquement. Le test ne couvre pas un Mac Intel physique, la reprise d’un profil utilisateur Mac ou un réseau Dante réel. Les signatures sont ad hoc, sans notarisation Apple.

## macOS verification

Apple Silicon and Intel DMGs came from a private Codemagic build at the same product commit as Windows. The two packages and their French and English manuals were checked against the qualified delivery. Automated checks and nonvisual launches from the mounted DMG on Apple Silicon passed. Keychain checks used the shipped component with simulated activation exchanges.

Intel was cross-built and inspected statically. The checks did not exercise a physical Intel Mac, a real-user-profile upgrade or a live Dante network. The packages use ad-hoc signatures without Apple notarization.
