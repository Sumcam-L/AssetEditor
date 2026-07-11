using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;

namespace Firaxis.ATF;

public interface IInstanceEntityDocument : IImportableDocument, IEntityDocument, IProjectSpecificDocument, IDocument, IResource
{
	bool HasNameSet(string newEntityName);

	bool IsBasedOnEntity(InstanceType entityType, string entityName);

	bool MeetsSavePreconditions(ICollection<string> errorMessages);
}
