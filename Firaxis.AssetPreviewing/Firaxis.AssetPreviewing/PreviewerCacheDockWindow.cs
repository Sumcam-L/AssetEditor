using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.AssetPreviewing;

[Export(typeof(IInitializable))]
[Export(typeof(PreviewerCacheDockWindow))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PreviewerCacheDockWindow : IInitializable, IControlHostClient, IDisposable
{
	[Import(AllowDefault = false)]
	private IControlHostService m_controlHostService = null;

	private ICivTechService m_civTechService;

	private IPreviewerCacheService m_previewerCacheService;

	private PreviewerCacheControl m_previewCacheControl;

	[ImportingConstructor]
	public PreviewerCacheDockWindow(ICivTechService civTechSvc, IPreviewerCacheService pcs)
	{
		m_civTechService = civTechSvc;
		m_previewerCacheService = pcs;
		m_previewCacheControl = new PreviewerCacheControl(pcs);
	}

	public void Initialize()
	{
		if (m_controlHostService != null)
		{
			m_previewerCacheService.EntityAdded += PreviewerCacheService_EntityAdded;
			m_previewerCacheService.EntityRemoved += PreviewerCacheService_EntityRemoved;
			m_controlHostService.RegisterControl(m_previewCacheControl, "Asset Previewer Cache", "Asset previewing cached entity view", StandardControlGroup.Bottom, null, this);
			m_controlHostService.Show(m_previewCacheControl);
		}
	}

	private void PreviewerCacheService_EntityRemoved(object sender, PreviewerCacheRemoved e)
	{
		m_previewCacheControl?.BeginInvoke((Action)delegate
		{
			m_previewCacheControl.RemoveCachedAsset(e.Removed);
		});
	}

	private void PreviewerCacheService_EntityAdded(object sender, PreviewerCacheAdded e)
	{
		string insName = "Unknown";
		InstanceType insType = InstanceType.IT_INVALID;
		StaticMethods.GetInstanceNameAndType(m_civTechService.ProjectMapService, e.Added.LocalPath, out insName, out insType);
		m_previewCacheControl?.BeginInvoke((Action)delegate
		{
			m_previewCacheControl.AddCachedAsset(insType, insName, e.Added);
		});
	}

	public void Activate(Control control)
	{
	}

	public void Deactivate(Control control)
	{
	}

	public bool Close(Control control)
	{
		return true;
	}

	public void Dispose()
	{
		m_previewCacheControl?.Dispose();
	}
}
