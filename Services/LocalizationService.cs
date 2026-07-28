namespace DanteConfigEditor.Services;

public static class LocalizationService
{
    private static readonly Dictionary<string, string> French = new(StringComparer.Ordinal)
    {
        ["Language.French"] = "Français",
        ["Language.English"] = "Anglais",
        ["Language.Label"] = "Langue",

        ["Search.Hint"] = "Tapez au moins 2 caractères pour chercher",
        ["Search.NoFileLoaded"] = "Chargez un XML pour chercher",
        ["Search.NoResult"] = "Aucun résultat",

        ["Filter.AllSenders"] = "Tous les émetteurs",
        ["Filter.AllReceivers"] = "Tous les récepteurs",
        ["Filter.AllRx"] = "Tous les RX",
        ["Filter.ActivePatches"] = "Patchs actifs",
        ["Filter.FreeRx"] = "RX libres",
        ["Filter.LocalPatches"] = "Patchs locaux",
        ["Filter.MissingTxDevices"] = "Devices TX absents",
        ["Filter.MissingTxChannels"] = "Canaux TX introuvables",
        ["Filter.Warnings"] = "Warnings",
        ["Filter.HealthWarnings"] = "Avertissements",
        ["Filter.Conflicts"] = "Conflits",
        ["Filter.Modified"] = "Modifiés",
        ["Filter.All"] = "Tous",
        ["Filter.Info"] = "Infos",
        ["Filter.Errors"] = "Erreurs",
        ["Filter.Patches"] = "Patchs",
        ["Filter.Devices"] = "Devices",
        ["Filter.Clock"] = "Clock",
        ["Filter.Network"] = "Réseau",
        ["Filter.XmlCompatibility"] = "Compatibilité XML",
        ["Filter.WarningsConflicts"] = "Warnings / conflits",

        ["DeviceFilter.All"] = "Toutes",
        ["DeviceFilter.Locked"] = "Verrouillées",
        ["DeviceFilter.StaticIp"] = "IP fixes",
        ["DeviceFilter.PreferredMaster"] = "Preferred masters",
        ["DeviceFilter.Redundant"] = "Redondantes",
        ["DeviceFilter.Daisychain"] = "Daisychain",
        ["DeviceFilter.NoTx"] = "Sans TX",
        ["DeviceFilter.NoRx"] = "Sans RX",
        ["DeviceFilter.Modified"] = "Modifiées uniquement",
        ["DeviceFilter.WarningSelection"] = "Alerte sélectionnée",
        ["DeviceFilter.SampleRateDifferent"] = "Sample rate différente",
        ["DeviceFilter.EncodingDifferent"] = "Bits différents",
        ["Target.AllUnlocked"] = "Toutes non verrouillées",
        ["Target.SelectedUnlocked"] = "Sélection non verrouillée",
        ["Target.FilteredUnlocked"] = "Filtre affiché non verrouillé",

        ["PatchView.Simple"] = "Simple",
        ["PatchView.Expert"] = "Expert",

        ["Status.Ready"] = "Prêt",
        ["Status.FileLoaded"] = "Fichier chargé. Les modifications seront enregistrées sous un nouveau nom.",
        ["Status.EditEnabled"] = "Mode édition activé.",
        ["Status.FileSaved"] = "Fichier sauvegardé.",
        ["Status.LastActionUndone"] = "Dernière action annulée.",
        ["Status.TopologyDisplayed"] = "Topologie simple affichée.",
        ["Status.TxtExported"] = "Rapport TXT exporté.",
        ["Status.PdfExported"] = "Rapport PDF exporté.",
        ["Status.PatchbookTxtExported"] = "Patchbook TXT exporté.",
        ["Status.PatchbookCsvExported"] = "Patchbook CSV exporté.",
        ["Status.EditMode"] = "Mode : Édition",
        ["Status.ReadOnlyMode"] = "Mode : Lecture seule",
        ["Status.NoFileLoaded"] = "Aucun fichier chargé.",
        ["Status.NoFileOpen"] = "Aucun fichier ouvert",
        ["Status.Unmodified"] = "Non modifié",
        ["Status.ModifiedUnsaved"] = "Modifié - non sauvegardé",
        ["Status.NoDeviceSettingsChanged"] = "Aucun paramètre de la machine à appliquer.",
        ["Status.RecoveryRestored"] = "Session automatique récupérée - modifications non sauvegardées.",
        ["Status.NoImportantWarning"] = "Aucun point important à vérifier.",
        ["Status.WarningDevicesDisplayed"] = "{0} machine(s) concernée(s) affichée(s).",
        ["Status.ProfileAlreadyApplied"] = "Le profil est déjà appliqué à toute la cible.",
        ["Status.EditActiveButton"] = "Édition active",
        ["Status.ActivateEditButton"] = "Activer l'édition",
        ["Status.LoadXmlToStart"] = "Chargez un fichier XML pour commencer.",

        ["Action.DeviceRenamed"] = "Nom mis à jour.",
        ["Action.NetworkModeUpdated"] = "Mode réseau mis à jour.",
        ["Action.LatencyUpdated"] = "Latence mise à jour.",
        ["Action.SampleRateUpdated"] = "Sample rate mise à jour.",
        ["Action.EncodingUpdated"] = "Bits par échantillon mis à jour.",
        ["Action.IpAutoApplied"] = "IP automatique appliquée.",
        ["Action.IpStaticApplied"] = "IP fixe appliquée.",
        ["Action.DevicePatchesReset"] = "Patchs RX/TX de la machine réinitialisés.",
        ["Action.DeviceRxPatchesReset"] = "Patchs RX de la machine réinitialisés.",
        ["Action.DeviceTxPatchesReset"] = "Patchs TX de la machine réinitialisés.",
        ["Action.DeviceDetailsUpdated"] = "Détail machine mis à jour.",
        ["Action.DeviceSettingsUpdated"] = "Paramètres de la machine mis à jour.",
        ["Action.QuickProfileApplied"] = "Profil rapide appliqué.",
        ["Action.PreferredMasterUpdated"] = "Preferred master mis à jour.",
        ["Action.ChannelsReset"] = "Canaux réinitialisés.",
        ["Action.ChannelRenamed"] = "Canal renommé.",
        ["Action.BatchRenameApplied"] = "Renommage en série appliqué.",
        ["Action.AllNetworkModesApplied"] = "Mode réseau appliqué à tous les devices.",
        ["Action.AllLatenciesApplied"] = "Latence appliquée à tous les devices.",
        ["Action.AllSampleRatesApplied"] = "Sample rate appliquée à tous les devices.",
        ["Action.AllEncodingsApplied"] = "Bits par échantillon appliqués à tous les devices.",
        ["Action.AllIpAutoApplied"] = "IP automatique appliquée à tous les devices.",
        ["Action.AllIpStaticApplied"] = "IP fixes appliquées en série.",
        ["Action.AllChannelsReset"] = "Tous les canaux ont été réinitialisés.",
        ["Action.PatchApplied"] = "Patch appliqué.",
        ["Action.PatchRemoved"] = "Patch supprimé.",
        ["Action.VisualPatchesApplied"] = "{0} changement(s) de patch visuel appliqué(s).",
        ["Action.TxChannelRenamed"] = "Canal TX renommé et patchs mis à jour.",
        ["Action.RxChannelRenamed"] = "Canal RX renommé.",
        ["Action.DeviceDeleted"] = "Machine supprimée.",
        ["Action.XmlMerged"] = "XML ajouté au projet.",
        ["Action.AtomicChaosApplied"] = "Exercice atomique généré - non sauvegardé.",

        ["Dialog.ConfirmTitle"] = "Confirmation requise",
        ["Dialog.OpenXmlTitle"] = "Ouvrir une configuration Dante",
        ["Dialog.MergeXmlTitle"] = "Ajouter un XML au projet ouvert",
        ["Dialog.SaveXmlTitle"] = "Enregistrer une nouvelle configuration",
        ["Dialog.ExportTxtTitle"] = "Exporter le rapport TXT",
        ["Dialog.ExportPdfTitle"] = "Exporter le rapport PDF",
        ["Dialog.ExportPatchbookTxtTitle"] = "Exporter le patchbook TXT",
        ["Dialog.ExportPatchbookCsvTitle"] = "Exporter le patchbook CSV",
        ["Dialog.XmlFilter"] = "Fichiers XML (*.xml)|*.xml|Tous les fichiers (*.*)|*.*",
        ["Dialog.TxtFilter"] = "Rapport texte (*.txt)|*.txt|Tous les fichiers (*.*)|*.*",
        ["Dialog.PdfFilter"] = "Rapport PDF (*.pdf)|*.pdf|Tous les fichiers (*.*)|*.*",
        ["Dialog.PatchbookTxtFilter"] = "Patchbook texte (*.txt)|*.txt|Tous les fichiers (*.*)|*.*",
        ["Dialog.PatchbookCsvFilter"] = "Patchbook CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
        ["Dialog.NoRecentFileTitle"] = "Aucun fichier récent",
        ["Dialog.NoRecentFileMessage"] = "Sélectionnez un fichier récent à ouvrir.",
        ["Dialog.FileMissingTitle"] = "Fichier introuvable",
        ["Dialog.FileMissingMessage"] = "Ce fichier récent n'existe plus.",
        ["Dialog.OpenFailedTitle"] = "Le fichier ne peut pas être ouvert.",
        ["Dialog.SaveImpossibleTitle"] = "Sauvegarde impossible",
        ["Dialog.ChooseAnotherNameTitle"] = "Choisissez un autre nom",
        ["Dialog.ChooseAnotherNameMessage"] = "Pour protéger le XML d'origine, l'application n'enregistre pas par-dessus le fichier ouvert. Choisissez un nouveau nom de fichier.",
        ["Dialog.OverwriteMessage"] = "Ce fichier existe déjà. Voulez-vous vraiment l'écraser ?",
        ["Dialog.SaveSummaryTitle"] = "Résumé avant sauvegarde",
        ["Dialog.OriginalBackupMessage"] = "Une sauvegarde du fichier original sera créée avant l'écriture. Continuer ?",
        ["Dialog.SaveErrorTitle"] = "Erreur pendant la sauvegarde",
        ["Dialog.RevertTitle"] = "Annuler les changements",
        ["Dialog.RevertMessage"] = "Les changements non sauvegardés seront perdus. Continuer ?",
        ["Dialog.ReloadErrorTitle"] = "Impossible de recharger le fichier original",
        ["Dialog.UndoErrorTitle"] = "Annulation impossible",
        ["Dialog.NoChannelTitle"] = "Aucun canal sélectionné",
        ["Dialog.NoChannelMessage"] = "Sélectionnez un canal TX ou RX à renommer.",
        ["Dialog.InvalidRangeTitle"] = "Plage invalide",
        ["Dialog.InvalidRangeMessage"] = "Sélectionnez un canal de début et un canal de fin.",
        ["Dialog.InvalidRangeOrderMessage"] = "Le canal de fin doit être placé après le canal de début.",
        ["Dialog.InvalidNumberTitle"] = "Numéro invalide",
        ["Dialog.InvalidNumberMessage"] = "Indiquez un numéro de départ valide.",
        ["Dialog.NoRxTitle"] = "Aucun canal RX sélectionné",
        ["Dialog.NoRxMessage"] = "Sélectionnez une ligne dans la table de patch.",
        ["Dialog.NoRxLineMessage"] = "Sélectionnez une ligne RX dans la table de patch.",
        ["Dialog.MissingTxTitle"] = "Canal TX manquant",
        ["Dialog.MissingTxMessage"] = "Sélectionnez un device TX et un canal TX dans la zone de patch.",
        ["Dialog.ExportImpossibleTitle"] = "Export impossible",
        ["Dialog.ExportPatchbookImpossibleTitle"] = "Export Patchbook impossible",
        ["Dialog.ExportPatchbookCsvImpossibleTitle"] = "Export Patchbook CSV impossible",
        ["Dialog.ActionImpossibleTitle"] = "Action impossible",
        ["Dialog.NoFileLoadedTitle"] = "Aucun fichier chargé",
        ["Dialog.NoFileLoadedMessage"] = "Ouvrez d'abord un fichier XML de configuration Dante.",
        ["Dialog.NoDeviceSelectedTitle"] = "Aucune machine sélectionnée",
        ["Dialog.NoDeviceSelectedMessage"] = "Sélectionnez une ou plusieurs machines.",
        ["Dialog.SynopticLayoutErrorTitle"] = "Mise en page du synoptique impossible",
        ["Dialog.DeleteDeviceWarning"] = "La machine '{0}' sera supprimée du projet. Les subscriptions/patchs qui pointent vers cette machine seront aussi supprimés. Continuer ?",
        ["Dialog.ResetDevicePatchesWarning"] = "Les entrées RX de la machine '{0}' seront déconnectées, et tous les patchs qui utilisent ses TX seront supprimés. Continuer ?",
        ["Dialog.MergeXmlWarning"] = "Les machines du fichier XML sélectionné seront ajoutées au projet courant. Les noms de machines déjà présents seront refusés. Continuer ?",
        ["DuplicateDialog.Title"] = "Doublons de machines",
        ["DuplicateDialog.Intro"] = "Certaines machines du XML importé existent déjà dans le projet ouvert. Vous pouvez importer seulement les machines sans doublon, renommer automatiquement les doublons, ou choisir vous-même les nouveaux noms.",
        ["DuplicateDialog.OriginalName"] = "Nom dans le XML importé",
        ["DuplicateDialog.NewName"] = "Nouveau nom à importer",
        ["DuplicateDialog.Suffix"] = "Suffixe du renommage automatique",
        ["DuplicateDialog.UniqueOnly"] = "Importer uniques seulement",
        ["DuplicateDialog.AutoRename"] = "Renommage auto",
        ["DuplicateDialog.ManualRename"] = "Importer avec ces noms",
        ["DuplicateDialog.Cancel"] = "Annuler",
        ["DuplicateDialog.InvalidTitle"] = "Noms invalides",
        ["DuplicateDialog.EmptyName"] = "Chaque machine renommée doit avoir un nouveau nom.",
        ["DuplicateDialog.DuplicateNewName"] = "Deux machines importées ne peuvent pas recevoir le même nouveau nom.",
        ["Dialog.LatencyWarning"] = "Modifier la latence Dante peut provoquer une reconfiguration des flux lors de l'import/application dans les outils Dante. Vérifiez toujours le preset dans Dante Controller.",
        ["Dialog.LatencyWarningContinue"] = "Modifier la latence Dante peut provoquer une reconfiguration des flux lors de l'import/application dans les outils Dante. Continuer ?",
        ["Dialog.AudioFormatWarning"] = "Modifier la sample rate ou les bits par échantillon peut rendre certaines machines incompatibles si elles ne supportent pas cette valeur. Vérifiez toujours le preset dans Dante Controller.",
        ["Dialog.AudioFormatWarningContinue"] = "Modifier la sample rate ou les bits par échantillon peut rendre certaines machines incompatibles si elles ne supportent pas cette valeur. Continuer ?",
        ["Dialog.IpStaticWarning"] = "Modifier une IP en fixe peut couper la communication si l'adresse, le masque ou la passerelle sont mauvais. Vérifiez toujours le preset dans Dante Controller.",
        ["Dialog.IpStaticWarningContinue"] = "Modifier les IP en fixe peut couper la communication si la plage, le masque ou la passerelle sont mauvais. Continuer ?",
        ["Dialog.DeviceDetailsWarning"] = "Les changements de cette fiche peuvent modifier le nom de la machine, ses formats, son IP et ses canaux. Continuer ?",
        ["Dialog.AtomicChaosTitle"] = "Atomic Bomb - exercice de dépannage",
        ["Dialog.AtomicChaosNothingSelected"] = "Cochez au moins une catégorie à saboter. Même une bombe pédagogique a besoin d'une cible.",
        ["Dialog.AtomicChaosCompleted"] = "Scénario atomique créé (graine {0}). {1} machine(s), {2} TX, {3} RX patché(s), {4} RX libre(s), {5} IP fixe(s), {6} IP automatique(s). Le XML original n'a pas été modifié. Utilisez Enregistrer sous pour conserver l'exercice.",
        ["Dialog.RecoveryTitle"] = "Récupération de session",
        ["Dialog.RecoveryFound"] = "Une copie automatique non enregistrée datant du {0:g} a été trouvée. Voulez-vous la récupérer ?\n\nNon supprimera cette copie temporaire et ouvrira le XML original.",
        ["Dialog.RecoverySourceChanged"] = "Attention : le fichier XML original a changé depuis cette récupération. Vérifiez attentivement les différences avant de sauvegarder.",
        ["Dialog.NoDeviceChanges"] = "Aucune modification de machine, canal ou patch n'est détectée depuis l'ouverture du XML.",
        ["Dialog.DeviceChangesTitle"] = "Modifications avant / après",
        ["Dialog.SelectProfile"] = "Sélectionnez un profil rapide.",
        ["Dialog.ProfileWarningContinue"] = "Ce profil peut modifier plusieurs paramètres audio et réseau en une seule action. Vérifiez la prévisualisation et contrôlez le XML final dans Dante Controller. Continuer ?",
        ["Dialog.ResetDeviceChannelsWarning"] = "Les noms des canaux du device sélectionné seront remplacés par 1, 2, 3...",
        ["Dialog.BatchRenameWarning"] = "Les noms des canaux {0} {1} à {2} seront remplacés en série. Continuer ?",
        ["Dialog.Continue"] = "Continuer ?",
        ["Dialog.RemovePatchWarning"] = "Le patch du canal RX sélectionné sera supprimé.",
        ["Dialog.ExternalPatchWarning"] = "Ce patch pointe vers un device qui n'est pas présent dans le preset. Cela peut être normal si le preset Dante est partiel. Ne le modifiez que si vous êtes certain de vouloir remplacer cette source. Continuer ?",
        ["Dialog.ExternalPatchStatus"] = "Ce patch pointe vers un device absent du preset. Cela peut être normal si le preset Dante est partiel.",

        ["Log.FileLoaded"] = "Fichier chargé : {0}",
        ["Log.EditEnabled"] = "Mode édition activé.",
        ["Log.EditEnabledAuto"] = "Mode édition activé automatiquement.",
        ["Log.OriginalBackupCreated"] = "Sauvegarde originale créée : {0}",
        ["Log.FileSaved"] = "Fichier enregistré : {0}",
        ["Log.ReloadOriginal"] = "Changements annulés. Rechargement du fichier original.",
        ["Log.ActionUndone"] = "Action annulée : {0}",
        ["Log.RecoveryRestored"] = "Session automatique récupérée.",
        ["Log.RecoveryUnavailable"] = "Récupération automatique indisponible : {0}",
        ["Log.TxtExported"] = "Rapport TXT exporté : {0}",
        ["Log.PdfExported"] = "Rapport PDF exporté : {0}",
        ["Log.PatchbookTxtExported"] = "Patchbook TXT exporté : {0}",
        ["Log.PatchbookCsvExported"] = "Patchbook CSV exporté : {0}",
        ["Log.XmlMerged"] = "XML ajouté au projet : {0}",

        ["Summary.PatchRows"] = "{0} lignes - {1} actifs - {2} locaux - {3} warning(s) - {4} conflit(s)",
        ["Summary.Health"] = "Preset : {0}  |  Version : {1}  |  Mode : {2}  |  Fichier : {3}\nDevices : {4}  |  TX : {5}  |  RX : {6}  |  Patchs actifs : {7}  |  RX libres : {8}\nPatchs locaux : {9}  |  Devices TX absents : {10}  |  Canaux TX introuvables : {11}  |  Preferred masters : {12}\nSamplerates : {13}  |  Encodages : {14}  |  Latences : {15}\nRedondants : {16}  |  Daisychain : {17}  |  IP fixes détectées : {18}  |  Erreurs : {19}  |  Warnings : {20}",
        ["Profile.48k24b1msAuto"] = "48 kHz / 24 bit / 1 ms / IP auto",
        ["Profile.48k24b2msAuto"] = "48 kHz / 24 bit / 2 ms / IP auto",
        ["Profile.96k24b1msAuto"] = "96 kHz / 24 bit / 1 ms / IP auto",
        ["Profile.96k24b2msAuto"] = "96 kHz / 24 bit / 2 ms / IP auto",
        ["Profile.48k24b1msRedundant"] = "48 kHz / 24 bit / 1 ms / Redondant / IP auto",
        ["Profile.48k24b1msDaisychain"] = "48 kHz / 24 bit / 1 ms / Daisychain / IP auto",

        ["ChannelLabel.Warning.MoreThanEightCharacters"] = "plus de 8 caractères",
        ["ChannelLabel.Warning.UnsupportedDmtCharacters"] = "caractères non pris en charge par DMT/dLive",
        ["Ui.Placeholder.RecentFiles"] = "Fichiers récents",

        ["Support.Title"] = "Soutenir DCE",
        ["Support.Subtitle"] = "Un geste entièrement facultatif pour accompagner ce projet gratuit.",
        ["Support.Free"] = "Dante Config Editor reste entièrement gratuit et toutes ses fonctions sont disponibles sans contribution.",
        ["Support.PayPalDescription"] = "Scannez ce QR code avec l’application PayPal de votre téléphone. DCE ne traite aucun paiement et ne reçoit aucune information financière.",
        ["Support.OtherWays"] = "Vous pouvez aussi aider gratuitement en ajoutant une étoile au projet GitHub ou en partageant un retour. Et si vous êtes vraiment fous, vous pouvez même faire les deux !",
        ["Support.PayPalMeButton"] = "Ouvrir PayPal sur cet ordinateur",
        ["Support.PayPalMeAutomation"] = "Ouvrir PayPal.Me",
        ["Support.PayPalMeHelp"] = "Ouvre la page PayPal.Me de MamatLeroy dans le navigateur par défaut.",
        ["Support.Close"] = "Fermer",
        ["Support.CloseAutomation"] = "Fermer la fenêtre de soutien",
        ["Support.OpenErrorTitle"] = "Ouverture de PayPal impossible",
        ["Support.OpenError"] = "La page PayPal.Me n’a pas pu être ouverte dans le navigateur.",
        ["Support.Reminder"] = "DCE vous fait gagner du temps ? Vous pouvez soutenir son développement, sans aucune obligation.",
        ["Support.Later"] = "Pas maintenant",
        ["Support.Never"] = "Ne plus afficher",
        ["Support.Tooltip"] = "Présente les moyens facultatifs de soutenir ce projet gratuit.",
        ["Support.ReminderTooltip"] = "Affiche les moyens facultatifs de soutenir le projet.",
        ["Support.LaterTooltip"] = "Masque ce rappel pendant 20 lancements.",
        ["Support.NeverTooltip"] = "Désactive définitivement ce rappel local.",

        ["Validation.Center.Title"] = "Centre de validation",
        ["Validation.Center.Scope"] = "Validation interne DCE : structure XML et cohérence hors ligne. Le comportement matériel et réseau reste à contrôler dans Dante Controller.",
        ["Validation.Center.NoSelection"] = "Sélectionnez un résultat pour afficher son détail technique.",
        ["Validation.Center.NoIssue"] = "Aucune anomalie n’est visible avec les filtres actuels.",
        ["Validation.Center.Summary"] = "{0} résultat(s) : {1} erreur(s), {2} avertissement(s), {3} information(s).",
        ["Validation.Center.Profile"] = "Profil XML : {0} | Reconnaissance : {1} | Accès : {2} | Version : {3}",
        ["Validation.Center.FilterLabel"] = "Filtre de validation",
        ["Validation.Center.SearchLabel"] = "Rechercher dans les résultats",
        ["Validation.Center.SearchTooltip"] = "Filtre les messages, catégories, machines et chemins XML.",
        ["Validation.Center.EmptyErrorCount"] = "0 erreur bloquante",
        ["Validation.Center.EmptyWarningCount"] = "0 avertissement",
        ["Validation.Center.EmptyInformationCount"] = "0 information",
        ["Validation.Center.GridTooltip"] = "Sélectionnez un résultat pour afficher ses détails. Double-cliquez pour ouvrir l’élément concerné.",
        ["Validation.Center.LevelColumn"] = "Niveau",
        ["Validation.Center.TargetColumn"] = "Élément concerné",
        ["Validation.Center.XmlPathColumn"] = "Chemin XML",
        ["Validation.Center.DetailTitle"] = "Détail du résultat",
        ["Validation.Center.OpenTarget"] = "Ouvrir l’élément",
        ["Validation.Center.OpenTargetTooltip"] = "Ouvre la machine, le canal ou le patch associé au résultat.",
        ["Validation.Center.Export"] = "Exporter le rapport",
        ["Validation.Center.ExportTooltip"] = "Exporte le résultat complet de la validation interne DCE.",
        ["Validation.Count.Errors"] = "{0} erreur(s) bloquante(s)",
        ["Validation.Count.Warnings"] = "{0} avertissement(s)",
        ["Validation.Count.Information"] = "{0} information(s)",
        ["Validation.Recognition.Unknown"] = "Inconnue",
        ["Validation.Recognition.Partial"] = "Partielle",
        ["Validation.Recognition.Complete"] = "Complète",
        ["Validation.Access.ReadOnly"] = "Lecture seule",
        ["Validation.Access.Restricted"] = "Restreint",
        ["Validation.Access.Full"] = "Complet",
        ["Validation.Severity.Error"] = "Erreur bloquante",
        ["Validation.Severity.Warning"] = "Avertissement",
        ["Validation.Severity.Information"] = "Information",
        ["Validation.Category.XmlCompatibility"] = "Compatibilité XML",
        ["Validation.Category.SaveSafety"] = "Sécurité de sauvegarde",
        ["Validation.Category.Patch"] = "Patch",
        ["Validation.Category.Device"] = "Machine",
        ["Validation.Category.Channel"] = "Canal",
        ["Validation.Category.Clock"] = "Horloge",
        ["Validation.Category.Network"] = "Réseau",
        ["Validation.Category.AudioFormat"] = "Format audio",
        ["Validation.Category.XmlProfile"] = "Profil XML",
        ["Validation.Category.Capabilities"] = "Capacités",
        ["Validation.Category.ExternalValidation"] = "Contrôle externe",
        ["Validation.Legacy.Error"] = "Une incohérence bloquante a été détectée. Consultez le détail technique.",
        ["Validation.Legacy.Warning"] = "Un point mérite une vérification. Consultez le détail technique.",
        ["Validation.Legacy.Information"] = "Information issue de l’analyse du projet.",
        ["Validation.Profile.SaveDisabled"] = "La structure XML n’est pas reconnue avec assez de certitude pour autoriser la sauvegarde.",
        ["Validation.Profile.Restricted"] = "Le fichier est reconnu partiellement : certaines opérations sont volontairement limitées.",
        ["Validation.Profile.Complete"] = "Le fichier correspond au profil XML complet reconnu par DCE.",
        ["Validation.Profile.CapabilitiesLimited"] = "Certaines capacités ne sont pas disponibles pour ce profil XML.",
        ["XmlProfile.RootNotRecognized"] = "La racine XML n’est pas un preset Dante reconnu.",
        ["XmlProfile.NoDevice"] = "Aucune machine Dante n’a été trouvée dans le preset.",
        ["XmlProfile.SomeDevicesHaveNoVisibleName"] = "Certaines machines ne possèdent pas de nom visible exploitable.",
        ["XmlProfile.SomeChannelsHaveNoDanteIdOrMediaType"] = "Certains canaux ne possèdent pas la forme technique complète attendue.",
        ["Validation.External.DanteController"] = "La validation automatique DCE est terminée. Le comportement matériel, le firmware et le réseau actif ne sont pas simulés.",
        ["Validation.Action.OpenPatch"] = "Ouvrir ce RX dans la vue Patch.",
        ["Validation.Action.OpenMachine"] = "Ouvrir la machine ou le canal concerné.",
        ["Validation.Action.InspectXml"] = "Contrôler la structure du XML source avant toute sauvegarde.",
        ["Validation.Action.ReviewChecklist"] = "Utiliser la checklist Dante Controller pour le contrôle final.",
        ["Validation.Action.None"] = "Aucune correction automatique sûre n’est proposée.",
        ["Validation.Report.Title"] = "RAPPORT DU CENTRE DE VALIDATION DCE",
        ["Validation.Report.Internal"] = "VALIDATION INTERNE DCE",
        ["Validation.Report.External"] = "ÉLÉMENTS HORS VALIDATION AUTOMATIQUE",
        ["Validation.Report.ExternalDetail"] = "Disponibilité matérielle, compatibilité firmware et comportement du réseau actif. Le fichier de référence 3.6 a été testé avec Dante Controller ; chaque projet modifié doit néanmoins être contrôlé avant exploitation.",
        ["Validation.Report.Exported"] = "Rapport de validation exporté.",

        ["Blank"] = "(vide)"
    };

