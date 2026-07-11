using System;

namespace Firaxis.Utility;

public interface IServiceProviderProvider
{
	IServiceProvider ServiceProvider { get; set; }

	T GetService<T>();
}
