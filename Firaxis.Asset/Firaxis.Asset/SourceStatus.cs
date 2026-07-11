namespace Firaxis.Asset;

public struct SourceStatus
{
	public string File;

	public string Type;

	public ulong Time;

	public ulong Change;

	public int Rev;

	public int OtherOpen;

	private static SourceStatus empty = default(SourceStatus);

	public static SourceStatus Empty => empty;
}