    private static readonly Dictionary<string, string> English = new(StringComparer.Ordinal)
    {
        ["Language.French"] = "French",
        ["Language.English"] = "English",
        ["Language.Label"] = "Language",

        ["Search.Hint"] = "Type at least 2 characters to search",
        ["Search.NoFileLoaded"] = "Load an XML file to search",
        ["Search.NoResult"] = "No result",

        ["Filter.AllSenders"] = "All transmitters",
        ["Filter.AllReceivers"] = "All receivers",
        ["Filter.AllRx"] = "All Rx channels",
        ["Filter.ActivePatches"] = "Active subscriptions",
        ["Filter.FreeRx"] = "Free Rx",
        ["Filter.LocalPatches"] = "Local subscriptions",
        ["Filter.MissingTxDevices"] = "Missing Tx devices",
        ["Filter.MissingTxChannels"] = "Missing Tx channels",
        ["Filter.Warnings"] = "Warnings",
        ["Filter.HealthWarnings"] = "Warnings",
        ["Filter.Conflicts"] = "Conflicts",
        ["Filter.Modified"] = "Modified",
        ["Filter.All"] = "All",
        ["Filter.Info"] = "Info",
        ["Filter.Errors"] = "Errors",
        ["Filter.Patches"] = "Subscriptions",
        ["Filter.Devices"] = "Devices",
        ["Filter.Clock"] = "Clock",
        ["Filter.Network"] = "Network",
        ["Filter.XmlCompatibility"] = "XML compatibility",
        ["Filter.WarningsConflicts"] = "Warnings / conflicts",

        ["DeviceFilter.All"] = "All",
        ["DeviceFilter.Locked"] = "Locked",
        ["DeviceFilter.StaticIp"] = "Static IPs",
        ["DeviceFilter.PreferredMaster"] = "Preferred Masters",
        ["DeviceFilter.Redundant"] = "Redundant",
        ["DeviceFilter.Daisychain"] = "Daisychain",
        ["DeviceFilter.NoTx"] = "No Tx",
        ["DeviceFilter.NoRx"] = "No Rx",
        ["DeviceFilter.Modified"] = "Modified only",
        ["DeviceFilter.WarningSelection"] = "Selected warning",
        ["DeviceFilter.SampleRateDifferent"] = "Different sample rate",
        ["DeviceFilter.EncodingDifferent"] = "Different bit depth",
        ["Target.AllUnlocked"] = "All unlocked",
        ["Target.SelectedUnlocked"] = "Selected unlocked",
        ["Target.FilteredUnlocked"] = "Unlocked devices in current filter",

        ["PatchView.Simple"] = "Simple",
        ["PatchView.Expert"] = "Expert",

        ["Status.Ready"] = "Ready",
        ["Status.FileLoaded"] = "File loaded. Changes must be saved under a new name.",
        ["Status.EditEnabled"] = "Edit mode enabled.",
        ["Status.FileSaved"] = "File saved.",
        ["Status.LastActionUndone"] = "Last action undone.",
        ["Status.TopologyDisplayed"] = "Simple topology displayed.",
        ["Status.TxtExported"] = "TXT report exported.",
        ["Status.PdfExported"] = "PDF report exported.",
        ["Status.PatchbookTxtExported"] = "Patchbook TXT exported.",
        ["Status.PatchbookCsvExported"] = "Patchbook CSV exported.",
        ["Status.EditMode"] = "Mode: Edit",
        ["Status.ReadOnlyMode"] = "Mode: Read-only",
        ["Status.NoFileLoaded"] = "No file loaded.",
        ["Status.NoFileOpen"] = "No file open",
        ["Status.Unmodified"] = "Unmodified",
        ["Status.ModifiedUnsaved"] = "Modified - not saved",
        ["Status.NoDeviceSettingsChanged"] = "No device settings to apply.",
        ["Status.RecoveryRestored"] = "Automatic session recovered - unsaved changes.",
        ["Status.NoImportantWarning"] = "No important item to check.",
        ["Status.WarningDevicesDisplayed"] = "{0} affected device(s) displayed.",
        ["Status.ProfileAlreadyApplied"] = "The profile is already applied to the whole target.",
        ["Status.EditActiveButton"] = "Edit active",
        ["Status.ActivateEditButton"] = "Enable editing",
        ["Status.LoadXmlToStart"] = "Load an XML file to begin.",

        ["Action.DeviceRenamed"] = "Device name updated.",
        ["Action.NetworkModeUpdated"] = "Network mode updated.",
        ["Action.LatencyUpdated"] = "Latency updated.",
        ["Action.SampleRateUpdated"] = "Sample rate updated.",
        ["Action.EncodingUpdated"] = "Bits per sample updated.",
        ["Action.IpAutoApplied"] = "Automatic IP applied.",
        ["Action.IpStaticApplied"] = "Static IP applied.",
        ["Action.DevicePatchesReset"] = "Device Rx/Tx subscriptions reset.",
        ["Action.DeviceRxPatchesReset"] = "Device Rx subscriptions reset.",
        ["Action.DeviceTxPatchesReset"] = "Device Tx subscriptions reset.",
        ["Action.DeviceDetailsUpdated"] = "Device details updated.",
        ["Action.DeviceSettingsUpdated"] = "Device settings updated.",
        ["Action.QuickProfileApplied"] = "Quick profile applied.",
        ["Action.PreferredMasterUpdated"] = "Preferred Master updated.",
        ["Action.ChannelsReset"] = "Channels reset.",
        ["Action.ChannelRenamed"] = "Channel renamed.",
        ["Action.BatchRenameApplied"] = "Batch rename applied.",
        ["Action.AllNetworkModesApplied"] = "Network mode applied to all devices.",
        ["Action.AllLatenciesApplied"] = "Latency applied to all devices.",
        ["Action.AllSampleRatesApplied"] = "Sample rate applied to all devices.",
        ["Action.AllEncodingsApplied"] = "Bits per sample applied to all devices.",
        ["Action.AllIpAutoApplied"] = "Automatic IP applied to all devices.",
        ["Action.AllIpStaticApplied"] = "Static IP range applied.",
        ["Action.AllChannelsReset"] = "All channels have been reset.",
        ["Action.PatchApplied"] = "Subscription applied.",
        ["Action.PatchRemoved"] = "Subscription removed.",
        ["Action.VisualPatchesApplied"] = "{0} visual subscription change(s) applied.",
        ["Action.TxChannelRenamed"] = "Tx channel renamed and subscriptions updated.",
        ["Action.RxChannelRenamed"] = "Rx channel renamed.",
        ["Action.DeviceDeleted"] = "Device deleted.",
        ["Action.XmlMerged"] = "XML added to project.",
        ["Action.AtomicChaosApplied"] = "Atomic exercise generated - not saved.",

        ["Dialog.ConfirmTitle"] = "Confirmation required",
        ["Dialog.OpenXmlTitle"] = "Open a Dante configuration",
        ["Dialog.MergeXmlTitle"] = "Add XML to the open project",
        ["Dialog.SaveXmlTitle"] = "Save a new configuration",
        ["Dialog.ExportTxtTitle"] = "Export TXT report",
        ["Dialog.ExportPdfTitle"] = "Export PDF report",
        ["Dialog.ExportPatchbookTxtTitle"] = "Export patchbook TXT",
        ["Dialog.ExportPatchbookCsvTitle"] = "Export patchbook CSV",
        ["Dialog.XmlFilter"] = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
        ["Dialog.TxtFilter"] = "Text report (*.txt)|*.txt|All files (*.*)|*.*",
        ["Dialog.PdfFilter"] = "PDF report (*.pdf)|*.pdf|All files (*.*)|*.*",
        ["Dialog.PatchbookTxtFilter"] = "Patchbook text (*.txt)|*.txt|All files (*.*)|*.*",
        ["Dialog.PatchbookCsvFilter"] = "Patchbook CSV (*.csv)|*.csv|All files (*.*)|*.*",
        ["Dialog.NoRecentFileTitle"] = "No recent file",
        ["Dialog.NoRecentFileMessage"] = "Select a recent file to open.",
        ["Dialog.FileMissingTitle"] = "File not found",
        ["Dialog.FileMissingMessage"] = "This recent file no longer exists.",
        ["Dialog.OpenFailedTitle"] = "Unable to open file",
        ["Dialog.SaveImpossibleTitle"] = "Save unavailable",
        ["Dialog.ChooseAnotherNameTitle"] = "Choose another name",
        ["Dialog.ChooseAnotherNameMessage"] = "To protect the original XML, the application does not save over the opened file. Choose a new file name.",
        ["Dialog.OverwriteMessage"] = "This file already exists. Do you really want to overwrite it?",
        ["Dialog.SaveSummaryTitle"] = "Summary before saving",
        ["Dialog.OriginalBackupMessage"] = "A backup of the original file will be created before writing. Continue?",
        ["Dialog.SaveErrorTitle"] = "Error while saving",
        ["Dialog.RevertTitle"] = "Revert changes",
        ["Dialog.RevertMessage"] = "Unsaved changes will be lost. Continue?",
        ["Dialog.ReloadErrorTitle"] = "Unable to reload the original file",
        ["Dialog.UndoErrorTitle"] = "Undo unavailable",
        ["Dialog.NoChannelTitle"] = "No channel selected",
        ["Dialog.NoChannelMessage"] = "Select a Tx or Rx channel to rename.",
        ["Dialog.InvalidRangeTitle"] = "Invalid range",
        ["Dialog.InvalidRangeMessage"] = "Select a start channel and an end channel.",
        ["Dialog.InvalidRangeOrderMessage"] = "The end channel must be after the start channel.",
        ["Dialog.InvalidNumberTitle"] = "Invalid number",
        ["Dialog.InvalidNumberMessage"] = "Enter a valid starting number.",
        ["Dialog.NoRxTitle"] = "No Rx channel selected",
        ["Dialog.NoRxMessage"] = "Select a row in the patch table.",
        ["Dialog.NoRxLineMessage"] = "Select an Rx row in the patch table.",
        ["Dialog.MissingTxTitle"] = "Missing Tx channel",
        ["Dialog.MissingTxMessage"] = "Select a Tx device and a Tx channel in the patch area.",
        ["Dialog.ExportImpossibleTitle"] = "Export unavailable",
        ["Dialog.ExportPatchbookImpossibleTitle"] = "Patchbook export unavailable",
        ["Dialog.ExportPatchbookCsvImpossibleTitle"] = "Patchbook CSV export unavailable",
        ["Dialog.ActionImpossibleTitle"] = "Action unavailable",
        ["Dialog.NoFileLoadedTitle"] = "No file loaded",
        ["Dialog.NoFileLoadedMessage"] = "Open a Dante configuration XML file first.",
        ["Dialog.NoDeviceSelectedTitle"] = "No device selected",
        ["Dialog.NoDeviceSelectedMessage"] = "Select one or several devices.",
        ["Dialog.SynopticLayoutErrorTitle"] = "Synoptic layout unavailable",
        ["Dialog.DeleteDeviceWarning"] = "Device '{0}' will be deleted from the project. Subscriptions/patches pointing to this device will also be removed. Continue?",
        ["Dialog.ResetDevicePatchesWarning"] = "The Rx inputs of device '{0}' will be disconnected, and all subscriptions using its Tx channels will be removed. Continue?",
        ["Dialog.MergeXmlWarning"] = "Devices from the selected XML file will be added to the current project. Device names that already exist will be rejected. Continue?",
        ["DuplicateDialog.Title"] = "Duplicate devices",
        ["DuplicateDialog.Intro"] = "Some devices from the imported XML already exist in the open project. You can import only non-duplicate devices, automatically rename duplicates, or choose the new names manually.",
        ["DuplicateDialog.OriginalName"] = "Name in imported XML",
        ["DuplicateDialog.NewName"] = "New name to import",
        ["DuplicateDialog.Suffix"] = "Automatic rename suffix",
        ["DuplicateDialog.UniqueOnly"] = "Import unique only",
        ["DuplicateDialog.AutoRename"] = "Auto rename",
        ["DuplicateDialog.ManualRename"] = "Import with these names",
        ["DuplicateDialog.Cancel"] = "Cancel",
        ["DuplicateDialog.InvalidTitle"] = "Invalid names",
        ["DuplicateDialog.EmptyName"] = "Each renamed device must have a new name.",
        ["DuplicateDialog.DuplicateNewName"] = "Two imported devices cannot receive the same new name.",
        ["Dialog.LatencyWarning"] = "Changing Dante latency may reconfigure flows when the preset is imported/applied in Dante tools. Always verify the preset in Dante Controller.",
        ["Dialog.LatencyWarningContinue"] = "Changing Dante latency may reconfigure flows when the preset is imported/applied in Dante tools. Continue?",
        ["Dialog.AudioFormatWarning"] = "Changing sample rate or bits per sample may make some devices incompatible if they do not support that value. Always verify the preset in Dante Controller.",
        ["Dialog.AudioFormatWarningContinue"] = "Changing sample rate or bits per sample may make some devices incompatible if they do not support that value. Continue?",
        ["Dialog.IpStaticWarning"] = "Setting a static IP can break communication if the address, netmask, or gateway is wrong. Always verify the preset in Dante Controller.",
        ["Dialog.IpStaticWarningContinue"] = "Setting static IPs can break communication if the range, netmask, or gateway is wrong. Continue?",
        ["Dialog.DeviceDetailsWarning"] = "This device sheet can change the device name, formats, IP address, and channels. Continue?",
        ["Dialog.AtomicChaosTitle"] = "Atomic Bomb - troubleshooting exercise",
        ["Dialog.AtomicChaosNothingSelected"] = "Select at least one category to sabotage. Even an educational bomb needs a target.",
        ["Dialog.AtomicChaosCompleted"] = "Atomic scenario created (seed {0}). {1} device(s), {2} Tx channels, {3} patched Rx, {4} free Rx, {5} static IP(s), {6} automatic IP(s). The original XML was not modified. Use Save As to keep the exercise.",
        ["Dialog.RecoveryTitle"] = "Session recovery",
        ["Dialog.RecoveryFound"] = "An unsaved automatic copy from {0:g} was found. Do you want to recover it?\n\nNo will delete this temporary copy and open the original XML.",
        ["Dialog.RecoverySourceChanged"] = "Warning: the original XML file has changed since this recovery. Carefully review the differences before saving.",
        ["Dialog.NoDeviceChanges"] = "No device, channel, or subscription change is detected since the XML was opened.",
        ["Dialog.DeviceChangesTitle"] = "Before / after changes",
        ["Dialog.SelectProfile"] = "Select a quick profile.",
        ["Dialog.ProfileWarningContinue"] = "This profile can change several audio and network settings in one operation. Review the preview and validate the final XML in Dante Controller. Continue?",
        ["Dialog.ResetDeviceChannelsWarning"] = "The selected device channel names will be replaced by 1, 2, 3...",
        ["Dialog.BatchRenameWarning"] = "Channel names {0} {1} to {2} will be replaced in a batch rename. Continue?",
        ["Dialog.Continue"] = "Continue?",
        ["Dialog.RemovePatchWarning"] = "The selected Rx channel subscription will be removed.",
        ["Dialog.ExternalPatchWarning"] = "This subscription points to a device that is not present in the preset. This may be normal if the Dante preset is partial. Only modify it if you are sure you want to replace this source. Continue?",
        ["Dialog.ExternalPatchStatus"] = "This subscription points to a device that is missing from the preset. This may be normal if the Dante preset is partial.",

        ["Log.FileLoaded"] = "File loaded: {0}",
        ["Log.EditEnabled"] = "Edit mode enabled.",
        ["Log.EditEnabledAuto"] = "Edit mode enabled automatically.",
        ["Log.OriginalBackupCreated"] = "Original backup created: {0}",
        ["Log.FileSaved"] = "File saved: {0}",
        ["Log.ReloadOriginal"] = "Changes reverted. Original file reloaded.",
        ["Log.ActionUndone"] = "Action undone: {0}",
        ["Log.RecoveryRestored"] = "Automatic session recovered.",
        ["Log.RecoveryUnavailable"] = "Automatic recovery unavailable: {0}",
        ["Log.TxtExported"] = "TXT report exported: {0}",
        ["Log.PdfExported"] = "PDF report exported: {0}",
        ["Log.PatchbookTxtExported"] = "Patchbook TXT exported: {0}",
        ["Log.PatchbookCsvExported"] = "Patchbook CSV exported: {0}",
        ["Log.XmlMerged"] = "XML added to project: {0}",

        ["Summary.PatchRows"] = "{0} rows - {1} active - {2} local - {3} warning(s) - {4} conflict(s)",
        ["Summary.Health"] = "Preset: {0}  |  Version: {1}  |  Mode: {2}  |  File: {3}\nDevices: {4}  |  TX: {5}  |  RX: {6}  |  Active subscriptions: {7}  |  Free RX: {8}\nLocal subscriptions: {9}  |  Missing TX devices: {10}  |  Missing TX channels: {11}  |  Preferred Masters: {12}\nSample rates: {13}  |  Encoding: {14}  |  Latencies: {15}\nRedundant: {16}  |  Daisy-chain: {17}  |  Static IPs detected: {18}  |  Errors: {19}  |  Warnings: {20}",
        ["Profile.48k24b1msAuto"] = "48 kHz / 24 bit / 1 ms / automatic IP",
        ["Profile.48k24b2msAuto"] = "48 kHz / 24 bit / 2 ms / automatic IP",
        ["Profile.96k24b1msAuto"] = "96 kHz / 24 bit / 1 ms / automatic IP",
        ["Profile.96k24b2msAuto"] = "96 kHz / 24 bit / 2 ms / automatic IP",
        ["Profile.48k24b1msRedundant"] = "48 kHz / 24 bit / 1 ms / Redundant / automatic IP",
        ["Profile.48k24b1msDaisychain"] = "48 kHz / 24 bit / 1 ms / Daisy-chain / automatic IP",

        ["ChannelLabel.Warning.MoreThanEightCharacters"] = "more than 8 characters",
        ["ChannelLabel.Warning.UnsupportedDmtCharacters"] = "characters not supported by DMT/dLive",
        ["Ui.Placeholder.RecentFiles"] = "Recent files",

        ["Support.Title"] = "Support DCE",
        ["Support.Subtitle"] = "A completely optional way to support this free project.",
        ["Support.Free"] = "Dante Config Editor remains completely free, and every feature is available without contributing.",
        ["Support.PayPalDescription"] = "Scan this QR code with the PayPal app on your phone. DCE does not process payments or receive financial information.",
        ["Support.OtherWays"] = "You can also help for free by starring the GitHub project or sharing feedback. And if you are truly crazy, you can even do both!",
        ["Support.PayPalMeButton"] = "Open PayPal on this computer",
        ["Support.PayPalMeAutomation"] = "Open PayPal.Me",
        ["Support.PayPalMeHelp"] = "Opens MamatLeroy's PayPal.Me page in the default browser.",
        ["Support.Close"] = "Close",
        ["Support.CloseAutomation"] = "Close the support window",
        ["Support.OpenErrorTitle"] = "Unable to open PayPal",
        ["Support.OpenError"] = "The PayPal.Me page could not be opened in the browser.",
        ["Support.Reminder"] = "Does DCE save you time? You can support its development, with no obligation.",
        ["Support.Later"] = "Not now",
        ["Support.Never"] = "Do not show again",
        ["Support.Tooltip"] = "Shows optional ways to support this free project.",
        ["Support.ReminderTooltip"] = "Shows optional ways to support the project.",
        ["Support.LaterTooltip"] = "Hides this reminder for 20 launches.",
        ["Support.NeverTooltip"] = "Permanently disables this local reminder.",

        ["Validation.Center.Title"] = "Validation center",
        ["Validation.Center.Scope"] = "DCE internal validation: offline XML structure and consistency. Hardware and live-network behavior must still be reviewed in Dante Controller.",
        ["Validation.Center.NoSelection"] = "Select a result to display its technical details.",
        ["Validation.Center.NoIssue"] = "No issue is visible with the current filters.",
        ["Validation.Center.Summary"] = "{0} result(s): {1} error(s), {2} warning(s), {3} information item(s).",
        ["Validation.Center.Profile"] = "XML profile: {0} | Recognition: {1} | Access: {2} | Version: {3}",
        ["Validation.Center.FilterLabel"] = "Validation filter",
        ["Validation.Center.SearchLabel"] = "Search results",
        ["Validation.Center.SearchTooltip"] = "Filters messages, categories, devices and XML paths.",
        ["Validation.Center.EmptyErrorCount"] = "0 blocking errors",
        ["Validation.Center.EmptyWarningCount"] = "0 warnings",
        ["Validation.Center.EmptyInformationCount"] = "0 information items",
        ["Validation.Center.GridTooltip"] = "Select a result to display its details. Double-click to open the affected item.",
        ["Validation.Center.LevelColumn"] = "Level",
        ["Validation.Center.TargetColumn"] = "Affected item",
        ["Validation.Center.XmlPathColumn"] = "XML path",
        ["Validation.Center.DetailTitle"] = "Result details",
        ["Validation.Center.OpenTarget"] = "Open item",
        ["Validation.Center.OpenTargetTooltip"] = "Opens the device, channel or subscription associated with the result.",
        ["Validation.Center.Export"] = "Export report",
        ["Validation.Center.ExportTooltip"] = "Exports the complete DCE internal validation result.",
        ["Validation.Count.Errors"] = "{0} blocking error(s)",
        ["Validation.Count.Warnings"] = "{0} warning(s)",
        ["Validation.Count.Information"] = "{0} information item(s)",
        ["Validation.Recognition.Unknown"] = "Unknown",
        ["Validation.Recognition.Partial"] = "Partial",
        ["Validation.Recognition.Complete"] = "Complete",
        ["Validation.Access.ReadOnly"] = "Read-only",
        ["Validation.Access.Restricted"] = "Restricted",
        ["Validation.Access.Full"] = "Full",
        ["Validation.Severity.Error"] = "Blocking error",
        ["Validation.Severity.Warning"] = "Warning",
        ["Validation.Severity.Information"] = "Information",
        ["Validation.Category.XmlCompatibility"] = "XML compatibility",
        ["Validation.Category.SaveSafety"] = "Save safety",
        ["Validation.Category.Patch"] = "Subscription",
        ["Validation.Category.Device"] = "Device",
        ["Validation.Category.Channel"] = "Channel",
        ["Validation.Category.Clock"] = "Clock",
        ["Validation.Category.Network"] = "Network",
        ["Validation.Category.AudioFormat"] = "Audio format",
        ["Validation.Category.XmlProfile"] = "XML profile",
        ["Validation.Category.Capabilities"] = "Capabilities",
        ["Validation.Category.ExternalValidation"] = "External review",
        ["Validation.Legacy.Error"] = "A blocking consistency issue was detected. See the technical details.",
        ["Validation.Legacy.Warning"] = "A point requires review. See the technical details.",
        ["Validation.Legacy.Information"] = "Information produced by the project analysis.",
        ["Validation.Profile.SaveDisabled"] = "The XML structure is not recognized with enough confidence to allow saving.",
        ["Validation.Profile.Restricted"] = "The file is only partially recognized, so some operations are intentionally limited.",
        ["Validation.Profile.Complete"] = "The file matches DCE’s complete recognized XML profile.",
        ["Validation.Profile.CapabilitiesLimited"] = "Some capabilities are unavailable for this XML profile.",
        ["XmlProfile.RootNotRecognized"] = "The XML root is not a recognized Dante preset.",
        ["XmlProfile.NoDevice"] = "No Dante device was found in the preset.",
        ["XmlProfile.SomeDevicesHaveNoVisibleName"] = "Some devices do not expose a usable visible name.",
        ["XmlProfile.SomeChannelsHaveNoDanteIdOrMediaType"] = "Some channels do not expose the expected complete technical shape.",
        ["Validation.External.DanteController"] = "DCE automatic validation is complete. Hardware behavior, firmware and the live network are not simulated.",
        ["Validation.Action.OpenPatch"] = "Open this Rx channel in the Patch view.",
        ["Validation.Action.OpenMachine"] = "Open the affected device or channel.",
        ["Validation.Action.InspectXml"] = "Review the source XML structure before saving.",
        ["Validation.Action.ReviewChecklist"] = "Use the Dante Controller checklist for the final review.",
        ["Validation.Action.None"] = "No safe automatic correction is proposed.",
        ["Validation.Report.Title"] = "DCE VALIDATION CENTER REPORT",
        ["Validation.Report.Internal"] = "DCE INTERNAL VALIDATION",
        ["Validation.Report.External"] = "ITEMS OUTSIDE AUTOMATIC VALIDATION",
        ["Validation.Report.ExternalDetail"] = "Hardware availability, firmware compatibility and live-network behavior. The 3.6 reference file was tested with Dante Controller; each modified project should nevertheless be reviewed before production use.",
        ["Validation.Report.Exported"] = "Validation report exported.",

        ["Blank"] = "(empty)"
    };

