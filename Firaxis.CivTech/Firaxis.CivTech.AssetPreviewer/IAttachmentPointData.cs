using System;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IAttachmentPointData : IAssemblyInstance, IDisposable
{
	string AssetName { get; set; }

	InstanceType AssetType { get; set; }

	IAttachmentPoint AttachmentPoint { get; set; }
}
