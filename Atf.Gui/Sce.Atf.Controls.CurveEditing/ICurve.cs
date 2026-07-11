using System.Collections.ObjectModel;
using System.Drawing;

namespace Sce.Atf.Controls.CurveEditing;

public interface ICurve
{
	string Name { get; set; }

	string DisplayName { get; set; }

	InterpolationTypes CurveInterpolation { get; set; }

	bool Visible { get; set; }

	float MinX { get; set; }

	float MaxX { get; set; }

	float MinY { get; set; }

	float MaxY { get; set; }

	string XLabel { get; set; }

	string YLabel { get; set; }

	Color CurveColor { get; set; }

	CurveLoopTypes PreInfinity { get; set; }

	CurveLoopTypes PostInfinity { get; set; }

	ReadOnlyCollection<IControlPoint> ControlPoints { get; }

	IControlPoint CreateControlPoint();

	void AddControlPoint(IControlPoint cp);

	void InsertControlPoint(int index, IControlPoint cp);

	void RemoveControlPoint(IControlPoint cp);

	void Clear();
}
