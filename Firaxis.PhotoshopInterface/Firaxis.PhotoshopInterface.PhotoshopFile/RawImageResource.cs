namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class RawImageResource : ImageResource
{
	private ResourceID id;

	public byte[] Data { get; private set; }

	public override ResourceID ID => id;

	public RawImageResource(ResourceID resourceId, string name)
		: base(name)
	{
		id = resourceId;
	}

	public RawImageResource(PsdBinaryReader reader, string signature, ResourceID resourceId, string name, int numBytes)
		: base(name)
	{
		base.Signature = signature;
		id = resourceId;
		Data = reader.ReadBytes(numBytes);
	}
}
