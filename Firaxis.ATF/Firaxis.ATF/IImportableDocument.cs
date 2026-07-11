using System.Collections.Generic;
using Sce.Atf;

namespace Firaxis.ATF;

public interface IImportableDocument : IEntityDocument, IProjectSpecificDocument, IDocument, IResource
{
	bool IsReadyForExport(ICollection<string> errorMessages);
}
