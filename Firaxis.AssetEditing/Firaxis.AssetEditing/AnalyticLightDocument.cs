using System;
using System.Collections.Generic;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetEditing;

public class AnalyticLightDocument : BaseInstanceEntityDocument, IPreviewableDocument, IDocument, IResource, IPreviewContext
{
	public IAnalyticLightInstance AnalyticLight => EntityAdapter.InstanceEntity as IAnalyticLightInstance;

	public virtual IInstanceEntityAdapter EntityAdapter => base.DomNode.As<IInstanceEntityAdapter>();

	public IEnumerable<string> AvailablePreviewModuleNames
	{
		get
		{
			yield return PreviewModule;
		}
	}

	public IPreviewWindow PreviewWindow { get; set; }

	public string PreviewModule
	{
		get
		{
			IClassEntity classEntity = base.CivTechService.PrimaryProject.Config.Classes.FindForInstance(AnalyticLight);
			if (classEntity == null)
			{
				return "";
			}
			return classEntity.PreviewModuleName;
		}
		set
		{
		}
	}

	public event EventHandler PreviewModuleChanged;

	public virtual void RaisePreviewModuleChanged()
	{
		this.PreviewModuleChanged?.Invoke(this, EventArgs.Empty);
	}
}
