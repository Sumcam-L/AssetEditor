using System.Reflection;

namespace Sce.Atf.Applications;

public static class WindowLayoutServiceExtension
{
	public static bool IsCurrent(this IWindowLayoutService windowLayoutService, string layoutName)
	{
		if (windowLayoutService == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(layoutName))
		{
			return false;
		}
		return string.Compare(windowLayoutService.CurrentLayout, layoutName) == 0;
	}

	public static void SetOrAddLayout(this IWindowLayoutService windowLayoutService, IDockStateProvider dockStateProvider, string layoutName, object dockState)
	{
		dockStateProvider.DockState = dockState;
		windowLayoutService.CurrentLayout = layoutName;
	}

	public static void AddLayout(this IWindowLayoutService windowLayoutService, string newLayoutName, object dockState)
	{
		MethodInfo method = windowLayoutService.GetType().GetMethod("AddLayout");
		object[] parameters = new object[2] { newLayoutName, dockState };
		method.Invoke(windowLayoutService, parameters);
	}
}
