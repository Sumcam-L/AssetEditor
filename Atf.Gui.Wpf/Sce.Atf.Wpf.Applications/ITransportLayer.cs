using System;
using System.Collections.Generic;
using System.Threading;

namespace Sce.Atf.Wpf.Applications;

public interface ITransportLayer : IDisposable
{
	bool Connected { get; }

	Exception Exception { get; }

	TimeSpan ConnectTimeout { get; set; }

	string DisconnectMessage { get; }

	AutoResetEvent TransportEvent { get; }

	void BeginConnect();

	void BeginSend(byte[] data);

	void Close();

	Queue<byte[]> GetIncomingPackets();
}
