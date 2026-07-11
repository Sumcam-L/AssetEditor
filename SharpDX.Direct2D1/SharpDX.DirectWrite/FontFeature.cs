namespace SharpDX.DirectWrite;

public struct FontFeature
{
	public FontFeatureTag NameTag;

	public int Parameter;

	public FontFeature(FontFeatureTag nameTag, int parameter)
	{
		NameTag = nameTag;
		Parameter = parameter;
	}
}
