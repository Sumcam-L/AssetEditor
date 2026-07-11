using System.Collections;

namespace Firaxis.Utility;

public interface ICollectionProvider
{
	ICollection PlatformCollection { get; }
}
