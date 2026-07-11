using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetEditing;

public class LightRigDocument : BaseInstanceEntityDocument, IPreviewableDocument, IDocument, IResource, IPreviewContext
{
	private string m_selectedPreviewModule = "";

	private ISet<string> m_knownPreviewModules;

	public virtual IInstanceEntityAdapter EntityAdapter => base.DomNode.As<IInstanceEntityAdapter>();

	public ILightRigInstance LightRig => base.InstanceEntity as ILightRigInstance;

	private ISet<string> KnownPreviewModules
	{
		get
		{
			if (m_knownPreviewModules == null)
			{
				m_knownPreviewModules = new HashSet<string>();
				foreach (IAssetClass item in base.CivTechService.PrimaryProject.Config.Classes.Items.OfType<IAssetClass>())
				{
					m_knownPreviewModules.Add(item.PreviewModuleName);
				}
			}
			return m_knownPreviewModules;
		}
	}

	public string PreviewModule
	{
		get
		{
			if (string.IsNullOrEmpty(m_selectedPreviewModule))
			{
				IEnumerable<string> availablePreviewModuleNames = AvailablePreviewModuleNames;
				if (availablePreviewModuleNames.Count() == 1)
				{
					m_selectedPreviewModule = availablePreviewModuleNames.First();
				}
			}
			return m_selectedPreviewModule;
		}
		set
		{
			m_selectedPreviewModule = value;
		}
	}

	public IPreviewWindow PreviewWindow { get; set; }

	public IEnumerable<string> AvailablePreviewModuleNames => GetPreviewerModulesThatSupportLightRigClass(LightRig.ClassName);

	public event EventHandler PreviewModuleChanged;

	public IEnumerable<string> GetAllowedLightRigClasses(string previewModuleName)
	{
		return base.DomNode.As<InstanceEntityAdapter>().PreviewerService.GetAllowedLightRigClasses(previewModuleName);
	}

	public virtual void RaisePreviewModuleChanged()
	{
		this.PreviewModuleChanged?.Invoke(this, EventArgs.Empty);
	}

	private IEnumerable<string> GetPreviewerModulesThatSupportLightRigClass(string lightRigClassName)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (string knownPreviewModule in KnownPreviewModules)
		{
			try
			{
				if (GetAllowedLightRigClasses(knownPreviewModule).Contains(lightRigClassName))
				{
					list.Add(knownPreviewModule);
				}
			}
			catch (System.Exception ex)
			{
				if (!ex.Message.Contains("No known previewer module"))
				{
					throw ex;
				}
				list2.Add(knownPreviewModule);
			}
		}
		foreach (string item in list2)
		{
			KnownPreviewModules.Remove(item);
		}
		return list;
	}
}
