# Dante Config Editor 2027.3 — qualification Windows x64

Installateur autonome Windows 11 x64 produit depuis le commit source `2d6b2ceefd46c33cb1d783afffc74bc84e006038`. Version technique 2027.3.0, taille 102 986 996 octets.

- 1 060 tests principaux et 20 contrôles du contrat de version ont réussi. La recette WPF a validé l’essai actif, le blocage à expiration, l’activation avec un code de test signé et la récupération du brouillon en cours.
- Après extraction de l’installateur, les 2 181 fichiers du moteur réseau ont correspondu à leur manifeste. Les quatre PDF Windows embarqués ont correspondu aux PDF publics. Les libellés de fermeture ont été vérifiés en français et en anglais.
- La simulation depuis l’installateur extrait a exercé le renommage TX avec remise des abonnements RX, l’ajout et le retrait du patch, la reconnexion d’un récepteur et le cycle de création et de suppression multicast.
- L’installateur n’est pas signé Authenticode. Les essais réseau se sont déroulés en simulation. La compatibilité avec un parc Dante réel et le transport H.264 sur le terrain restent à vérifier.

SHA-256 de l’installateur : `efa1f361c784d50c64e2d0e73d5083e01a70386c2eb4a1a441153fd8452dad73`.
