using System.ComponentModel;
using System.Reflection;

namespace Firaxis.Utility;

public static class TypeDescriptorContextExtension
{
	public static T GetService<T>(this ITypeDescriptorContext context)
	{
		if (context.Instance is T)
		{
			return (T)context.Instance;
		}
		PropertyInfo property = context.GetType().GetProperty("OwnerGrid", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (property != null)
		{
			IServiceProviderProvider serviceProviderProvider = (IServiceProviderProvider)property.GetValue(context, null);
			if (serviceProviderProvider != null)
			{
				return serviceProviderProvider.GetService<T>();
			}
		}
		return default(T);
	}
}
