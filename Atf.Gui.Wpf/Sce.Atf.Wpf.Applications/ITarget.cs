using System;

namespace Sce.Atf.Wpf.Applications;

public interface ITarget : IEquatable<ITarget>
{
	string Name { get; }

	string Host { get; }

	string HardwareId { get; }

	bool IsConnected { get; }

	string ConnectionInfo { get; }

	string Status { get; }

	string ProtocolId { get; }

	string ProtocolName { get; }
}
