using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class AssetComponentAdapterBase : EntityComponentAdapterBase
{
	private protected AssetAdapter AssetAdapter { get; set; }

	protected string LocalPantry => AssetAdapter.CivTechService.PrimaryProject.Paths.GamePantry;

	protected IAssetInstance ParentAsset => AssetAdapter.Asset;

	protected TransactionContext ParentContext => AssetAdapter.As<TransactionContext>();

	protected IProjectConfig ProjectConfig => AssetAdapter.CivTechService.PrimaryProject.Config;

	protected override void OnParentNodeSet()
	{
		base.OnParentNodeSet();
		AssetAdapter = base.DomNode.GetRoot().As<AssetAdapter>();
		PlatformAssert.If(base.DomNode == null);
		PlatformAssert.If(base.DomNode.GetRoot() == null);
		PlatformAssert.If(AssetAdapter == null);
	}
}
