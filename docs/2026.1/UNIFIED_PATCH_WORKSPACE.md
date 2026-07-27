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

Un seul onglet Patch expose cinq représentations :

| Mode | Usage |
|---|---|
| Matrice | clic, glissement et Patch 1:1 directement dans la grille RX/TX |
| Easy patch | sélection multiple et affectation par plage |
| Liste RX vers TX | lecture détaillée de chaque RX et de sa source courante |
| Par machine | même liste, filtrée sur la machine sélectionnée |
| Modifications en attente | contrôle d'un lot explicitement préparé |

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

La vue « Modifications en attente » n'intercepte pas les clics directs. Elle
sert aux opérations qui ont été volontairement préparées comme un lot. Un lot
est appliqué dans une seule transaction et crée une seule étape
Annuler/Rétablir.

## Identités et synchronisation

Une modification en attente conserve :

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
en-têtes TX devient adaptative. Les commandes, l'option d'avertissement et les
labels de canaux restent présents. Les grandes listes et la matrice conservent
la virtualisation WPF.

## Vérifications réalisées

Le 27 juillet 2026 :

- application réelle ouverte avec la fixture anonymisée
  `representative-preset.xml` ;
- contrôle à environ `1266 x 813`, avec la bannière de soutien visible ;
- première ligne RX visible dans la matrice ;
- déconnexion directe d'une cellule ;
- Annuler puis Rétablir depuis la barre supérieure ;
- cohérence visuelle en français et en anglais ;
- cohérence visuelle en thèmes sombre et clair ;
- tests automatisés de session stable, rebase, lots atomiques et contrats UI.

## Limites actuelles

- L'interface Patch macOS conserve son organisation existante. Elle compile
  avec le Core partagé, mais n'a pas reçu la refonte visuelle Windows.
- Les renommages de canaux depuis certaines anciennes vues utilisent encore le
  chemin V3.6 ; ils invalident proprement la pile Rétablir 2026.1.
- Aucune validation sur un Mac physique n'a été effectuée dans ce lot.
- La validation manuelle finale dans Dante Controller reste distincte des
  tests automatisés DCE.
