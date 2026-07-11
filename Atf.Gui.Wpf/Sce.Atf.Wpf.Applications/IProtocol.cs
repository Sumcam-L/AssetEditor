using System;
using System.Collections.Generic;

namespace Sce.Atf.Wpf.Applications;

public interface IProtocol
{
	string Name { get; }

	string Id { get; }

	Version Version { get; }

	bool CanFindTargets { get; }

	bool CanCreateUserTarget { get; }

	IEnumerable<ITarget> FindTargets();

	ITransportLayer CreateTransportLayer(ITarget target);

	ITarget CreateUserTarget(string[] args);

	bool EditUserTarget(ITarget target);
}
