using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.CivTech.Packages;

namespace Firaxis.ATF;

public static class SmartPreviewWindowHelpers
{
	public static void OpenAsset(IPreviewWindow window, string previewModuleName, IInstanceEntityAdapter asset, int slotID, IInstanceSet instances, IXLPClassSet xlpClasses)
	{
		window.SetPreviewModule(previewModuleName);
		if (!window.IsUnbound())
		{
			window.OpenAsset(asset.InstanceEntity, slotID, delayBind: false);
			return;
		}
		foreach (IPreviewerSlotInfo slotInfo in window.GetSlotsInfo())
		{
			if (slotInfo.SlotID != slotID)
			{
				string text = slotInfo.DefaultAsset;
				if (slotInfo.SlotID == 0 && string.IsNullOrEmpty(text))
				{
					text = previewModuleName + "_DEFAULT_ASSET";
				}
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				IXLPClass iXLPClass = xlpClasses.Items.FirstOrDefault((IXLPClass cls) => cls.Name == slotInfo.XLPClass);
				if (xlpClasses != null)
				{
					IInstanceEntity instanceEntity = instances.LoadEntityIfUnique(slotInfo.DefaultAsset, iXLPClass.InstanceType);
					if (instanceEntity == null)
					{
						instanceEntity = instances.Push<IAssetInstance>(text);
						instanceEntity.ClassName = iXLPClass.AllowedEntityClasses.FirstOrDefault();
					}
					window.OpenAsset(instanceEntity, slotInfo.SlotID, delayBind: true);
				}
			}
			else
			{
				window.OpenAsset(asset.InstanceEntity, slotID, delayBind: true);
			}
		}
		window.BindAssetsToWindow();
	}
}
