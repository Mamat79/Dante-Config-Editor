namespace DanteConfigEditor.Services;

public sealed record PatchSourceDescriptor(
    string DeviceName,
    int DanteId,
    int PositionIndex,
    string ChannelName)
{
    public string Display => $"{DanteId:000} - {ChannelName}";

    public string FullDisplay => $"{DeviceName} / {Display}";

    public bool CanExtendNameSeries => ChannelNameSeriesService.CanExtend(ChannelName);
}

public sealed record PatchTargetDescriptor(
    string DeviceName,
    int DanteId,
    int PositionIndex,
    string ChannelName)
{
    public string Display => $"{DanteId:000} - {ChannelName}";

    public string FullDisplay => $"{DeviceName} / {Display}";

    public bool CanExtendNameSeries => ChannelNameSeriesService.CanExtend(ChannelName);
}

public sealed record PlannedPatchAssignment(
    PatchSourceDescriptor Source,
    PatchTargetDescriptor Target);

public sealed record SequentialPatchPlan(
    IReadOnlyList<PlannedPatchAssignment> Assignments,
    IReadOnlyList<PatchSourceDescriptor> UnassignedSources);

public sealed record PatchAssignmentPlan(
    IReadOnlyList<PlannedPatchAssignment> Assignments);

public sealed record PatchRangeCapacity(
    int TxAvailable,
    int RxAvailable)
{
    public int MaximumCount => Math.Min(TxAvailable, RxAvailable);
}

public sealed record PatchEditRequest(
    string RxDeviceName,
    int RxDanteId,
    string? TxDeviceName,
    string? TxChannelName)
{
    public bool IsRemoval => string.IsNullOrWhiteSpace(TxDeviceName);

    public static PatchEditRequest Apply(PlannedPatchAssignment assignment)
    {
        return new PatchEditRequest(
            assignment.Target.DeviceName,
            assignment.Target.DanteId,
            assignment.Source.DeviceName,
            assignment.Source.ChannelName);
    }

    public static PatchEditRequest Remove(PatchTargetDescriptor target)
    {
        return new PatchEditRequest(target.DeviceName, target.DanteId, null, null);
    }
}

public static class PatchAssignmentPlanner
{
    public static PatchAssignmentPlan PlanSelection(
        IReadOnlyList<PatchSourceDescriptor> selectedSources,
        IReadOnlyList<PatchTargetDescriptor> selectedTargets)
    {
        ArgumentNullException.ThrowIfNull(selectedSources);
        ArgumentNullException.ThrowIfNull(selectedTargets);

        if (selectedSources.Count == 0)
        {
            throw new InvalidOperationException("Sélectionnez au moins un canal TX.");
        }

        if (selectedTargets.Count == 0)
        {
            throw new InvalidOperationException("Sélectionnez au moins un canal RX.");
        }

        EnsureUniqueTargets(selectedTargets);

        if (selectedSources.Count > 1 && selectedTargets.Count == 1)
        {
            throw new InvalidOperationException("Plusieurs TX ne peuvent pas être affectés à un seul RX.");
        }

        if (selectedSources.Count != 1 && selectedSources.Count != selectedTargets.Count)
        {
            throw new InvalidOperationException("Sélectionnez autant de TX que de RX, ou un seul TX pour plusieurs RX.");
        }

        PlannedPatchAssignment[] assignments = selectedSources.Count == 1
            ? selectedTargets.Select(target => new PlannedPatchAssignment(selectedSources[0], target)).ToArray()
            : selectedSources.Zip(selectedTargets, (source, target) => new PlannedPatchAssignment(source, target)).ToArray();

        return new PatchAssignmentPlan(assignments);
    }

    public static PatchAssignmentPlan PlanRange(
        IReadOnlyList<PatchSourceDescriptor> availableSources,
        PatchSourceDescriptor firstSource,
        IReadOnlyList<PatchTargetDescriptor> availableTargets,
        PatchTargetDescriptor firstTarget,
        int count)
    {
        ArgumentNullException.ThrowIfNull(availableSources);
        ArgumentNullException.ThrowIfNull(firstSource);
        ArgumentNullException.ThrowIfNull(availableTargets);
        ArgumentNullException.ThrowIfNull(firstTarget);

        if (count <= 0)
        {
            throw new InvalidOperationException("Le nombre de canaux doit être supérieur à zéro.");
        }

        PatchRangeCapacity capacity = GetRangeCapacity(
            availableSources,
            firstSource,
            availableTargets,
            firstTarget);
        if (count > capacity.MaximumCount)
        {
            throw new InvalidOperationException(
                $"La plage demandée dépasse les canaux disponibles " +
                $"({capacity.TxAvailable} TX et {capacity.RxAvailable} RX à partir des canaux choisis). " +
                "Aucun patch n'a été préparé.");
        }

        PatchSourceDescriptor knownFirstSource = FindKnownSource(availableSources, firstSource)
            ?? throw new InvalidOperationException("Le canal TX de départ n'appartient pas à la liste disponible.");
        PatchTargetDescriptor knownFirstTarget = FindKnownTarget(availableTargets, firstTarget)
            ?? throw new InvalidOperationException("Le canal RX de départ n'appartient pas à la liste disponible.");

        PatchSourceDescriptor[] sources = availableSources
            .Where(source => string.Equals(source.DeviceName, knownFirstSource.DeviceName, StringComparison.OrdinalIgnoreCase))
            .Where(source => source.PositionIndex >= knownFirstSource.PositionIndex)
            .OrderBy(source => source.PositionIndex)
            .Take(count)
            .ToArray();
        PatchTargetDescriptor[] targets = availableTargets
            .Where(target => string.Equals(target.DeviceName, knownFirstTarget.DeviceName, StringComparison.OrdinalIgnoreCase))
            .Where(target => target.PositionIndex >= knownFirstTarget.PositionIndex)
            .OrderBy(target => target.PositionIndex)
            .Take(count)
            .ToArray();

        return new PatchAssignmentPlan(
            sources.Zip(targets, (source, target) => new PlannedPatchAssignment(source, target)).ToArray());
    }

