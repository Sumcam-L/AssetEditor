using System;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.TextureExport;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.AssetEditing;

public class EnvironmentLightDocument : BaseInstanceEntityDocument, ISourceFileProvider
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
		base.DocumentSpecificReimportStep();
		IEnvironmentLightEditingContext environmentLightEditingContext = base.DomNode.As<IEnvironmentLightEditingContext>();
		if (environmentLightEditingContext != null && environmentLightEditingContext.Source != null)
		{
			EnvironmentMapParameterization eSourceParametrization = (environmentLightEditingContext.Source.IsCubeMap ? EnvironmentMapParameterization.ENVMAP_CUBE : EnvironmentMapParameterization.ENVMAP_LATLONG);
			environmentLightEditingContext.CreateCube(eSourceParametrization, environmentLightEditingContext.LastSampleCount, environmentLightEditingContext.Cube.Width);
		}
		base.DomNode.As<BaseEntityPropertyContext>().History = new CommandHistory();
	}
}
