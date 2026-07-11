using System;

namespace SharpDX;

public interface IServiceRegistry : IServiceProvider
{
	event EventHandler<ServiceEventArgs> ServiceAdded;

	event EventHandler<ServiceEventArgs> ServiceRemoved;

	T GetService<T>();

	void AddService(Type type, object provider);

	void AddService<T>(T provider);

	void RemoveService(Type type);
}