    public static PatchAssignmentPlan PlanOneToOne(
        IReadOnlyList<PatchSourceDescriptor> availableSources,
        PatchSourceDescriptor firstSource,
        IReadOnlyList<PatchTargetDescriptor> availableTargets,
        PatchTargetDescriptor firstTarget,
        int count)
    {
        return PlanRange(availableSources, firstSource, availableTargets, firstTarget, count);
    }

    public static PatchRangeCapacity GetRangeCapacity(
        IReadOnlyList<PatchSourceDescriptor> availableSources,
        PatchSourceDescriptor firstSource,
        IReadOnlyList<PatchTargetDescriptor> availableTargets,
        PatchTargetDescriptor firstTarget)
    {
        ArgumentNullException.ThrowIfNull(availableSources);
        ArgumentNullException.ThrowIfNull(firstSource);
        ArgumentNullException.ThrowIfNull(availableTargets);
        ArgumentNullException.ThrowIfNull(firstTarget);

        PatchSourceDescriptor knownFirstSource = FindKnownSource(availableSources, firstSource)
            ?? throw new InvalidOperationException("Le canal TX de départ n'appartient pas à la liste disponible.");
        PatchTargetDescriptor knownFirstTarget = FindKnownTarget(availableTargets, firstTarget)
            ?? throw new InvalidOperationException("Le canal RX de départ n'appartient pas à la liste disponible.");

        int txAvailable = availableSources.Count(source =>
            string.Equals(source.DeviceName, knownFirstSource.DeviceName, StringComparison.OrdinalIgnoreCase)
            && source.PositionIndex >= knownFirstSource.PositionIndex);
        int rxAvailable = availableTargets.Count(target =>
            string.Equals(target.DeviceName, knownFirstTarget.DeviceName, StringComparison.OrdinalIgnoreCase)
            && target.PositionIndex >= knownFirstTarget.PositionIndex);
        return new PatchRangeCapacity(txAvailable, rxAvailable);
    }

    public static PatchAssignmentPlan PlanMatrixGesture(
        IReadOnlyList<PatchSourceDescriptor> availableSources,
        IReadOnlyList<PatchTargetDescriptor> availableTargets,
        int startSourceIndex,
        int startTargetIndex,
        int endSourceIndex,
        int endTargetIndex)
    {
        ArgumentNullException.ThrowIfNull(availableSources);
        ArgumentNullException.ThrowIfNull(availableTargets);
        EnsureMatrixIndex(startSourceIndex, availableSources.Count, "TX de départ");
        EnsureMatrixIndex(endSourceIndex, availableSources.Count, "TX de fin");
        EnsureMatrixIndex(startTargetIndex, availableTargets.Count, "RX de départ");
        EnsureMatrixIndex(endTargetIndex, availableTargets.Count, "RX de fin");

        int sourceDelta = endSourceIndex - startSourceIndex;
        int targetDelta = endTargetIndex - startTargetIndex;

        if (sourceDelta == 0)
        {
            int targetStep = Math.Sign(targetDelta);
            int count = Math.Abs(targetDelta) + 1;
            PlannedPatchAssignment[] assignments = Enumerable.Range(0, count)
                .Select(offset => new PlannedPatchAssignment(
                    availableSources[startSourceIndex],
                    availableTargets[startTargetIndex + (offset * targetStep)]))
                .ToArray();
            return new PatchAssignmentPlan(assignments);
        }

        if (targetDelta == 0)
        {
            // Une ligne horizontale ne peut pas affecter plusieurs TX au même
            // RX. Elle représente donc une série un-à-un à partir du RX de départ.
            int sourceStep = Math.Sign(sourceDelta);
            int count = Math.Abs(sourceDelta) + 1;
            int targetStep = sourceStep;
            if (!RangeFits(startTargetIndex, targetStep, count, availableTargets.Count))
            {
                targetStep = -targetStep;
            }

            if (!RangeFits(startTargetIndex, targetStep, count, availableTargets.Count))
            {
                throw new InvalidOperationException(
                    "Le glissement horizontal dépasse les canaux RX disponibles. Aucun patch n'a été préparé.");
            }

            PlannedPatchAssignment[] assignments = Enumerable.Range(0, count)
                .Select(offset => new PlannedPatchAssignment(
                    availableSources[startSourceIndex + (offset * sourceStep)],
                    availableTargets[startTargetIndex + (offset * targetStep)]))
                .ToArray();
            return new PatchAssignmentPlan(assignments);
        }

        if (Math.Abs(sourceDelta) != Math.Abs(targetDelta))
        {
            throw new InvalidOperationException(
                "Pour une série diagonale, déplacez-vous du même nombre de cases TX et RX. Aucun patch n'a été préparé.");
        }

        int diagonalCount = Math.Abs(sourceDelta) + 1;
        int diagonalSourceStep = Math.Sign(sourceDelta);
        int diagonalTargetStep = Math.Sign(targetDelta);
        return new PatchAssignmentPlan(
            Enumerable.Range(0, diagonalCount)
                .Select(offset => new PlannedPatchAssignment(
                    availableSources[startSourceIndex + (offset * diagonalSourceStep)],
                    availableTargets[startTargetIndex + (offset * diagonalTargetStep)]))
                .ToArray());
    }

