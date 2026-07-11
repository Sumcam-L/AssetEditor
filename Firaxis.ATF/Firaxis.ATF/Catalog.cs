using System.Collections.Generic;

namespace Firaxis.ATF;

public class Catalog
{
	public IEnumerable<Attachment> Attachments { get; set; }

	public Attachment Dump { get; set; }

	public Attachment Log { get; set; }

	public Catalog()
	{
		Dump = new Attachment();
		Log = new Attachment();
		Attachments = new List<Attachment>();
	}
}
