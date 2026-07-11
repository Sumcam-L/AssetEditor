namespace Sce.Atf.Applications;

public static class ContextRegistries
{
	public static T GetCommandTarget<T>(this IContextRegistry contextRegistry) where T : class
	{
		T val = null;
		ISelectionContext activeContext = contextRegistry.GetActiveContext<ISelectionContext>();
		if (activeContext != null)
		{
			val = activeContext.GetLastSelected<T>();
		}
		if (val == null)
		{
			val = contextRegistry.GetActiveContext<T>();
		}
		return val;
	}
}