    private static readonly Dictionary<string, string> LiteralFrenchToEnglish = BuildLiteralMap();
    private static readonly Dictionary<string, string> LiteralEnglishToFrench = BuildInverseLiteralMap();

    public static string Text(UiLanguage language, string key)
    {
        Dictionary<string, string> dictionary = language == UiLanguage.English ? English : French;
        return dictionary.TryGetValue(key, out string? value) ? value : key;
    }

    public static string Format(UiLanguage language, string key, params object[] args)
    {
        return string.Format(Text(language, key), args);
    }

    public static string TranslateLiteral(UiLanguage language, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (language == UiLanguage.English && LiteralFrenchToEnglish.TryGetValue(value, out string? english))
        {
            return english;
        }

        if (language == UiLanguage.French && LiteralEnglishToFrench.TryGetValue(value, out string? french))
        {
            return french;
        }

        return value;
    }

    public static string TranslateHistoryDetail(UiLanguage language, string value)
    {
        string exact = TranslateLiteral(language, value);
        if (language != UiLanguage.English || !string.Equals(exact, value, StringComparison.Ordinal))
        {
            return exact;
        }

        string translated = value;
        translated = System.Text.RegularExpressions.Regex.Replace(
            translated,
            @"^(\d+) machine\(s\) ajoutée\(s\) depuis (.+)$",
            "$1 device(s) added from $2");
        translated = System.Text.RegularExpressions.Regex.Replace(
            translated,
            @"^(\d+) machine\(s\) passée\(s\) en dynamique$",
            "$1 device(s) switched to automatic IP");
        translated = System.Text.RegularExpressions.Regex.Replace(
            translated,
            @"^(\d+) machine\(s\) depuis (.+), (\d+) ignorée\(s\) sans interface IPv4$",
            "$1 device(s) starting at $2, $3 skipped without an IPv4 interface");
        translated = System.Text.RegularExpressions.Regex.Replace(
            translated,
            @"^(.+) appliqué à (\d+) machine\(s\)$",
            "$1 applied to $2 device(s)");
        translated = System.Text.RegularExpressions.Regex.Replace(
            translated,
            @"^(.+) supprimé, (\d+) patch\(s\) nettoyé\(s\)$",
            "$1 deleted, $2 subscription(s) removed");
        translated = System.Text.RegularExpressions.Regex.Replace(
            translated,
            @"^(.+): (\d+) entrée\(s\) RX et (\d+) départ\(s\) TX supprimé\(s\)$",
            "$1: $2 Rx input(s) and $3 Tx route(s) removed");
        translated = System.Text.RegularExpressions.Regex.Replace(
            translated,
            @"^(.+): (\d+) entrée\(s\) RX supprimée\(s\)$",
            "$1: $2 Rx input(s) disconnected");
        translated = System.Text.RegularExpressions.Regex.Replace(
            translated,
            @"^(.+): (\d+) départ\(s\) TX supprimé\(s\)$",
            "$1: $2 Tx route(s) removed");
        translated = System.Text.RegularExpressions.Regex.Replace(
            translated,
            @"^Réinitialisation des canaux de (.+)$",
            "Channels reset for $1");
        translated = System.Text.RegularExpressions.Regex.Replace(
            translated,
            @"^(.+) (TX|RX): canaux (\d+)-(\d+), (.+) depuis (\d+)$",
            "$1 $2: channels $3-$4, $5 starting at $6");
        translated = System.Text.RegularExpressions.Regex.Replace(
            translated,
            @"^(\d+) label\(s\) de canal appliqué\(s\)$",
            "$1 channel label(s) applied");
        translated = System.Text.RegularExpressions.Regex.Replace(
            translated,
            @"^Fichier sauvegardé sous (.+)$",
            "File saved as $1");
        translated = System.Text.RegularExpressions.Regex.Replace(
            translated,
            @"^Créé depuis le modèle (.+), format preset (.+)$",
            "Created from template $1, preset format $2");

        return translated
            .Replace("Tous redondants", "All devices redundant", StringComparison.Ordinal)
            .Replace("Tous en daisychain", "All devices in daisy-chain mode", StringComparison.Ordinal)
            .Replace("Tous ->", "All ->", StringComparison.Ordinal)
            .Replace("Réinitialisation des canaux de tous les devices", "Channels reset for all devices", StringComparison.Ordinal)
            .Replace("Channels reset for tous les devices", "Channels reset for all devices", StringComparison.Ordinal)
            .Replace(" est le seul Preferred Master", " is the only Preferred Master", StringComparison.Ordinal)
            .Replace(" déjà en automatique ou sans adresse IPv4 modifiable", " already uses automatic IP or has no editable IPv4 address", StringComparison.Ordinal)
            .Replace(" -> dynamique", " -> automatic IP", StringComparison.Ordinal)
            .Replace(" -> redondant", " -> redundant", StringComparison.Ordinal)
            .Replace("identité matérielle neutralisée", "hardware identity neutralized", StringComparison.Ordinal)
            .Replace("abonnement(s) recopié(s)", "subscription(s) copied", StringComparison.Ordinal)
            .Replace("machine(s)", "device(s)", StringComparison.Ordinal)
            .Replace("RX patché(s)", "Rx patched", StringComparison.Ordinal)
            .Replace("RX libre(s)", "Rx disconnected", StringComparison.Ordinal);
    }

