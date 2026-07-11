using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetEditing;

public class LightRigContext : BaseEntityPropertyContext, ILightRigEditorContext, IEntityEditorContext, IObservableContext, IAssetBrowserServiceProvider
{
	private IAssetBrowserDialogService m_assetBrowserService;

	public ILightRigInstance LightRigInstance => base.DomNode.As<LightRigDocument>().LightRig;

	public IPropertyEditingListContext AnalyticLightContext => base.DomNode.As<LightRigAdapter>().AnalyticLightSet;

	public IPropertyEditingListContext AnimationSetContext => base.DomNode.As<LightRigAdapter>().AnimationBindingSet;

	public IPropertyEditingListContext EnvironmentLightContext => base.DomNode.As<LightRigAdapter>().EnvironmentLightSet;

	public IAssetBrowserDialogService AssetBrowserService
	{
		get
		{
			return m_assetBrowserService;
		}
		set
		{
			m_assetBrowserService = value;
		}
	}

	protected override void Dispose(bool bDisposing)
	{
		if (bDisposing)
		{
			base.GUI?.Dispose();
			base.GUI = null;
		}
		base.Dispose(bDisposing);
	}
}
