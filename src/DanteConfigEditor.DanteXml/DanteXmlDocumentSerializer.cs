using System.Xml.Linq;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.DanteXml;

public interface IDanteXmlDocumentSerializer
{
    byte[] Serialize(DanteProject project);

    DanteXmlOpenResult Load(Stream stream, string sourceIdentity);
}

public sealed class DanteXmlDocumentSerializer : IDanteXmlDocumentSerializer
{
    private readonly Profiles.IDanteXmlProfileDetector _profileDetector;

    public DanteXmlDocumentSerializer(
        Profiles.IDanteXmlProfileDetector? profileDetector = null)
    {
        _profileDetector = profileDetector ?? new Profiles.DanteXmlProfileDetector();
    }

    public byte[] Serialize(DanteProject project)
    {
        ArgumentNullException.ThrowIfNull(project);

        using MemoryStream stream = new();
        project.Document.Save(stream, SaveOptions.DisableFormatting);
        return stream.ToArray();
    }

    public DanteXmlOpenResult Load(Stream stream, string sourceIdentity)
    {
        DanteProject project = DanteProject.LoadFromStream(sourceIdentity, stream);
        return new DanteXmlOpenResult(project, _profileDetector.Detect(project));
    }
}
