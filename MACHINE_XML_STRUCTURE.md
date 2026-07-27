# Structure d'une machine dans un preset Dante

Cette note décrit les structures réellement observées par DCE et les règles
retenues pour la duplication et la banque de machines. Elle ne remplace pas une
spécification XML officielle d'Audinate.

## Racine du preset

Les fichiers étudiés utilisent une racine :

```xml
<preset version="3.0.0">
  <name>Nom du preset</name>
  <description>Description</description>
  <device>...</device>
</preset>
```

Des presets 2.1.0 et 3.0.0 ont été observés. Certains fichiers peuvent utiliser
un namespace par défaut. DCE conserve le nom qualifié de chaque élément et ne
retire pas ce namespace lors d'une sauvegarde.

## Machine physique capturée

Une machine issue d'un réseau peut notamment contenir :

```xml
<device>
  <captureInfo />
  <name>DEVICE-A</name>
  <default_name>Device-A-Default</default_name>
  <instance_id>
    <device_id>001DC1FFFE000001</device_id>
    <process_id>0</process_id>
  </instance_id>
  <manufacturer_name>Manufacturer</manufacturer_name>
  <model_name>Model</model_name>
  <friendly_name>DEVICE-A</friendly_name>
  <samplerate>48000</samplerate>
  <encoding>24</encoding>
  <unicast_latency>1000</unicast_latency>
  <interface network="0">...</interface>
  <txchannel danteId="1" mediaType="audio">...</txchannel>
  <rxchannel danteId="1" mediaType="audio">...</rxchannel>
</device>
```

La liste exacte varie selon le matériel, la version de Dante Controller et le
fabricant. DCE conserve les éléments inconnus.

## Rôle générique hors ligne

Le Preset Creator officiel produit des Custom Devices sans
`instance_id/device_id`. DCE suit cette logique :

```xml
<device>
  <captureInfo>...</captureInfo>
  <friendly_name>CUSTOM-IO</friendly_name>
  <samplerate>48000</samplerate>
  <encoding>24</encoding>
  <unicast_latency>1000</unicast_latency>
  <txchannel danteId="1" mediaType="audio">
    <label>Ch 1</label>
  </txchannel>
</device>
```

Un rôle générique n'usurpe donc pas l'identité EUI-64 d'un appareil réel.
L'association avec un matériel est laissée à Dante Controller lors de
l'application du preset.

## Classification des données

### Propriétés intrinsèques réutilisables

- fabricant et modèle ;
- nombre et structure des canaux TX/RX ;
- `danteId` et `mediaType` internes au rôle ;
- labels génériques ;
- sample rate, encodage et latence proposés ;
- extensions constructeur conservées avec prudence.

### Données propres à une instance

- nom et friendly name ;
- preferred master ;
- mode réseau ;
- interfaces et adresses IP ;
- labels liés à une émission ;
- subscriptions ;
- flows et adresses multicast.

### Identifiants uniques

- `instance_id/device_id` ;
- `instance_id/process_id` associé ;
- identité interne de session DCE, non sérialisée.

`device_id` ne doit jamais être inventé pour dupliquer une machine. L'identité
de session DCE est une annotation LINQ to XML : elle sert à suivre un rôle après
renommage, mais n'apparaît jamais dans le fichier.

### Références croisées

Une entrée RX peut utiliser :

```xml
<subscribed_channel>PROGRAM L</subscribed_channel>
<subscribed_device>DEVICE-A</subscribed_device>
```

Le marqueur `.` représente une source locale dans les structures reconnues.
Lorsqu'un TX est renommé, toutes les subscriptions qui le désignent sont mises
à jour. Lorsqu'une machine est supprimée, les subscriptions qui pointent vers
elle sont nettoyées.

Des alias historiques sont reconnus par DCE pour les noms de machine et de
canal source. Ils sont couverts par les tests de non-régression.

## Règles de duplication

La duplication V3.6 :

1. effectue une copie profonde du nœud `<device>` ;
2. retire `instance_id` et `default_name` ;
3. écrit le nouveau `friendly_name` ;
4. retire par défaut les interfaces réseau ;
5. retire par défaut les subscriptions RX ;
6. retire par défaut les `txflow` ;
7. désactive par défaut preferred master ;
8. conserve par défaut les labels et paramètres audio ;
9. valide la copie dans un document candidat ;
10. n'insère la copie que si aucune nouvelle erreur structurelle n'apparaît.

La machine source n'est jamais modifiée. L'opération est annulable.

Si l'utilisateur demande explicitement de conserver les subscriptions, une
référence locale écrite avec le nom de la machine source est réécrite vers le
nouveau nom. Le marqueur `.` reste inchangé.

## Règles de modèle de banque

Avant enregistrement dans la banque :

- `instance_id` et `default_name` sont retirés ;
- les interfaces sont retirées ;
- les subscriptions sont retirées ;
- les `txflow` sont retirés ;
- preferred master est désactivé ;
- le nom du projet est remplacé par un nom neutre interne ;
- les labels peuvent être remplacés par des valeurs génériques.

Le fragment XML assaini est conservé pour ne pas perdre les propriétés
constructeur inconnues. Il est accompagné de métadonnées versionnées et d'une
empreinte SHA-256.

## Insertion dans un autre projet

Une insertion :

1. vérifie le format et l'empreinte du modèle ;
2. exige la même version de preset dans le format de banque V1 ;
3. clone le fragment assaini ;
4. applique un nouveau nom unique ;
5. rebascule tous les éléments dans le namespace du projet cible ;
6. retire à nouveau identité, réseau et subscriptions par défense en profondeur ;
7. valide un document candidat ;
8. autorise précisément cet ajout dans le garde-fou XML.

Une modification ultérieure de l'instance ne modifie jamais le modèle stocké.

## Valeurs à ne jamais copier aveuglément

- `device_id` et `process_id` ;
- IP, masque, passerelle et DNS ;
- subscriptions vers d'autres machines ;
- adresses ou flows multicast ;
- preferred master ;
- données temporaires non comprises ;
- références à une machine absente du nouveau projet.

## Limites

- DCE ne connaît pas les capacités réelles d'un matériel hors ligne.
- Une extension constructeur conservée peut contenir une donnée propre à
  l'instance ; elle doit être vérifiée avec le fabricant.
- La conversion automatique entre versions de preset est bloquée en V1.
- La création d'un preset neuf reste expérimentale jusqu'à une validation
  réelle dans Dante Controller.

