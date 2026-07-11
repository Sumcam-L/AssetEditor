using System.Collections.Generic;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public interface ICommandContext
{
	ICommandClient CommandClient { get; }

	IEnumerable<CommandInfo> Commands { get; }
}
