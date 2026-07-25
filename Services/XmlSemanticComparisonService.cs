using System.Xml.Linq;

namespace DanteConfigEditor.Services;

public sealed class XmlSemanticComparisonResult
{
    internal XmlSemanticComparisonResult(IReadOnlyList<string> differences)
    {
        Differences = differences;
    }

    public IReadOnlyList<string> Differences { get; }

    public bool AreEquivalent => Differences.Count == 0;

    public string ToDisplayText()
    {
        return AreEquivalent
            ? "Les documents XML sont sémantiquement équivalents."
            : string.Join(Environment.NewLine, Differences.Select(difference => "- " + difference));
    }
}

public static class XmlSemanticComparisonService
{
    private const int MaximumDifferences = 100;

    public static XmlSemanticComparisonResult Compare(XDocument expected, XDocument actual)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        List<string> differences = [];
        CompareDeclaration(expected.Declaration, actual.Declaration, differences);
        CompareNodes(
            expected.Nodes().Where(IsSemanticNode).ToArray(),
            actual.Nodes().Where(IsSemanticNode).ToArray(),
            "/",
            differences);
        return new XmlSemanticComparisonResult(differences);
    }

    private static void CompareDeclaration(
        XDeclaration? expected,
        XDeclaration? actual,
        ICollection<string> differences)
    {
        if (expected is null && actual is null)
        {
            return;
        }

        if (expected is null || actual is null)
        {
            AddDifference(differences, "/ : déclaration XML ajoutée ou supprimée.");
            return;
        }

        CompareScalarIgnoreCase("version de déclaration", expected.Version ?? string.Empty, actual.Version ?? string.Empty, "/", differences);
        CompareScalarIgnoreCase("encodage de déclaration", expected.Encoding ?? string.Empty, actual.Encoding ?? string.Empty, "/", differences);
        CompareScalarIgnoreCase("standalone de déclaration", expected.Standalone ?? string.Empty, actual.Standalone ?? string.Empty, "/", differences);
    }

    private static void CompareNodes(
        IReadOnlyList<XNode> expected,
        IReadOnlyList<XNode> actual,
        string parentPath,
        ICollection<string> differences)
    {
        if (expected.Count != actual.Count)
        {
            AddDifference(differences, $"{parentPath} : {expected.Count} nœud(s) attendu(s), {actual.Count} trouvé(s).");
        }

        int count = Math.Min(expected.Count, actual.Count);
        for (int index = 0; index < count && differences.Count < MaximumDifferences; index++)
        {
            CompareNode(expected[index], actual[index], BuildNodePath(parentPath, expected[index], index), differences);
        }

        for (int index = count; index < expected.Count && differences.Count < MaximumDifferences; index++)
        {
            AddDifference(
                differences,
                $"{parentPath} : nœud supprimé à la position {index + 1} ({DescribeNode(expected[index])}).");
        }

        for (int index = count; index < actual.Count && differences.Count < MaximumDifferences; index++)
        {
            AddDifference(
                differences,
                $"{parentPath} : nœud ajouté à la position {index + 1} ({DescribeNode(actual[index])}).");
        }
    }

    private static void CompareNode(
        XNode expected,
        XNode actual,
        string path,
        ICollection<string> differences)
    {
        if (expected.NodeType != actual.NodeType)
        {
            AddDifference(differences, $"{path} : type de nœud modifié ({expected.NodeType} -> {actual.NodeType}).");
            return;
        }

        switch (expected)
        {
            case XElement expectedElement when actual is XElement actualElement:
                CompareElement(expectedElement, actualElement, path, differences);
                break;
            case XCData expectedCData when actual is XCData actualCData:
                CompareScalar("CDATA", expectedCData.Value, actualCData.Value, path, differences);
                break;
            case XText expectedText when actual is XText actualText:
                CompareScalar("texte", expectedText.Value, actualText.Value, path, differences);
                break;
            case XComment expectedComment when actual is XComment actualComment:
                CompareScalar("commentaire", expectedComment.Value, actualComment.Value, path, differences);
                break;
            case XProcessingInstruction expectedInstruction when actual is XProcessingInstruction actualInstruction:
                CompareScalar("cible d'instruction", expectedInstruction.Target, actualInstruction.Target, path, differences);
                CompareScalar("instruction", expectedInstruction.Data, actualInstruction.Data, path, differences);
                break;
            case XDocumentType expectedType when actual is XDocumentType actualType:
                CompareScalar("doctype", expectedType.ToString(), actualType.ToString(), path, differences);
                break;
            default:
                CompareScalar("contenu", expected.ToString(), actual.ToString(), path, differences);
                break;
        }
    }

    private static void CompareElement(
        XElement expected,
        XElement actual,
        string path,
        ICollection<string> differences)
    {
        if (expected.Name != actual.Name)
        {
            AddDifference(differences, $"{path} : balise modifiée ({expected.Name} -> {actual.Name}).");
            return;
        }

        Dictionary<XName, string> expectedAttributes = expected.Attributes().ToDictionary(attribute => attribute.Name, attribute => attribute.Value);
        Dictionary<XName, string> actualAttributes = actual.Attributes().ToDictionary(attribute => attribute.Name, attribute => attribute.Value);
        foreach (XName name in expectedAttributes.Keys.Union(actualAttributes.Keys).OrderBy(name => name.ToString(), StringComparer.Ordinal))
        {
            bool hasExpected = expectedAttributes.TryGetValue(name, out string? expectedValue);
            bool hasActual = actualAttributes.TryGetValue(name, out string? actualValue);
            if (!hasExpected || !hasActual)
            {
                AddDifference(differences, $"{path}/@{name.LocalName} : attribut ajouté ou supprimé.");
            }
            else
            {
                CompareScalar($"attribut @{name.LocalName}", expectedValue!, actualValue!, path, differences);
            }
        }

        CompareNodes(
            expected.Nodes().Where(IsSemanticNode).ToArray(),
            actual.Nodes().Where(IsSemanticNode).ToArray(),
            path,
            differences);
    }

    private static bool IsSemanticNode(XNode node)
    {
        return node is not XText text || !string.IsNullOrWhiteSpace(text.Value);
    }

    private static string BuildNodePath(string parentPath, XNode node, int index)
    {
        string suffix = node switch
        {
            XElement element => element.Name.LocalName,
            XComment => "comment()",
            XCData => "cdata()",
            XText => "text()",
            XProcessingInstruction instruction => $"processing-instruction({instruction.Target})",
            _ => node.NodeType.ToString()
        };
        string separator = parentPath.EndsWith("/", StringComparison.Ordinal) ? string.Empty : "/";
        return $"{parentPath}{separator}{suffix}[{index + 1}]";
    }

    private static void CompareScalar(
        string label,
        string expected,
        string actual,
        string path,
        ICollection<string> differences)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            AddDifference(differences, $"{path} : {label} modifié ({Format(expected)} -> {Format(actual)}).");
        }
    }

    private static void CompareScalarIgnoreCase(
        string label,
        string expected,
        string actual,
        string path,
        ICollection<string> differences)
    {
        if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
        {
            AddDifference(differences, $"{path} : {label} modifié ({Format(expected)} -> {Format(actual)}).");
        }
    }

    private static string DescribeNode(XNode node)
    {
        return node switch
        {
            XElement element => $"<{element.Name.LocalName}>",
            XComment => "commentaire",
            XCData => "CDATA",
            XText => "texte",
            XProcessingInstruction instruction => $"instruction {instruction.Target}",
            _ => node.NodeType.ToString()
        };
    }

    private static void AddDifference(ICollection<string> differences, string difference)
    {
        if (differences.Count < MaximumDifferences)
        {
            differences.Add(difference);
        }
    }

    private static string Format(string value)
    {
        string compact = value.ReplaceLineEndings(" ");
        return compact.Length <= 80 ? $"'{compact}'" : $"'{compact[..77]}...'";
    }
}
