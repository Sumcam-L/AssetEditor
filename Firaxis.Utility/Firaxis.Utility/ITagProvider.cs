using Firaxis.Reflection;

namespace Firaxis.Utility;

public interface ITagProvider
{
	[NoSerialize]
	object Tag { get; set; }
}
