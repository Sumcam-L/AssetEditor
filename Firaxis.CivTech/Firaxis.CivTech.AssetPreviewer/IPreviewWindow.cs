using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IPreviewWindow : IDisposable
{
	string AssetName { get; }

	void OnMouseDown(MouseButtons button);

	void OnMouseUp(MouseButtons button);

	void OnMouseWheel(int nWheelDelta);

	void OnMouseMove(int nMouseX, int nMouseY);

	void OnMouseDoubleClick(MouseButtons button);

	void OnKeyDown(int keyValue);

	void OnKeyUp(int keyUp);

	bool SetPreviewModule(string previewModuleName);

	string GetPreviewModule();

	IEnumerable<IPreviewerSlotInfo> GetSlotsInfo();

	bool IsUnbound();

	void ForceRefreshAsset(int slotID);

	void OpenAsset(IInstanceEntity asset, int slotID, bool delayBind);

	void ClearSlot(int slotID, bool delayBind);

	void AddAttachment(IInstanceEntity attachedAsset, IAttachmentPointData attachmentData, int slotID, bool delayBind);

	IEnumerable<string> GetAttachmentOverrides(string attachmentPointName, InstanceType attachedAssetType, int slotID);

	void RemoveAttachment(string attachmentPointName, string attachedAssetName, InstanceType attachedAssetType, int slotID, bool delayBind);

	void BindAssetsToWindow();

	void UpdateAsset(IEnumerable<IEntityChangedEvent> changes, int slotID);

	IKnobSet GetPreviewModuleKnobSet();

	IKnobSet GetEntityKnobSet(IInstanceEntity entity, int slotID);

	IEnumerable<PickResult> PickPoint(int x, int y, float slack);

	IWidget CreateWidget(string WidgetType, IValueSet arguments, object BoundObject);
}
