using Firaxis.CivTech.AssetObjects;

namespace Firaxis.ATF;

public class SerializableAttachmentPointData
{
	public string AssetName { get; set; }

	public InstanceType AssetType { get; set; }

	public string AttachmentPointName { get; set; }

	public string BoneName { get; set; }

	public string ModelInstanceName { get; set; }

	public SerializableAttachmentPointData()
	{
	}

	public SerializableAttachmentPointData(string assetName, InstanceType assetType, IAttachmentPoint attachment)
	{
		AssetName = assetName;
		AssetType = assetType;
		AttachmentPointName = attachment.Name;
		BoneName = attachment.BoneName;
		ModelInstanceName = attachment.ModelInstanceName;
	}
}