    public static SequentialPatchPlan PlanSequential(
        IReadOnlyList<PatchSourceDescriptor> selectedSources,
        IReadOnlyList<PatchTargetDescriptor> availableTargets,
        PatchTargetDescriptor firstTarget)
    {
        ArgumentNullException.ThrowIfNull(selectedSources);
        ArgumentNullException.ThrowIfNull(availableTargets);
        ArgumentNullException.ThrowIfNull(firstTarget);

        if (selectedSources.Count == 0)
        {
            throw new InvalidOperationException("Sélectionnez au moins un canal TX.");
        }

        PatchTargetDescriptor? knownFirstTarget = availableTargets.FirstOrDefault(target =>
            string.Equals(target.DeviceName, firstTarget.DeviceName, StringComparison.OrdinalIgnoreCase)
            && target.DanteId == firstTarget.DanteId);
        if (knownFirstTarget is null)
        {
            throw new InvalidOperationException("Le canal RX de départ n'appartient pas à la liste disponible.");
        }

        // Le danteId n'est pas forcément contigu. L'ordre XML, conservé dans
        // PositionIndex, détermine donc les RX qui suivent réellement. La
        // position est reprise de la liste connue, jamais d'un objet appelant.
        PatchTargetDescriptor[] followingTargets = availableTargets
            .Where(target => string.Equals(target.DeviceName, firstTarget.DeviceName, StringComparison.OrdinalIgnoreCase))
            .Where(target => target.PositionIndex >= knownFirstTarget.PositionIndex)
            .OrderBy(target => target.PositionIndex)
            .ToArray();

        int assignmentCount = Math.Min(selectedSources.Count, followingTargets.Length);
        List<PlannedPatchAssignment> assignments = new(assignmentCount);
        for (int index = 0; index < assignmentCount; index++)
        {
            assignments.Add(new PlannedPatchAssignment(selectedSources[index], followingTargets[index]));
        }

        return new SequentialPatchPlan(
            assignments,
            selectedSources.Skip(assignmentCount).ToArray());
    }

    private static void EnsureUniqueTargets(IEnumerable<PatchTargetDescriptor> targets)
    {
        bool hasDuplicate = targets
            .GroupBy(target => (target.DeviceName.Trim().ToUpperInvariant(), target.DanteId))
            .Any(group => group.Count() > 1);
        if (hasDuplicate)
        {
            throw new InvalidOperationException("La sélection RX contient plusieurs fois le même canal.");
        }
    }

    private static void EnsureMatrixIndex(int index, int count, string label)
    {
        if (index < 0 || index >= count)
        {
            throw new InvalidOperationException($"Le {label} n'appartient pas à la grille visible.");
        }
    }

    private static bool RangeFits(int startIndex, int step, int count, int availableCount)
    {
        int endIndex = startIndex + ((count - 1) * step);
        return endIndex >= 0 && endIndex < availableCount;
    }

    private static PatchSourceDescriptor? FindKnownSource(
        IEnumerable<PatchSourceDescriptor> availableSources,
        PatchSourceDescriptor requested)
    {
        return availableSources.FirstOrDefault(source =>
            string.Equals(source.DeviceName, requested.DeviceName, StringComparison.OrdinalIgnoreCase)
            && source.DanteId == requested.DanteId);
    }

    private static PatchTargetDescriptor? FindKnownTarget(
        IEnumerable<PatchTargetDescriptor> availableTargets,
        PatchTargetDescriptor requested)
    {
        return availableTargets.FirstOrDefault(target =>
            string.Equals(target.DeviceName, requested.DeviceName, StringComparison.OrdinalIgnoreCase)
            && target.DanteId == requested.DanteId);
    }
}
