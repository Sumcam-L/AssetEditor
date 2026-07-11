using Firaxis.Utility;

namespace Firaxis.CivTech.AssetObjects;

public static class IValueSetExtensions
{
	public static IValueSet Clone(this IValueSet values)
	{
		IValueSet valueSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IValueSet>();
		valueSet.CopyFrom(values);
		return valueSet;
	}
}
