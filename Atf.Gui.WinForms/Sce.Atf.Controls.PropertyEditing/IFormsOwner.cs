using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Controls.PropertyEditing;

internal interface IFormsOwner
{
	IEnumerable<Form> Forms { get; }
}
