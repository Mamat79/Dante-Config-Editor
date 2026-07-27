using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.DanteXml.Profiles;

public interface IDanteXmlProfileDetector
{
    DanteXmlProfileDescriptor Detect(DanteProject project);
}
