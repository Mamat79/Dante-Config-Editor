# Espace Patch unifié 2026.1

## Objectif

L'espace Patch Windows utilise une seule session pour la matrice, Easy Patch
et la liste RX vers TX. Une affectation n'est plus conservée dans un état
propre à une vue : elle est identifiée par l'identité stable des machines et
par les Dante Id des canaux.

Le moteur ne reconstruit pas le XML à partir d'une matrice simplifiée. Il
applique les mutations ciblées au `DanteProject` courant, puis laisse le
garde-fou et la sauvegarde atomique contrôler l'export.

## Modes

Un seul onglet Patch expose trois représentations :

| Mode | Usage |
|---|---|
| Matrice | clic, glissement et Patch 1:1 directement dans la grille RX/TX |
| Easy patch | sélection multiple et affectation par plage |
| Liste RX vers TX | lecture détaillée de chaque RX et de sa source courante |

Les modes Matrice et Easy patch réutilisent la même instance
`UnifiedPatchSession`. La liste est reconstruite depuis le même projet après
chaque commande.

## Application directe

Dans l'interface intégrée, un clic, un glissement ou un Patch 1:1 applique
immédiatement le résultat. Il n'existe pas d'étape de prévisualisation
obligatoire.

L'option « M'avertir si le RX est déjà patché » est cochée par défaut. Si elle
est décochée, un remplacement reste explicite dans la grille mais ne déclenche
plus la confirmation.

## Navigation entre les liaisons

La matrice permet de suivre une liaison sans rechercher manuellement les deux
machines :

- le bouton placé sur une ligne RX retrouve sa source TX, sélectionne la
  machine émettrice, fait défiler la grille et surligne le croisement ;
- le bouton placé sous un en-tête TX affiche toutes ses destinations RX ;
- lorsqu'un TX alimente plusieurs RX, l'utilisateur choisit la destination à
  afficher avant que la machine réceptrice et le croisement soient
  sélectionnés ;
- une source libre, absente, ambiguë ou incomplète produit un message précis
  et n'entraîne aucune modification du projet.

Cette navigation est strictement en lecture seule. Elle résout les
subscriptions du projet et les changements encore présents dans la session,
mais ne crée ni ne supprime aucun patch.

## Matrice détachée

Sous Windows, **Détacher la matrice** ouvre une grande fenêtre indépendante.
Elle conserve les sélecteurs RX/TX, FLIP, Patch 1:1, le zoom, les
renommages et l'application immédiate. La fenêtre réutilise le même projet et
la même session que la vue intégrée ; une action reste donc synchronisée dans
les deux affichages.

Sous macOS, l'espace Patch est déjà présenté dans une fenêtre dédiée et offre
les mêmes commandes de navigation RX/TX.

## Identités et synchronisation

Chaque commande de patch conserve :

- l'identité stable de la machine RX ;
- le Dante Id du canal RX ;
- l'identité stable de la machine TX, si une source est demandée ;
- le Dante Id du canal TX.

Après un renommage, la session rebase les affichages sans dépendre des noms. Si
une machine ou un canal a réellement disparu, le changement concerné est
écarté avec un avertissement visible ; il n'est jamais redirigé silencieusement
vers un autre canal portant le même label.

## Annuler et rétablir

Les mutations Patch passent par `ProjectCommandDispatcher` :

- un clic direct forme une transaction ;
- un glissement ou un lot forme une seule transaction ;
- `Ctrl+Z` et le bouton Annuler restaurent la transaction ;
- `Ctrl+Y`, `Ctrl+Maj+Z` et le bouton Rétablir la rejouent ;
- une mutation V3.6 encore extérieure au dispatcher invalide la pile Rétablir
  afin qu'un ancien état ne puisse pas écraser une modification plus récente.

## Affichage compact

À faible hauteur, les textes explicatifs sont compactés et la hauteur des
en-têtes TX devient adaptative. Dans la matrice intégrée, le titre et l'option
d'avertissement partagent une ligne, les sélecteurs gardent les repères courts
RX/TX et Patch 1:1 partage sa barre avec le zoom. Les commandes, l'option
d'avertissement et les labels de canaux restent présents. Les grandes listes
et la matrice conservent la virtualisation WPF.

Le bouton **Voir dans Patch** de l'inspecteur sélectionne la vue Matrice et
recentre les sélecteurs RX/TX sur la machine concernée lorsqu'elle expose des
canaux correspondants.

## Vérifications réalisées

Le 29 juillet 2026 :

- application réelle ouverte avec la fixture anonymisée
  `representative-preset.xml` ;
- contrôle à environ `1266 x 813`, avec la bannière de soutien visible ;
- première ligne RX visible dans la matrice ;
- navigation d'un RX de `DEVICE-B` vers la source
  `DEVICE-A / 001 - PROGRAM L` ;
- affichage des deux destinations RX de cette source puis ouverture de la
  destination locale choisie ;
- ouverture de la matrice détachée avec RX, TX, FLIP, Patch 1:1 et zoom ;
- déconnexion directe d'une cellule ;
- Annuler puis Rétablir depuis la barre supérieure ;
- cohérence visuelle en français et en anglais ;
- cohérence visuelle en thèmes sombre et clair ;
- captures `1920 x 1024` et `1536 x 864`, cette dernière représentant l'espace
  logique d'un écran Full HD à 125 % ;
- 430 tests Core/Windows et 22 tests macOS réussis, dont les résolutions de
  source locale/externe, destinations multiples, références ambiguës et
  absence de mutation XML.

## Limites actuelles

- L'interface Patch macOS suit les trois mêmes parcours, avec un rendu natif
  Avalonia qui peut différer légèrement de WPF.
- Les renommages de canaux depuis certaines anciennes vues utilisent encore le
  chemin V3.6 ; ils invalident proprement la pile Rétablir 2026.1.
- Aucune validation sur un Mac physique n'a été effectuée dans ce lot.
- La validation manuelle finale dans Dante Controller reste distincte des
  tests automatisés DCE.
