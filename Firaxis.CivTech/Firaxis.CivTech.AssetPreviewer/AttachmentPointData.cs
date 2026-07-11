using System;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.AssetPreviewer;

public class AttachmentPointData : IAttachmentPointData, IAssemblyInstance, IDisposable
{
	public string AssetName { get; set; }

	public InstanceType AssetType { get; set; }

	public IAttachmentPoint AttachmentPoint { get; set; }

	public AttachmentPointData()
	{
		AssetName = string.Empty;
		AssetType = InstanceType.IT_INVALID;
		AttachmentPoint = null;
	}

	public AttachmentPointData(string assetName, InstanceType assetType, IAttachmentPoint attachmentPoint)
	{
		AssetName = assetName;
		AssetType = assetType;
		AttachmentPoint = attachmentPoint;
	}

	public void Dispose()
	{
	}
}
