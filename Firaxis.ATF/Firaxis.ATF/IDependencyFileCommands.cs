using System.Collections.Generic;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public interface IDependencyFileCommands : ICommandClient
{
	IEnumerable<CommandInfo> Commands { get; }
}
