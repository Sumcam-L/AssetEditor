using System.Diagnostics;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

[DebuggerDisplay("Layer Info: { key }")]
public class RawLayerInfo : LayerInfo
{
	private string key;

	public override string Key => key;

	public byte[] Data { get; private set; }

	public RawLayerInfo(string key)
	{
		this.key = key;
	}

	public RawLayerInfo(PsdBinaryReader reader, string key, int dataLength)
	{
		this.key = key;
		Data = reader.ReadBytes(dataLength);
	}
}
