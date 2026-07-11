using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.CurveEditing;

public interface IControlPoint
{
	ICurve Parent { get; }

	float X { get; set; }

	float Y { get; set; }

	Vec2F TangentIn { get; set; }

	CurveTangentTypes TangentInType { get; set; }

	Vec2F TangentOut { get; set; }

	CurveTangentTypes TangentOutType { get; set; }

	bool BrokenTangents { get; set; }

	PointEditorData EditorData { get; }

	IControlPoint Clone();
}
