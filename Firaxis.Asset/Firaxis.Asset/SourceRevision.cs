using System;

namespace Firaxis.Asset;

public class SourceRevision
{
	public int Rev { get; set; }

	public ulong Change { get; set; }

	public string User { get; set; }

	public string Description { get; set; }

	public DateTime Date { get; set; }
}
