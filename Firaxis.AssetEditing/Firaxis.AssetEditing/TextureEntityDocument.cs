using System;
using System.Collections.Generic;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.AssetEditing;

public class TextureEntityDocument : BaseInstanceEntityDocument, ISourceFileProvider, IPreviewableDocument, IDocument, IResource, IPreviewContext
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

	public IEnumerable<string> AvailablePreviewModuleNames
	{
		get
		{
			yield return PreviewModule;
		}
	}

	public string PreviewModule
	{
		get
		{
			IClassEntity classEntity = base.CivTechService.PrimaryProject.Config.Classes.FindForInstance(base.InstanceEntity);
			if (classEntity == null)
			{
				return string.Empty;
			}
			return classEntity.PreviewModuleName;
		}
		set
		{
		}
	}

	public IPreviewWindow PreviewWindow { get; set; }

	public IInstanceEntityAdapter EntityAdapter => base.DomNode.As<IInstanceEntityAdapter>();

	public event EventHandler PreviewModuleChanged;

	protected override void DocumentSpecificReimportStep()
	{
		base.DomNode.As<BaseEntityPropertyContext>().History = new CommandHistory();
		base.DocumentSpecificReimportStep();
	}
}
