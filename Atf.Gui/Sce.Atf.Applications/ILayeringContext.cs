namespace Sce.Atf.Applications;

public interface ILayeringContext : IVisibilityContext, ITreeView, IItemView
{
	void SetActiveItem(object item);
}
