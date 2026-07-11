using System.Collections.Generic;
using Sce.Atf.Applications;

namespace Firaxis.AssetEditing;

public interface ITimelineTrackCommands : ICommandClient
{
	float TargetTime { get; set; }

	IEnumerable<CommandInfo> Commands { get; }
}
