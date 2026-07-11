using System;

namespace Firaxis.Utility;

public interface IServiceProviderEx : IServiceProvider
{
	T GetService<T>();
}