    public static string TranslateValidationMessage(UiLanguage language, string value)
    {
        string exact = TranslateLiteral(language, value);
        if (language != UiLanguage.English || !string.Equals(exact, value, StringComparison.Ordinal))
        {
            return exact;
        }

        string? fixedTranslation = value switch
        {
            "Compatibilité XML : aucune modification interdite détectée." =>
                "XML compatibility: no prohibited change detected.",
            "Le document XML ne contient pas de racine." =>
                "The XML document has no root element.",
            "Le document XML ne contient plus de racine." =>
                "The XML document no longer has a root element.",
            "La racine XML doit rester <preset> pour un preset Dante Controller." =>
                "The XML root must remain <preset> for a Dante Controller preset.",
            "Le namespace XML du preset a été modifié." =>
                "The preset XML namespace was modified.",
            "La déclaration XML d'origine n'est plus présente." =>
                "The original XML declaration is no longer present.",
            "Aucun device Dante n'a été détecté." or
            "Aucun device Dante n'est présent dans le XML." =>
                "No Dante device was found in the XML.",
            "Un device a un nom vide." =>
                "A device has an empty name.",
            "Aucune machine preferred master n'est déclarée." =>
                "No device is declared as Preferred Master.",
            "Sauvegarde refusée : la racine <preset> est absente." =>
                "Save blocked: the <preset> root element is missing.",
            "Sauvegarde refusée : le namespace XML de la racine a été modifié." =>
                "Save blocked: the root XML namespace was modified.",
            "Conflit - abonnement incomplet" =>
                "Conflict - incomplete subscription",
            _ => null
        };
        if (fixedTranslation is not null)
        {
            return fixedTranslation;
        }

        string translated = ReplaceValidationPattern(
            value,
            @"^ATTENTION : le fichier mélange (\d+) machine\(s\) en redondant et (\d+) machine\(s\) en daisychain\. Vérifiez que c'est volontaire pour ce réseau\.$",
            "WARNING: the file mixes $1 redundant device(s) and $2 daisy-chain device(s). Check that this is intentional for this network.");
        if (!string.Equals(translated, value, StringComparison.Ordinal))
        {
            return translated;
        }

        translated = ReplaceValidationPattern(
            value,
            @"^IP fixe détectée sur (\d+) machine\(s\) : (.+)\.$",
            "Static IP detected on $1 device(s): $2.");
        if (!string.Equals(translated, value, StringComparison.Ordinal))
        {
            return translated;
        }

        translated = ReplaceValidationPattern(
            value,
            @"^ATTENTION : plusieurs fréquences d'échantillonnage sont présentes dans le preset : (.+)\.$",
            "WARNING: multiple sample rates are present in the preset: $1.");
        if (!string.Equals(translated, value, StringComparison.Ordinal))
        {
            return translated;
        }

        translated = ReplaceValidationPattern(
            value,
            @"^ATTENTION : plusieurs bits par échantillon sont présentes dans le preset : (.+)\.$",
            "WARNING: multiple bit depths are present in the preset: $1.");
        if (!string.Equals(translated, value, StringComparison.Ordinal))
        {
            return translated;
        }

        translated = ReplaceValidationPattern(
            value,
            @"^Plusieurs samplerates sont présents dans le preset : (.+)\.$",
            "Multiple sample rates are present in the preset: $1.");
        if (!string.Equals(translated, value, StringComparison.Ordinal))
        {
            return translated;
        }

        translated = ReplaceValidationPattern(
            value,
            @"^Plusieurs encodages sont présents dans le preset : (.+)\.$",
            "Multiple bit depths are present in the preset: $1.");
        if (!string.Equals(translated, value, StringComparison.Ordinal))
        {
            return translated;
        }

        translated = ReplaceValidationPattern(
            value,
            @"^Plusieurs latences sont présentes dans le preset : (.+)\.$",
            "Multiple latencies are present in the preset: $1.");
        if (!string.Equals(translated, value, StringComparison.Ordinal))
        {
            return translated;
        }

        translated = ReplaceValidationPattern(
            value,
            @"^(.+) ne contient aucun canal (TX|RX)\.$",
            "$1 has no $2 channel.");
        if (!string.Equals(translated, value, StringComparison.Ordinal))
        {
            return translated;
        }

        translated = ReplaceValidationPattern(
            value,
            @"^(.+) utilise une source locale '\.'\.$",
            "$1 uses the local source '.'.");
        if (!string.Equals(translated, value, StringComparison.Ordinal))
        {
            return translated;
        }

        translated = ReplaceValidationPattern(
            value,
            @"^(.+) est libre\.$",
            "$1 is unassigned.");
        if (!string.Equals(translated, value, StringComparison.Ordinal))
        {
            return translated;
        }

        translated = ReplaceValidationPattern(
            value,
            @"^(.+) pointe vers un device TX absent du preset : (.+)\.$",
            "$1 references a Tx device missing from the preset: $2.");
        if (!string.Equals(translated, value, StringComparison.Ordinal))
        {
            return translated;
        }

        translated = ReplaceValidationPattern(
            value,
            @"^(.+) pointe vers un canal TX non retrouvé : (.+)\.$",
            "$1 references a Tx channel that could not be found: $2.");
        if (!string.Equals(translated, value, StringComparison.Ordinal))
        {
            return translated;
        }

        translated = ReplaceValidationPattern(
            value,
            @"^(\d+) machines sont déclarées preferred master\.$",
            "$1 devices are declared as Preferred Master.");
        if (!string.Equals(translated, value, StringComparison.Ordinal))
        {
            return translated;
        }

        translated = ReplaceValidationPattern(
            value,
            @"^(.+) contient un doublon de Dante Id (\d+) dans les canaux (TX|RX)\.$",
            "$1 contains duplicate Dante ID $2 among $3 channels.");
        if (!string.Equals(translated, value, StringComparison.Ordinal))
        {
            return translated;
        }

        // Les diagnostics ci-dessous sont moins fréquents, mais ils doivent
        // rester exploitables dans une interface anglaise.
        (string Pattern, string Replacement)[] technicalPatterns =
        [
            (@"^La version du preset doit rester (.+)\.$",
                "The preset version must remain $1."),
            (@"^Le nombre de devices a changé : (\d+) au chargement, (\d+) maintenant\.$",
                "The device count changed: $1 when loaded, $2 now."),
            (@"^Device d'origine absent du XML courant : (.+)\.$",
                "Original device missing from the current XML: $1."),
            (@"^La version de déclaration XML d'origine \((.+)\) n'est pas conservée\.$",
                "The original XML declaration version ($1) was not preserved."),
            (@"^L'encodage XML d'origine \((.+)\) n'est pas conservé\.$",
                "The original XML encoding ($1) was not preserved."),
            (@"^Le standalone XML d'origine \((.+)\) n'est pas conservé\.$",
                "The original XML standalone value ($1) was not preserved."),
            (@"^Balise technique supprimée : <(.+)>\.$",
                "Technical element removed: <$1>."),
            (@"^(.+) : Dante Id (.+) en doublon sur les canaux (TX|RX)\.$",
                "$1: duplicate Dante ID $2 among $3 channels."),
            (@"^(.+) : nombre de canaux (TX|RX) modifié \((\d+) attendu\(s\), (\d+) trouvé\(s\)\)\.$",
                "$1: $2 channel count changed ($3 expected, $4 found)."),
            (@"^(.+) : Dante Id supprimé ou modifié sur (TX|RX) position (\d+)\.$",
                "$1: Dante ID removed or modified at $2 position $3."),
            (@"^(.+) : mediaType supprimé ou modifié sur (TX|RX) (.+)\.$",
                "$1: mediaType removed or modified on $2 $3."),
            (@"^(.+) RX Dante Id (.+) : subscribed_channel renseigné sans subscribed_device\.$",
                "$1 Rx Dante ID $2: subscribed_channel is set without subscribed_device."),
            (@"^(.+) RX Dante Id (.+) : subscribed_device renseigné sans subscribed_channel\.$",
                "$1 Rx Dante ID $2: subscribed_device is set without subscribed_channel."),
            (@"^Le device '(.+)' contient des caractères non imprimables\.$",
                "Device '$1' contains non-printable characters."),
            (@"^(.+) contient un canal (TX|RX) sans nom lisible\.$",
                "$1 contains a $2 channel without a readable name."),
            (@"^(.+) / (.+) contient des caractères non imprimables\.$",
                "$1 / $2 contains non-printable characters."),
            (@"^Le nom de device '(.+)' est présent plusieurs fois\.$",
                "Device name '$1' appears more than once."),
            (@"^Identité technique dupliquée : device_id (.+), process_id (.+), utilisée par (.+)\. Supprimez le doublon ou recréez la copie comme rôle générique sans instance_id\.$",
                "Duplicate technical identity: device_id $1, process_id $2, used by $3. Remove the duplicate or recreate the copy as a generic role without instance_id."),
            (@"^Le device_id (.+) est partagé par plusieurs process_id \((.+)\)\. Vérifiez que cette structure correspond bien au matériel d'origine\.$",
                "device_id $1 is shared by several process_id values ($2). Check that this structure matches the original hardware."),
            (@"^(.+) : le device_id '(.+)' ne suit pas le format EUI-64 hexadécimal de 16 caractères observé\. La valeur est conservée, mais doit être vérifiée dans Dante Controller\.$",
                "$1: device_id '$2' does not match the observed 16-character hexadecimal EUI-64 format. The value is preserved but must be checked in Dante Controller."),
            (@"^Adresse IPv4 fixe dupliquée : (.+) est utilisée par (.+)\. Attribuez une adresse unique ou repassez les machines concernées en IP automatique\.$",
                "Duplicate static IPv4 address: $1 is used by $2. Assign a unique address or switch the affected devices back to automatic IP."),
            (@"^(.+) (TX|RX) position (\d+) : attribut danteId absent\. Restaurez l'identifiant du canal avant la sauvegarde\.$",
                "$1 $2 position $3: missing danteId attribute. Restore the channel identifier before saving."),
            (@"^(.+) (TX|RX) position (\d+) : danteId '(.+)' invalide\. Un entier strictement positif est attendu\.$",
                "$1 $2 position $3: invalid danteId '$4'. A strictly positive integer is required."),
            (@"^(.+) : danteId (.+) dupliqué parmi les canaux (TX|RX)\. Chaque canal d'une même direction doit avoir un identifiant unique\.$",
                "$1: duplicate danteId $2 among $3 channels. Every channel in the same direction must have a unique identifier."),
            (@"^(.+) (TX|RX) (.+) : attribut mediaType absent\. Restaurez la valeur du XML source avant la sauvegarde\.$",
                "$1 $2 $3: missing mediaType attribute. Restore the value from the source XML before saving."),
            (@"^(.+) : le txflow (.+) référence le canal TX '(.+)', qui n'existe pas\. Supprimez le slot invalide ou restaurez le canal TX concerné\.$",
                "$1: txflow $2 references Tx channel '$3', which does not exist. Remove the invalid slot or restore the affected Tx channel."),
            (@"^Sauvegarde refusée : racine XML modifiée \((.+) -> (.+)\)\.$",
                "Save blocked: XML root changed ($1 -> $2)."),
            (@"^Ajout de device non autorisé par défaut : (.+)\. Utilisez la duplication, la banque de machines ou l'import XML contrôlé\.$",
                "Device addition is not allowed by default: $1. Use duplication, the device bank, or controlled XML import.")
        ];

        foreach ((string pattern, string replacement) in technicalPatterns)
        {
            translated = ReplaceValidationPattern(value, pattern, replacement);
            if (!string.Equals(translated, value, StringComparison.Ordinal))
            {
                return translated;
            }
        }

        translated = TranslateXmlGuardMessage(value);
        if (!string.Equals(translated, value, StringComparison.Ordinal))
        {
            return translated;
        }

        return value;
    }

