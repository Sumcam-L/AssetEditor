using System.Collections.Generic;

namespace Firaxis.Asset;

public interface IAttachmentPointNameProvider
{
	IEnumerable<string> AttachementPointNames { get; }
}
