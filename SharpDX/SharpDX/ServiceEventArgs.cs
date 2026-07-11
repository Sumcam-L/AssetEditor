using System;

namespace SharpDX;

public class ServiceEventArgs : EventArgs
{
	public Type ServiceType { get; private set; }

	public object Instance { get; private set; }

	public ServiceEventArgs(Type serviceType, object serviceInstance)
	{
		ServiceType = serviceType;
		Instance = serviceInstance;
	}
}
