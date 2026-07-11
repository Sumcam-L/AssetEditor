using System.Windows.Input;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications;

public class CommandDef
{
	public readonly object CommandTag;

	public readonly object MenuTag;

	public readonly object GroupTag;

	public readonly string Text;

	public readonly string[] MenuPath = EmptyArray<string>.Instance;

	public readonly string Description;

	public readonly object ImageSourceKey;

	public readonly InputGesture[] InputGestures = EmptyArray<InputGesture>.Instance;

	public readonly CommandVisibility Visibility = CommandVisibility.Menu;

	public CommandDef(object commandTag)
	{
		Requires.NotNull(commandTag, "commandTag");
		CommandTag = commandTag;
	}

	public CommandDef(object commandTag, object menuTag, object groupTag, string text, string description)
		: this(commandTag)
	{
		MenuTag = menuTag;
		GroupTag = groupTag;
		Text = text;
		Description = description;
	}

	public CommandDef(object commandTag, object menuTag, object groupTag, string text, string[] menuPath, string description, object imageSourceKey, InputGesture[] inputGestures, CommandVisibility visibility)
		: this(commandTag, menuTag, groupTag, text, description)
	{
		ImageSourceKey = imageSourceKey;
		if (menuPath != null)
		{
			MenuPath = menuPath;
		}
		if (inputGestures != null)
		{
			InputGestures = inputGestures;
		}
		Visibility = visibility;
	}
}
