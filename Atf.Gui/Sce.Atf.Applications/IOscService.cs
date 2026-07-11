using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications;

public interface IOscService
{
	event EventHandler<OscMessageReceivedArgs> MessageReceived;

	void Send(IEnumerable<Tuple<string, object>> addressesAndData);
}
