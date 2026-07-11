using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.AssetPreviewing;

public interface IPaintedPropertyEditor
{
	Rectangle HandleMouseDown(MouseButtons downBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt);

	Rectangle HandleMouseUp(MouseButtons upBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt);

	Rectangle HandleMouseMove(MouseButtons pressedBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt);

	Rectangle HandleMouseClick(MouseButtons clickedBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt);

	void PaintValue(PaintedStyleInfo colors, PaintedState state, object value, Graphics canvas, Rectangle rectangle);
}
