# Dante Config Editor 2027.2.7 — qualification macOS / macOS qualification

Source exacte / exact source: `257fd4594b149be4b25307d74e9539e8d0becc3a`. Codemagic job `6aca3a8715254bd723b27d52`. Minimum macOS **15.0**. 821 tests réussis / passed, zéro échec/ignoré / zero failures/skipped. Apphosts ARM64/x64 et bibliothèques Mach-O contrôlés ; guides identiques entre architectures. Runtime .NET 10.0.12 embarqué, sans installation externe .NET.

Apple Silicon : lancement natif depuis le DMG monté en mode non visuel ; aucune recette interactive de bureau ou de clavier. / Native ARM64 launch from the mounted DMG was nonvisual; desktop and keyboard interaction were not qualified.

Intel : compilation croisée depuis un Mac Apple Silicon et contrôle structurel ; aucun lancement sur un Mac Intel physique. / Intel was cross-built and structurally inspected, not executed on a physical Intel Mac.

Signature ad-hoc vérifiée sur le runner : aucune signature Apple Developer ID ni notarisation. / Runner-verified ad-hoc signing only; no Apple Developer ID signature or notarization. Windows rehashes the delivered DMGs and checksums but does not independently verify macOS signing.

Les contrôles techniques et rendus headless ne sont pas une validation visuelle du DMG installé. Matériel audio/MIDI/Stream Deck, import Dante Controller réel et tests matériels non exercés. / Technical checks and headless renders do not qualify the installed DMG visually. Audio/MIDI/Stream Deck hardware, actual Dante Controller import and hardware behavior were not exercised.

La liaison native avec Excel ne concerne que StageFlow sous Windows avec Excel de bureau installé. Aucune liaison native Excel Mac validée ou implémentée n'est annoncée. / Native Excel linking is Windows desktop Excel only; no native Excel Mac link is claimed.
