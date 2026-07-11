using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable;

public interface ILabelEditAdapter
{
	void BeginEdit(INamingContext namingContext, object item, DiagramLabel label);

	void EndEdit();
}
