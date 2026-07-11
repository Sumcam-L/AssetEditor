using System.Drawing;

namespace Sce.Atf.Controls.Adaptable;

public interface IAnnotation
{
	string Text { get; }

	Rectangle Bounds { get; }

	void SetTextSize(Size size);
}
