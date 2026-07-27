# Tests manuels dans Dante Controller

## Règle de preuve

Ne cocher aucun résultat sans avoir réellement importé le fichier généré dans la version de Dante Controller indiquée. Une ouverture ou une sauvegarde automatisée dans Dante Config Editor ne vaut pas preuve d'import.

Toujours utiliser une copie et un environnement de test. Ne jamais modifier le XML original avec le script de validation.

## Identification du test

- identifiant du pack de validation :
- fichier anonymisé / référence interne :
- SHA-256 de l'original copié :
- SHA-256 du scénario importé :
- version exacte de Dante Controller :
- système d'exploitation :
- date :
- testeur :
- capture ou journal conservé hors du dépôt public :

## Préparation

- [ ] Le pack a été créé avec `tools/New-ValidationPack.ps1` depuis une copie du XML.
- [ ] Les hashes du pack correspondent aux fichiers testés.
- [ ] L'original n'a pas été modifié.
- [ ] Le rapport avant/après a été lu.
- [ ] Les avertissements du rapport de compatibilité sont compris.
- [ ] Le test est réalisé sans connexion à un réseau Dante de production.

## Import et structure générale

- [ ] Dante Controller accepte l'import sans erreur bloquante.
- [ ] Le nombre de devices correspond au rapport.
- [ ] Les noms des devices attendus sont présents.
- [ ] Aucun device inattendu n'a été créé ou supprimé.
- [ ] Les presets partiels restent identifiés comme tels.
- [ ] Les avertissements affichés par Dante Controller sont consignés mot pour mot ou par capture.

## Identité technique

- [ ] Les `Dante Id` sont inchangés sauf modification explicitement attendue et autorisée.
- [ ] Les `mediaType` sont inchangés.
- [ ] Les `instance_id` et `device_id` sont inchangés.
- [ ] L'ordre de balises n'a pas provoqué de perte de contenu.
- [ ] Les valeurs techniques inconnues du fichier source sont préservées.

## Canaux et patchs

- [ ] Les nombres de canaux TX et RX correspondent au rapport.
- [ ] Le device renommé porte le nouveau nom attendu.
- [ ] Le canal TX renommé porte le nouveau nom attendu.
- [ ] Les subscriptions utilisant ce TX ont été mises à jour.
- [ ] Le patch modifié relie le bon TX au bon RX.
- [ ] Les patchs non concernés sont inchangés.
- [ ] Les patchs locaux utilisant `subscribed_device="."` restent locaux.
- [ ] Les références vers un TX absent d'un preset partiel sont conservées comme attendu.
- [ ] Une machine sans TX reste valide.
- [ ] Une machine sans RX reste valide.
- [ ] Les affectations créées avec le patch visuel suivent l'ordre attendu.

## Audio et horloge

- [ ] La latence modifiée correspond à la valeur attendue.
- [ ] Les latences non concernées sont inchangées.
- [ ] Le sample rate de chaque device est correct.
- [ ] L'encodage / les bits par échantillon de chaque device sont corrects.
- [ ] Les mélanges volontaires de formats restent visibles comme avertissements.
- [ ] Le ou les preferred masters correspondent au scénario.
- [ ] Le cas sans preferred master est accepté ou documenté par Dante Controller.
- [ ] Le cas avec plusieurs preferred masters est accepté ou documenté par Dante Controller.

## Réseau

- [ ] Le mode redondant / daisychain de chaque device est correct.
- [ ] L'adresse IPv4 principale modifiée correspond au scénario.
- [ ] Le masque de l'interface principale est correct.
- [ ] La passerelle n'a pas été modifiée sans demande explicite.
- [ ] Le DNS n'a pas été modifié implicitement.
- [ ] L'interface secondaire est strictement inchangée.
- [ ] Le retour en IP automatique ne touche que les champs attendus.

## Comparaison finale

- [ ] Les différences observées correspondent au rapport avant/après.
- [ ] Aucune différence technique supplémentaire n'est visible.
- [ ] Les anomalies sont reportées dans `COMPATIBILITY_MATRIX.md` sans publier de données sensibles.
- [ ] Le résultat d'import, la date et le testeur sont renseignés dans la matrice.

## Scénarios V3.6 : rôles génériques et banque

Réaliser ces scénarios séparément afin qu'un échec reste attribuable :

- [ ] Une machine physique existante a été dupliquée avec les options par défaut.
- [ ] Le rôle dupliqué ne contient ni `instance_id` ni `device_id`.
- [ ] Dante Controller accepte ou refuse explicitement ce rôle générique ; le message exact est conservé.
- [ ] Le rôle peut être associé à l'appareil ou au modèle attendu sans modifier la machine source.
- [ ] Le nombre, les Dante Id, mediaType et labels TX/RX correspondent à la source.
- [ ] Aucune IP, subscription, flow ni Preferred Master n'a été recopié par défaut.
- [ ] Un modèle enregistré dans la banque a été ajouté à un autre preset de même version.
- [ ] L'instance ajoutée reste indépendante du modèle et ne réutilise aucune identité matérielle.
- [ ] Les propriétés constructeur inconnues conservées n'entraînent aucun avertissement inattendu.

## Scénario V3.6 : nouveau projet expérimental

- [ ] Le fichier minimal 3.0.0 se rouvre dans DCE sans erreur.
- [ ] Santé du fichier ne contient aucune erreur bloquante non expliquée.
- [ ] Dante Controller accepte ou refuse l'import ; le message exact est conservé.
- [ ] Le rôle initial éventuel est visible avec le bon nombre de TX/RX.
- [ ] Une sauvegarde depuis Dante Controller, si possible, est comparée au XML DCE.
- [ ] Le résultat reste marqué expérimental tant que plusieurs versions de Dante Controller n'ont pas été testées.

## Conclusion

- résultat : réussi / échec / partiel / interrompu ;
- anomalies :
- limitations confirmées :
- décision d'utilisation :
