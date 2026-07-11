using System.Drawing;

namespace Sce.Atf.Applications;

public interface IColoringContext
{
	Color GetColor(ColoringTypes type, object item);

	bool CanSetColor(ColoringTypes type, object item);

	void SetColor(ColoringTypes type, object item, Color newValue);
}
