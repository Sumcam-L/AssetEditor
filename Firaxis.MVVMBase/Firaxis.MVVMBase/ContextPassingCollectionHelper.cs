using System;
using System.Linq;
using System.Reflection;

namespace Firaxis.MVVMBase;

public static class ContextPassingCollectionHelper
{
	private static MethodInfo _ProvideContextForObject;

	private static MethodInfo _RemoveContextFromObject;

	public static MethodInfo ProvideContextForObject => _ProvideContextForObject ?? (_ProvideContextForObject = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault((Assembly a) => a.GetName().Name == "WindowsBase")?.GetType("MS.Internal.InheritanceContextHelper")?.GetMethod("ProvideContextForObject", BindingFlags.Static | BindingFlags.NonPublic));

	public static MethodInfo RemoveContextFromObject => _RemoveContextFromObject ?? (_RemoveContextFromObject = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault((Assembly assembly) => assembly?.GetName().Name == "WindowsBase")?.GetType("MS.Internal.InheritanceContextHelper")?.GetMethod("RemoveContextFromObject", BindingFlags.Static | BindingFlags.NonPublic));
}
