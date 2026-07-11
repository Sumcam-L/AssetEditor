namespace Sce.Atf.Wpf;

public interface IToolBarItem
{
	object Tag { get; }

	object ToolBarTag { get; }

	bool IsVisible { get; }
}
