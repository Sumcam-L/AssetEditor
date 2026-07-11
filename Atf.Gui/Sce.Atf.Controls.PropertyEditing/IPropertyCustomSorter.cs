using System.Collections;

namespace Sce.Atf.Controls.PropertyEditing;

public interface IPropertyCustomSorter
{
	IComparer GetComparer(bool ascending);
}
