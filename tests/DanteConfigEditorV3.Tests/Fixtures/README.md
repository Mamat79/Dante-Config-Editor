# Corpus XML anonymisé 2026.1

Ces fixtures sont synthétiques. Elles ne proviennent d'aucun projet de
production et ne contiennent ni identité matérielle réelle, ni nom de lieu, ni
adresse réseau exploitable.

| Fixture | Cas couverts |
|---|---|
| `representative-preset.xml` | preset complet, patch local, formats et modes réseau mixtes |
| `merge-preset.xml` | fusion et conflit de nom |
| `official-preset-creator-custom.xml` | rôle générique créé hors ligne |
| `compat-partial-one-way.xml` | preset partiel, TX uniquement, RX uniquement |
| `compat-subscription-edges.xml` | source absente, canal absent, subscription locale `.` |
| `compat-namespace-unknown.xml` | namespace par défaut, ordre différent, Unicode, balise inconnue, interfaces multiples |
| `compat-mixed-network-audio.xml` | sample rates, encodages, latences, modes réseau et interfaces secondaires |

Les presets 10, 50 et 200 machines sont produits à la volée par
`SyntheticPresetFactory`; ils ne sont pas versionnés afin d'éviter des fichiers
volumineux régénérables. Les sorties de duplication, fusion et création sont
également produites dans des répertoires temporaires par les tests concernés.
