using System;
using System.Collections.Generic;

namespace Firaxis.ATF;

public interface IWorkspaceChangeMediator
{
	void AddChangeToQueue(Uri uri);

	void AddChangesToQueue(IEnumerable<Uri> uris);
}
