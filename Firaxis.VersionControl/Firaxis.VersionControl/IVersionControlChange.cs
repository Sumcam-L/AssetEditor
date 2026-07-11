using System;
using System.Collections.Generic;

namespace Firaxis.VersionControl;

public interface IVersionControlChange
{
	int Number { get; }

	DateTime Date { get; }

	string Email { get; }

	string Description { get; set; }

	IEnumerable<VersionControlFileChange> Files { get; }

	void AddFiles(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result);

	void RemoveFiles(IEnumerable<VersionControlPath> files, Action<RequestResultCode> result);

	void Submit(Action<RequestResultCode> result);

	void Revert(Action<RequestResultCode> result);
}
