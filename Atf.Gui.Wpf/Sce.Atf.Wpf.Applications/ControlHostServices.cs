using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications;

public static class ControlHostServices
{
	public static IControlInfo RegisterControl(this IControlHostService controlHostService, object control, string name, string description, StandardControlGroup group, string id, IControlHostClient client)
	{
		ControlDef definition = new ControlDef
		{
			Name = name,
			Description = description,
			Group = group,
			Id = id
		};
		return controlHostService.RegisterControl(definition, control, client);
	}

	public static IControlInfo RegisterControl(this IControlHostService controlHostService, object control, string name, string description, StandardControlGroup group, object imageSourceKey, string id, IControlHostClient client)
	{
		ControlDef definition = new ControlDef
		{
			Name = name,
			Description = description,
			Group = group,
			Id = id,
			ImageSourceKey = imageSourceKey
		};
		return controlHostService.RegisterControl(definition, control, client);
	}

	public static void UnregisterContent(this IControlHostService controlHostService, IControlInfo info)
	{
		controlHostService.UnregisterContent(info.Content);
	}

	public static void Show(this IControlHostService controlHostService, IControlInfo info)
	{
		controlHostService.Show(info.Content);
	}
}
