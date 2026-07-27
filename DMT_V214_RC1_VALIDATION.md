# Validation DMT 2.14.0-RC1

## Périmètre

- Dépôt : `togrupe/dlive-midi-tools`
- Branche : `feature/add-json-export`
- Commit contrôlé : `3c34052b84545a8bc946fb1b2d402af19e1b3ed0`
- Date du contrôle : 2026-07-23
- Exporteurs : `src/export/JsonExporter.py` et `src/export/CsvExporter.py`

## Structure constatée

L'export JSON déclare :

- `format = dante-config-editor-channel-labels`
- `schemaVersion = 1`
- `sourceVersion = 2.14.0-RC1`
- une ou plusieurs listes sous `sets`
- `deviceName`, `direction`, `channelNumber`, `danteId` et `label`

L'export CSV utilise exactement les colonnes :

`format_version,source_app,source_version,device,direction,channel,dante_id,label`

## Validation reproductible

Les deux exporteurs DMT du commit indiqué ont été exécutés avec trois canaux synthétiques. Le JSON et le CSV produits ont été comparés aux fixtures versionnées dans :

`tests/DanteConfigEditorV3.Tests/Fixtures/DmtV214Rc1`

Résultats :

- objet JSON identique : oui ;
- lignes et champs CSV identiques : oui ;
- lecture par les adaptateurs DCE : oui ;
- version source détectée : `2.14.0-RC1` ;
- trois canaux et leurs Dante ID préservés ;
- test automatisé JSON : réussi ;
- test automatisé CSV : réussi.

## Limites

- Le test couvre les exporteurs JSON/CSV présents dans les sources de la branche et non une Release binaire finale de DMT.
- Aucun échange en temps réel entre DMT et DCE n'est réalisé.
- Les classeurs XLSX/ODS restent gérés par leurs adaptateurs historiques et ne sont pas assimilés au nouveau schéma JSON/CSV.
- Toute future version de schéma différente de `1` sera refusée jusqu'à ajout d'une migration testée.
