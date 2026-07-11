using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.ATF;

public class AssetData
{
	public KnobSetData AssetKnobSetData { get; set; }

	public string AssetName { get; set; }

	public InstanceType AssetType { get; set; }

	public List<SerializableAttachmentPointData> AttachedAssetData { get; set; }

	public int SlotID { get; set; }

	public AssetData()
		: this(string.Empty, InstanceType.IT_INVALID)
	{
	}

	public AssetData(IInstanceEntity entity)
		: this(entity.Name, entity.Type)
	{
	}

	public AssetData(string assetName, InstanceType assetType)
	{
		AssetName = assetName;
		AssetType = assetType;
		AttachedAssetData = new List<SerializableAttachmentPointData>();
		AssetKnobSetData = new KnobSetData();
	}

	public static AssetData CreateAssetData(string assetName, InstanceType assetType)
	{
		return new AssetData(assetName, assetType);
	}

	public void SetAssetKnobData(IKnobSet knobSet)
	{
		AssetKnobSetData.BuildKnobSetData(knobSet);
	}

	public void SetAttachedAssetData(IEnumerable<AttachmentPointData> attachments)
	{
		AttachedAssetData.Clear();
		foreach (AttachmentPointData attachment in attachments)
		{
			AttachedAssetData.Add(new SerializableAttachmentPointData(attachment.AssetName, attachment.AssetType, attachment.AttachmentPoint));
		}
	}
}
