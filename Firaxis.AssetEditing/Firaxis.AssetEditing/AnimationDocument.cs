using System;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.AssetEditing;

public class AnimationDocument : BaseInstanceEntityDocument, ISourceFileProvider
{
	public EntityID ReferencingEntity => new EntityID(base.InstanceEntity);

	public Uri SourceFileUri
	{
		get
		{
			Uri result = null;
			if (base.InstanceEntity is IImportedEntity importedEntity)
			{
				string localPath = base.CivTechService.PrimaryProject.VersionControl.GetLocalPath(importedEntity.SourceFilePath);
				if (!string.IsNullOrEmpty(localPath))
				{
					result = new Uri(localPath);
				}
			}
			return result;
		}
	}

	protected override void DocumentSpecificReimportStep()
	{
		base.DomNode.As<BaseEntityPropertyContext>().History = new CommandHistory();
		base.DocumentSpecificReimportStep();
	}
}