    private static string TranslateXmlGuardMessage(string value)
    {
        string translated = value
            .Replace("Modification technique interdite : ", "Forbidden technical change: ", StringComparison.Ordinal)
            .Replace("Chemin XML non autorisé par défaut : ", "XML path not allowed by default: ", StringComparison.Ordinal)
            .Replace("Balise modifiée :", "Element changed:", StringComparison.Ordinal)
            .Replace("Namespace modifié sur", "Namespace changed on", StringComparison.Ordinal)
            .Replace("Valeur modifiée :", "Value changed:", StringComparison.Ordinal)
            .Replace("Balise supprimée :", "Element removed:", StringComparison.Ordinal)
            .Replace("Balise ajoutée :", "Element added:", StringComparison.Ordinal)
            .Replace("Attribut supprimé :", "Attribute removed:", StringComparison.Ordinal)
            .Replace("Attribut ajouté :", "Attribute added:", StringComparison.Ordinal)
            .Replace("Attribut @", "Attribute @", StringComparison.Ordinal)
            .Replace(" modifié :", " changed:", StringComparison.Ordinal)
            .Replace("(vide)", "(empty)", StringComparison.Ordinal);

        return translated;
    }

    private static string ReplaceValidationPattern(string value, string pattern, string replacement)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            value,
            pattern,
            replacement,
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);
    }

    private static Dictionary<string, string> BuildLiteralMap()
    {
        Dictionary<string, string> map = new(StringComparer.Ordinal);

        foreach (KeyValuePair<string, string> pair in English)
        {
            if (French.TryGetValue(pair.Key, out string? frenchText) && !string.Equals(frenchText, pair.Value, StringComparison.Ordinal))
            {
                map[frenchText] = pair.Value;
            }
        }

        Add(map, "Ouvrir XML", "Open XML");
        Add(map, "Nouveau projet", "New project");
        Add(map, "Crée un nouveau projet hors ligne à partir d'une structure prise en charge.", "Creates a new offline project from a supported structure.");
        Add(map, "Banque de machines", "Device bank");
        Add(map, "Consulte, importe, exporte et administre les modèles de machines réutilisables.", "Browses, imports, exports, and manages reusable device templates.");
        Add(map, "Ajouter XML", "Add XML");
        Add(map, "Ajouter XML au projet", "Add XML to project");
        Add(map, "Enregistrer sous", "Save as");
        Add(map, "Annuler action", "Undo action");
        Add(map, "Annuler les changements", "Revert changes");
        Add(map, "Ouvrir récent", "Open recent");
        Add(map, "Thème clair", "Light theme");
        Add(map, "Thème sombre", "Dark theme");
        Add(map, "Langue de l'interface", "Interface language");
        Add(map, "Changer le thème", "Change theme");
        Add(map, "Fichiers XML récents", "Recent XML files");
        Add(map, "Projet", "Project");
        Add(map, "Recherche", "Search");
        Add(map, "Machine ou canal", "Device or channel");
        Add(map, "Mode hors ligne : l'application modifie uniquement les fichiers XML chargés. Elle ne se connecte pas au réseau Dante.", "Offline mode: the application only modifies loaded XML files. It does not connect to the Dante network.");
        Add(map, "Configuration", "Configuration");
        Add(map, "Machines", "Devices");
        Add(map, "POINTS À VÉRIFIER", "ITEMS TO CHECK");
        Add(map, "Voir les machines", "Show devices");
        Add(map, "Filtre machines", "Device filter");
        Add(map, "Cible actions", "Action target");
        Add(map, "Sélectionner visibles", "Select visible");
        Add(map, "Effacer sélection", "Clear selection");
        Add(map, "Verrouiller sélection", "Lock selection");
        Add(map, "Déverrouiller sélection", "Unlock selection");
        Add(map, "Avant / après", "Before / after");
        Add(map, "Lock", "Lock");
        Add(map, "Machine sélectionnée", "Selected device");
        Add(map, "Machine", "Device");
        Add(map, "Nouveau nom", "New name");
        Add(map, "Changer le nom", "Change name");
        Add(map, "Appliquer les paramètres", "Apply settings");
        Add(map, "Supprimer la machine", "Delete device");
        Add(map, "Mode réseau", "Network mode");
        Add(map, "Redondant", "Redundant");
        Add(map, "Changer le mode", "Change mode");
        Add(map, "Latence unicast", "Unicast latency");
        Add(map, "Changer la latence", "Change latency");
        Add(map, "Sample rate", "Sample rate");
        Add(map, "Changer la fréquence", "Change sample rate");
        Add(map, "Bits par échantillon", "Bits per sample");
        Add(map, "Changer les bits", "Change bits");
        Add(map, "Adresse IP fixe", "Static IP address");
        Add(map, "Adresse IP", "IP address");
        Add(map, "Masque", "Netmask");
        Add(map, "Passerelle", "Gateway");
        Add(map, "Fixer l'IP", "Set static IP");
        Add(map, "Mettre l'IP en automatique", "Set IP to automatic");
        Add(map, "Reset patch RX/TX machine", "Reset device Rx/Tx subscriptions");
        Add(map, "Reset patch RX/TX", "Clear Rx/Tx");
        Add(map, "Reset patch RX", "Clear Rx");
        Add(map, "Reset patch TX", "Clear Tx");
        Add(map, "Reset RX/TX", "Clear Rx/Tx");
        Add(map, "Reset RX", "Clear Rx");
        Add(map, "Reset TX", "Clear Tx");
        Add(map, "Étendre la série de noms jusqu'ici", "Extend the name series to this channel");
        Add(map, "Étendre la série RX jusqu'à la ligne déposée", "Extend the Rx series to the dropped row");
        Add(map, "Étendre la série TX jusqu'à la ligne déposée", "Extend the Tx series to the dropped row");
        Add(map, "Horloge", "Clock");
        Add(map, "Changer preferred master", "Change Preferred Master");
        Add(map, "Preferred Master à conserver", "Preferred Master to keep");
        Add(map, "Conserver un seul Preferred Master", "Keep one Preferred Master");
        Add(map, "Choisissez la seule machine qui restera Preferred Master.", "Select the only device that will remain Preferred Master.");
        Add(map, "Active le Preferred Master sur la machine choisie et le désactive sur toutes les autres.", "Enables Preferred Master on the selected device and disables it on every other device.");
        Add(map, "Canaux de la machine", "Device channels");
        Add(map, "Canal à renommer", "Channel to rename");
        Add(map, "Nouveau nom de canal", "New channel name");
        Add(map, "Renommer le canal", "Rename channel");
        Add(map, "Réinitialiser les canaux de la machine", "Reset device channels");
        Add(map, "Reset", "Reset");
        Add(map, "Efface les positions et l’ordre manuels, puis reconstruit automatiquement un synoptique propre.", "Clears manual positions and ordering, then automatically rebuilds a clean synoptic.");
        Add(map, "Renommage en série", "Batch rename");
        Add(map, "Canal début", "Start channel");
        Add(map, "Canal fin", "End channel");
        Add(map, "Préfixe", "Prefix");
        Add(map, "Numéro", "Number");
        Add(map, "Renommer la série", "Rename range");
        Add(map, "Importer des labels", "Import labels");
        Add(map, "Exporter des labels", "Export labels");
        Add(map, "Labels", "Labels");
        Add(map, "Import et export de labels", "Import and export labels");
        Add(map, "Formats JSON et CSV, plus XLSX compatible DMT pour dLive / Avantis.", "JSON and CSV formats, plus DMT-compatible XLSX for dLive / Avantis.");
        Add(map, "Exportez en JSON ou CSV, ou créez une copie d'un modèle XLSX/ODS DMT dLive / Avantis.", "Export as JSON or CSV, or create a copy of a DMT XLSX/ODS template for dLive / Avantis.");
        Add(map, "Labels JSON/CSV, DMT XLSX/ODS, A&H dLive/Avantis et Yamaha CL/QL.", "JSON/CSV labels, DMT XLSX/ODS, A&H dLive/Avantis, and Yamaha CL/QL.");
        Add(map, "Importez des labels JSON/CSV, DMT XLSX/ODS, A&H CSV ou Yamaha CL/QL ZIP/CSV.", "Import JSON/CSV labels, DMT XLSX/ODS, A&H CSV, or Yamaha CL/QL ZIP/CSV.");
        Add(map, "Exportez en générique ou créez une copie d'un modèle DMT, A&H ou Yamaha.", "Export a generic file or create a copy of a DMT, A&H, or Yamaha template.");
        Add(map, "Détecte les formats génériques, DMT, A&H et Yamaha, puis prévisualise les labels avant de les appliquer aux machines Dante.", "Detects generic, DMT, A&H, and Yamaha formats, then previews labels before applying them to Dante devices.");
        Add(map, "Exporte les labels TX/RX et ne modifie jamais les modèles DMT, A&H ou Yamaha originaux.", "Exports Tx/Rx labels and never modifies the original DMT, A&H, or Yamaha templates.");
        Add(map, "Compatibilité DMT dLive / Avantis - ouvrir le projet", "DMT dLive / Avantis compatibility - open project");
        Add(map, "Ouvre la page GitHub de dLive MIDI Tools par togrupe, dont les modèles dLive et Avantis sont compatibles.", "Opens the dLive MIDI Tools GitHub page by togrupe, whose dLive and Avantis templates are supported.");
        Add(map, "Ouverture du projet DMT impossible", "Cannot open the DMT project");
        Add(map, "Actions globales", "Global actions");
        Add(map, "Réseau / audio", "Network / audio");
        Add(map, "Profils", "Profiles");
        Add(map, "Profil rapide", "Quick profile");
        Add(map, "Appliquer le profil à la cible", "Apply profile to target");
        Add(map, "Appliquer le mode à tous", "Apply mode to all");
        Add(map, "Latence globale", "Global latency");
        Add(map, "Appliquer la latence à tous", "Apply latency to all");
        Add(map, "Sample rate globale", "Global sample rate");
        Add(map, "Appliquer la fréquence à tous", "Apply sample rate to all");
        Add(map, "Bits par échantillon globaux", "Global bits per sample");
        Add(map, "Appliquer les bits à tous", "Apply bits to all");
        Add(map, "Préfixe IP", "IP prefix");
        Add(map, "Premier numéro", "First number");
        Add(map, "Fixer les IP en série", "Set static IP range");
        Add(map, "Mettre toutes les IP en automatique", "Set all IPs to automatic");
        Add(map, "Réinitialiser tous les canaux", "Reset all channels");
        Add(map, "Liste rapide", "Quick list");
        Add(map, "Afficher", "Show");
        Add(map, "Choisissez l'information de contrôle à afficher.", "Choose the control information to display.");
        Add(map, "Affiche la liste de contrôle sélectionnée.", "Shows the selected control list.");
        Add(map, "Redondants", "Redundant");
        Add(map, "Latences", "Latencies");
        Add(map, "Sample rates", "Sample rates");
        Add(map, "Bits", "Bits");
        Add(map, "IP fixes", "Static IPs");
        Add(map, "Détail machine", "Device details");
        Add(map, "Détails", "Details");
        Add(map, "Réduire les réglages", "Hide settings");
        Add(map, "Afficher les réglages", "Show settings");
        Add(map, "Identité et formats", "Identity and formats");
        Add(map, "Nom machine", "Device name");
        Add(map, "Adresse IP", "IP address");
        Add(map, "Mode IP", "IP mode");
        Add(map, "Automatique", "Automatic");
        Add(map, "Fixe", "Static");
        Add(map, "Canaux", "Channels");
        Add(map, "Nom", "Name");
        Add(map, "Annuler", "Cancel");
        Add(map, "Appliquer", "Apply");
        Add(map, "Les changements seront appliqués au XML après validation.", "Changes will be applied to the XML after confirmation.");
        Add(map, "Friendly name", "Friendly name");
        Add(map, "Latence", "Latency");
        Add(map, "IP", "IP");
        Add(map, "Preferred", "Preferred");
        Add(map, "Patch", "Patch");
        Add(map, "Patch visuel / grille", "Visual patch / matrix");
        Add(map, "Easy patch / Matrice", "Easy patch / Matrix");
        Add(map, "Consultez chaque RX et sa source actuelle, ou ouvrez l’atelier pour patcher dans la matrice.", "Review each Rx channel and its current source, or open the workspace to patch in the matrix.");
        Add(map, "Ouvre Easy patch et la matrice. Chaque action y est appliquée immédiatement.", "Opens Easy patch and the matrix. Every action is applied immediately.");
        Add(map, "Affecte immédiatement la source TX choisie au RX sélectionné.", "Immediately assigns the selected Tx source to the selected Rx channel.");
        Add(map, "Déconnecte immédiatement le RX sélectionné.", "Immediately disconnects the selected Rx channel.");
        Add(map, "Ouvre l'onglet Easy patch.", "Opens the Easy patch tab.");
        Add(map, "Chargez un XML pour utiliser Easy patch.", "Load an XML file to use Easy patch.");
        Add(map, "Matrice", "Matrix");
        Add(map, "Liste RX vers TX", "Rx to Tx list");
        Add(map, "Par machine", "Per device");
        Add(map, "Modifications en attente", "Pending changes");
        Add(map, "0 modification en attente", "0 pending changes");
        Add(map, "Compteur des modifications Patch en attente", "Pending Patch changes counter");
        Add(map, "Mode Matrice du Patch", "Patch matrix mode");
        Add(map, "Mode Easy Patch", "Easy Patch mode");
        Add(map, "Mode Liste RX vers TX", "Rx to Tx list mode");
        Add(map, "Mode Patch par machine", "Per-device Patch mode");
        Add(map, "Modifications Patch en attente", "Pending Patch changes");
        Add(map, "Affiche la matrice RX/TX. Un clic applique directement le patch.", "Shows the Rx/Tx matrix. One click applies the patch immediately.");
        Add(map, "Affecte des sélections et des plages RX/TX dans le même moteur de Patch.", "Assigns Rx/Tx selections and ranges through the same Patch engine.");
        Add(map, "Affiche chaque RX et sa source TX courante.", "Shows each Rx and its current Tx source.");
        Add(map, "Filtre la liste Patch sur la machine actuellement sélectionnée.", "Filters the Patch list to the currently selected device.");
        Add(map, "Contrôle les lots préparés avant leur application atomique.", "Reviews prepared batches before their atomic application.");
        Add(map, "Les clics directs sont appliqués immédiatement. Cette liste contient seulement les lots explicitement préparés.", "Direct clicks are applied immediately. This list only contains explicitly prepared batches.");
        Add(map, "Liste des modifications Patch en attente", "Pending Patch changes list");
        Add(map, "Source actuelle", "Current source");
        Add(map, "Source demandée", "Requested source");
        Add(map, "Action", "Action");
        Add(map, "Annuler le lot", "Discard batch");
        Add(map, "Appliquer le lot", "Apply batch");
        Add(map, "Supprime uniquement les modifications Patch encore en attente.", "Discards only Patch changes that are still pending.");
        Add(map, "Applique toutes les modifications en attente en une transaction annulable.", "Applies all pending changes as one undoable transaction.");
        Add(map, "Chargez un XML pour commencer.", "Load an XML file to begin.");
        Add(map, "Cliquez pour affecter ou retirer ce patch. Maintenez et glissez pour préparer une série.", "Click to add or remove this patch. Hold and drag to prepare a range.");
        Add(map, "Machine RX précédente", "Previous Rx device");
        Add(map, "Machine réceptrice RX", "Receiving device (Rx)");
        Add(map, "Machine RX suivante", "Next Rx device");
        Add(map, "Machine TX précédente", "Previous Tx device");
        Add(map, "Machine émettrice TX", "Transmitting device (Tx)");
        Add(map, "Machine TX suivante", "Next Tx device");
        Add(map, "Canaux RX et source actuelle", "Rx channels and current source");
        Add(map, "Canaux TX disponibles", "Available Tx channels");
        Add(map, "Filtre émetteur TX", "Tx transmitter filter");
        Add(map, "Filtre récepteur RX", "Rx receiver filter");
        Add(map, "Recherche device ou canal", "Search device or channel");
        Add(map, "Filtre état", "State filter");
        Add(map, "Source TX à appliquer", "Tx source to apply");
        Add(map, "Canal TX à appliquer", "Tx channel to apply");
        Add(map, "Affichage", "View");
        Add(map, "Appliquer", "Apply");
        Add(map, "Supprimer", "Remove");
        Add(map, "RX device", "Rx device");
        Add(map, "RX Dante Id", "Rx Dante ID");
        Add(map, "RX canal", "Rx channel");
        Add(map, "Source complète", "Full source");
        Add(map, "TX affiché", "Displayed Tx");
        Add(map, "TX brut XML", "Raw XML Tx");
        Add(map, "TX résolu", "Resolved Tx");
        Add(map, "TX canal", "Tx channel");
        Add(map, "Actif", "Active");
        Add(map, "Modifié", "Modified");
        Add(map, "État", "Status");
        Add(map, "Libre", "Free");
        Add(map, "Patch actif", "Active subscription");
        Add(map, "Patch local", "Local subscription");
        Add(map, "Aucune source", "No source");
        Add(map, "Normal", "Normal");
        Add(map, "Afficher seulement les conflits", "Show conflicts only");
        Add(map, "Renommer le RX sélectionné", "Rename selected Rx");
        Add(map, "Renommer le TX source", "Rename source Tx");
        Add(map, "Renommer RX", "Rename Rx");
        Add(map, "Renommer TX", "Rename Tx");
        Add(map, "Import / Export", "Import / Export");
        Add(map, "Rapports et patchbook", "Reports and patchbook");
        Add(map, "Rapports du projet", "Project reports");
        Add(map, "Exportez un résumé lisible de la configuration chargée.", "Export a readable summary of the loaded configuration.");
        Add(map, "Choisissez le périmètre, puis exportez les affectations de patch.", "Choose the scope, then export the patch assignments.");
        Add(map, "Exportez les affectations de patch au format TXT ou CSV.", "Export patch assignments as TXT or CSV.");
        Add(map, "Analyse simple", "Simple analysis");
        Add(map, "Créez une vue textuelle compacte des liaisons entre machines.", "Create a compact text view of connections between devices.");
        Add(map, "Synoptique", "Synoptic");
        Add(map, "Synoptique visuel", "Visual synoptic");
        Add(map, "Machines regroupées par emplacement et patchs consécutifs réunis sur un même câble.", "Devices grouped by location, with consecutive subscriptions combined into one cable.");
        Add(map, "Emplacement de la sélection", "Selected devices location");
        Add(map, "Emplacement", "Location");
        Add(map, "Ordre", "Order");
        Add(map, "Zoom", "Zoom");
        Add(map, "Zoom arrière", "Zoom out");
        Add(map, "Zoom avant", "Zoom in");
        Add(map, "Revenir à la taille réelle", "Return to actual size");
        Add(map, "Ajuster", "Fit");
        Add(map, "Afficher tout le synoptique", "Fit the entire synoptic");
        Add(map, "Centrer", "Center");
        Add(map, "Centre la machine ou la liaison sélectionnée.", "Centers the selected device or link.");
        Add(map, "Tous les emplacements", "All locations");
        Add(map, "Filtre l’aperçu sans masquer durablement les machines et sans modifier le XML.", "Filters the preview without permanently hiding devices or changing the XML.");
        Add(map, "Sélection du synoptique", "Synoptic selection");
        Add(map, "Ouvrir dans Patch", "Open in Patch");
        Add(map, "Ouvrir la machine", "Open device");
        Add(map, "Ouvre la machine ou la première subscription de cette liaison dans la vue correspondante.", "Opens the device or the first subscription in this link in the corresponding view.");
        Add(map, "Disposition automatique", "Automatic layout");
        Add(map, "Efface les positions manuelles et recalcule le synoptique.", "Clears manual positions and recalculates the synoptic.");
        Add(map, "Affecter", "Assign");
        Add(map, "Saisissez une nouvelle zone ou choisissez une zone déjà utilisée.", "Enter a new location or choose one already used.");
        Add(map, "Zones déjà utilisées", "Locations already used");
        Add(map, "Voir", "Show");
        Add(map, "Tout afficher", "Show all");
        Add(map, "Masquer sélection", "Hide selection");
        Add(map, "Ouvrir après export", "Open after export");
        Add(map, "Actualiser l’aperçu", "Refresh preview");
        Add(map, "Exporter le synoptique SVG", "Export synoptic SVG");
        Add(map, "Exporter le synoptique PDF", "Export synoptic PDF");
        Add(map, "Exemples : Scène, Régie, Local amplis, Studio A.", "Examples: Stage, Control room, Amplifier room, Studio A.");
        Add(map, "Scène, Régie, Studio A…", "Stage, Control room, Studio A...");
        Add(map, "Préfixe ou modèle HF {00}", "Prefix or pattern HF {00}");
        Add(map, "Affecte cet emplacement aux machines sélectionnées.", "Assigns this location to the selected devices.");
        Add(map, "Place la machine sélectionnée plus haut dans son emplacement.", "Moves the selected device up within its location.");
        Add(map, "Place la machine sélectionnée plus bas dans son emplacement.", "Moves the selected device down within its location.");
        Add(map, "Crée un fichier vectoriel en couleur sans modifier le XML Dante.", "Creates a colored vector file without modifying the Dante XML.");
        Add(map, "Crée un PDF vectoriel en couleur sans modifier le XML Dante.", "Creates a colored vector PDF without modifying the Dante XML.");
        Add(map, "Santé du fichier", "File health");
        Add(map, "Centre de validation", "Validation center");
        Add(map, "Points à vérifier", "Items to check");
        Add(map, "Rapports", "Reports");
        Add(map, "Vérifie la structure et les références du XML chargé.", "Checks the loaded XML structure and references.");
        Add(map, "Prépare un rapport de compatibilité à relire avant l’import dans Dante Controller.", "Builds a compatibility report to review before importing into Dante Controller.");
        Add(map, "Résume les machines, formats, adresses et points de vigilance.", "Summarizes devices, formats, addresses, and items requiring attention.");
        Add(map, "Compare le projet avec un autre fichier XML sans modifier aucun des deux.", "Compares the project with another XML file without modifying either file.");
        Add(map, "Filtre santé", "Health filter");
        Add(map, "Gravité", "Severity");
        Add(map, "Catégorie", "Category");
        Add(map, "Canal", "Channel");
        Add(map, "Message", "Message");
        Add(map, "Sécurité et journal", "Safety and log");
        Add(map, "Vérifier le fichier", "Validate file");
        Add(map, "Rapport final avant Dante", "Final Dante check");
        Add(map, "Rapport compatibilité Dante Controller", "Dante Controller compatibility report");
        Add(map, "Rapport compatibilité Dante Controller affiché.", "Dante Controller compatibility report displayed.");
        Add(map, "Rapport final avant Dante affiché.", "Final pre-Dante report displayed.");
        Add(map, "Historique des actions affiché.", "Action history displayed.");
        Add(map, "Récupération automatique", "Automatic recovery");
        Add(map, "Session temporaire restaurée après interruption", "Temporary session restored after interruption");
        Add(map, "Import XML", "XML import");
        Add(map, "Mode réseau global", "Global network mode");
        Add(map, "IP automatique globale", "Global automatic IP");
        Add(map, "IP fixes globales", "Global static IP");
        Add(map, "Canaux global", "Global channels");
        Add(map, "Preferred master exclusif", "Exclusive Preferred Master");
        Add(map, "Import labels", "Label import");
        Add(map, "Nom device", "Device name");
        Add(map, "IP automatique", "Automatic IP");
        Add(map, "IP fixe", "Static IP");
        Add(map, "Machine supprimée", "Device deleted");
        Add(map, "Machine dupliquée", "Device duplicated");
        Add(map, "Machine ajoutée depuis la banque", "Device added from the bank");
        Add(map, "Nouveau projet expérimental", "New experimental project");
        Add(map, "Patch machine reset", "Device Rx/Tx patch reset");
        Add(map, "Patch RX machine reset", "Device Rx patch reset");
        Add(map, "Patch TX machine reset", "Device Tx patch reset");
        Add(map, "Nom canal", "Channel name");
        Add(map, "Renommage série", "Series rename");
        Add(map, "Patch supprimé", "Patch removed");
        Add(map, "Sauvegarde", "Save");
        Add(map, "Vérification", "Validation");
        Add(map, "Actualiser le résumé", "Refresh summary");
        Add(map, "Historique actions", "Action history");
        Add(map, "Exporter TXT", "Export TXT");
        Add(map, "Exporter PDF", "Export PDF");
        Add(map, "Patchbook TXT", "Patchbook TXT");
        Add(map, "Patchbook CSV", "Patchbook CSV");
        Add(map, "Topologie simple", "Simple topology");
        Add(map, "Comparer XML", "Compare XML");
        Add(map, "Quick start", "Quick start");
        Add(map, "Notice complète", "Full guide");
        Add(map, "Journal", "Log");
        Add(map, "Historique", "History");
        Add(map, "Historique des actions", "Action history");
        Add(map, "Les dernières modifications de cette session sont affichées ci-dessous.", "The latest changes from this session are shown below.");
        Add(map, "Outils avancés", "Advanced tools");
        Add(map, "GÉNÉRATEUR D'EXPÉRIENCE HORRIBLE (MAIS PÉDAGOGIQUE)", "HORRIBLE EXPERIENCE GENERATOR (BUT EDUCATIONAL)");
        Add(map, "Composez le pire réseau de formation possible, sans toucher au vrai fichier. Décochez simplement ce que vous souhaitez épargner.", "Build the worst training network imaginable without touching the real file. Simply clear anything you want to spare.");
        Add(map, "Que faut-il saboter ?", "What should be sabotaged?");
        Add(map, "Générateur d'expérience horrible, mais pédagogique. Le projet chargé devient une copie d'exercice volontairement chaotique.", "Horrible, but educational, experience generator. The loaded project becomes a deliberately chaotic training copy.");
        Add(map, "PÉRIMÈTRE DE L'EXERCICE", "EXERCISE SCOPE");
        Add(map, "Décochez les catégories qui doivent rester intactes.", "Clear the categories that must remain untouched.");
        Add(map, "PUPITRE DE COMMANDE", "CONTROL PANEL");
        Add(map, "AUTORISATION", "AUTHORIZATION");
        Add(map, "3", "3");
        Add(map, "ARM", "ARM");
        Add(map, "LOCK", "LOCK");
        Add(map, "FIRE", "FIRE");
        Add(map, "1. CLÉ", "1. KEY");
        Add(map, "OFF", "OFF");
        Add(map, "ON", "ON");
        Add(map, "SAFE · Tournez la clé", "SAFE · Turn the key");
        Add(map, "2. CAPOT DE SÉCURITÉ", "2. SAFETY COVER");
        Add(map, "VERROUILLÉ · tourner la clé pour ouvrir", "LOCKED · turn the key to open");
        Add(map, "Tourne la clé et ouvre automatiquement le capot. Un second clic remet le pupitre en sécurité.", "Turns the key and opens the cover automatically. A second click returns the panel to safe.");
        Add(map, "Clé de sécurité Atomic Bomb", "Atomic Bomb safety key");
        Add(map, "Arme la copie de travail sans encore la modifier.", "Arms the working copy without changing it yet.");
        Add(map, "Verrouille les options choisies et autorise FIRE.", "Locks the selected options and enables FIRE.");
        Add(map, "Déclenche immédiatement l'exercice hors ligne lorsque ARM et LOCK sont actifs.", "Immediately starts the offline exercise once ARM and LOCK are active.");
        Add(map, "La clé ouvre ce capot et révèle ARM, LOCK et FIRE.", "The key opens this cover and reveals ARM, LOCK, and FIRE.");
        Add(map, "Séquence : tournez la clé pour ouvrir le capot → ARM → LOCK → FIRE. FIRE agit immédiatement. Le XML original reste intact tant que vous n'écrasez pas volontairement son fichier.", "Sequence: turn the key to open the cover → ARM → LOCK → FIRE. FIRE acts immediately. The original XML remains intact unless you deliberately overwrite its file.");
        Add(map, "Noms des machines", "Device names");
        Add(map, "Labels des canaux TX", "Tx channel labels");
        Add(map, "Labels des canaux RX", "Rx channel labels");
        Add(map, "Patchs / subscriptions", "Subscriptions / patches");
        Add(map, "Modes réseau", "Network modes");
        Add(map, "Latences", "Latencies");
        Add(map, "Fréquences d'échantillonnage", "Sample rates");
        Add(map, "Bits par échantillon", "Bits per sample");
        Add(map, "IP principales", "Primary IP settings");
        Add(map, "ATOMIC BOMB", "ATOMIC BOMB");
        Add(map, "Charge un export XML Dante Controller.", "Loads a Dante Controller XML export.");
        Add(map, "Ajoute les machines d'un autre XML au projet ouvert. Les doublons peuvent être renommés.", "Adds devices from another XML to the open project. Duplicates can be renamed.");
        Add(map, "Enregistre un nouveau XML et crée un backup de sécurité.", "Saves a new XML file and creates a safety backup.");
        Add(map, "Autorise les modifications dans l'interface. La sauvegarde reste faite sous un nouveau nom.", "Allows changes in the interface. Saving still uses a new file name.");
        Add(map, "Annule la dernière action réalisée dans cette session.", "Undoes the last action made in this session.");
        Add(map, "Recharge le fichier XML d'origine et abandonne les modifications non sauvegardées.", "Reloads the original XML file and discards unsaved changes.");
        Add(map, "Liste les derniers XML ouverts.", "Lists the most recently opened XML files.");
        Add(map, "Ouvre le fichier sélectionné dans la liste récente.", "Opens the selected file from the recent list.");
        Add(map, "Déconnecte les RX de la machine et supprime les patchs qui utilisent ses TX.", "Disconnects the device Rx channels and removes subscriptions using its Tx channels.");
        Add(map, "Déconnecte seulement les entrées RX de la machine sélectionnée.", "Disconnects only the selected device Rx inputs.");
        Add(map, "Supprime seulement les patchs qui utilisent les TX de la machine sélectionnée.", "Removes only subscriptions using the selected device Tx channels.");
        Add(map, "Préfixe simple : HF donnera HF 01, HF 02. Modèle avancé : HF {00}, IN-{device}-{000}, ou {n} sans zéro.", "Simple prefix: HF gives HF 01, HF 02. Advanced pattern: HF {00}, IN-{device}-{000}, or {n} without leading zeros.");
        Add(map, "Applique le mode réseau à la cible choisie, en ignorant les machines verrouillées.", "Applies the network mode to the chosen target, ignoring locked devices.");
        Add(map, "Applique la latence à la cible choisie après prévisualisation.", "Applies latency to the chosen target after preview.");
        Add(map, "Applique la sample rate à la cible choisie. À vérifier avant import Dante.", "Applies the sample rate to the chosen target. Verify before Dante import.");
        Add(map, "Applique les bits par échantillon à la cible choisie.", "Applies bits per sample to the chosen target.");
        Add(map, "Réinitialise les noms de canaux TX/RX de la cible choisie, en respectant les verrous.", "Resets Tx/Rx channel names for the chosen target, respecting locks.");
        Add(map, "Attribue des IP fixes en série aux machines de la cible qui ont une interface IPv4 modifiable.", "Assigns static IPs in sequence to target devices with an editable IPv4 interface.");
        Add(map, "Repasse les IP reconnues en automatique pour la cible choisie.", "Sets recognized IP fields back to automatic for the chosen target.");
        Add(map, "Filtre seulement le tableau des machines, sans modifier le XML.", "Filters only the device table without modifying the XML.");
        Add(map, "Affiche rapidement les machines en IP fixe, preferred master, redondantes, daisychain, sans TX/RX ou avec formats différents.", "Quickly shows static IP, Preferred Master, redundant, daisychain, no Tx/Rx, or different-format devices.");
        Add(map, "Détermine quelles machines seront touchées par les actions globales.", "Defines which devices global actions will affect.");
        Add(map, "Choisissez si les actions globales s'appliquent à toutes les machines non verrouillées, à la sélection ou au filtre affiché.", "Choose whether global actions apply to all unlocked devices, the selection, or the visible filter.");
        Add(map, "Sélectionne toutes les machines actuellement visibles dans le tableau.", "Selects all devices currently visible in the table.");
        Add(map, "Vide la sélection multiple du tableau.", "Clears the table multi-selection.");
        Add(map, "Les machines verrouillées sont ignorées par les actions globales.", "Locked devices are ignored by global actions.");
        Add(map, "Retire le verrou des machines sélectionnées.", "Unlocks the selected devices.");
        Add(map, "Verrouille cette machine pour que les actions globales ne la modifient pas.", "Locks this device so global actions do not modify it.");
        Add(map, "Affiche un résumé OK / points à vérifier avant d'importer le XML dans Dante Controller.", "Shows an OK / items-to-check summary before importing the XML into Dante Controller.");
        Add(map, "Affiche les dernières actions réalisées dans l'application.", "Shows the latest actions made in the application.");
        Add(map, "Ouvre la notice rapide PDF.", "Opens the quick start PDF.");
        Add(map, "Ouvre la notice complète PDF.", "Opens the full guide PDF.");
        Add(map, "Le XML conserve la valeur brute Dante.", "The XML keeps the raw Dante value.");
        Add(map, "La valeur affichée est en ms. Le XML conserve la valeur brute Dante.", "Displayed value is in ms. The XML keeps the raw Dante value.");
        Add(map, "Applique en une seule fois le nom, le mode réseau, la latence et le statut Preferred master.", "Applies the name, network mode, latency, and Preferred Master status in one operation.");
        Add(map, "Ouvre tous les réglages de la machine : IP automatique ou fixe, formats audio et noms des canaux.", "Opens all device settings: automatic or static IP, audio formats, and channel names.");
        Add(map, "Supprime la machine et nettoie les patchs qui lui sont associés.", "Deletes the device and removes its associated subscriptions.");
        Add(map, "Dupliquer", "Duplicate");
        Add(map, "Crée un rôle générique indépendant sans recopier l'identité matérielle.", "Creates an independent generic role without copying the hardware identity.");
        Add(map, "Enregistrer dans la banque", "Save to device bank");
        Add(map, "Crée un modèle réutilisable et partageable à partir de cette machine.", "Creates a reusable, shareable template from this device.");
        Add(map, "Ouvrir les journaux", "Open logs");
        Add(map, "Ouvre le dossier des journaux techniques utiles au diagnostic.", "Opens the technical log folder used for diagnostics.");
        Add(map, "Masque les panneaux de réglage pour agrandir le tableau des machines.", "Hides the settings panels to enlarge the device table.");
        Add(map, "Affiche les panneaux de réglage de la configuration.", "Shows the configuration settings panels.");
        Add(map, "Affiche toutes les différences de machines, canaux et patchs depuis l'ouverture du XML.", "Shows all device, channel, and subscription differences since the XML was opened.");
        Add(map, "Applique en une seule action les formats audio, la latence, l'IP automatique et éventuellement le mode réseau du profil à la cible choisie.", "Applies the profile audio formats, latency, automatic IP, and optional network mode to the selected target in one operation.");
        Add(map, "Ouvre une vue TX/RX avec glisser-déposer, affectation en série et grille de patch.", "Opens a Tx/Rx view with drag and drop, sequential assignment, and a patch matrix.");
        Add(map, "Ouvrir l’aperçu dans une fenêtre séparée", "Open the preview in a separate window");
        Add(map, "Ouvrir l’aperçu du synoptique dans une fenêtre séparée", "Open the synoptic preview in a separate window");
        Add(map, "Patch RX", "Rx patch");
        Add(map, "Ouvrir Easy patch", "Open Easy patch");
        Add(map, "Affectez des canaux TX disponibles aux entrées RX de cette machine.", "Assign available Tx channels to this device's Rx inputs.");
        Add(map, "Aucun changement de patch en attente.", "No pending patch change.");
        Add(map, "Les changements de patch seront appliqués avec les autres réglages de cette fenêtre.", "Patch changes will be applied with the other settings in this window.");

        // Shell Windows 2026.1.
        Add(map, "Dante Config Editor 2026.1 Beta", "Dante Config Editor 2026.1 Beta");
        Add(map, "Fichier", "File");
        Add(map, "Édition", "Edit");
        Add(map, "Affichage", "View");
        Add(map, "Outils", "Tools");
        Add(map, "Aide", "Help");
        Add(map, "Ouvrir un XML", "Open XML");
        Add(map, "Enregistrer sous", "Save as");
        Add(map, "Quitter", "Exit");
        Add(map, "Annuler les changements non enregistrés", "Discard unsaved changes");
        Add(map, "Dupliquer la machine sélectionnée", "Duplicate selected device");
        Add(map, "Enregistrer la machine dans la banque", "Save device to library");
        Add(map, "Supprimer la machine sélectionnée", "Delete selected device");
        Add(map, "Réglages de la page Machines", "Device page settings");
        Add(map, "Thème sombre", "Dark theme");
        Add(map, "Thème clair", "Light theme");
        Add(map, "Démarrage rapide", "Quick start");
        Add(map, "Notes de version", "Release notes");
        Add(map, "Projet GitHub", "GitHub project");
        Add(map, "À propos de DCE", "About DCE");
        Add(map, "XML Dante", "Dante XML");
        Add(map, "Profil : non détecté", "Profile: not detected");
        Add(map, "Pas encore enregistré", "Not saved yet");
        Add(map, "Navigation", "Navigation");
        Add(map, "<", "<");
        Add(map, ">", ">");
        Add(map, "▲", "▲");
        Add(map, "▼", "▼");
        Add(map, "Validation", "Validation");
        Add(map, "Audio", "Audio");
        Add(map, "TX / RX", "TX / RX");
        Add(map, "0 / 0", "0 / 0");
        Add(map, "Nouveau", "New");
        Add(map, "Ouvrir", "Open");
        Add(map, "Enregistrer", "Save");
        Add(map, "Annuler", "Undo");
        Add(map, "Rétablir", "Redo");
        Add(map, "Inspecteur", "Inspector");
        Add(map, "Réduire", "Collapse");
        Add(map, "Masquer", "Hide");
        Add(map, "Masquer navigation", "Hide navigation");
        Add(map, "Afficher navigation", "Show navigation");
        Add(map, "Rouvre la navigation principale masquée.", "Reopens the hidden main navigation.");
        Add(map, "Masquer inspecteur", "Hide inspector");
        Add(map, "Afficher inspecteur", "Show inspector");
        Add(map, "Masquer les réglages", "Hide settings");
        Add(map, "Vue d'ensemble", "Overview");
        Add(map, "Machines", "Devices");
        Add(map, "Ajouter depuis la banque", "Add from bank");
        Add(map, "Gérer les banques", "Manage banks");
        Add(map, "Ouvre la vue globale des banques et ajoute une nouvelle machine indépendante au projet.", "Opens the global bank view and adds a new independent device to the project.");
        Add(map, "Ouvre la vue globale pour rechercher, filtrer et administrer les banques de machines.", "Opens the global view to search, filter, and manage device banks.");
        Add(map, "Synoptique", "Synoptic");
        Add(map, "Centre de validation", "Validation center");
        Add(map, "Historique", "History");
        Add(map, "Outils avancés", "Advanced tools");
        Add(map, "Affiche ou masque la navigation principale.", "Shows or hides the main navigation.");
        Add(map, "Crée un nouveau projet hors ligne à partir d'une structure prise en charge.", "Creates a new offline project from a supported structure.");
        Add(map, "Enregistre le projet courant. Pour un XML ouvert, une nouvelle destination est demandée.", "Saves the current project. An open XML file requires a new destination.");
        Add(map, "Enregistre sous un nouveau nom avec une sauvegarde de sécurité.", "Saves under a new name with a safety backup.");
        Add(map, "Rétablit la dernière action annulée lorsque la session 2026.1 le permet.", "Redoes the last undone action when the 2026.1 session supports it.");
        Add(map, "Recherche globale", "Global search");
        Add(map, "Recherche les machines, canaux et subscriptions du projet.", "Searches project devices, channels, and subscriptions.");
        Add(map, "Ouvre le centre de validation.", "Opens the validation center.");
        Add(map, "Affiche ou masque l'inspecteur contextuel.", "Shows or hides the contextual inspector.");
        Add(map, "Masque l'inspecteur contextuel.", "Hides the contextual inspector.");
        Add(map, "Ouvre l'accueil et les fichiers récents.", "Opens the home page and recent files.");
        Add(map, "Masque la navigation principale.", "Hides the main navigation.");
        Add(map, "Ouvrez un XML Dante existant, reprenez un fichier récent ou créez un projet hors ligne.", "Open an existing Dante XML file, resume a recent file, or create an offline project.");
        Add(map, "Ouvrir un XML Dante", "Open a Dante XML file");
        Add(map, "Charge un fichier XML exporté depuis Dante Controller.", "Loads an XML file exported from Dante Controller.");
        Add(map, "Nouveau projet", "New project");
        Add(map, "Crée un projet à partir d'une structure prise en charge.", "Creates a project from a supported structure.");
        Add(map, "Ouvrir la banque de machines", "Open the device library");
        Add(map, "Affiche les modèles de machines réutilisables.", "Shows reusable device templates.");
        Add(map, "Ajouter un XML au projet", "Add an XML file to the project");
        Add(map, "Fusionne un autre XML dans le projet ouvert.", "Merges another XML file into the open project.");
        Add(map, "Abandonner les modifications", "Discard changes");
        Add(map, "Recharge la source et abandonne les modifications non enregistrées.", "Reloads the source and discards unsaved changes.");
        Add(map, "Fichiers récents", "Recent files");
        Add(map, "Double-cliquez sur un fichier ou sélectionnez-le puis ouvrez-le.", "Double-click a file or select it and open it.");
        Add(map, "Ouvrir le fichier sélectionné", "Open selected file");
        Add(map, "Récupération et sécurité", "Recovery and safety");
        Add(map, "Aucune récupération en attente détectée.", "No pending recovery was detected.");
        Add(map, "DCE travaille hors ligne sur des fichiers. Une sauvegarde de sécurité est conservée lors des remplacements atomiques.", "DCE works offline on files. A safety backup is retained during atomic replacements.");
        Add(map, "Chargez un XML pour analyser rapidement sa configuration.", "Load an XML file to quickly inspect its configuration.");
        Add(map, "Subscriptions", "Subscriptions");
        Add(map, "Erreurs / avertissements", "Errors / warnings");
        Add(map, "Machines sans patch", "Devices without subscriptions");
        Add(map, "Ouvre la liste des machines.", "Opens the device list.");
        Add(map, "Ouvre l'espace Patch.", "Opens the Patch workspace.");
        Add(map, "Ouvre les erreurs et avertissements.", "Opens errors and warnings.");
        Add(map, "Ouvre la liste des machines et ses filtres.", "Opens the device list and its filters.");
        Add(map, "Réseau et horloge", "Network and clock");
        Add(map, "Dernières modifications", "Latest changes");
        Add(map, "La banque stocke des modèles réutilisables dans un dossier lisible et partageable. L'ouverture reste séparée pendant sa migration vers le shell 2026.1.", "The library stores reusable templates in a readable, shareable folder. It remains a separate window while it is migrated into the 2026.1 shell.");
        Add(map, "Ouvre l'administration, l'import, l'export et la migration de la banque.", "Opens library administration, import, export, and migration.");
        Add(map, "Aucun projet", "No project");
        Add(map, "Chargez un projet ou sélectionnez une machine pour afficher ses propriétés.", "Load a project or select a device to display its properties.");
        Add(map, "Aucun projet chargé", "No project loaded");
        Add(map, "Constructeur", "Manufacturer");
        Add(map, "Modèle", "Model");
        Add(map, "Horloge", "Clock");
        Add(map, "Détails", "Details");
        Add(map, "Voir dans Patch", "Show in Patch");
        Add(map, "Ouvre toutes les propriétés de la machine sélectionnée.", "Opens all properties of the selected device.");
        Add(map, "Filtre le Patch autour de la machine sélectionnée.", "Filters Patch around the selected device.");
        Add(map, "Identité technique", "Technical identity");
        Add(map, "Informations avancées conservées par le XML.", "Advanced information retained by the XML.");

        return map;
    }

    private static Dictionary<string, string> BuildInverseLiteralMap()
    {
        Dictionary<string, string> map = new(StringComparer.Ordinal);
        foreach (KeyValuePair<string, string> pair in LiteralFrenchToEnglish)
        {
            // Une entrée identique dans les deux langues ("Sample rates" par
            // exemple) ne doit pas masquer une vraie traduction française
            // ajoutée plus loin dans le catalogue.
            if (!map.TryGetValue(pair.Value, out string? existingFrench)
                || string.Equals(existingFrench, pair.Value, StringComparison.Ordinal))
            {
                map[pair.Value] = pair.Key;
            }
        }

        return map;
    }

    private static void Add(Dictionary<string, string> map, string french, string english)
    {
        map[french] = english;
    }
}
