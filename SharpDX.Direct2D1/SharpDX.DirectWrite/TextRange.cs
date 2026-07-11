namespace SharpDX.DirectWrite;

public struct TextRange
{
	public int StartPosition;

	public int Length;

	public TextRange(int startPosition, int length)
	{
		StartPosition = startPosition;
		Length = length;
	}
}
