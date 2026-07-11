using System.Drawing;

namespace Sce.Atf.Applications;

public interface IStatusText
{
	string Text { get; set; }

	Color ForeColor { get; set; }
}
