namespace SharpDX.DXGI;

public struct SampleDescription
{
	public int Count;

	public int Quality;

	public SampleDescription(int count, int quality)
	{
		Count = count;
		Quality = quality;
	}

	public override string ToString()
	{
		return $"{{{Count}, {Quality}}}";
	}
}
