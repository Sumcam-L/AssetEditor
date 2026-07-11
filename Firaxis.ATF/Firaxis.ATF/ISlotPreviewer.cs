using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.ATF;

public interface ISlotPreviewer
{
	void BeginBatchMode();

	void EndBatchMode();

	void ForceRecook(int slotID);

	IKnobSet GetKnobSet(IInstanceEntity entity, int slotID);

	void OpenAttachment(int parentAssetSlot, IAttachmentPoint attachmentPoint, string attachedAssetName, InstanceType attachedAssetType);

	void OpenEntityInSlot(IInstanceEntity entity, int slotID);

	void ClearSlot(int slotID);

	void RemoveAttachment(int parentAssetSlot, string attachmentPointName, string attachedAssetName, InstanceType attachedAssetType);
}
