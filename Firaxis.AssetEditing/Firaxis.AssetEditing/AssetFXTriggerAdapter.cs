using System.Drawing;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer._INTERNAL;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class AssetFXTriggerAdapter : TriggerAdapter, IDurableTriggerAdapter
{
	public override Image TriggerImage => ResourceUtil.GetImage16(Resources.AssetFileIcon);

	public string VFXAsset
	{
		get
		{
			return GetAttribute<string>(EntitySchema.AssetFXTriggerType.VFXAssetAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.AssetFXTriggerType.VFXAssetAttribute, value);
		}
	}

	public string AttachmentPoint
	{
		get
		{
			return GetAttribute<string>(EntitySchema.AssetFXTriggerType.AttachmentPointNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.AssetFXTriggerType.AttachmentPointNameAttribute, value);
		}
	}

	public float Duration
	{
		get
		{
			return GetAttribute<float>(EntitySchema.AssetFXTriggerType.DurationAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.AssetFXTriggerType.DurationAttribute, value);
		}
	}

	public override void UpdateNativeDataOnlyFromAdapter()
	{
		base.UpdateNativeDataOnlyFromAdapter();
		base.Trigger.FXName = VFXAsset;
		base.Trigger.AttachmentPointName = AttachmentPoint;
		base.Trigger.Duration = Duration;
	}

	public override void UpdateAdapterDataOnlyFromNative(ITrigger trigger)
	{
		base.UpdateAdapterDataOnlyFromNative(trigger);
		BugSubmitter.SilentAssert(trigger.Type == TriggerType.TT_ASSET_VFX, "@summary AssetFXTriggerAdapter.Update called with wrong type of ITrigger @assign bwhitman");
		VFXAsset = trigger.FXName;
		AttachmentPoint = trigger.AttachmentPointName;
		Duration = trigger.Duration;
	}

	protected override void OnDomNodeAttributeChanged(AttributeEventArgs attr)
	{
		base.OnDomNodeAttributeChanged(attr);
		if (attr.AttributeInfo == EntitySchema.AssetFXTriggerType.VFXAssetAttribute)
		{
			base.Trigger.FXName = (string)attr.NewValue;
			IPreviewableDocument previewableDocument = base.DomNode.GetRoot().As<IPreviewableDocument>();
			if (previewableDocument == null)
			{
				return;
			}
			if (previewableDocument.PreviewWindow is WindowWrapper windowWrapper)
			{
				AssetPreviewPack packInSlot = windowWrapper.GetPackInSlot(0);
				if (packInSlot != null)
				{
					packInSlot.RefreshContents();
					MessageBox.Show("资产已刷新！");
				}
			}
			else
			{
				MessageBox.Show("错误：找不到关联的预览窗口。");
			}
		}
		else if (attr.AttributeInfo == EntitySchema.AssetFXTriggerType.AttachmentPointNameAttribute)
		{
			base.Trigger.AttachmentPointName = (string)attr.NewValue;
		}
		else if (attr.AttributeInfo == EntitySchema.AssetFXTriggerType.DurationAttribute)
		{
			base.Trigger.Duration = (float)attr.NewValue;
		}
	}
}
